#!/usr/bin/env python3
"""
Script para iniciar o ORB Backend Service
"""

import sys
import os

# Adiciona o diretório src ao path
sys.path.insert(0, os.path.join(os.path.dirname(__file__), '..', 'src'))

def main():
    """Função principal"""
    print("🚀 Iniciando ORB Backend Service...")
    
    try:
        from services.windows_service import start_service
        if start_service():
            print("✅ Serviço iniciado com sucesso!")
            print("🌐 API disponível em: http://localhost:8000")
            print("📚 Documentação: http://localhost:8000/docs")
            print("⚡ WebSocket: ws://localhost:8000/ws")
        else:
            print("❌ Falha ao iniciar serviço")
            sys.exit(1)
    except Exception as e:
        print(f"❌ Erro: {str(e)}")
        sys.exit(1)

if __name__ == "__main__":
    main()
