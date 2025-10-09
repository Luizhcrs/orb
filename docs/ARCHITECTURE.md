# 🏗️ Arquitetura do ORB Agent

## 📊 Visão Geral

```
┌──────────────────────────────────────────┐
│         Windows Desktop                   │
│                                           │
│  ┌────────────────────────────────────┐  │
│  │  Frontend (WPF .NET 9.0)           │  │
│  │                                    │  │
│  │  ├─ OrbWindow.xaml                │  │
│  │  ├─ ChatWindow.xaml               │  │
│  │  ├─ ConfigWindow.xaml             │  │
│  │  └─ AboutWindow.xaml              │  │
│  │                                    │  │
│  │  Services:                         │  │
│  │  ├─ BackendService (HTTP)         │  │
│  │  ├─ ScreenshotService             │  │
│  │  ├─ HotCornerService               │  │
│  │  └─ SystemTrayService             │  │
│  └────────────────────────────────────┘  │
│           ↕ HTTP (127.0.0.1:8000)        │
│  ┌────────────────────────────────────┐  │
│  │  Backend (Python FastAPI)          │  │
│  │                                    │  │
│  │  API Routes:                       │  │
│  │  ├─ /api/v1/agent/message         │  │
│  │  ├─ /api/v1/config                │  │
│  │  └─ /api/v1/history               │  │
│  │                                    │  │
│  │  Database:                         │  │
│  │  ├─ SQLite (orb.db)               │  │
│  │  ├─ ConfigManager                  │  │
│  │  └─ ChatMemoryManager             │  │
│  │                                    │  │
│  │  LLM Integration:                  │  │
│  │  └─ OpenAI GPT-4o/GPT-4o-mini    │  │
│  └────────────────────────────────────┘  │
└──────────────────────────────────────────┘
```

---

## 🎨 Frontend (WPF)

### Tecnologias
- **Framework:** WPF (Windows Presentation Foundation)
- **Linguagem:** C# 12
- **.NET Version:** 9.0
- **UI:** XAML + Code-Behind

### Componentes Principais

#### Windows (Janelas)
- **OrbWindow** - Orb flutuante animado
- **ChatWindow** - Interface de chat com liquid glass
- **ConfigWindow** - Tela de configurações
- **AboutWindow** - Modal "Sobre"
- **ConfirmationWindow** - Confirmação de saída

#### Services (Serviços)
- **BackendService** - Comunicação HTTP com backend
- **BackendProcessManager** - Gerencia processo do backend
- **ScreenshotService** - Captura de tela
- **HotCornerService** - Detecção de hot corner
- **SystemTrayService** - Ícone na bandeja do sistema
- **LoggingService** - Logs em arquivo

#### Models (Modelos)
- **AppConfig** - Configurações da aplicação
- **ChatMessage** - Mensagens do chat
- **AgentRequest/Response** - DTOs para backend

---

## 🐍 Backend (Python)

### Tecnologias
- **Framework:** FastAPI
- **Server:** Uvicorn
- **Database:** SQLite
- **ORM:** SQLAlchemy (parcial)
- **LLM:** OpenAI SDK

### Estrutura de Pastas

```
backend/
├── src/
│   ├── main.py                 # Entry point
│   ├── api/
│   │   ├── routers/
│   │   │   ├── agent.py       # /agent/message
│   │   │   ├── config.py      # /config
│   │   │   └── history.py     # /history
│   │   └── config/
│   │       └── api_config.py  # Configuração da API
│   ├── agentes/
│   │   └── orb_agent/
│   │       ├── agente.py      # Pipeline LLM
│   │       └── prompts.py     # System prompts
│   └── database/
│       ├── config_manager.py  # Gerenciar configs
│       └── chat_memory.py     # Histórico de chat
├── orb.db                      # SQLite database
└── build_standalone.py         # Build para .exe
```

### API Endpoints

#### 1. Health Check
```
GET /health
Response: { "status": "healthy", "service": "ORB Backend API", "version": "1.0.0" }
```

#### 2. Agent Message
```
POST /api/v1/agent/message
Body: {
  "message": "string",
  "session_id": "string",
  "image_data": "string (base64)"
}
Response: {
  "content": "string",
  "model_used": "string",
  "provider": "string"
}
```

#### 3. Config
```
GET /api/v1/config
PUT /api/v1/config
Body: {
  "general": { "theme": "dark", "language": "pt-BR", ... },
  "agent": { "llm_provider": "openai", "api_key": "***", ... }
}
```

