#!/usr/bin/env python3
"""
Script de teste simples para o ORB Backend
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
            print(f"[FAIL] Health Check falhou: {response.status_code}")
            return False
    except requests.exceptions.ConnectionError:
        print("[FAIL] Servidor não está rodando em localhost:8000")
        return False
    except Exception as e:
        print(f"[FAIL] Erro no Health Check: {str(e)}")
        return False

def test_docs_endpoint():
    """Testa se a documentação está acessível"""
    print("Testando Documentação da API...")
    try:
        response = requests.get("http://localhost:8000/docs", timeout=5)
        if response.status_code == 200:
            print("[OK] Documentação Swagger acessível")
            return True
        else:
            print(f"[FAIL] Documentação falhou: {response.status_code}")
            return False
    except Exception as e:
        print(f"[FAIL] Erro na documentação: {str(e)}")
        return False

def test_openapi_spec():
    """Testa o endpoint OpenAPI"""
    print("Testando OpenAPI Spec...")
    try:
        response = requests.get("http://localhost:8000/openapi.json", timeout=5)
        if response.status_code == 200:
            data = response.json()
            endpoints = len(data.get('paths', {}))
            print(f"[OK] OpenAPI Spec OK - {endpoints} endpoints")
            return True
        else:
            print(f"[FAIL] OpenAPI Spec falhou: {response.status_code}")
            return False
    except Exception as e:
        print(f"[FAIL] Erro no OpenAPI Spec: {str(e)}")
        return False

def test_agent_endpoint():
    """Testa o endpoint do agente"""
    print("Testando Agente...")
    try:
        message = {"message": "Olá, como você está?"}
        response = requests.post(
            "http://localhost:8000/agent/message",
            json=message,
            timeout=30
        )
        if response.status_code == 200:
            data = response.json()
            response_text = data.get('response', '')
            model = data.get('model_used', 'N/A')
            provider = data.get('provider', 'N/A')
            print(f"[OK] Agente respondeu: {response_text[:50]}...")
            print(f"   Modelo usado: {model}")
            print(f"   Provider: {provider}")
            return True
        else:
            print(f"[FAIL] Agente falhou: {response.status_code}")
            return False
    except Exception as e:
        print(f"[FAIL] Erro no teste do agente: {str(e)}")
        return False

def test_system_endpoints():
    """Testa endpoints do sistema"""
    print("Testando Endpoints do Sistema...")
    
    all_passed = True

    # Teste screenshot
    try:
        response = requests.post("http://localhost:8000/system/screenshot", timeout=10)
        if response.status_code == 200:
            data = response.json()
            print(f"[OK] Screenshot: {data.get('message', 'N/A')}")
        else:
            print(f"[FAIL] Screenshot falhou: {response.status_code}")
            all_passed = False
    except Exception as e:
        print(f"[FAIL] Erro no screenshot: {str(e)}")
        all_passed = False
    
    # Teste toggle orb
    try:
        response = requests.post("http://localhost:8000/system/toggle-orb", timeout=5)
        if response.status_code == 200:
            data = response.json()
            print(f"[OK] Toggle Orb: {data.get('message', 'N/A')}")
        else:
            print(f"[FAIL] Toggle Orb falhou: {response.status_code}")
            all_passed = False
    except Exception as e:
        print(f"[FAIL] Erro no toggle orb: {str(e)}")
        all_passed = False
        
    return all_passed

def main():
    """Função principal de teste"""
    print("TESTE COMPLETO DO ORB BACKEND")
    print("=" * 50)
    
    # Aguarda servidor iniciar
    print("Aguardando servidor iniciar...")
    time.sleep(2)

    # Executa testes
    tests = [
        ("Health Check", test_health_endpoint),
        ("API Documentation", test_docs_endpoint),
        ("OpenAPI Spec", test_openapi_spec),
        ("Agent Endpoint", test_agent_endpoint),
        ("System Endpoints", test_system_endpoints)
    ]

    passed = 0
    total = len(tests)

    for test_name, test_func in tests:
        print(f"\n{test_name}...")
        if test_func():
            passed += 1
        print()

    # Relatório final
    print("=" * 50)
    print("RELATORIO FINAL DOS TESTES")
    print("=" * 50)
    
    for test_name, _ in tests:
        status = "[PASSOU]" if test_name in [tests[i][0] for i in range(passed)] else "[FALHOU]"
        print(f"{test_name:20} {status}")

    print(f"\nResultado: {passed}/{total} testes passaram")
    
    if passed == total:
        print("TODOS OS TESTES PASSARAM! Backend está funcionando perfeitamente!")
        return 0
    else:
        print("Alguns testes falharam. Verifique os logs acima.")
        return 1

if __name__ == "__main__":
    sys.exit(main())
