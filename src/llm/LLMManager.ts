import { OpenAIProvider } from './OpenAIProvider';
import { AnthropicProvider } from './AnthropicProvider';
import { LLMProvider, LLMResponse } from './LLMProvider';
import * as dotenv from 'dotenv';

dotenv.config();

export class LLMManager {
  private provider: LLMProvider | null = null;
  private conversationHistory: Array<{role: 'user' | 'assistant' | 'system', content: string}> = [];

  constructor() {
    this.initializeProvider();
  }

  private initializeProvider() {
    const openaiKey = process.env.OPENAI_API_KEY;
    const anthropicKey = process.env.ANTHROPIC_API_KEY;

    if (openaiKey) {
      this.provider = new OpenAIProvider({
        apiKey: openaiKey,
        model: process.env.DEFAULT_MODEL || 'gpt-3.5-turbo',
        maxTokens: parseInt(process.env.MAX_TOKENS || '1000'),
        temperature: parseFloat(process.env.TEMPERATURE || '0.7')
      });
      console.log('✅ OpenAI provider inicializado');
    } else if (anthropicKey) {
      this.provider = new AnthropicProvider({
        apiKey: anthropicKey,
        model: process.env.DEFAULT_MODEL || 'claude-3-haiku-20240307',
        maxTokens: parseInt(process.env.MAX_TOKENS || '1000'),
        temperature: parseFloat(process.env.TEMPERATURE || '0.7')
      });
      console.log('✅ Anthropic provider inicializado');
    } else {
      console.warn('⚠️ Nenhuma chave de API configurada. Usando modo de demonstração.');
    }
  }

  async processMessage(message: string): Promise<LLMResponse> {
    try {
      // Adicionar mensagem do usuário ao histórico
      this.conversationHistory.push({
        role: 'user',
        content: message
      });

      let response: LLMResponse;

      if (this.provider) {
        response = await this.provider.sendMessage(message, this.conversationHistory);
        
        // Adicionar resposta do assistente ao histórico
        this.conversationHistory.push({
          role: 'assistant',
          content: response.response
        });

        // Manter apenas as últimas 20 mensagens para não exceder limites
        if (this.conversationHistory.length > 20) {
          this.conversationHistory = this.conversationHistory.slice(-20);
        }
      } else {
        // Modo de demonstração quando não há API key configurada
        response = this.getDemoResponse(message);
      }

      return response;
    } catch (error) {
      console.error('Erro ao processar mensagem:', error);
      
      // Remover a última mensagem do histórico em caso de erro
      this.conversationHistory.pop();
      
      return {
        response: 'Desculpe, ocorreu um erro ao processar sua mensagem. Verifique sua conexão com a internet e suas configurações de API.',
        timestamp: new Date().toISOString()
      };
    }
  }

  private getDemoResponse(message: string): LLMResponse {
    const responses = [
      `Interessante! Você disse: "${message}". Para usar o assistente AI real, configure uma chave de API no arquivo .env.`,
      `Entendi: "${message}". Atualmente estou no modo demonstração. Configure OPENAI_API_KEY ou ANTHROPIC_API_KEY para ativar o assistente AI.`,
      `Mensagem recebida: "${message}". Para respostas inteligentes, adicione suas chaves de API no arquivo .env.`,
      `Você escreveu: "${message}". Estou pronto para ajudar assim que você configurar as credenciais de API!`
    ];

    const randomResponse = responses[Math.floor(Math.random() * responses.length)];
    
    return {
      response: randomResponse,
      timestamp: new Date().toISOString(),
      model: 'demo-mode'
    };
  }

  clearHistory() {
    this.conversationHistory = [];
  }

  getHistoryLength(): number {
    return this.conversationHistory.length;
  }
}
