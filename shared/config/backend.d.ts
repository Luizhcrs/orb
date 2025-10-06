/**
 * Configuração compartilhada para comunicação com o backend
 */
import { BackendConfig } from '../types/api';
export declare const BACKEND_CONFIG: BackendConfig;
export declare const API_ENDPOINTS: {
    readonly HEALTH: "/health";
    readonly AGENT_MESSAGE: "/agent/message";
    readonly AGENT_WEBSOCKET: "/ws";
    readonly SYSTEM_SCREENSHOT: "/system/screenshot";
    readonly SYSTEM_TOGGLE_ORB: "/system/toggle-orb";
    readonly SYSTEM_HOT_CORNER: "/system/hot-corner";
    readonly DOCS: "/docs";
    readonly OPENAPI: "/openapi.json";
};
export declare const WEBSOCKET_EVENTS: {
    readonly CONNECT: "connect";
    readonly DISCONNECT: "disconnect";
    readonly MESSAGE: "message";
    readonly STATUS: "status";
    readonly ERROR: "error";
    readonly NOTIFICATION: "notification";
};
export declare const HTTP_STATUS: {
    readonly OK: 200;
    readonly CREATED: 201;
    readonly BAD_REQUEST: 400;
    readonly UNAUTHORIZED: 401;
    readonly FORBIDDEN: 403;
    readonly NOT_FOUND: 404;
    readonly INTERNAL_SERVER_ERROR: 500;
    readonly SERVICE_UNAVAILABLE: 503;
};
//# sourceMappingURL=backend.d.ts.map