/**
 * Gerenciador de Processo do Backend Python
 * Inicia e gerencia o servidor FastAPI empacotado
 */

import { spawn, ChildProcess } from 'child_process';
import * as path from 'path';
import * as fs from 'fs';
import { app } from 'electron';

export class BackendProcessManager {
  private backendProcess: ChildProcess | null = null;
  private readonly backendPort = 8000;
  private readonly maxRetries = 3;
  private retryCount = 0;

  /**
   * Inicia o processo do backend Python
   */
  async start(): Promise<void> {
    console.log('🐍 Verificando backend...');

    // Verificar se backend já está rodando (como serviço ou desenvolvimento)
    if (await this.isRunning()) {
      console.log('✅ Backend já está rodando');
      return;
    }

    // Em produção (app empacotado), o backend deve estar rodando como serviço
    if (app.isPackaged) {
      console.warn('⚠️ Backend não está rodando!');
      console.warn('⚠️ O serviço OrbBackendService deve estar ativo.');
      throw new Error('Backend service não encontrado. Verifique se o serviço Windows está rodando.');
    }

    // Em desenvolvimento, tentar iniciar o backend
    console.log('🔧 Modo desenvolvimento: tentando iniciar backend...');

    try {
      const backendPath = this.getBackendPath();
      const pythonPath = this.getPythonPath();

      console.log('📂 Backend path:', backendPath);
      console.log('🐍 Python path:', pythonPath);

      // Verificar se os arquivos existem
      if (!fs.existsSync(backendPath)) {
        console.warn(`⚠️ Backend não encontrado em: ${backendPath}`);
        console.warn('⚠️ Certifique-se de que o backend está rodando separadamente em http://127.0.0.1:8000');
        return;
      }

      // Iniciar processo Python
      const scriptPath = path.join(backendPath, 'scripts', 'dev.py');
      
      this.backendProcess = spawn(pythonPath, [scriptPath], {
        cwd: backendPath,
        env: {
          ...process.env,
          PYTHONPATH: backendPath,
          PORT: String(this.backendPort),
          HOST: '127.0.0.1',
          ENVIRONMENT: 'production',
        },
        stdio: ['ignore', 'pipe', 'pipe'],
      });

      // Logging de stdout
      this.backendProcess.stdout?.on('data', (data) => {
        console.log(`[Backend] ${data.toString().trim()}`);
      });

      // Logging de stderr
      this.backendProcess.stderr?.on('data', (data) => {
        console.error(`[Backend Error] ${data.toString().trim()}`);
      });

      // Handle process exit
      this.backendProcess.on('exit', (code, signal) => {
        console.log(`🐍 Backend process exited with code ${code}, signal ${signal}`);
        
        if (code !== 0 && this.retryCount < this.maxRetries) {
          this.retryCount++;
          console.log(`🔄 Tentando reiniciar backend (tentativa ${this.retryCount}/${this.maxRetries})...`);
          setTimeout(() => this.start(), 2000);
        }
      });

      // Handle process errors
      this.backendProcess.on('error', (error) => {
        console.error('❌ Erro ao iniciar backend:', error);
      });

      // Aguardar backend estar pronto
      await this.waitForBackend();
      console.log('✅ Backend iniciado com sucesso!');

    } catch (error) {
      console.error('❌ Falha ao iniciar backend:', error);
      throw error;
    }
  }

  /**
   * Para o processo do backend
   */
  stop(): void {
    if (this.backendProcess) {
      console.log('🛑 Parando backend...');
      this.backendProcess.kill();
      this.backendProcess = null;
    }
  }

  /**
   * Verifica se o backend está rodando
   */
  async isRunning(): Promise<boolean> {
    try {
      const response = await fetch(`http://127.0.0.1:${this.backendPort}/api/v1/health`);
      return response.ok;
    } catch {
      return false;
    }
  }

  /**
   * Aguarda o backend estar pronto
   */
  private async waitForBackend(timeoutMs: number = 30000): Promise<void> {
    const startTime = Date.now();
    
    while (Date.now() - startTime < timeoutMs) {
      if (await this.isRunning()) {
        return;
      }
      await this.sleep(500);
    }

    throw new Error('Timeout aguardando backend iniciar');
  }

  /**
   * Obtém o caminho do backend baseado no ambiente
   */
  private getBackendPath(): string {
    if (app.isPackaged) {
      // Aplicação empacotada - backend está em resources/backend
      return path.join(process.resourcesPath, 'backend');
    } else {
      // Desenvolvimento - backend está na raiz do projeto
      return path.join(app.getAppPath(), '..', 'backend');
    }
  }

  /**
   * Obtém o caminho do Python
   */
  private getPythonPath(): string {
    if (app.isPackaged) {
      // Em produção, usar Python empacotado ou do sistema
      // TODO: Empacotar Python standalone com pyinstaller
      if (process.platform === 'win32') {
        return 'python';
      } else {
        return 'python3';
      }
    } else {
      // Desenvolvimento - usar python do sistema
      if (process.platform === 'win32') {
        return 'python';
      } else {
        return 'python3';
      }
    }
  }

  /**
   * Utilitário sleep
   */
  private sleep(ms: number): Promise<void> {
    return new Promise(resolve => setTimeout(resolve, ms));
  }
}

