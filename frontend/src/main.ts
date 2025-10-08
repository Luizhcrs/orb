import { app, ipcMain } from 'electron';
import * as path from 'path';
import { BackendLLMManager } from './llm/BackendLLMManager';
import { WindowManager } from './managers/WindowManager';
import { ShortcutManager } from './managers/ShortcutManager';
import { MouseDetector } from './managers/MouseDetector';
import { BackendProcessManager } from './managers/BackendProcessManager';
import { ScreenshotService } from './services/ScreenshotService';
import { GlobalShortcuts } from './types/ShortcutTypes';

class OrbApp {
  private windowManager: WindowManager;
  private shortcutManager: ShortcutManager;
  private mouseDetector: MouseDetector;
  private backendProcess: BackendProcessManager;
  private _screenshotService: ScreenshotService | null = null;
  private _llmManager: BackendLLMManager | null = null;
  private lastClickTime = 0;
  private isLoadingHistory: boolean = false;

  constructor() {
    // Configurar ícone da aplicação
    const iconPath = path.join(__dirname, '../../assets/icon.svg');
    app.setAppUserModelId('com.orb.agent');
    
    // ⚡ OTIMIZAÇÃO: Lazy load - só instanciar quando necessário
    // this.llmManager e this.screenshotService agora são getters
    
    // Criar gerenciador de processo do backend
    this.backendProcess = new BackendProcessManager();
    
    // Criar window manager com callbacks
    this.windowManager = new WindowManager(
      () => this.handleChatOpen(),
      () => this.handleChatClose(),
      () => this.handleOrbShow(),
      () => this.handleOrbHide()
    );

    // Criar mouse detector com callbacks
    this.mouseDetector = new MouseDetector(
      undefined, // usar configuração padrão
      () => this.windowManager.showOrb(),
      () => this.windowManager.hideOrb()
    );

    // Criar shortcut manager
    this.shortcutManager = new ShortcutManager(this.createShortcutConfig());


    this.initializeApp();
  }

  // ⚡ Getters para lazy loading
  private get llmManager(): BackendLLMManager {
    if (!this._llmManager) {
      console.log('⚡ Lazy loading: BackendLLMManager');
      this._llmManager = new BackendLLMManager();
    }
    return this._llmManager;
  }

  private get screenshotService(): ScreenshotService {
    if (!this._screenshotService) {
      console.log('⚡ Lazy loading: ScreenshotService');
      this._screenshotService = new ScreenshotService();
    }
    return this._screenshotService;
  }

  private initializeApp() {
    app.whenReady().then(async () => {
      // Iniciar backend primeiro
      try {
        await this.backendProcess.start();
        console.log('✅ Backend iniciado com sucesso');
      } catch (error) {
        console.error('❌ Falha ao iniciar backend:', error);
        // Continuar mesmo se backend falhar (pode estar rodando separadamente em dev)
      }
      
      this.windowManager.createOrbWindow();
      this.shortcutManager.registerAll();
      this.mouseDetector.startDetection();
      this.setupIpcHandlers();
    });

    app.on('window-all-closed', () => {
      if (process.platform !== 'darwin') {
        this.cleanup();
        app.quit();
      }
    });

    app.on('activate', () => {
      if (!this.windowManager.getState().orbWindow) {
        this.windowManager.createOrbWindow();
      }
    });
  }

  private createShortcutConfig(): GlobalShortcuts {
    return {
      toggleOrb: {
        key: 'CommandOrControl+Shift+Space',
        callback: () => this.handleToggleOrb(),
        description: 'Toggle orb visibility'
      },
      toggleChat: {
        key: 'CommandOrControl+Shift+C',
        callback: () => this.windowManager.toggleChat(),
        description: 'Toggle chat window'
      },
      captureScreen: {
        key: 'CommandOrControl+Shift+S',
        callback: () => this.handleCaptureScreen(),
        description: 'Capture screen and open chat'
      },
      openConfig: {
        key: 'CommandOrControl+Shift+O',
        callback: () => {
          console.log('🔧 Atalho Ctrl+Shift+O pressionado!');
          this.windowManager.openConfig();
        },
        description: 'Open configuration window'
      }
    };
  }

