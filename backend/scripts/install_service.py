#!/usr/bin/env python3
"""
Script para instalar o ORB Backend como serviço Windows
"""

import sys
import os
import subprocess

# Adiciona o diretório src ao path
sys.path.insert(0, os.path.join(os.path.dirname(__file__), '..', 'src'))

def check_requirements():
    """Verifica se os requisitos estão instalados"""
    try:
        import win32serviceutil
        print("✅ pywin32 está instalado")
        return True
    except ImportError:
        print("❌ pywin32 não está instalado")
        print("Execute: pip install pywin32")
        return False

def install_service():
    """Instala o serviço Windows"""
    try:
        from services.windows_service import install_service
        return install_service()
    except Exception as e:
        print(f"❌ Erro ao instalar serviço: {str(e)}")
        return False

def main():
    """Função principal"""
    print("🔧 Instalador do ORB Backend Service")
    print("=" * 50)
    
    # Verifica requisitos
    if not check_requirements():
        sys.exit(1)
    
    # Instala serviço
    if install_service():
        print("\n🎉 Serviço instalado com sucesso!")
        print("\nPróximos passos:")
        print("1. Inicie o serviço: python scripts/start_service.py")
        print("2. Ou use: net start ORBBackend")
        print("3. Verifique logs no Event Viewer do Windows")
        print("4. Acesse: http://localhost:8000/docs")
    else:
        print("\n❌ Falha na instalação do serviço")
        sys.exit(1)

if __name__ == "__main__":
    main()
