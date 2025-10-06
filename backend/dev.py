"""
Script de desenvolvimento com reload habilitado
Use este script apenas para desenvolvimento isolado
"""

import sys
import os
import uvicorn

# Configura logging limpo
from logging_config import setup_clean_logging
setup_clean_logging()

# Adiciona src ao path para importações
sys.path.insert(0, os.path.join(os.path.dirname(__file__), 'src'))

from api.main import app

def main():
    """Função principal para desenvolvimento"""
    print("ORB Backend - Modo Desenvolvimento (com reload)")
    print("=" * 50)
    print("⚠️  ATENÇÃO: Use este script apenas para desenvolvimento isolado")
    print("⚠️  Para integração com frontend, use: python main.py")
    print("=" * 50)
    
    uvicorn.run(
        "api.main:app",
        host="0.0.0.0",
        port=8000,
        reload=True,  # Reload habilitado apenas para desenvolvimento
        log_level="warning",
        reload_dirs=[os.path.join(os.path.dirname(__file__), 'src')]
    )

if __name__ == "__main__":
    main()
