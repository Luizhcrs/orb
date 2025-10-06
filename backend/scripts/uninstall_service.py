#!/usr/bin/env python3
"""
Script para remover o ORB Backend do Windows Services
"""

import sys
import os

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

def uninstall_service():
    """Remove o serviço Windows"""
    try:
        from services.windows_service import uninstall_service
        return uninstall_service()
    except Exception as e:
        print(f"❌ Erro ao remover serviço: {str(e)}")
        return False

def main():
    """Função principal"""
    print("🗑️  Removedor do ORB Backend Service")
    print("=" * 50)
    
    # Confirma remoção
    confirm = input("Tem certeza que deseja remover o serviço ORB Backend? (s/N): ")
    if confirm.lower() not in ['s', 'sim', 'y', 'yes']:
        print("❌ Operação cancelada")
        sys.exit(0)
    
    # Verifica requisitos
    if not check_requirements():
        sys.exit(1)
    
    # Remove serviço
    if uninstall_service():
        print("\n✅ Serviço removido com sucesso!")
        print("\nO serviço ORB Backend foi completamente removido do Windows.")
    else:
        print("\n❌ Falha na remoção do serviço")
        sys.exit(1)

if __name__ == "__main__":
    main()
