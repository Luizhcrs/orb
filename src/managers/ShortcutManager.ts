import { globalShortcut } from 'electron';
import { GlobalShortcuts, ShortcutConfig, DEFAULT_SHORTCUTS } from '../types/ShortcutTypes';

export class ShortcutManager {
  private shortcuts: GlobalShortcuts;
  private registeredShortcuts: Set<string> = new Set();

  constructor(shortcuts: GlobalShortcuts = DEFAULT_SHORTCUTS) {
    this.shortcuts = shortcuts;
  }

  /**
   * Registra todos os atalhos globais
   */
  registerAll(): void {
    console.log('⌨️ Registrando atalhos globais...');
    
    this.registerShortcut('toggleOrb', this.shortcuts.toggleOrb);
    this.registerShortcut('toggleChat', this.shortcuts.toggleChat);
    this.registerShortcut('captureScreen', this.shortcuts.captureScreen);
    
    console.log('✅ Atalhos globais registrados');
  }

  /**
   * Registra um atalho específico
   */
  private registerShortcut(name: keyof GlobalShortcuts, shortcut: ShortcutConfig): void {
    try {
      if (!shortcut.callback) {
        console.warn(`⚠️ Atalho ${shortcut.key} não possui callback, pulando registro`);
        return;
      }

      if (this.registeredShortcuts.has(shortcut.key)) {
        console.warn(`⚠️ Atalho ${shortcut.key} já está registrado`);
        return;
      }

      const success = globalShortcut.register(shortcut.key, shortcut.callback);
      
      if (success) {
        this.registeredShortcuts.add(shortcut.key);
        console.log(`✅ Atalho registrado: ${shortcut.key} (${shortcut.description || name})`);
      } else {
        console.error(`❌ Falha ao registrar atalho: ${shortcut.key}`);
      }
    } catch (error) {
      console.error(`❌ Erro ao registrar atalho ${shortcut.key}:`, error);
    }
  }

  /**
   * Desregistra um atalho específico
   */
  unregisterShortcut(key: string): void {
    try {
      globalShortcut.unregister(key);
      this.registeredShortcuts.delete(key);
      console.log(`🗑️ Atalho desregistrado: ${key}`);
    } catch (error) {
      console.error(`❌ Erro ao desregistrar atalho ${key}:`, error);
    }
  }

  /**
   * Desregistra todos os atalhos
   */
  unregisterAll(): void {
    console.log('🗑️ Desregistrando todos os atalhos...');
    
    this.registeredShortcuts.forEach(key => {
      this.unregisterShortcut(key);
    });
    
    this.registeredShortcuts.clear();
  }

  /**
   * Atualiza os callbacks dos atalhos
   */
  updateShortcuts(newShortcuts: Partial<GlobalShortcuts>): void {
    // Desregistrar atalhos antigos
    Object.entries(newShortcuts).forEach(([name, shortcut]) => {
      if (shortcut && this.shortcuts[name as keyof GlobalShortcuts]) {
        const oldKey = this.shortcuts[name as keyof GlobalShortcuts].key;
        this.unregisterShortcut(oldKey);
      }
    });

    // Atualizar configurações
    this.shortcuts = { ...this.shortcuts, ...newShortcuts };

    // Re-registrar com novos callbacks
    Object.entries(newShortcuts).forEach(([name, shortcut]) => {
      if (shortcut) {
        this.registerShortcut(name as keyof GlobalShortcuts, shortcut);
      }
    });
  }

  /**
   * Verifica se um atalho está registrado
   */
  isRegistered(key: string): boolean {
    return this.registeredShortcuts.has(key);
  }

  /**
   * Obtém lista de atalhos registrados
   */
  getRegisteredShortcuts(): string[] {
    return Array.from(this.registeredShortcuts);
  }

  /**
   * Obtém configuração atual dos atalhos
   */
  getShortcuts(): GlobalShortcuts {
    return { ...this.shortcuts };
  }

  /**
   * Limpa recursos (chamado no cleanup)
   */
  cleanup(): void {
    this.unregisterAll();
  }
}
