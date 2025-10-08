# 🚀 Setup do Banco de Dados SQLite

## 📋 Passo a Passo

### 1. Criar arquivo `.env`

Copie o `env.example` para `.env`:

```bash
cd backend
cp env.example .env
```

### 2. Adicionar ENCRYPTION_KEY

No teste, foi gerada esta key:
```
ENCRYPTION_KEY=IqOeg1EjIBjtjG7Am1Z8vjtUxXjfXqJjRlQ1FdYxH58=
```

**Abra o arquivo `backend/.env` e adicione:**

```env
# Database Encryption (IMPORTANTE: Guardar em local seguro!)
ENCRYPTION_KEY=IqOeg1EjIBjtjG7Am1Z8vjtUxXjfXqJjRlQ1FdYxH58=
```

### 3. Testar novamente

```bash
python test_config.py
```

Agora deve executar sem erros e sem gerar nova key!

## ✅ Resultado Esperado

```
✅ Todos os testes passaram!

📊 Resumo:
   - ConfigManager inicializado corretamente
   - Configurações básicas funcionando
   - Config LLM com criptografia funcionando
   - Fallback para .env funcionando
   - Banco SQLite criado em: orb.db

🎉 Sistema pronto para uso!
```

## 📁 Estrutura Criada

Após executar o teste, você terá:

```
orb/
├── orb.db                     ← Banco SQLite (criado automaticamente)
├── backend/
│   ├── .env                   ← Configurações (você cria)
│   ├── env.example           ← Template
│   ├── test_config.py        ← Script de teste
│   └── src/
│       └── database/
│           ├── config_manager.py
│           ├── schema.sql
│           └── README.md
```

## 🔐 Segurança

⚠️ **IMPORTANTE:**

1. **NUNCA** commite o arquivo `.env` no git
2. **Guarde** a `ENCRYPTION_KEY` em local seguro
3. **Se perder** a key, precisará reconfigurar todas as API keys
4. **Backup** do arquivo `orb.db` regularmente

## 🧪 Testes Adicionais

### Testar manualmente no Python:

```python
from src.database.config_manager import ConfigManager

# Criar instância
cm = ConfigManager()

# Salvar configuração
cm.set_setting('theme', 'dark')

# Obter configuração
theme = cm.get_setting('theme')
print(f"Tema: {theme}")  # Output: Tema: dark

# Salvar config LLM
cm.save_llm_config('openai', 'sk-your-real-key', 'gpt-4o-mini')

# Obter config LLM
config = cm.get_llm_config()
print(config)  # Output: {'provider': 'openai', 'api_key': 'sk-...', 'model': 'gpt-4o-mini'}
```

## 🔄 Próximos Passos

1. ✅ Executar `python test_config.py` - **CONCLUÍDO**
2. ✅ Adicionar `ENCRYPTION_KEY` ao `.env` - **PRÓXIMO PASSO**
3. 🔌 Integrar com API REST (endpoints)
4. 🖥️ Conectar frontend à API
5. 📚 Implementar histórico de conversas

---

**Criado por:** Luiz Henrique  
**Data:** 2025-01-07


