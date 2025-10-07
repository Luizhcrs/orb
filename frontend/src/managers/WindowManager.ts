import { BrowserWindow, screen, ipcMain } from 'electron';
import * as path from 'path';
import { WindowManagerState, OrbWindowConfig, ChatWindowConfig } from '../types/WindowTypes';

export class WindowManager {
  private state: WindowManagerState;
  private onChatOpen: () => void;
  private onChatClose: () => void;
  private onOrbShow: () => void;
  private onOrbHide: () => void;

  constructor(
    onChatOpen: () => void,
    onChatClose: () => void,
    onOrbShow: () => void,
    onOrbHide: () => void
  ) {
    this.onChatOpen = onChatOpen;
    this.onChatClose = onChatClose;
    this.onOrbShow = onOrbShow;
    this.onOrbHide = onOrbHide;
    
    this.state = {
      orbWindow: null,
      chatWindow: null,
      configWindow: null,
      isChatOpen: false,
      isOrbVisible: false,
      capturedImage: null
    };

    this.setupIpcHandlers();
  }

  /**
   * Cria a janela do orb
   */
  createOrbWindow(): BrowserWindow {
    if (this.state.orbWindow && !this.state.orbWindow.isDestroyed()) {
      return this.state.orbWindow;
    }

    console.log('🔵 Criando janela do orb...');
    
    const orbConfig: OrbWindowConfig = {
      width: 90,
      height: 90,
      x: 0,
      y: 0,
      frame: false,
      transparent: true,
      alwaysOnTop: true,
      skipTaskbar: true,
      resizable: false,
      movable: false,
      minimizable: false,
      maximizable: false,
      closable: false,
      focusable: true,
      show: false
    };

    this.state.orbWindow = new BrowserWindow({
      ...orbConfig,
      icon: path.join(__dirname, '../../assets/icon.svg'),
      webPreferences: {
        nodeIntegration: true,
        contextIsolation: false
      }
    });

    this.state.orbWindow.loadFile(path.join(__dirname, '..', 'src', 'orb.html'));
    this.state.isOrbVisible = false;

    this.state.orbWindow.on('closed', () => {
      this.state.orbWindow = null;
    });

    console.log('✅ Janela do orb criada');
    return this.state.orbWindow;
  }

  /**
   * Cria a janela do chat
   */
  createChatWindow(): BrowserWindow {
    if (this.state.chatWindow && !this.state.chatWindow.isDestroyed()) {
      return this.state.chatWindow;
    }

    console.log('💬 Criando janela do chat...');
    
    const { width: screenWidth, height: screenHeight } = screen.getPrimaryDisplay().workAreaSize;
    const chatConfig: ChatWindowConfig = {
      width: 380,
      height: 480,
      x: Math.floor((screenWidth - 380) / 2),
      y: Math.floor((screenHeight - 480) / 2),
      frame: false,
      transparent: true,
      alwaysOnTop: true,
      skipTaskbar: true,
      resizable: false,
      movable: true,
      minimizable: false,
      maximizable: false,
      closable: false,
      focusable: true,
      show: false
    };

    this.state.chatWindow = new BrowserWindow({
      ...chatConfig,
      webPreferences: {
        nodeIntegration: true,
        contextIsolation: false
      }
    });

    this.state.chatWindow.loadFile(path.join(__dirname, '..', 'src', 'chat.html'));

    this.state.chatWindow.on('closed', () => {
      this.state.chatWindow = null;
      this.state.isChatOpen = false;
      this.onChatClose();
    });

    console.log('✅ Janela do chat criada');
    return this.state.chatWindow;
  }

  /**
   * Abre o chat
   */
  openChat(): void {
    if (!this.state.chatWindow || this.state.chatWindow.isDestroyed()) {
      this.createChatWindow();
    }

    if (this.state.chatWindow) {
      this.state.chatWindow.show();
      this.state.chatWindow.focus();
      this.state.isChatOpen = true;
      this.onChatOpen();
      console.log('🔵 Chat aberto');
    }
  }

  /**
   * Fecha o chat
   */
  closeChat(): void {
    if (this.state.chatWindow && !this.state.chatWindow.isDestroyed()) {
      this.state.chatWindow.hide();
      this.state.isChatOpen = false;
      this.onChatClose();
      console.log('🔵 Chat fechado');
    }
  }

  /**
   * Toggle do chat (abre se fechado, fecha se aberto)
   */
  toggleChat(): void {
    if (this.state.isChatOpen) {
      this.closeChat();
    } else {
      this.openChat();
    }
  }

