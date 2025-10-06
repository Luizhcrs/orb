"""
Configurações da API ORB
"""

import os
from typing import List

class APIConfig:
    """Configurações da API ORB"""
    
    # Informações básicas
    TITLE = "ORB Backend API"
    DESCRIPTION = "API backend para o assistente ORB - Assistente de IA flutuante"
    VERSION = "1.0.0"
    
    # Contato
    CONTACT_NAME = "ORB Team"
    CONTACT_EMAIL = "orb@example.com"
    
    # Licença
    LICENSE_NAME = "MIT"
    LICENSE_URL = "https://opensource.org/licenses/MIT"
    
    # URLs para documentação
    SWAGGER_JS_URL = "https://cdn.jsdelivr.net/npm/swagger-ui-dist@5/swagger-ui-bundle.js"
    SWAGGER_CSS_URL = "https://cdn.jsdelivr.net/npm/swagger-ui-dist@5/swagger-ui.css"
    REDOC_JS_URL = "https://cdn.jsdelivr.net/npm/redoc@2.0.0/bundles/redoc.standalone.js"
    
    # Configurações do servidor
    HOST = os.getenv("HOST", "0.0.0.0")
    PORT = int(os.getenv("PORT", 8000))
    DEBUG = os.getenv("DEBUG", "true").lower() == "true"
    
    # CORS
    CORS_ORIGINS = os.getenv("CORS_ORIGINS", "http://localhost:3000").split(",")
    
    # Configurações LLM
    DEFAULT_MODEL = os.getenv("DEFAULT_MODEL", "gpt-3.5-turbo")
    MAX_TOKENS = int(os.getenv("MAX_TOKENS", 1000))
    TEMPERATURE = float(os.getenv("TEMPERATURE", 0.7))
    
    # Configurações do ORB
    ORB_POSITION_X = os.getenv("ORB_POSITION_X", "right")
    ORB_POSITION_Y = os.getenv("ORB_POSITION_Y", "top")
    ORB_ALWAYS_ON_TOP = os.getenv("ORB_ALWAYS_ON_TOP", "true").lower() == "true"
    
    # Hot corner settings
    HOT_CORNER_X = int(os.getenv("HOT_CORNER_X", 0))
    HOT_CORNER_Y = int(os.getenv("HOT_CORNER_Y", 0))
    HOT_CORNER_WIDTH = int(os.getenv("HOT_CORNER_WIDTH", 50))
    HOT_CORNER_HEIGHT = int(os.getenv("HOT_CORNER_HEIGHT", 50))
    HIDE_DELAY = int(os.getenv("HIDE_DELAY", 2000))
    SHOW_DELAY = int(os.getenv("SHOW_DELAY", 500))
