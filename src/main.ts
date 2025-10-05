import { app, BrowserWindow, ipcMain, globalShortcut, screen } from 'electron';
import * as path from 'path';
import { LLMManager } from './llm/LLMManager';

class OrbApp {
  private orbWindow: BrowserWindow | null = null;
  private chatWindow: BrowserWindow | null = null;
  private isChatOpen = false;
  private llmManager: LLMManager;
  private lastClickTime = 0;
  private isOrbVisible = false;
  private mouseCheckInterval: NodeJS.Timeout | null = null;
  private hideTimeout: NodeJS.Timeout | null = null;

  constructor() {
    this.llmManager = new LLMManager();
    this.initializeApp();
  }

  private initializeApp() {
    app.whenReady().then(() => {
      this.createOrbWindow();
      this.registerGlobalShortcuts();
      this.setupIpcHandlers();
    });

    app.on('window-all-closed', () => {
      // Mantém o app rodando mesmo sem janelas abertas
      if (process.platform !== 'darwin') {
        this.cleanup();
        app.quit();
      }
    });

    app.on('activate', () => {
      if (this.orbWindow === null) {
        this.createOrbWindow();
      }
    });
  }

  private createOrbWindow() {
    const { width, height } = screen.getPrimaryDisplay().workAreaSize;
    
    this.orbWindow = new BrowserWindow({
      width: 90, // Aumentado mais para acomodar todos os efeitos de glow
      height: 90, // Aumentado mais para acomodar todos os efeitos de glow
      x: 0,
      y: 0,
      frame: false,
      transparent: true,
      alwaysOnTop: true,
      skipTaskbar: true,
      resizable: false,
      show: false, // Iniciar escondido
      webPreferences: {
        nodeIntegration: true,
        contextIsolation: false
      }
    });

    this.orbWindow.loadFile(path.join(__dirname, 'orb.html'));
    
    // Remove o menu
    this.orbWindow.setMenu(null);
    
    // Torna a janela sempre visível
    this.orbWindow.setAlwaysOnTop(true, 'screen-saver');
    
    // Evento de clique no orb - usando JavaScript no HTML
    this.orbWindow.webContents.on('did-finish-load', () => {
      console.log('🔵 Orb carregado, configurando eventos...');
      this.orbWindow?.webContents.executeJavaScript(`
        console.log('🔵 Script executado no orb');
        document.addEventListener('click', (e) => {
          console.log('🔵 Clique detectado no orb!');
          require('electron').ipcRenderer.send('orb-clicked');
        });
        document.addEventListener('mousedown', (e) => {
          console.log('🔵 Mouse down detectado no orb!');
          require('electron').ipcRenderer.send('orb-clicked');
        });
      `);
    });

    this.orbWindow.on('closed', () => {
      this.orbWindow = null;
    });

    // Inicialmente escondido (já está com show: false)
    this.isOrbVisible = false;
    
    // Iniciar detecção de mouse
    this.startMouseDetection();
  }

  private createChatWindow() {
    if (this.chatWindow) return;

    const { width, height } = screen.getPrimaryDisplay().workAreaSize;
    
    this.chatWindow = new BrowserWindow({
      width: 380, // Aumentado mais para acomodar efeitos de glow
      height: 480, // Aumentado mais para acomodar efeitos de glow
      x: Math.round((width - 380) / 2), // Centralizar horizontalmente
      y: Math.round((height - 480) / 2), // Centralizar verticalmente
      frame: false,
      transparent: true,
      alwaysOnTop: true,
      resizable: true,
      minimizable: false,
      maximizable: false,
      webPreferences: {
        nodeIntegration: true,
        contextIsolation: false
      }
    });

    this.chatWindow.loadFile(path.join(__dirname, 'chat.html'));
    this.chatWindow.setMenu(null);

    this.chatWindow.on('closed', () => {
      console.log('🔵 Janela de chat fechada');
      this.chatWindow = null;
      this.isChatOpen = false;
      // Esconder orb quando chat for fechado
      this.hideOrb();
    });
  }

  private toggleChat() {
    console.log('🔵 Toggle chat chamado, isChatOpen:', this.isChatOpen);
    if (this.isChatOpen) {
      console.log('🔵 Fechando chat...');
      this.closeChat();
    } else {
      console.log('🔵 Abrindo chat...');
      this.openChat();
    }
  }

  private openChat() {
    if (this.chatWindow && !this.chatWindow.isDestroyed()) {
      console.log('🔵 Chat já existe, apenas mostrando...');
      this.chatWindow.show();
      this.chatWindow.focus();
      return;
    }
    console.log('🔵 Criando nova janela de chat...');
    this.createChatWindow();
    this.isChatOpen = true;
    // Manter orb visível quando chat estiver aberto
    this.showOrb();
  }