  /**
   * Expande o chat para tamanho maior
   */
  expandChat(): void {
    if (!this.state.chatWindow || this.state.chatWindow.isDestroyed()) {
      return;
    }

    const { width: screenWidth, height: screenHeight } = screen.getPrimaryDisplay().workAreaSize;
    const currentBounds = this.state.chatWindow.getBounds();
    
    // Alternar entre tamanho compacto (380x480) e expandido (660x760)
    const isExpanded = currentBounds.width > 400;
    
    const newWidth = isExpanded ? 380 : 660;
    const newHeight = isExpanded ? 480 : 760;
    const newX = Math.floor((screenWidth - newWidth) / 2);
    const newY = Math.floor((screenHeight - newHeight) / 2);

    this.state.chatWindow.setBounds({
      x: newX,
      y: newY,
      width: newWidth,
      height: newHeight
    });

    console.log(`🔵 Chat ${isExpanded ? 'reduzido' : 'expandido'} para ${newWidth}x${newHeight}`);
  }

  /**
   * Mostra o orb
   */
  showOrb(): void {
    if (this.state.orbWindow && !this.state.orbWindow.isDestroyed()) {
      this.state.orbWindow.show();
      this.state.isOrbVisible = true;
      this.onOrbShow();
      console.log('👁️ Orb mostrado');
    }
  }

  /**
   * Esconde o orb
   */
  hideOrb(): void {
    if (this.state.orbWindow && !this.state.orbWindow.isDestroyed()) {
      this.state.orbWindow.hide();
      this.state.isOrbVisible = false;
      this.onOrbHide();
      console.log('👁️‍🗨️ Orb escondido');
    }
  }

  /**
   * Define a imagem capturada
   */
  setCapturedImage(imageData: string | null): void {
    this.state.capturedImage = imageData;
  }

  /**
   * Obtém a imagem capturada
   */
  getCapturedImage(): string | null {
    return this.state.capturedImage;
  }

  /**
   * Envia imagem capturada para o chat
   */
  sendImageToChat(imageData: string): void {
    if (this.state.chatWindow && !this.state.chatWindow.isDestroyed()) {
      this.state.chatWindow.webContents.send('image-captured', imageData);
    }
  }

  /**
   * Configura handlers IPC
   */
  private setupIpcHandlers(): void {
    ipcMain.handle('close-chat', () => {
      this.closeChat();
    });

    ipcMain.handle('expand-chat', () => {
      this.expandChat();
    });

    ipcMain.handle('clear-captured-image', () => {
      this.state.capturedImage = null;
      console.log('🗑️ Imagem capturada removida');
    });
  }

  /**
   * Obtém o estado atual
   */
  getState(): WindowManagerState {
    return { ...this.state };
  }

  /**
   * Cria janela de configuração
   */
  createConfigWindow(): void {
    if (this.state.configWindow && !this.state.configWindow.isDestroyed()) {
      this.state.configWindow.focus();
      return;
    }

    const configPath = path.join(__dirname, '..', '..', 'src', 'src', 'config.html');
    console.log('🔧 Caminho do config:', configPath);
    
    this.state.configWindow = new BrowserWindow({
      width: 900,
      height: 600,
      minWidth: 700,
      minHeight: 500,
      resizable: true,
      movable: true,
      frame: false,
      transparent: true,
      alwaysOnTop: true,
      skipTaskbar: true,
      show: true,
      webPreferences: {
        nodeIntegration: true,
        contextIsolation: false
      }
    });

    this.state.configWindow.loadFile(configPath);

    this.state.configWindow.once('ready-to-show', () => {
      this.state.configWindow?.center();
      this.state.configWindow?.focus();
    });

    this.state.configWindow.on('closed', () => {
      this.state.configWindow = null;
    });

    // Garantir que a janela seja focada e visível
    this.state.configWindow.focus();
  }

  /**
   * Abre janela de configuração
   */
  openConfig(): void {
    console.log('🔧 Abrindo janela de configuração...');
    this.createConfigWindow();
  }

  /**
   * Fecha janela de configuração
   */
  closeConfig(): void {
    console.log('🔧 WindowManager.closeConfig() chamado');
    if (this.state.configWindow && !this.state.configWindow.isDestroyed()) {
      console.log('🔧 Fechando configWindow...');
      this.state.configWindow.close();
    } else {
      console.log('🔧 configWindow não existe ou já foi destruída');
    }
  }

  /**
   * Minimiza janela de configuração
   */
  minimizeConfig(): void {
    if (this.state.configWindow && !this.state.configWindow.isDestroyed()) {
      this.state.configWindow.minimize();
    }
  }

  /**
   * Limpa recursos (chamado no cleanup)
   */
  cleanup(): void {
    if (this.state.orbWindow && !this.state.orbWindow.isDestroyed()) {
      this.state.orbWindow.close();
    }
    
    if (this.state.chatWindow && !this.state.chatWindow.isDestroyed()) {
      this.state.chatWindow.close();
    }

    if (this.state.configWindow && !this.state.configWindow.isDestroyed()) {
      this.state.configWindow.close();
    }
  }
}
