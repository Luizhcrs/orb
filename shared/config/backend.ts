/**
 * Configuração compartilhada para comunicação com o backend
 */

import { BackendConfig } from '../types/api';

export const BACKEND_CONFIG: BackendConfig = {
  url: process.env.ORB_BACKEND_URL || 'http://localhost:8000',
  api_version: 'v1',
  timeout: 30000, // 30 segundos
  retry_attempts: 3,
  websocket_url: process.env.ORB_WEBSOCKET_URL || 'ws://localhost:8000/ws'
};

export const API_ENDPOINTS = {
  HEALTH: '/health',
  AGENT_MESSAGE: '/agent/message',
  AGENT_WEBSOCKET: '/ws',
  SYSTEM_SCREENSHOT: '/system/screenshot',
  SYSTEM_TOGGLE_ORB: '/system/toggle-orb',
  SYSTEM_HOT_CORNER: '/system/hot-corner',
  DOCS: '/docs',
  OPENAPI: '/openapi.json'
} as const;

export const WEBSOCKET_EVENTS = {
  CONNECT: 'connect',
  DISCONNECT: 'disconnect',
  MESSAGE: 'message',
  STATUS: 'status',
  ERROR: 'error',
  NOTIFICATION: 'notification'
} as const;

export const HTTP_STATUS = {
  OK: 200,
  CREATED: 201,
  BAD_REQUEST: 400,
  UNAUTHORIZED: 401,
  FORBIDDEN: 403,
  NOT_FOUND: 404,
  INTERNAL_SERVER_ERROR: 500,
  SERVICE_UNAVAILABLE: 503
} as const;
