/**
 * HistoryService - Serviço para gerenciar histórico de conversas
 * Consome a API de histórico do backend
 */

export interface ChatSession {
  session_id: string;
  title: string;
  created_at: string;
  updated_at: string;
  message_count: number;
}

export interface ChatMessage {
  role: 'user' | 'assistant' | 'system';
  content: string;
  additional_kwargs?: {
    image_data?: string;
    [key: string]: any;
  };
}

export interface SessionWithMessages {
  session: ChatSession;
  messages: ChatMessage[];
}

export class HistoryService {
  private baseUrl: string;

  constructor(baseUrl: string = 'http://localhost:8000/api/v1/history') {
    this.baseUrl = baseUrl;
  }

  /**
   * Lista todas as sessões de chat
   */
  async listSessions(limit: number = 50): Promise<ChatSession[]> {
    try {
      const response = await fetch(`${this.baseUrl}/sessions?limit=${limit}`);
      
      if (!response.ok) {
        throw new Error(`Erro ao listar sessões: ${response.statusText}`);
      }
      
      return await response.json();
    } catch (error) {
      console.error('Erro ao listar sessões:', error);
      return [];
    }
  }

  /**
   * Obtém informações de uma sessão específica
   */
  async getSession(sessionId: string): Promise<ChatSession | null> {
    try {
      const response = await fetch(`${this.baseUrl}/sessions/${sessionId}`);
      
      if (!response.ok) {
        if (response.status === 404) {
          return null;
        }
        throw new Error(`Erro ao obter sessão: ${response.statusText}`);
      }
      
      return await response.json();
    } catch (error) {
      console.error('Erro ao obter sessão:', error);
      return null;
    }
  }

  /**
   * Obtém mensagens de uma sessão
   */
  async getSessionMessages(sessionId: string, limit?: number): Promise<ChatMessage[]> {
    try {
      const url = limit 
        ? `${this.baseUrl}/sessions/${sessionId}/messages?limit=${limit}`
        : `${this.baseUrl}/sessions/${sessionId}/messages`;
      
      const response = await fetch(url);
      
      if (!response.ok) {
        throw new Error(`Erro ao obter mensagens: ${response.statusText}`);
      }
      
      return await response.json();
    } catch (error) {
      console.error('Erro ao obter mensagens:', error);
      return [];
    }
  }

  /**
   * Obtém sessão completa com mensagens
   */
  async getSessionWithMessages(sessionId: string, limit?: number): Promise<SessionWithMessages | null> {
    try {
      const url = limit 
        ? `${this.baseUrl}/sessions/${sessionId}/full?limit=${limit}`
        : `${this.baseUrl}/sessions/${sessionId}/full`;
      
      const response = await fetch(url);
      
      if (!response.ok) {
        if (response.status === 404) {
          return null;
        }
        throw new Error(`Erro ao obter sessão completa: ${response.statusText}`);
      }
      
      return await response.json();
    } catch (error) {
      console.error('Erro ao obter sessão completa:', error);
      return null;
    }
  }

  /**
   * Deleta uma sessão e todas as suas mensagens
   */
  async deleteSession(sessionId: string): Promise<boolean> {
    try {
      const response = await fetch(`${this.baseUrl}/sessions/${sessionId}`, {
        method: 'DELETE'
      });
      
      if (!response.ok) {
        throw new Error(`Erro ao deletar sessão: ${response.statusText}`);
      }
      
      return true;
    } catch (error) {
      console.error('Erro ao deletar sessão:', error);
      return false;
    }
  }

  /**
   * Limpa todas as mensagens de uma sessão
   */
  async clearSessionMessages(sessionId: string): Promise<boolean> {
    try {
      const response = await fetch(`${this.baseUrl}/sessions/${sessionId}/clear`, {
        method: 'POST'
      });
      
      if (!response.ok) {
        throw new Error(`Erro ao limpar mensagens: ${response.statusText}`);
      }
      
      return true;
    } catch (error) {
      console.error('Erro ao limpar mensagens:', error);
      return false;
    }
  }

  /**
   * Obtém estatísticas do histórico
   */
  async getStats(): Promise<{
    total_sessions: number;
    total_messages: number;
    sessions: ChatSession[];
  } | null> {
    try {
      const response = await fetch(`${this.baseUrl}/stats`);
      
      if (!response.ok) {
        throw new Error(`Erro ao obter estatísticas: ${response.statusText}`);
      }
      
      return await response.json();
    } catch (error) {
      console.error('Erro ao obter estatísticas:', error);
      return null;
    }
  }

  /**
   * Formata data para exibição
   */
  formatDate(dateString: string): string {
    const date = new Date(dateString);
    const now = new Date();
    const diff = now.getTime() - date.getTime();
    const days = Math.floor(diff / (1000 * 60 * 60 * 24));
    
    if (days === 0) {
      // Hoje
      return `Hoje, ${date.toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' })}`;
    } else if (days === 1) {
      // Ontem
      return `Ontem, ${date.toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' })}`;
    } else if (days < 7) {
      // Esta semana
      return date.toLocaleDateString('pt-BR', { weekday: 'long', hour: '2-digit', minute: '2-digit' });
    } else {
      // Mais antigo
      return date.toLocaleDateString('pt-BR', { day: '2-digit', month: 'short', hour: '2-digit', minute: '2-digit' });
    }
  }
}


