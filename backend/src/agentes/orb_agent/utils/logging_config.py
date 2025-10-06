"""
Configuração de logging com suporte a UTF-8 para o Agente ORB
"""

import logging
import sys
import os
from typing import Optional

def get_utf8_logger(name: str, level: int = logging.INFO) -> logging.Logger:
    """
    Cria um logger com suporte a UTF-8 e proteção contra erros de buffer
    
    Args:
        name: Nome do logger
        level: Nível de logging
        
    Returns:
        Logger configurado
    """
    logger = logging.getLogger(name)
    
    # Evita duplicar handlers
    if logger.handlers:
        return logger
    
    logger.setLevel(level)
    
    # Handler customizado que evita erros de buffer
    class SafeStreamHandler(logging.StreamHandler):
        def emit(self, record):
            try:
                super().emit(record)
            except (ValueError, OSError) as e:
                # Ignora erros de buffer detached e outros erros de I/O
                if "underlying buffer has been detached" not in str(e):
                    # Log apenas erros não relacionados ao buffer
                    pass
    
    # Handler para console com proteção contra erros
    console_handler = SafeStreamHandler(sys.stdout)
    console_handler.setLevel(level)
    
    # Formato de log simplificado
    formatter = logging.Formatter(
        '%(levelname)s - %(message)s'
    )
    console_handler.setFormatter(formatter)
    
    logger.addHandler(console_handler)
    
    # Configura para não propagar para o root logger (evita duplicação)
    logger.propagate = False
    
    return logger

def setup_file_logging(logger_name: str, log_file: str, level: int = logging.INFO) -> logging.Logger:
    """
    Configura logging para arquivo com UTF-8
    
    Args:
        logger_name: Nome do logger
        log_file: Caminho do arquivo de log
        level: Nível de logging
        
    Returns:
        Logger configurado
    """
    logger = logging.getLogger(logger_name)
    
    # Evita duplicar handlers
    if any(isinstance(h, logging.FileHandler) for h in logger.handlers):
        return logger
    
    # Cria diretório se não existir
    log_dir = os.path.dirname(log_file)
    if log_dir and not os.path.exists(log_dir):
        os.makedirs(log_dir, exist_ok=True)
    
    # Handler para arquivo com UTF-8
    file_handler = logging.FileHandler(log_file, encoding='utf-8')
    file_handler.setLevel(level)
    
    # Formato de log
    formatter = logging.Formatter(
        '%(asctime)s - %(name)s - %(levelname)s - %(message)s',
        datefmt='%Y-%m-%d %H:%M:%S'
    )
    file_handler.setFormatter(formatter)
    
    logger.addHandler(file_handler)
    
    return logger

def setup_rotating_file_logging(
    logger_name: str, 
    log_file: str, 
    max_bytes: int = 10 * 1024 * 1024,  # 10MB
    backup_count: int = 5,
    level: int = logging.INFO
) -> logging.Logger:
    """
    Configura logging rotativo para arquivo com UTF-8
    
    Args:
        logger_name: Nome do logger
        log_file: Caminho do arquivo de log
        max_bytes: Tamanho máximo do arquivo antes de rotacionar
        backup_count: Número de arquivos de backup
        level: Nível de logging
        
    Returns:
        Logger configurado
    """
    logger = logging.getLogger(logger_name)
    
    # Evita duplicar handlers
    if any(isinstance(h, logging.handlers.RotatingFileHandler) for h in logger.handlers):
        return logger
    
    # Cria diretório se não existir
    log_dir = os.path.dirname(log_file)
    if log_dir and not os.path.exists(log_dir):
        os.makedirs(log_dir, exist_ok=True)
    
    # Handler rotativo para arquivo com UTF-8
    from logging.handlers import RotatingFileHandler
    file_handler = RotatingFileHandler(
        log_file, 
        maxBytes=max_bytes, 
        backupCount=backup_count,
        encoding='utf-8'
    )
    file_handler.setLevel(level)
    
    # Formato de log
    formatter = logging.Formatter(
        '%(asctime)s - %(name)s - %(levelname)s - %(message)s',
        datefmt='%Y-%m-%d %H:%M:%S'
    )
    file_handler.setFormatter(formatter)
    
    logger.addHandler(file_handler)
    
    return logger

# Configuração global de logging
def setup_global_logging(
    level: int = logging.INFO,
    log_file: Optional[str] = None,
    enable_console: bool = True
) -> None:
    """
    Configura logging global para a aplicação
    
    Args:
        level: Nível de logging
        log_file: Arquivo de log (opcional)
        enable_console: Habilitar log no console
    """
    # Configura root logger
    root_logger = logging.getLogger()
    root_logger.setLevel(level)
    
    # Remove handlers existentes
    for handler in root_logger.handlers[:]:
        root_logger.removeHandler(handler)
    
    # Handler para console
    if enable_console:
        console_handler = logging.StreamHandler(sys.stdout)
        console_handler.setLevel(level)
        
        formatter = logging.Formatter(
            '%(asctime)s - %(name)s - %(levelname)s - %(message)s',
            datefmt='%Y-%m-%d %H:%M:%S'
        )
        console_handler.setFormatter(formatter)
        
        # Configura encoding para UTF-8
        if hasattr(console_handler.stream, 'reconfigure'):
            try:
                console_handler.stream.reconfigure(encoding='utf-8')
            except Exception:
                pass
        
        root_logger.addHandler(console_handler)
    
    # Handler para arquivo se especificado
    if log_file:
        setup_file_logging('root', log_file, level)
