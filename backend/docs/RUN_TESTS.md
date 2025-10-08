# 🧪 Guia de Testes - ORB Backend

## 📋 Testes Disponíveis

### 1. **test_config.py** - Testes do Database
Testa `ConfigManager` e `ChatMemoryManager` isoladamente.

```bash
cd backend
python test_config.py
```

**O que testa:**
- ✅ ConfigManager (configurações gerais)
- ✅ ConfigManager (LLM com criptografia)
- ✅ ChatMemoryManager (sessões e mensagens)
- ✅ Suporte a imagens
- ✅ Fallback para `.env`

---

### 2. **test_agent_integration.py** - Teste de Integração
Testa `AgenteORB` integrado com SQLite.

```bash
cd backend
python test_agent_integration.py
```

**O que testa:**
- ✅ Agente criado com database
- ✅ Mensagens processadas e salvas no banco
- ✅ Mensagens com imagens
- ✅ Histórico de conversação persistido
- ✅ Sessões gerenciadas automaticamente
- ✅ Fallback para memória (sem database)
- ✅ Continuidade da conversação

---

## 🚀 Executar Todos os Testes

### Windows (PowerShell):
```powershell
cd backend

# Teste 1: Database
python test_config.py
echo ""
echo "======================================"
echo ""

# Teste 2: Integração
python test_agent_integration.py
```

### Linux/Mac:
```bash
cd backend

# Teste 1: Database
python test_config.py
echo ""
echo "======================================"
echo ""

# Teste 2: Integração
python test_agent_integration.py
```

---

## ✅ Resultado Esperado

### Test 1 (test_config.py):
```
🧪 Iniciando testes do ConfigManager...
✅ ConfigManager criado
✅ Tema: dark
✅ Idioma: pt-BR
...
🧪 Iniciando testes do ChatMemoryManager...
✅ ChatMemoryManager criado
✅ Sessão criada
...
🎉 TODOS OS TESTES PASSARAM!
```

### Test 2 (test_agent_integration.py):
```
🧪 Iniciando teste de integração AgenteORB + SQLite...
✅ Agente criado
✅ Database disponível
✅ Mensagem processada
✅ Mensagem com imagem processada
📊 Total de mensagens no banco: 6
...
🎉 TODOS OS TESTES PASSARAM!
```

---

## ⚠️ Pré-requisitos

### 1. **Adicionar ENCRYPTION_KEY ao `.env`**

Se ainda não criou o `.env`:

```bash
cd backend
cp env.example .env
```

Adicione a key gerada no primeiro teste:

```env
ENCRYPTION_KEY=IqOeg1EjIBjtjG7Am1Z8vjtUxXjfXqJjRlQ1FdYxH58=
```

### 2. **Instalar Dependências**

```bash
cd backend
pip install cryptography
```

### 3. **Configurar API Keys** (para teste de integração)

No arquivo `backend/.env`, adicione pelo menos uma API key:

```env
# OpenAI
OPENAI_API_KEY=sk-your-real-key-here

# OU Anthropic
ANTHROPIC_API_KEY=sk-ant-your-real-key-here
```

💡 **Dica:** Você pode usar o provider "demo" para testar sem API key real, mas as respostas serão simuladas.

---

## 🔍 Troubleshooting

### Erro: "ModuleNotFoundError: No module named 'database'"
```bash
# Certifique-se que está executando do diretório backend/
cd backend
python test_config.py
```

### Erro: "No module named 'cryptography'"
```bash
pip install cryptography
```

### Erro: "ENCRYPTION_KEY não definida"
1. Copie `env.example` para `.env`
2. Execute `test_config.py` uma vez - ele gera a key
3. Adicione a key gerada ao `.env`
4. Execute novamente

### Erro: "API key not configured"
1. Adicione pelo menos uma API key no `.env`:
   - `OPENAI_API_KEY` ou
   - `ANTHROPIC_API_KEY`

### Database não disponível no teste de integração
```bash
# Execute primeiro o test_config.py para criar o banco
python test_config.py

# Depois execute o test de integração
python test_agent_integration.py
```

---

## 📊 Arquivos Gerados

Após executar os testes:

```
orb/
├── orb.db                     ← Banco SQLite (criado automaticamente)
└── backend/
    ├── .env                   ← Configurações (você cria)
    ├── test_config.py        ← Teste 1
    └── test_agent_integration.py  ← Teste 2
```

---

## 🧹 Limpar Dados de Teste

Se quiser começar do zero:

```bash
# Remover banco de dados
rm orb.db

# Executar testes novamente
python test_config.py
python test_agent_integration.py
```

---

## 📝 Próximos Passos

Após todos os testes passarem:

1. ✅ Database está funcionando
2. ✅ Agente está integrado
3. 🚀 **Pronto para usar em produção!**

Para usar no backend real:
- O agente já está salvando automaticamente no banco
- Todas as conversas ficam persistidas
- Histórico pode ser acessado via `ChatMemoryManager`

---

**Criado por:** Luiz Henrique  
**Data:** 2025-01-07  
**Projeto:** ORB - AI Agent Assistant


