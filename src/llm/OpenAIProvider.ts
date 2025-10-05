import OpenAI from 'openai';
import { LLMProvider, LLMResponse } from './LLMProvider';

export class OpenAIProvider extends LLMProvider {
  private client: OpenAI;

  constructor(config: { apiKey: string; model?: string; maxTokens?: number; temperature?: number }) {
    super({
      apiKey: config.apiKey,
      model: config.model || 'gpt-3.5-turbo',
      maxTokens: config.maxTokens || 1000,
      temperature: config.temperature || 0.7
    });

    this.client = new OpenAI({
      apiKey: this.config.apiKey
    });
  }

  async sendMessage(message: string, conversationHistory: Array<{role: 'user' | 'assistant' | 'system', content: string}> = []): Promise<LLMResponse> {
    try {
      const messages = [
        {
          role: 'system' as const,
          content: 'Você é um assistente AI útil e amigável. Responda sempre em português brasileiro de forma concisa e clara.'
        },
        ...conversationHistory.slice(-10).map(msg => ({
          role: msg.role as 'user' | 'assistant' | 'system',
          content: msg.content
        })), // Manter apenas as últimas 10 mensagens para não exceder limites
        {
          role: 'user' as const,
          content: message
        }
      ];

      const completion = await this.client.chat.completions.create({
        model: this.config.model,
        messages,
        max_tokens: this.config.maxTokens,
        temperature: this.config.temperature
      });

      const response = completion.choices[0]?.message?.content || 'Desculpe, não consegui processar sua mensagem.';
      
      return this.formatResponse(
        response,
        this.config.model,
        completion.usage?.total_tokens
      );
    } catch (error) {
      console.error('Erro ao chamar OpenAI:', error);
      throw new Error('Erro ao processar mensagem com OpenAI');
    }
  }
}
