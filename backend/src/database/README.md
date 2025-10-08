# 💾 Database - Gerenciamento de Configurações com SQLite

## 📋 Visão Geral

Sistema de configurações usando SQLite para armazenar preferências do usuário, configurações de LLM e (futuro) histórico de conversas.

### ✨ Características

- **🔒 Segurança**: API keys criptografadas com Fernet
- **🔄 Fallback**: Usa `.env` se banco não disponível
- **📦 Incremental**: Não quebra funcionalidades existentes
- **🚀 Simples**: API Python fácil de usar

## 🗄️ Estrutura do Banco

### Tabela: `settings`
Configurações gerais do usuário.

```sql
CREATE TABLE settings (
    key TEXT PRIMARY KEY,
    value TEXT NOT NULL,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

**Exemplos:**
- `theme`: 'dark' | 'light'
- `language`: 'pt-BR'
- `startup`: 'true' | 'false'
- `keep_history`: 'true' | 'false'

### Tabela: `llm_config`
Configurações dos provedores LLM.

```sql
CREATE TABLE llm_config (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    provider TEXT NOT NULL,        -- 'openai' | 'anthropic'
    api_key_encrypted TEXT,        -- Criptografada
    model TEXT NOT NULL,
    is_active BOOLEAN DEFAULT 1,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

## 🔧 Uso Básico

### Importar

```python
from database.config_manager import ConfigManager, get_config_manager

# Opção 1: Criar nova instância
cm = ConfigManager()

# Opção 2: Usar singleton
cm = get_config_manager()
```

### Configurações Gerais

```python
# Salvar configuração
cm.set_setting('theme', 'dark')

# Obter configuração
theme = cm.get_setting('theme', default='dark')

# Obter todas
all_settings = cm.get_all_settings()
```

### Configurações LLM

```python
# Salvar (API key será criptografada automaticamente)
cm.save_llm_config(
    provider='openai',
    api_key='sk-...',
    model='gpt-4o-mini'
)

# Obter (API key descriptografada, com fallback para .env)
config = cm.get_llm_config()
# Returns: {'provider': 'openai', 'api_key': 'sk-...', 'model': 'gpt-4o-mini'}
```

## 🔐 Criptografia

### Setup da Encryption Key

1. **Gerar nova key:**
```python
from cryptography.fernet import Fernet
key = Fernet.generate_key().decode()
print(f"ENCRYPTION_KEY={key}")
```

2. **Adicionar ao `.env`:**
```env
ENCRYPTION_KEY=your_generated_key_here
```

⚠️ **IMPORTANTE:** 
- Nunca commite a `ENCRYPTION_KEY` no git
- Guarde-a em local seguro (sem ela, API keys não podem ser recuperadas)
- Se perder, precisará reconfigurar todas as API keys

## 📁 Localização do Banco

```
orb/
├── orb.db              ← Banco SQLite (criado automaticamente)
├── backend/
│   └── src/
│       └── database/
│           ├── config_manager.py   ← Código principal
│           ├── schema.sql          ← Schema do banco
│           └── README.md          ← Este arquivo
```

## 🧪 Testes

### Executar teste básico:

```bash
cd backend
python test_config.py
```

### Teste manual:

```python
from database.config_manager import ConfigManager

cm = ConfigManager()

# Testar configurações
cm.set_setting('test', 'value')
print(cm.get_setting('test'))  # Output: value

# Testar LLM
cm.save_llm_config('openai', 'sk-test', 'gpt-4o-mini')
config = cm.get_llm_config()
print(config)  # Output: {'provider': 'openai', ...}
```

## 🔄 Migração Incremental

### Fase Atual (✅ Completa)
- ✅ Estrutura SQLite criada
- ✅ ConfigManager implementado
- ✅ Criptografia funcionando
- ✅ Fallback para `.env`
- ✅ Testes básicos

### Próximas Fases
1. **Integração com API:**
   - Criar endpoints para salvar/carregar configs
   - Conectar tela de configuração ao banco

2. **Histórico de Conversas:**
   - Tabelas para sessões e mensagens
   - Interface para visualizar histórico

## 🚨 Troubleshooting

### Erro: "No module named 'cryptography'"
```bash
pip install cryptography
```

### Erro: "ENCRYPTION_KEY não definida"
1. Gerar nova key (código acima)
2. Adicionar ao `.env`
3. Reiniciar aplicação

### Banco corrompido
```bash
# Remover e deixar recriar
rm orb.db
python test_config.py
```

## 📚 Referências

- [SQLite Python](https://docs.python.org/3/library/sqlite3.html)
- [Cryptography Fernet](https://cryptography.io/en/latest/fernet/)
- [Python dotenv](https://pypi.org/project/python-dotenv/)

---

**Desenvolvido por:** Luiz Henrique  
**Projeto:** ORB - AI Agent Assistant  
**Versão:** 1.0.0


