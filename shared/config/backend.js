"use strict";
/**
 * Configuração compartilhada para comunicação com o backend
 */
Object.defineProperty(exports, "__esModule", { value: true });
exports.HTTP_STATUS = exports.WEBSOCKET_EVENTS = exports.API_ENDPOINTS = exports.BACKEND_CONFIG = void 0;
exports.BACKEND_CONFIG = {
    url: process.env.ORB_BACKEND_URL || 'http://localhost:8000',
    api_version: 'v1',
    timeout: 30000, // 30 segundos
    retry_attempts: 3,
    websocket_url: process.env.ORB_WEBSOCKET_URL || 'ws://localhost:8000/ws'
};
exports.API_ENDPOINTS = {
    HEALTH: '/health',
    AGENT_MESSAGE: '/agent/message',
    AGENT_WEBSOCKET: '/ws',
    SYSTEM_SCREENSHOT: '/system/screenshot',
    SYSTEM_TOGGLE_ORB: '/system/toggle-orb',
    SYSTEM_HOT_CORNER: '/system/hot-corner',
    DOCS: '/docs',
    OPENAPI: '/openapi.json'
};
exports.WEBSOCKET_EVENTS = {
    CONNECT: 'connect',
    DISCONNECT: 'disconnect',
    MESSAGE: 'message',
    STATUS: 'status',
    ERROR: 'error',
    NOTIFICATION: 'notification'
};
exports.HTTP_STATUS = {
    OK: 200,
    CREATED: 201,
    BAD_REQUEST: 400,
    UNAUTHORIZED: 401,
    FORBIDDEN: 403,
    NOT_FOUND: 404,
    INTERNAL_SERVER_ERROR: 500,
    SERVICE_UNAVAILABLE: 503
};
//# sourceMappingURL=backend.js.map