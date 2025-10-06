/**
 * LLM Manager que usa o backend Python/FastAPI
 */

import { backendService } from '../services/BackendService';
import { 
  AgentMessageRequest, 
  AgentMessageResponse,
  ConversationContext,
  ConversationMessage 
} from '../../../shared/types/api';

export interface LLMResponse {
  response: string;
  timestamp: string;
  model?: string;
  provider?: string;
  processing_time?: number;
}

export class BackendLLMManager {
  private conversationHistory: ConversationMessage[] = [];
  private sessionId: string;

  constructor() {
    this.sessionId = this.generateSessionId();
  }

  async processMessage(message: string, imageData?: string): Promise<LLMResponse> {
    try {
      // Preparar contexto da conversa incluindo a mensagem atual do usuário
      const currentUserMessage: ConversationMessage = {
        role: 'user',
        content: message,
        timestamp: new Date().toISOString()
      };
      
      const conversationContext: ConversationContext = {
        messages: [...this.conversationHistory, currentUserMessage],
        session_data: {
          session_id: this.sessionId,
          frontend_version: '1.0.0'
        }
      };

      // Preparar requisição
      const request: AgentMessageRequest = {
        message,
        conversation_context: conversationContext,
        image_data: imageData,
        session_id: this.sessionId
      };

      // Enviar para o backend
      console.log('📤 Enviando mensagem para backend:', { message, sessionId: this.sessionId });
      const response: AgentMessageResponse = await backendService.sendMessage(request);
      console.log('📥 Resposta recebida do backend:', response);

      // Adicionar apenas a resposta do assistente ao histórico
      this.conversationHistory.push({
        role: 'assistant',
        content: response.content || response.response || 'Sem resposta',
        timestamp: new Date().toISOString(),
        metadata: {
          model_used: response.model_used,
          provider: response.provider,
          processing_time: response.processing_time || 0
        }
      });

      // Manter apenas as últimas 20 mensagens para não exceder limites
      if (this.conversationHistory.length > 20) {
        this.conversationHistory = this.conversationHistory.slice(-20);
      }

      // Retornar resposta formatada
      return {
        response: response.content || response.response || 'Sem resposta',
        timestamp: new Date().toISOString(),
        model: response.model_used || 'unknown',
        provider: response.provider || 'unknown',
        processing_time: response.processing_time || 0
      };

    } catch (error) {
      console.error('Erro ao processar mensagem:', error);
      
      return {
        response: 'Desculpe, ocorreu um erro ao processar sua mensagem. Verifique se o backend está funcionando.',
        timestamp: new Date().toISOString(),
        model: 'error',
        provider: 'backend'
      };
    }
  }

  async checkBackendHealth(): Promise<boolean> {
    try {
      await backendService.checkHealth();
      return true;
    } catch (error) {
      console.error('Backend não está disponível:', error);
      return false;
    }
  }

  clearHistory(): void {
    this.conversationHistory = [];
    this.sessionId = this.generateSessionId();
  }

  getHistoryLength(): number {
    return this.conversationHistory.length;
  }

  getConversationHistory(): ConversationMessage[] {
    return [...this.conversationHistory];
  }

  private generateSessionId(): string {
    return `session_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
  }
}
