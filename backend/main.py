"""
ORB Backend - Servidor principal
Aplicação FastAPI para o assistente ORB
Suporte para execução como serviço Windows ou modo desenvolvimento
"""

import sys
import os
import uvicorn

# Configura logging limpo antes de qualquer import
from src.config.logging_config import setup_clean_logging
setup_clean_logging()

# Adiciona src ao path para importações
sys.path.insert(0, os.path.join(os.path.dirname(__file__), 'src'))

from api.main import app

def is_windows_service():
    """Verifica se está rodando como serviço Windows"""
    try:
        import win32serviceutil
        # Força modo desenvolvimento para testes
        return False  # Sempre roda em modo desenvolvimento para testes
    except ImportError:
        return False

def main():
    """Função principal"""
    print("ORB Backend - Assistente de IA Flutuante")
    print("=" * 50)
    
    # Verifica se deve rodar como serviço Windows
    if is_windows_service():
        print("Executando como servico Windows...")
        from services.windows_service import ORBService
        import servicemanager
        servicemanager.Initialize()
        servicemanager.PrepareToHostSingle(ORBService)
        servicemanager.StartServiceCtrlDispatcher()
    else:
        # Modo desenvolvimento
        print("Executando em modo desenvolvimento...")
        print("Documentacao: http://localhost:8000/docs")
        print("API: http://localhost:8000")
        print("WebSocket: ws://localhost:8000/ws")
        print("=" * 50)
        
        uvicorn.run(
            "api.main:app",
            host="0.0.0.0",
            port=8000,
            reload=False,  # Desabilita reload para estabilidade
            log_level="warning"  # Reduz logs do uvicorn para evitar spam
        )

if __name__ == "__main__":
    main()
