import { app, ipcMain } from 'electron';
import { LLMManager } from './llm/LLMManager';
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
  private llmManager: LLMManager;
  private lastClickTime = 0;

  constructor() {
    this.llmManager = new LLMManager();
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
        key: 'CommandOrControl+Shift+O',
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
      }
    };
  }

  private setupIpcHandlers() {
    ipcMain.handle('send-message', async (event, message: string, imageData?: string) => {
      try {
        return await this.llmManager.processMessage(message, imageData);
      } catch (error) {
        console.error('Erro ao processar mensagem:', error);
        return { error: 'Erro ao processar mensagem' };
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
