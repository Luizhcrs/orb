"""
ORB API - Aplicação principal FastAPI
"""

from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import JSONResponse
import logging
import os

# Importa routers
from .routers import health, agent, websocket, system

# Importa configurações
from .config.api_config import APIConfig

# Configuração de logging estruturado
import structlog
import sys

# Configura logging básico para evitar erros de buffer
import logging
logging.basicConfig(
    level=logging.INFO,
    format='%(levelname)s - %(message)s',
    handlers=[
        logging.StreamHandler(sys.stdout)
    ]
)

# Configura structlog com proteção
structlog.configure(
    processors=[
        structlog.processors.TimeStamper(fmt="iso"),
        structlog.processors.JSONRenderer(),
    ],
    context_class=dict,
    logger_factory=structlog.stdlib.LoggerFactory(),
    wrapper_class=structlog.stdlib.BoundLogger,
    cache_logger_on_first_use=True,
)

logger = structlog.get_logger(__name__)

# Inicializa FastAPI
app = FastAPI(
    title=APIConfig.TITLE,
    description=APIConfig.DESCRIPTION,
    version=APIConfig.VERSION,
    contact={
        "name": APIConfig.CONTACT_NAME,
        "email": APIConfig.CONTACT_EMAIL,
    },
    license_info={
        "name": APIConfig.LICENSE_NAME,
        "url": APIConfig.LICENSE_URL,
    },
    root_path="/api/v1",  # Adiciona prefixo global para todos os endpoints
)

# Configuração de CORS
cors_origins = os.getenv("CORS_ORIGINS", "http://localhost:3000").split(",")

app.add_middleware(
    CORSMiddleware,
    allow_origins=cors_origins,
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
    expose_headers=["*"],
)

# Middleware de logging estruturado
from starlette.middleware.base import BaseHTTPMiddleware
import time

class LoggingMiddleware(BaseHTTPMiddleware):
    async def dispatch(self, request, call_next):
        start_time = time.time()
        response = await call_next(request)
        process_time = time.time() - start_time
        
        logger.info(
            "Request processed",
            method=request.method,
            url=str(request.url),
            status_code=response.status_code,
            process_time=process_time,
        )
        return response

app.add_middleware(LoggingMiddleware)

# Inclui routers
app.include_router(health.router)
app.include_router(agent.router)
app.include_router(websocket.router)
app.include_router(system.router)

@app.on_event("startup")
async def startup_event():
    """
    Evento de inicialização da aplicação
    """
    try:
        logger.info("Iniciando ORB Backend API...")
        logger.info("API pronta para receber conexões")
        
    except Exception as e:
        logger.error(f"Erro ao inicializar API: {str(e)}")
        raise e

@app.on_event("shutdown")
async def shutdown_event():
    """
    Evento de encerramento da aplicação
    """
    logger.info("Encerrando ORB Backend API...")

# Endpoint raiz
@app.get("/")
async def root():
    return {
        "message": "ORB Backend API",
        "version": APIConfig.VERSION,
        "docs": "/docs",
        "health": "/health"
    }

# Endpoints de documentação customizados
@app.get("/docs", include_in_schema=False)
async def custom_swagger_ui():
    """
    Documentação Swagger UI customizada
    """
    from fastapi.openapi.docs import get_swagger_ui_html
    return get_swagger_ui_html(
        openapi_url="/openapi.json",
        title=f"{APIConfig.TITLE} - Swagger UI",
        swagger_js_url=APIConfig.SWAGGER_JS_URL,
        swagger_css_url=APIConfig.SWAGGER_CSS_URL,
    )

@app.get("/redoc", include_in_schema=False)
async def custom_redoc():
    """
    Documentação ReDoc customizada
    """
    from fastapi.openapi.docs import get_redoc_html
    return get_redoc_html(
        openapi_url="/openapi.json",
        title=f"{APIConfig.TITLE} - ReDoc",
        redoc_js_url=APIConfig.REDOC_JS_URL,
    )

@app.get("/openapi.json", include_in_schema=False)
async def get_openapi_spec():
    """
    Especificação OpenAPI
    """
    return app.openapi()

# Handler de erro global
@app.exception_handler(Exception)
async def global_exception_handler(request, exc):
    """
    Handler global para exceções não tratadas
    """
    logger.error(f"Erro não tratado: {str(exc)}")
    return JSONResponse(
        status_code=500,
        content={
            "error": "Erro interno do servidor",
            "detail": str(exc) if logger.level <= logging.DEBUG else "Erro interno"
        }
    )
