import { app, ipcMain } from 'electron';
import * as path from 'path';
import { BackendLLMManager } from './llm/BackendLLMManager';
import { WindowManager } from './managers/WindowManager';
import { ShortcutManager } from './managers/ShortcutManager';
import { MouseDetector } from './managers/MouseDetector';
import { ScreenshotService } from './services/ScreenshotService';
import { GlobalShortcuts } from './types/ShortcutTypes';

class OrbApp {
  private windowManager: WindowManager;
  private shortcutManager: ShortcutManager;
  private mouseDetector: MouseDetector;
  private screenshotService: ScreenshotService;
  private llmManager: BackendLLMManager;
  private lastClickTime = 0;

  constructor() {
    // Configurar ícone da aplicação
    const iconPath = path.join(__dirname, '../../assets/icon.svg');
    app.setAppUserModelId('com.orb.agent');
    
    this.llmManager = new BackendLLMManager();
    this.screenshotService = new ScreenshotService();
    
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

  private initializeApp() {
    app.whenReady().then(() => {
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
      } catch (error) {
        console.error('Erro ao processar mensagem:', error);
        return { 
          error: 'Erro ao processar mensagem',
          content: 'Desculpe, ocorreu um erro. Tente novamente.',
          timestamp: new Date().toISOString()
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
    // Callback quando orb é mostrado
  }

  private handleOrbHide() {
    // Callback quando orb é escondido
  }


  private cleanup() {
    console.log('🧹 Limpando recursos...');
    
    this.shortcutManager.cleanup();
    this.mouseDetector.cleanup();
    this.windowManager.cleanup();
    
    console.log('✅ Limpeza concluída');
  }
}

// Inicializar a aplicação
new OrbApp();
