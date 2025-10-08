#!/usr/bin/env python3
"""
Script de teste completo para o ORB Backend
Testa todos os endpoints e funcionalidades
"""

import requests
import json
import time
import sys

def test_health_endpoint():
    """Testa o endpoint de health check"""
    print("Testando Health Check...")
    try:
        response = requests.get("http://localhost:8000/health/", timeout=5)
        if response.status_code == 200:
            data = response.json()
            print(f"[OK] Health Check: {data}")
            return True
        else:
            print(f"ERRO: Health Check falhou: {response.status_code}")
            return False
    except requests.exceptions.ConnectionError:
        print("ERRO: Servidor não está rodando em localhost:8000")
        return False
    except Exception as e:
        print(f"ERRO: Erro no Health Check: {str(e)}")
        return False

def test_agent_endpoint():
    """Testa o endpoint do agente"""
    print("\n Testando Agente...")
    try:
        payload = {
            "message": "Olá! Como você pode me ajudar?",
            "session_id": "test-session-123"
        }
        
        response = requests.post(
            "http://localhost:8000/agent/message",
            json=payload,
            timeout=30
        )
        
        if response.status_code == 200:
            data = response.json()
            print(f"OK: Agente respondeu: {data.get('content', 'Sem conteúdo')[:100]}...")
            print(f"   Modelo usado: {data.get('model_used', 'N/A')}")
            print(f"   Provider: {data.get('provider', 'N/A')}")
            return True
        else:
            print(f"ERRO: Agente falhou: {response.status_code}")
            print(f"   Resposta: {response.text}")
            return False
    except Exception as e:
        print(f"ERRO: Erro no teste do agente: {str(e)}")
        return False

def test_system_endpoints():
    """Testa endpoints do sistema"""
    print("\n  Testando Endpoints do Sistema...")
    
    screenshot_success = False
    toggle_orb_success = False
    
    # Teste screenshot
    try:
        response = requests.post("http://localhost:8000/system/screenshot", timeout=10)
        if response.status_code == 200:
            data = response.json()
            print(f"OK: Screenshot: {data.get('message', 'N/A')}")
            screenshot_success = True
        else:
            print(f"ERRO: Screenshot falhou: {response.status_code}")
    except Exception as e:
        print(f"ERRO: Erro no screenshot: {str(e)}")
    
    # Teste toggle orb
    try:
        response = requests.post("http://localhost:8000/system/toggle-orb", timeout=5)
        if response.status_code == 200:
            data = response.json()
            print(f"OK: Toggle Orb: {data.get('message', 'N/A')}")
            toggle_orb_success = True
        else:
            print(f"ERRO: Toggle Orb falhou: {response.status_code}")
    except Exception as e:
        print(f"ERRO: Erro no toggle orb: {str(e)}")
    
    return screenshot_success and toggle_orb_success

def test_api_docs():
    """Testa se a documentação da API está acessível"""
    print("\nDOCS: Testando Documentação da API...")
    try:
        response = requests.get("http://localhost:8000/docs", timeout=5)
        if response.status_code == 200:
            print("OK: Documentação Swagger acessível")
            return True
        else:
            print(f"ERRO: Documentação não acessível: {response.status_code}")
            return False
    except Exception as e:
        print(f"ERRO: Erro na documentação: {str(e)}")
        return False

def test_openapi_spec():
    """Testa o endpoint OpenAPI"""
    print("\nCONFIG: Testando OpenAPI Spec...")
    try:
        response = requests.get("http://localhost:8000/openapi.json", timeout=5)
        if response.status_code == 200:
            spec = response.json()
            print(f"OK: OpenAPI Spec OK - {len(spec.get('paths', {}))} endpoints")
            return True
        else:
            print(f"ERRO: OpenAPI Spec falhou: {response.status_code}")
            return False
    except Exception as e:
        print(f"ERRO: Erro no OpenAPI Spec: {str(e)}")
        return False

def main():
    """Função principal de teste"""
    print("TESTE COMPLETO DO ORB BACKEND")
    print("=" * 50)
    
    # Aguarda servidor iniciar
    print("WAIT: Aguardando servidor iniciar...")
    time.sleep(2)
    
    # Lista de testes
    tests = [
        ("Health Check", test_health_endpoint),
        ("API Documentation", test_api_docs),
        ("OpenAPI Spec", test_openapi_spec),
        ("Agent Endpoint", test_agent_endpoint),
        ("System Endpoints", test_system_endpoints),
    ]
    
    results = []
    
    # Executa todos os testes
    for test_name, test_func in tests:
        try:
            result = test_func()
            results.append((test_name, result))
        except Exception as e:
            print(f"ERRO: Erro no teste {test_name}: {str(e)}")
            results.append((test_name, False))
    
    # Relatório final
    print("\n" + "=" * 50)
    print("STATS: RELATÓRIO FINAL DOS TESTES")
    print("=" * 50)
    
    passed = 0
    total = len(results)
    
    for test_name, result in results:
        status = "OK: PASSOU" if result else "ERRO: FALHOU"
        print(f"{test_name:20} {status}")
        if result:
            passed += 1
    
    print(f"\nResultado: {passed}/{total} testes passaram")
    
    if passed == total:
        print(" TODOS OS TESTES PASSARAM! Backend está funcionando perfeitamente!")
        return 0
    else:
        print("AVISO:  Alguns testes falharam. Verifique os logs acima.")
        return 1

if __name__ == "__main__":
    sys.exit(main())
