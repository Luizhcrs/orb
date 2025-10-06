/**
 * Serviço para comunicação com o backend Python/FastAPI
 */

import axios, { AxiosInstance, AxiosResponse } from 'axios';
import WebSocket from 'ws';
import { 
  ApiResponse, 
  AgentMessageRequest, 
  AgentMessageResponse,
  ScreenshotRequest,
  ScreenshotResponse,
  ToggleOrbRequest,
  ToggleOrbResponse,
  HotCornerRequest,
  HotCornerResponse,
  HealthResponse,
  WebSocketMessage,
  WebSocketStatus
} from '../../../shared/types/api';
import { BACKEND_CONFIG, API_ENDPOINTS, WEBSOCKET_EVENTS } from '../../../shared/config/backend';

export class BackendService {
  private httpClient: AxiosInstance;
  private websocket: WebSocket | null = null;
  private websocketStatus: WebSocketStatus = {
    connected: false,
    last_activity: new Date().toISOString()
  };

  constructor() {
    this.httpClient = axios.create({
      baseURL: `${BACKEND_CONFIG.url}/api/${BACKEND_CONFIG.api_version}`,
      timeout: BACKEND_CONFIG.timeout,
      headers: {
        'Content-Type': 'application/json',
      },
    });

    this.setupHttpInterceptors();
  }

  // ============================================================================
  // HTTP METHODS
  // ============================================================================

  /**
   * Verifica se o backend está funcionando
   */
  async checkHealth(): Promise<HealthResponse> {
    const response = await this.httpClient.get<HealthResponse>(API_ENDPOINTS.HEALTH);
    return response.data;
  }

  /**
   * Envia mensagem para o agente AI
   */
  async sendMessage(request: AgentMessageRequest): Promise<AgentMessageResponse> {
    const response = await this.httpClient.post<AgentMessageResponse>(
      API_ENDPOINTS.AGENT_MESSAGE,
      request
    );
    return response.data;
  }

  /**
   * Captura screenshot da tela
   */
  async takeScreenshot(request: ScreenshotRequest = {}): Promise<ScreenshotResponse> {
    const response = await this.httpClient.post<ScreenshotResponse>(
      API_ENDPOINTS.SYSTEM_SCREENSHOT,
      request
    );
    return response.data;
  }

  /**
   * Alterna visibilidade do orb
   */
  async toggleOrb(request: ToggleOrbRequest): Promise<ToggleOrbResponse> {
    const response = await this.httpClient.post<ToggleOrbResponse>(
      API_ENDPOINTS.SYSTEM_TOGGLE_ORB,
      request
    );
    return response.data;
  }

  /**
   * Configura hot corner
   */
  async configureHotCorner(request: HotCornerRequest): Promise<HotCornerResponse> {
    const response = await this.httpClient.post<HotCornerResponse>(
      API_ENDPOINTS.SYSTEM_HOT_CORNER,
      request
    );
    return response.data;
  }

  // ============================================================================
  // WEBSOCKET METHODS
  // ============================================================================

  /**
   * Conecta ao WebSocket do backend
   */
  connectWebSocket(): Promise<void> {
    return new Promise((resolve, reject) => {
      try {
        const wsUrl = BACKEND_CONFIG.websocket_url;
        if (!wsUrl) {
          throw new Error('WebSocket URL não configurada');
        }
        this.websocket = new WebSocket(wsUrl);

        this.websocket.on('open', () => {
          console.log('WebSocket conectado');
          this.websocketStatus.connected = true;
          this.websocketStatus.connection_id = Math.random().toString(36);
          resolve();
        });

        this.websocket.on('message', (data: WebSocket.Data) => {
          try {
            const message: WebSocketMessage = JSON.parse(data.toString());
            this.handleWebSocketMessage(message);
          } catch (error) {
            console.error('Erro ao processar mensagem WebSocket:', error);
          }
        });

        this.websocket.on('close', () => {
          console.log('WebSocket desconectado');
          this.websocketStatus.connected = false;
        });

        this.websocket.on('error', (error: Error) => {
          console.error('Erro WebSocket:', error);
          reject(error);
        });

      } catch (error) {
        reject(error);
      }
    });
  }

  /**
   * Desconecta do WebSocket
   */
  disconnectWebSocket(): void {
    if (this.websocket) {
      this.websocket.close();
      this.websocket = null;
      this.websocketStatus.connected = false;
    }
  }

  /**
   * Envia mensagem via WebSocket
   */
  sendWebSocketMessage(type: 'message' | 'status' | 'error' | 'notification', payload: any): void {
    if (this.websocket && this.websocketStatus.connected) {
      const message: WebSocketMessage = {
        type,
        payload,
        timestamp: new Date().toISOString(),
        id: Math.random().toString(36)
      };
      this.websocket.send(JSON.stringify(message));
    } else {
      console.warn('WebSocket não está conectado');
    }
  }

  /**
   * Obtém status da conexão WebSocket
   */
  getWebSocketStatus(): WebSocketStatus {
    return { ...this.websocketStatus };
  }

  // ============================================================================
  // PRIVATE METHODS
  // ============================================================================

  /**
   * Configura interceptors HTTP
   */
  private setupHttpInterceptors(): void {
    // Request interceptor
    this.httpClient.interceptors.request.use(
      (config) => {
        console.log(`HTTP ${config.method?.toUpperCase()} ${config.url}`);
        return config;
      },
      (error) => {
        console.error('Erro na requisição HTTP:', error);
        return Promise.reject(error);
      }
    );

    // Response interceptor
    this.httpClient.interceptors.response.use(
      (response: AxiosResponse) => {
        console.log(`HTTP ${response.status} ${response.config.url}`);
        return response;
      },
      (error) => {
        console.error('Erro na resposta HTTP:', error);
        return Promise.reject(error);
      }
    );
  }

  /**
   * Processa mensagens recebidas via WebSocket
   */
  private handleWebSocketMessage(message: WebSocketMessage): void {
    this.websocketStatus.last_activity = new Date().toISOString();

    switch (message.type) {
      case WEBSOCKET_EVENTS.STATUS:
        console.log('Status do backend:', message.payload);
        break;
      
      case WEBSOCKET_EVENTS.NOTIFICATION:
        console.log('Notificação:', message.payload);
        break;
      
      case WEBSOCKET_EVENTS.ERROR:
        console.error('Erro do backend:', message.payload);
        break;
      
      default:
        console.log('Mensagem WebSocket:', message);
    }
  }
}

// Instância singleton
export const backendService = new BackendService();
