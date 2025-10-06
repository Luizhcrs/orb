#!/usr/bin/env python3
"""
Script para parar o ORB Backend Service
"""

import sys
import os

# Adiciona o diretório src ao path
sys.path.insert(0, os.path.join(os.path.dirname(__file__), '..', 'src'))

def main():
    """Função principal"""
    print("🛑 Parando ORB Backend Service...")
    
    try:
        from services.windows_service import stop_service
        if stop_service():
            print("✅ Serviço parado com sucesso!")
        else:
            print("❌ Falha ao parar serviço")
            sys.exit(1)
    except Exception as e:
        print(f"❌ Erro: {str(e)}")
        sys.exit(1)

if __name__ == "__main__":
    main()