#### 4. History
```
GET /api/v1/history/sessions
GET /api/v1/history/sessions/{session_id}/full
DELETE /api/v1/history/sessions/{session_id}
```

---

## 💾 Database Schema

### Tabela: `config`

| Coluna | Tipo | Descrição |
|--------|------|-----------|
| id | INTEGER | Primary key |
| category | TEXT | "general" ou "agent" |
| key | TEXT | Nome da configuração |
| value | TEXT | Valor (criptografado se sensível) |
| encrypted | INTEGER | 1 = criptografado, 0 = plaintext |

### Tabela: `chat_sessions`

| Coluna | Tipo | Descrição |
|--------|------|-----------|
| session_id | TEXT | UUID da sessão |
| title | TEXT | Título (primeira mensagem do usuário) |
| created_at | TEXT | ISO timestamp |
| updated_at | TEXT | ISO timestamp |

### Tabela: `chat_messages`

| Coluna | Tipo | Descrição |
|--------|------|-----------|
| id | INTEGER | Primary key |
| session_id | TEXT | Foreign key |
| role | TEXT | "user" ou "assistant" |
| content | TEXT | Conteúdo da mensagem |
| additional_kwargs | TEXT | JSON (image_data, etc.) |
| created_at | TEXT | ISO timestamp |

---

## 🔐 Segurança

### Criptografia
- **API Keys:** Criptografadas com Fernet (symmetric encryption)
- **Chave Fernet:** Armazenada em `.env` (não distribuída)
- **Banco de dados:** SQLite local (não exposto)

### Comunicação
- **Backend:** `127.0.0.1:8000` (loopback apenas)
- **CORS:** Desabilitado
- **Autenticação:** Não necessária (app local)

---

## 🚀 Deployment

### Como Serviço Windows

O backend roda como serviço Windows:
- **Nome:** `OrbBackendService`
- **Startup:** Automático
- **Execução:** Background (sem janela)
- **Logs:** Windows Event Viewer + arquivo de log

### Processo de Inicialização

1. **Windows inicia** → Serviço `OrbBackendService` inicia
2. **Serviço inicia** → `orb-backend.exe` executa
3. **Backend carrega** → FastAPI + Uvicorn em `127.0.0.1:8000`
4. **Usuário abre ORB** → Frontend WPF conecta ao backend
5. **Frontend solicita dados** → Backend consulta SQLite
6. **Usuário envia mensagem** → Backend chama OpenAI API
7. **Resposta retorna** → Frontend exibe no chat

---

## 📊 Performance

### Métricas

| Métrica | Valor |
|---------|-------|
| Startup do backend | ~2-3s |
| Uso de memória (idle) | ~50 MB |
| Uso de memória (ativo) | ~80-120 MB |
| Tempo de resposta (health) | < 10ms |
| Tempo de resposta (agent) | 1-3s (depende LLM) |

### Otimizações

- ✅ Cache de configurações em memória
- ✅ Pool de conexões HTTP (agentkeepalive)
- ✅ Lazy loading de módulos
- ✅ SQLite com WAL mode
- ✅ Logging assíncrono

---

## 🔧 Manutenção

### Logs

**Localização:**
- Desenvolvimento: `backend/logs/orb_backend.log`
- Produção: `C:\Program Files\Orb Agent\backend\logs\`

**Níveis:**
- `DEBUG` - Desenvolvimento
- `INFO` - Produção (padrão)
- `WARNING` - Alertas
- `ERROR` - Erros críticos

### Monitoramento

```bash
# Status do serviço
sc query OrbBackendService

# Parar serviço
sc stop OrbBackendService

# Iniciar serviço
sc start OrbBackendService

# Reiniciar serviço
sc stop OrbBackendService && sc start OrbBackendService
```

---

## 📞 Troubleshooting

### Backend não inicia

1. Verificar se porta 8000 está livre:
```bash
netstat -ano | findstr :8000
```

2. Executar manualmente para ver erros:
```bash
cd "C:\Program Files\Orb Agent\backend"
orb-backend.exe
```

3. Verificar logs:
```bash
type logs\orb_backend.log
```

### Erro "DLL not found"

Recompile com `--collect-all`:
```python
'--collect-all=uvicorn',
'--collect-all=fastapi',
```

---

**Backend Empacotado e Pronto! 🐍**