  private setupIpcHandlers() {
    ipcMain.handle('send-message', async (event, message: string, imageData?: string) => {
      try {
        const result = await this.llmManager.processMessage(message, imageData);
        return result;
      } catch (error: any) {
        console.error('Erro ao processar mensagem:', error);
        // Retornar objeto de erro ao invés de throw para o IPC serializar corretamente
        return {
          error: true,
          message: error.message || 'Erro ao processar mensagem'
        };
      }
    });

    ipcMain.on('orb-clicked', () => {
      const now = Date.now();
      if (now - this.lastClickTime < 500) {
        console.log('🔵 Clique ignorado (muito rápido)');
        return;
      }
      this.lastClickTime = now;
      console.log('🔵 Evento orb-clicked recebido!');
      this.windowManager.toggleChat();
    });

    ipcMain.handle('clear-chat-messages', () => {
      // Enviar comando para limpar mensagens da interface
      const state = this.windowManager.getState();
      if (state.chatWindow && !state.chatWindow.isDestroyed()) {
        state.chatWindow.webContents.send('clear-messages');
      }
    });

    ipcMain.on('close-config', () => {
      console.log('🔧 IPC close-config recebido no main process');
      this.windowManager.closeConfig();
    });

    ipcMain.handle('load-session-history', async (event, sessionId: string) => {
      try {
        console.log('📚 Carregando histórico da sessão:', sessionId);
        
        // Carregar histórico no LLMManager
        const success = await this.llmManager.loadSessionHistory(sessionId);
        
        if (!success) {
          throw new Error('Falha ao carregar histórico');
        }
        
        // Buscar mensagens da API para retornar ao frontend
        const response = await fetch(`http://localhost:8000/api/v1/history/sessions/${sessionId}/messages`);
        
        if (!response.ok) {
          throw new Error(`Erro HTTP: ${response.status}`);
        }
        
        const messages = await response.json();
        console.log('✅ Histórico carregado:', messages.length, 'mensagens');
        
        // Abrir/criar janela de chat
        let chatWindow = this.windowManager.getState().chatWindow;
        let windowWasCreated = false;
        
        if (!chatWindow || chatWindow.isDestroyed()) {
          console.log('💬 Criando janela do chat para carregar histórico...');
          this.windowManager.createChatWindow();
          windowWasCreated = true;
          chatWindow = this.windowManager.getState().chatWindow;
        }
        
        if (!chatWindow) {
          throw new Error('Falha ao criar janela de chat');
        }
        
        // Se a janela foi recém criada, aguardar ChatInterface estar pronto
        if (windowWasCreated) {
          console.log('⏳ Aguardando ChatInterface ser inicializado...');
          await new Promise<void>((resolve) => {
            // Listener para o sinal de prontidão do ChatInterface
            const readyHandler = () => {
              console.log('✅ ChatInterface enviou sinal de prontidão!');
              resolve();
            };
            
            ipcMain.once('chat-interface-ready', readyHandler);
            
            // Timeout de segurança (5 segundos)
            setTimeout(() => {
              console.log('⚠️ Timeout atingido, prosseguindo mesmo assim...');
              ipcMain.removeListener('chat-interface-ready', readyHandler);
              resolve();
            }, 5000);
          });
        }
        
        // Marcar que estamos carregando histórico (para não limpar no handleChatOpen)
        this.isLoadingHistory = true;
        
        // Garantir que a janela está visível
        this.windowManager.openChat();
        
        // Pequeno delay adicional para garantir
        await new Promise(resolve => setTimeout(resolve, 100));
        
        // Enviar mensagens para a janela de chat
        console.log('📤 Enviando', messages.length, 'mensagens para o chat...');
        chatWindow.webContents.send('load-session-messages', messages);
        
        return {
          success: true,
          sessionId: sessionId,
          messages: messages
        };
        
      } catch (error) {
        console.error('❌ Erro ao carregar histórico:', error);
        return {
          success: false,
          error: error instanceof Error ? error.message : 'Erro desconhecido'
        };
      }
    });
  }

  private handleToggleOrb() {
    const state = this.windowManager.getState();
    if (state.orbWindow) {
      state.orbWindow.isVisible() ? this.windowManager.hideOrb() : this.windowManager.showOrb();
    }
  }

  private async handleCaptureScreen() {
    try {
      console.log('📸 Iniciando captura de tela...');
      
      const result = await this.screenshotService.captureFullScreen();
      
      if (result.success && result.imageData) {
        this.windowManager.setCapturedImage(result.imageData);
        this.openChatWithImage(result.imageData);
      } else {
        console.error('❌ Falha na captura de tela:', result.error);
      }
    } catch (error) {
      console.error('❌ Erro ao capturar tela:', error);
    }
  }

  private openChatWithImage(imageData: string) {
    console.log('🔵 Abrindo chat com imagem capturada...');
    
    this.windowManager.openChat();
    this.windowManager.sendImageToChat(imageData);
    this.windowManager.showOrb(); // Manter orb visível
  }

  private handleChatOpen() {
    // Não limpar se estamos carregando histórico
    if (this.isLoadingHistory) {
      console.log('📚 Carregando histórico, não limpando mensagens');
      this.isLoadingHistory = false;
      return;
    }
    
    // Criar nova sessão a cada abertura do chat
    this.llmManager.clearHistory();
    console.log('🔄 Nova sessão criada para o chat');
    
    // Limpar mensagens da interface visual
    const state = this.windowManager.getState();
    if (state.chatWindow && !state.chatWindow.isDestroyed()) {
      state.chatWindow.webContents.send('clear-messages');
    }
    
    this.mouseDetector.forceShowOrb();
  }

  private handleChatClose() {
    this.mouseDetector.hideOrb();
  }

  private handleOrbShow() {
    // ⚡ OTIMIZAÇÃO: Pausar polling quando Orb está visível (economizar CPU)
    this.mouseDetector.pauseDetection();
  }

  private handleOrbHide() {
    // ⚡ OTIMIZAÇÃO: Retomar polling quando Orb é escondido
    this.mouseDetector.resumeDetection();
  }


  private cleanup() {
    console.log('🧹 Limpando recursos...');
    
    this.shortcutManager.cleanup();
    this.mouseDetector.cleanup();
    this.windowManager.cleanup();
    this.backendProcess.stop();
    
    console.log('✅ Limpeza concluída');
  }
}

// Inicializar a aplicação
new OrbApp();
