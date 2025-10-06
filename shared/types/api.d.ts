/**
 * Tipos compartilhados para comunicação entre Frontend e Backend
 */
export interface ApiResponse<T = any> {
    success: boolean;
    data?: T;
    error?: string;
    message?: string;
    timestamp: string;
}
export interface HealthResponse {
    status: string;
    timestamp: string;
    service: string;
    version: string;
}
export interface AgentMessageRequest {
    message: string;
    conversation_context?: ConversationContext;
    image_data?: string;
    user_id?: string;
    session_id?: string;
}
export interface AgentMessageResponse {
    response: string;
    model_used: string;
    provider: string;
    processing_time: number;
    conversation_context: ConversationContext;
    tool_result?: ToolResult;
    metadata?: Record<string, any>;
}
export interface ConversationContext {
    messages: ConversationMessage[];
    user_preferences?: UserPreferences;
    session_data?: Record<string, any>;
}
export interface ConversationMessage {
    role: 'user' | 'assistant' | 'system';
    content: string;
    timestamp: string;
    metadata?: Record<string, any>;
}
export interface UserPreferences {
    language: string;
    theme: 'light' | 'dark' | 'auto';
    model_preference: string;
    response_style: 'concise' | 'detailed' | 'balanced';
}
export interface ToolResult {
    tool_name: string;
    success: boolean;
    result?: any;
    error?: string;
    execution_time: number;
}
export interface ScreenshotRequest {
    format?: 'png' | 'jpeg';
    quality?: number;
    region?: {
        x: number;
        y: number;
        width: number;
        height: number;
    };
}
export interface ScreenshotResponse {
    success: boolean;
    image_path?: string;
    image_data?: string;
    message: string;
    timestamp: string;
}
export interface ToggleOrbRequest {
    action: 'show' | 'hide' | 'toggle';
    duration?: number;
}
export interface ToggleOrbResponse {
    success: boolean;
    visible: boolean;
    message: string;
    timestamp: string;
}
export interface HotCornerRequest {
    action: 'enable' | 'disable' | 'configure';
    position?: 'top-left' | 'top-right' | 'bottom-left' | 'bottom-right';
    sensitivity?: number;
}
export interface HotCornerResponse {
    success: boolean;
    enabled: boolean;
    position?: string;
    message: string;
    timestamp: string;
}
export interface WebSocketMessage {
    type: 'message' | 'status' | 'error' | 'notification';
    payload: any;
    timestamp: string;
    id?: string;
}
export interface WebSocketStatus {
    connected: boolean;
    last_activity: string;
    connection_id?: string;
}
export interface ApiError {
    code: string;
    message: string;
    details?: Record<string, any>;
    timestamp: string;
}
export interface ValidationError {
    field: string;
    message: string;
    value?: any;
}
export interface BackendConfig {
    url: string;
    api_version: string;
    timeout: number;
    retry_attempts: number;
    websocket_url?: string;
}
export interface FrontendConfig {
    window: {
        width: number;
        height: number;
        min_width: number;
        min_height: number;
        always_on_top: boolean;
        skip_taskbar: boolean;
    };
    orb: {
        size: number;
        opacity: number;
        hot_corner: {
            enabled: boolean;
            position: string;
            sensitivity: number;
        };
    };
    chat: {
        max_messages: number;
        auto_scroll: boolean;
        show_timestamps: boolean;
    };
}
export interface LLMConfig {
    provider: 'openai' | 'anthropic';
    model: string;
    max_tokens: number;
    temperature: number;
    api_key?: string;
}
export interface LLMResponse {
    content: string;
    model: string;
    provider: string;
    usage?: {
        prompt_tokens: number;
        completion_tokens: number;
        total_tokens: number;
    };
    finish_reason?: string;
}
//# sourceMappingURL=api.d.ts.map