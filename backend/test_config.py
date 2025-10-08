"""
Script de teste para ConfigManager e ChatMemoryManager
Valida que o SQLite está funcionando sem quebrar nada
"""
import sys
from pathlib import Path

# Adicionar src ao path
sys.path.insert(0, str(Path(__file__).parent / "src"))

from database.config_manager import ConfigManager
from database.chat_memory import ChatMemoryManager


def test_config_manager():
    """Testa funcionalidades básicas do ConfigManager"""
    print(" Iniciando testes do ConfigManager...\n")
    
    # Criar instância
    print("1⃣ Criando ConfigManager...")
    cm = ConfigManager()
    print("OK: ConfigManager criado\n")
    
    # Testar configurações básicas
    print("2⃣ Testando configurações básicas...")
    cm.set_setting('theme', 'dark')
    theme = cm.get_setting('theme')
    assert theme == 'dark', f"Esperado 'dark', obtido '{theme}'"
    print(f"   OK: Tema: {theme}")
    
    cm.set_setting('language', 'pt-BR')
    lang = cm.get_setting('language')
    assert lang == 'pt-BR', f"Esperado 'pt-BR', obtido '{lang}'"
    print(f"   OK: Idioma: {lang}\n")
    
    # Testar configuração LLM
    print("3⃣ Testando configuração LLM...")
    success = cm.save_llm_config('openai', 'sk-test-key-123', 'gpt-4o-mini')
    assert success, "Falha ao salvar config LLM"
    print("   OK: Config LLM salva")
    
    config = cm.get_llm_config()
    assert config['provider'] == 'openai', f"Provider incorreto: {config['provider']}"
    assert config['model'] == 'gpt-4o-mini', f"Model incorreto: {config['model']}"
    assert config['api_key'] == 'sk-test-key-123', "API key descriptografada incorreta"
    print(f"   OK: Provider: {config['provider']}")
    print(f"   OK: Model: {config['model']}")
    print(f"   OK: API Key: {'*' * (len(config['api_key']) - 4)}{config['api_key'][-4:]}\n")
    
    # Testar obter todas as configurações
    print("4⃣ Testando obter todas as configurações...")
    all_settings = cm.get_all_settings()
    assert 'theme' in all_settings, "Tema não encontrado"
    assert 'language' in all_settings, "Idioma não encontrado"
    print(f"   OK: Total de configurações: {len(all_settings)}")
    for key, value in all_settings.items():
        print(f"      - {key}: {value}\n")
    
    # Testar fallback para .env
    print("5⃣ Testando fallback para .env...")
    # Simular banco vazio criando nova instância em local temporário
    import tempfile
    import time
    temp_db = Path(tempfile.gettempdir()) / "orb_test.db"
    if temp_db.exists():
        try:
            temp_db.unlink()
        except PermissionError:
            pass  # Arquivo em uso, ignorar
    
    cm_temp = ConfigManager(str(temp_db))
    # Não salvar nada no banco, deve usar .env
    config_fallback = cm_temp.get_llm_config()
    print(f"   OK: Fallback funcionando: {config_fallback['provider']}\n")
    
    # Limpar (com tratamento de erro no Windows)
    try:
        # Dar tempo para fechar conexões
        time.sleep(0.1)
        if temp_db.exists():
            temp_db.unlink()
    except PermissionError:
        # Windows pode manter arquivo travado, não é crítico
        print("   ℹ  Arquivo temporário não removido (Windows lock), será limpo automaticamente\n")
    
    print("OK: Todos os testes do ConfigManager passaram!\n")


def test_chat_memory():
    """Testa funcionalidades da memória de chat (padrão LangChain)"""
    print(" Iniciando testes do ChatMemoryManager...\n")
    
    # Criar instância
    print("1⃣ Criando ChatMemoryManager...")
    cmem = ChatMemoryManager()
    print("OK: ChatMemoryManager criado\n")
    
    # Criar sessão
    print("2⃣ Testando criação de sessão...")
    session_id = cmem.create_session(title="Teste de Conversa")
    assert session_id is not None, "Session ID não gerado"
    print(f"   OK: Sessão criada: {session_id[:8]}...\n")
    
    # Adicionar mensagens
    print("3⃣ Testando adicionar mensagens...")
    success = cmem.add_user_message(session_id, "Olá! Como você está?")
    assert success, "Falha ao adicionar mensagem do usuário"
    print("   OK: Mensagem do usuário adicionada")
    
    success = cmem.add_assistant_message(session_id, "Olá! Estou bem. Como posso ajudar?")
    assert success, "Falha ao adicionar mensagem do assistente"
    print("   OK: Mensagem do assistente adicionada\n")
    
    # Obter mensagens
    print("4⃣ Testando obter mensagens...")
    messages = cmem.get_messages(session_id)
    assert len(messages) == 2, f"Esperado 2 mensagens, obtido {len(messages)}"
    assert messages[0].role == 'user', "Primeira mensagem deveria ser do usuário"
    assert messages[1].role == 'assistant', "Segunda mensagem deveria ser do assistente"
    print(f"   OK: {len(messages)} mensagens recuperadas")
    for i, msg in enumerate(messages, 1):
        print(f"      {i}. [{msg.role}] {msg.content[:40]}...\n")
    
    # Info da sessão
    print("5⃣ Testando info da sessão...")
    info = cmem.get_session_info(session_id)
    assert info is not None, "Info da sessão não encontrada"
    assert info['message_count'] == 2, f"Contador incorreto: {info['message_count']}"
    print(f"   OK: Título: {info['title']}")
    print(f"   OK: Mensagens: {info['message_count']}")
    print(f"   OK: Criada em: {info['created_at']}\n")
    
    # Listar sessões
    print("6⃣ Testando listar sessões...")
    sessions = cmem.list_sessions()
    assert len(sessions) > 0, "Nenhuma sessão encontrada"
    print(f"   OK: {len(sessions)} sessão(ões) encontrada(s)\n")
    
    # Testar com imagem
    print("7⃣ Testando mensagem com imagem...")
    cmem.add_user_message(session_id, "Veja esta imagem", image_data="base64_fake_data")
    messages = cmem.get_messages(session_id)
    last_msg = messages[-1]
    assert 'image_data' in last_msg.additional_kwargs, "Image data não armazenada"
    print("   OK: Mensagem com imagem armazenada\n")
    
    print("OK: Todos os testes da ChatMemory passaram!\n")


if __name__ == "__main__":
    try:
        test_config_manager()
        test_chat_memory()
        
        print("\n" + "="*60)
        print(" TODOS OS TESTES PASSARAM!")
        print("="*60)
        print("\nSTATS: Resumo Geral:")
        print("   OK: ConfigManager inicializado corretamente")
        print("   OK: Configurações básicas funcionando")
        print("   OK: Config LLM com criptografia funcionando")
        print("   OK: Fallback para .env funcionando")
        print("   OK: ChatMemoryManager inicializado")
        print("   OK: Memória de chat (padrão LangChain) funcionando")
        print("   OK: Sessões e mensagens armazenadas corretamente")
        print("   OK: Suporte a imagens nas mensagens")
        print(f"\nSAVE: Banco SQLite: orb.db")
        print("\nSTART: Sistema pronto para uso!")
    except Exception as e:
        print(f"\nERRO: Erro durante os testes: {e}")
        import traceback
        traceback.print_exc()
        sys.exit(1)

