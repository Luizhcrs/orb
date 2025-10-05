export interface LLMResponse {
  response: string;
  timestamp: string;
  model?: string;
  tokens?: number;
}

export interface LLMConfig {
  apiKey: string;
  model: string;
  maxTokens: number;
  temperature: number;
}

export abstract class LLMProvider {
  protected config: LLMConfig;

  constructor(config: LLMConfig) {
    this.config = config;
  }

  abstract sendMessage(message: string, conversationHistory?: Array<{role: 'user' | 'assistant' | 'system', content: string}>): Promise<LLMResponse>;

  protected formatResponse(response: string, model?: string, tokens?: number): LLMResponse {
    return {
      response,
      timestamp: new Date().toISOString(),
      model,
      tokens
    };
  }
}
