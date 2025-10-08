"""
Configuração global de logging para o ORB Backend
Evita erros de buffer e reduz spam de logs
"""

import logging
import sys
import os

def setup_clean_logging():
    """
    Configura logging limpo para evitar erros de buffer
    """
    # Remove todos os handlers existentes
    root_logger = logging.getLogger()
    for handler in root_logger.handlers[:]:
        root_logger.removeHandler(handler)
    
    # Handler customizado que evita erros de buffer
    class SafeStreamHandler(logging.StreamHandler):
        def emit(self, record):
            try:
                super().emit(record)
            except (ValueError, OSError) as e:
                # Ignora erros de buffer detached
                if "underlying buffer has been detached" not in str(e):
                    pass
    
    # Configura handler simples
    handler = SafeStreamHandler(sys.stdout)
    handler.setLevel(logging.INFO)
    
    # Formato simples
    formatter = logging.Formatter('%(levelname)s - %(message)s')
    handler.setFormatter(formatter)
    
    # Aplica configuração
    root_logger.setLevel(logging.INFO)
    root_logger.addHandler(handler)
    
    # Configura loggers específicos para reduzir spam
    logging.getLogger("uvicorn").setLevel(logging.WARNING)
    logging.getLogger("uvicorn.error").setLevel(logging.WARNING)
    logging.getLogger("uvicorn.access").setLevel(logging.WARNING)
    logging.getLogger("fastapi").setLevel(logging.WARNING)
    
    # Configura structlog para não duplicar logs
    logging.getLogger("structlog").setLevel(logging.WARNING)

def get_clean_logger(name: str) -> logging.Logger:
    """
    Retorna um logger limpo sem spam
    """
    logger = logging.getLogger(name)
    logger.setLevel(logging.INFO)
    logger.propagate = False  # Evita duplicação
    
    # Adiciona handler apenas se não existir
    if not logger.handlers:
        handler = logging.StreamHandler(sys.stdout)
        handler.setLevel(logging.INFO)
        formatter = logging.Formatter('%(levelname)s - %(message)s')
        handler.setFormatter(formatter)
        logger.addHandler(handler)
    
    return logger