  private closeChat() {
    if (this.chatWindow && !this.chatWindow.isDestroyed()) {
      console.log('🔵 Fechando janela de chat...');
      this.chatWindow.hide();
      // Não destruir a janela, apenas esconder
      // this.chatWindow.close();
      // this.chatWindow = null;
    }
    this.isChatOpen = false;
  }

  private expandChat() {
    if (this.chatWindow && !this.chatWindow.isDestroyed()) {
      const { width, height } = screen.getPrimaryDisplay().workAreaSize;
      
      // Alternar entre tamanho compacto e expandido
      const currentBounds = this.chatWindow.getBounds();
      const isExpanded = currentBounds.width > 400;
      
      if (isExpanded) {
        // Voltar ao tamanho compacto
        this.chatWindow.setBounds({
          x: Math.round((width - 380) / 2),
          y: Math.round((height - 480) / 2),
          width: 380,
          height: 480
        });
      } else {
        // Expandir para tamanho maior
        this.chatWindow.setBounds({
          x: Math.round((width - 660) / 2),
          y: Math.round((height - 760) / 2),
          width: 660,
          height: 760
        });
      }
    }
  }

  private registerGlobalShortcuts() {
    // Ctrl+Shift+O para toggle do orb
    globalShortcut.register('CommandOrControl+Shift+O', () => {
      if (this.orbWindow) {
        this.orbWindow.isVisible() ? this.orbWindow.hide() : this.orbWindow.show();
      }
    });

    // Ctrl+Shift+C para toggle do chat
    globalShortcut.register('CommandOrControl+Shift+C', () => {
      this.toggleChat();
    });
  }

  private setupIpcHandlers() {
    ipcMain.handle('send-message', async (event, message: string) => {
      try {
        // Aqui será implementada a integração com LLM
        return await this.processMessage(message);
      } catch (error) {
        console.error('Erro ao processar mensagem:', error);
        return { error: 'Erro ao processar mensagem' };
      }
    });

    ipcMain.handle('close-chat', () => {
      this.closeChat();
    });

    ipcMain.handle('expand-chat', () => {
      this.expandChat();
    });

    ipcMain.on('orb-clicked', () => {
      const now = Date.now();
      if (now - this.lastClickTime < 500) {
        console.log('🔵 Clique ignorado (muito rápido)');
        return;
      }
      this.lastClickTime = now;
      console.log('🔵 Evento orb-clicked recebido!');
      this.toggleChat();
    });
  }

  private async processMessage(message: string): Promise<any> {
    return await this.llmManager.processMessage(message);
  }

  private startMouseDetection() {
    // Aguardar um pouco antes de começar a detectar para evitar aparecer na inicialização
    setTimeout(() => {
      this.mouseCheckInterval = setInterval(() => {
        const { screen } = require('electron');
        const cursor = screen.getCursorScreenPoint();
        
        // Verificar se o mouse está exatamente na quina (área muito pequena de 3x3px)
        if (cursor.x <= 3 && cursor.y <= 3) {
          if (!this.isOrbVisible) {
            this.showOrb();
          }
          // Cancelar timeout de esconder se ainda estiver na quina
          if (this.hideTimeout) {
            clearTimeout(this.hideTimeout);
            this.hideTimeout = null;
          }
        } else {
          if (this.isOrbVisible && !this.isChatOpen) {
            // Adicionar delay antes de esconder para evitar flickering
            if (!this.hideTimeout) {
              this.hideTimeout = setTimeout(() => {
                this.hideOrb();
                this.hideTimeout = null;
              }, 200); // Reduzido para 200ms
            }
          }
        }
      }, 50); // Verificar a cada 50ms
    }, 1000); // Aguardar 1 segundo antes de começar a detectar
  }

  private showOrb() {
    if (this.orbWindow && !this.isOrbVisible) {
      this.orbWindow.show();
      this.isOrbVisible = true;
      console.log('🔵 Orb aparecendo...');
    }
  }

  private hideOrb() {
    if (this.orbWindow && this.isOrbVisible && !this.isChatOpen) {
      this.orbWindow.hide();
      this.isOrbVisible = false;
      console.log('🔵 Orb escondendo...');
    }
  }

  private cleanup() {
    if (this.mouseCheckInterval) {
      clearInterval(this.mouseCheckInterval);
      this.mouseCheckInterval = null;
    }
    if (this.hideTimeout) {
      clearTimeout(this.hideTimeout);
      this.hideTimeout = null;
    }
  }
}

new OrbApp();
