# ORB Backend

Backend Python para o assistente ORB - Assistente de IA flutuante.

## 🚀 Características

- **FastAPI**: Framework web moderno e rápido
- **WebSocket**: Comunicação em tempo real
- **LLM Integration**: Suporte para OpenAI e Anthropic
- **Tool Selector**: Sistema flexível para ferramentas futuras
- **Screenshot**: Captura de tela integrada
- **Hot Corner**: Detecção de canto quente (planejado)

## 📁 Estrutura do Projeto

```
backend/
├── src/
│   ├── agentes/
│   │   └── orb_agent/
│   │       ├── agente.py          # Pipeline principal do agente
│   │       ├── llms/
│   │       │   └── llm_provider.py # Provedores LLM (OpenAI, Anthropic)
│   │       ├── tools/
│   │       │   └── tool_selector.py # Seletor de ferramentas
│   │       ├── prompts/
│   │       │   └── system_prompt.yaml # Prompt do sistema
│   │       └── utils/
│   │           └── logging_config.py # Configuração de logging
│   ├── api/
│   │   ├── main.py               # Aplicação FastAPI principal
│   │   ├── config/
│   │   │   └── api_config.py     # Configurações da API
│   │   └── routers/
│   │       ├── agent.py          # Endpoints do agente
│   │       ├── health.py         # Health checks
│   │       ├── system.py         # Funcionalidades do sistema
│   │       └── websocket.py      # WebSocket em tempo real
│   ├── services/                 # Serviços do sistema
│   ├── models/                   # Modelos de dados
│   └── utils/                    # Utilitários
├── tests/                        # Testes
├── scripts/                      # Scripts auxiliares
├── docs/                         # Documentação
├── main.py                       # Ponto de entrada
├── requirements.txt              # Dependências Python
└── env.example                   # Exemplo de variáveis de ambiente
```

## 🛠️ Instalação

1. **Clone o repositório e navegue para o backend:**
   ```bash
   cd backend
   ```

2. **Crie um ambiente virtual:**
   ```bash
   python -m venv venv
   source venv/bin/activate  # Linux/Mac
   # ou
   venv\Scripts\activate     # Windows
   ```

3. **Instale as dependências:**
   ```bash
   pip install -r requirements.txt
   ```

4. **Configure as variáveis de ambiente:**
   ```bash
   cp env.example .env
   # Edite o arquivo .env com suas chaves de API
   ```

## ⚙️ Configuração

### Variáveis de Ambiente

Crie um arquivo `.env` baseado no `env.example`:

```env
# LLM Providers
OPENAI_API_KEY=your_openai_api_key_here
ANTHROPIC_API_KEY=your_anthropic_api_key_here

# Configurações do modelo
DEFAULT_MODEL=gpt-3.5-turbo
MAX_TOKENS=1000
TEMPERATURE=0.7

# Configurações do servidor
HOST=0.0.0.0
PORT=8000
DEBUG=true

# CORS settings
CORS_ORIGINS=http://localhost:3000,http://localhost:8080
```

### Chaves de API

Configure pelo menos uma das chaves de API:

- **OpenAI**: Obtenha em https://platform.openai.com/api-keys
- **Anthropic**: Obtenha em https://console.anthropic.com/

## 🚀 Execução

### Desenvolvimento

```bash
python main.py
```

O servidor estará disponível em:
- **API**: http://localhost:8000
- **Documentação**: http://localhost:8000/docs
- **WebSocket**: ws://localhost:8000/ws

### Serviço Windows (Recomendado para Produção)

```bash
# Instalar como serviço Windows
python scripts/install_service.py

# Iniciar serviço
python scripts/start_service.py

# Gerenciar serviço
python scripts/service_manager.py
```

**Vantagens do Serviço Windows:**
- ✅ Inicia automaticamente com o Windows
- ✅ Execução em background (sem interface)
- ✅ Restart automático em caso de falha
- ✅ Logs centralizados no Event Viewer
- ✅ Melhor integração com sistema Windows

📖 **Documentação completa**: [docs/WINDOWS_SERVICE.md](docs/WINDOWS_SERVICE.md)

### Produção (Alternativa)

```bash
uvicorn src.api.main:app --host 0.0.0.0 --port 8000
```

## 📡 API Endpoints

### Health Check
- `GET /health/` - Health check básico
- `GET /health/detailed` - Health check detalhado
- `GET /health/ready` - Readiness check
- `GET /health/live` - Liveness check

### Agente
- `POST /agent/message` - Enviar mensagem para o agente
- `GET /agent/status` - Status do agente
- `POST /agent/reset` - Resetar contexto de sessão
- `GET /agent/sessions` - Sessões ativas
- `DELETE /agent/sessions/{session_id}` - Remover sessão

### Sistema
- `POST /system/screenshot` - Capturar screenshot
- `GET /system/status` - Status do sistema
- `POST /system/hot-corner/configure` - Configurar hot corner
- `POST /system/orb/toggle` - Alternar orb
- `GET /system/orb/status` - Status do orb

### WebSocket
- `WS /ws` - Conexão WebSocket em tempo real

## 🔌 WebSocket

### Tipos de Mensagem

#### Enviar para o servidor:
```json
{
  "type": "message",
  "message": "Olá, como você pode me ajudar?",
  "session_id": "uuid-opcional",
  "image_data": "base64-opcional"
}
```

#### Receber do servidor:
```json
{
  "type": "response",
  "content": "Olá! Como posso ajudá-lo hoje?",
  "session_id": "uuid",
  "model_used": "gpt-3.5-turbo",
  "provider": "openai",
  "timestamp": "2025-01-05T20:00:00"
}
```

### Outros tipos de mensagem:
- `ping/pong` - Heartbeat
- `processing` - Indicador de processamento
- `error` - Erro
- `connection` - Confirmação de conexão

## 🧪 Testes

```bash
# Executar todos os testes
pytest

# Executar com coverage
pytest --cov=src

# Executar testes específicos
pytest tests/test_agent.py
```

## 🏗️ Arquitetura

### Pipeline do Agente

1. **Recebe mensagem** → Valida session_id
2. **Verifica contexto** → Recupera histórico da conversa
3. **Seleciona tools** → Decide se precisa de ferramentas específicas
4. **Gera resposta** → Usa LLM para gerar resposta
5. **Salva contexto** → Armazena conversa no histórico

### Componentes

- **AgenteORB**: Pipeline principal do agente
- **LLMProvider**: Gerenciador de provedores LLM
- **ToolSelector**: Seletor inteligente de ferramentas
- **ConnectionManager**: Gerenciador de conexões WebSocket

## 🔮 Roadmap

- [ ] Integração com banco de dados para persistência
- [ ] Implementação de hot corner
- [ ] Controle do orb flutuante
- [ ] Ferramentas específicas (RAG, cálculos, etc.)
- [ ] Autenticação e autorização
- [ ] Métricas e monitoramento
- [ ] Testes automatizados completos

## 🤝 Contribuição

1. Fork o projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

## 📄 Licença

Este projeto está sob a licença MIT. Veja o arquivo `LICENSE` para mais detalhes.

## 🆘 Suporte

Para suporte, abra uma issue no repositório ou entre em contato com a equipe.
