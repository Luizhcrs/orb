export interface ShortcutConfig {
  key: string;
  callback?: () => void;
  description?: string;
}

export interface GlobalShortcuts {
  toggleOrb: ShortcutConfig;
  toggleChat: ShortcutConfig;
  captureScreen: ShortcutConfig;
}

export const DEFAULT_SHORTCUTS: GlobalShortcuts = {
  toggleOrb: {
    key: 'CommandOrControl+Shift+O',
    description: 'Toggle orb visibility'
  },
  toggleChat: {
    key: 'CommandOrControl+Shift+C',
    description: 'Toggle chat window'
  },
  captureScreen: {
    key: 'CommandOrControl+Shift+S',
    description: 'Capture screen and open chat'
  }
};
