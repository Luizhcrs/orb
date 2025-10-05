import Anthropic from '@anthropic-ai/sdk';
import { LLMProvider, LLMResponse } from './LLMProvider';

export class AnthropicProvider extends LLMProvider {
  private client: Anthropic;

  constructor(config: { apiKey: string; model?: string; maxTokens?: number; temperature?: number }) {
    super({
      apiKey: config.apiKey,
      model: config.model || 'claude-3-haiku-20240307',
      maxTokens: config.maxTokens || 1000,
      temperature: config.temperature || 0.7
    });

    this.client = new Anthropic({
      apiKey: this.config.apiKey
    });
  }

  async sendMessage(message: string, conversationHistory: Array<{role: 'user' | 'assistant' | 'system', content: string}> = [], imageData?: string): Promise<LLMResponse> {
    try {
      // Construir contexto da conversa
      let context = 'Você é um assistente AI útil e amigável. Responda sempre em português brasileiro de forma concisa e clara.\n\n';
      
      // Adicionar histórico recente (últimas 5 trocas)
      const recentHistory = conversationHistory.slice(-10);
      for (let i = 0; i < recentHistory.length; i += 2) {
        if (recentHistory[i] && recentHistory[i + 1]) {
          context += `Usuário: ${recentHistory[i].content}\n`;
          context += `Assistente: ${recentHistory[i + 1].content}\n\n`;
        }
      }

      const response = await this.client.messages.create({
        model: this.config.model,
        max_tokens: this.config.maxTokens,
        temperature: this.config.temperature,
        system: context,
        messages: [
          {
            role: 'user',
            content: imageData ? [
              {
                type: 'text',
                text: message
              },
              {
                type: 'image',
                source: {
                  type: 'base64',
                  media_type: 'image/png',
                  data: imageData.split(',')[1] // Remove o prefixo data:image/png;base64,
                }
              }
            ] : message
          }
        ]
      });

      const responseText = response.content[0]?.type === 'text' 
        ? response.content[0].text 
        : 'Desculpe, não consegui processar sua mensagem.';

      return this.formatResponse(
        responseText,
        this.config.model,
        response.usage.input_tokens + response.usage.output_tokens
      );
    } catch (error) {
      console.error('Erro ao chamar Anthropic:', error);
      throw new Error('Erro ao processar mensagem com Anthropic');
    }
  }
}
