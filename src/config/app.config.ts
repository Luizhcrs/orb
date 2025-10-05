export interface AppConfig {
  orb: {
    size: number;
    position: {
      x: number;
      y: number;
    };
    alwaysOnTop: boolean;
    transparent: boolean;
  };
  chat: {
    width: number;
    height: number;
    maxMessages: number;
  };
  shortcuts: {
    toggleOrb: string;
    toggleChat: string;
  };
  llm: {
    defaultProvider: 'openai' | 'anthropic' | 'demo';
    maxTokens: number;
    temperature: number;
  };
}

export const defaultConfig: AppConfig = {
  orb: {
    size: 80,
    position: {
      x: -100, // Relativo à direita da tela
      y: 50    // Relativo ao topo da tela
    },
    alwaysOnTop: true,
    transparent: true
  },
  chat: {
    width: 400,
    height: 600,
    maxMessages: 50
  },
  shortcuts: {
    toggleOrb: 'CommandOrControl+Shift+O',
    toggleChat: 'CommandOrControl+Shift+C'
  },
  llm: {
    defaultProvider: 'demo',
    maxTokens: 1000,
    temperature: 0.7
  }
};

export function loadConfig(): AppConfig {
  // Aqui poderia carregar configurações de um arquivo JSON
  // Por enquanto, retorna a configuração padrão
  return defaultConfig;
}
