#!/usr/bin/env python3
"""
Gerenciador de Serviço ORB Backend
Script para controlar o serviço Windows (iniciar, parar, status)
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

def show_menu():
    """Mostra menu de opções"""
    print("\n🔧 Gerenciador do ORB Backend Service")
    print("=" * 50)
    print("1. Ver status do serviço")
    print("2. Iniciar serviço")
    print("3. Parar serviço")
    print("4. Reiniciar serviço")
    print("5. Ver logs do Event Viewer")
    print("6. Testar API")
    print("0. Sair")
    print("=" * 50)

def get_status():
    """Obtém status do serviço"""
    try:
        from services.windows_service import get_service_status
        get_service_status()
    except Exception as e:
        print(f"❌ Erro ao obter status: {str(e)}")

def start_service():
    """Inicia o serviço"""
    try:
        from services.windows_service import start_service
        if start_service():
            print("✅ Serviço iniciado com sucesso!")
            print("🌐 API disponível em: http://localhost:8000")
            print("📚 Documentação: http://localhost:8000/docs")
        else:
            print("❌ Falha ao iniciar serviço")
    except Exception as e:
        print(f"❌ Erro ao iniciar serviço: {str(e)}")

def stop_service():
    """Para o serviço"""
    try:
        from services.windows_service import stop_service
        if stop_service():
            print("✅ Serviço parado com sucesso!")
        else:
            print("❌ Falha ao parar serviço")
    except Exception as e:
        print(f"❌ Erro ao parar serviço: {str(e)}")

def restart_service():
    """Reinicia o serviço"""
    print("🔄 Reiniciando serviço...")
    stop_service()
    import time
    time.sleep(2)
    start_service()

def show_event_log_instructions():
    """Mostra instruções para ver logs no Event Viewer"""
    print("\n📋 Como ver logs no Event Viewer:")
    print("1. Pressione Win + R")
    print("2. Digite: eventvwr.msc")
    print("3. Navegue para: Windows Logs > Application")
    print("4. Procure por eventos com fonte 'ORB Backend'")
    print("5. Ou use o filtro: Origem = 'ORB Backend'")

def test_api():
    """Testa se a API está respondendo"""
    import requests
    try:
        response = requests.get("http://localhost:8000/health/", timeout=5)
        if response.status_code == 200:
            print("✅ API está respondendo!")
            data = response.json()
            print(f"   Status: {data.get('status', 'unknown')}")
            print(f"   Versão: {data.get('version', 'unknown')}")
        else:
            print(f"❌ API retornou status: {response.status_code}")
    except requests.exceptions.ConnectionError:
        print("❌ API não está respondendo - serviço pode estar parado")
    except Exception as e:
        print(f"❌ Erro ao testar API: {str(e)}")

def main():
    """Função principal"""
    # Verifica requisitos
    if not check_requirements():
        sys.exit(1)
    
    while True:
        show_menu()
        
        try:
            choice = input("\nEscolha uma opção: ").strip()
            
            if choice == "0":
                print("👋 Até logo!")
                break
            elif choice == "1":
                get_status()
            elif choice == "2":
                start_service()
            elif choice == "3":
                stop_service()
            elif choice == "4":
                restart_service()
            elif choice == "5":
                show_event_log_instructions()
            elif choice == "6":
                test_api()
            else:
                print("❌ Opção inválida")
            
            input("\nPressione Enter para continuar...")
            
        except KeyboardInterrupt:
            print("\n👋 Até logo!")
            break
        except Exception as e:
            print(f"❌ Erro: {str(e)}")
            input("\nPressione Enter para continuar...")

if __name__ == "__main__":
    main()
