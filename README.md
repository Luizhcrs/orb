# 🌟 ORB - Desktop AI Assistant

> **Desktop AI Assistant** com interface Electron e backend Python/FastAPI  
> **Desenvolvido por [Luiz Henrique](https://github.com/luizhcrs)** - Projeto open source para a comunidade

## 📁 Estrutura do Projeto

```
orb/
├── frontend/                 # Node.js/Electron (Interface Desktop)
│   ├── src/
│   │   ├── main.ts          # Processo principal do Electron
│   │   ├── managers/        # Gerenciadores (Window, Mouse, Shortcuts)
│   │   ├── services/        # Serviços (Screenshot, LLM)
│   │   ├── components/      # Componentes da UI
│   │   └── types/           # Tipos TypeScript
│   ├── package.json
│   └── tsconfig.json
├── backend/                  # Python/FastAPI (Backend API)
│   ├── src/
│   │   ├── api/            # Rotas da API
│   │   ├── agentes/        # Agente ORB com LLM
│   │   ├── services/       # Serviços do sistema
│   │   └── utils/          # Utilitários
│   ├── requirements.txt
│   └── Dockerfile
├── shared/                   # Tipos e configurações compartilhadas
│   ├── types/              # Tipos TypeScript compartilhados
│   └── config/             # Configurações comuns
├── scripts/                 # Scripts de build e deploy
├── docker-compose.yml       # Orquestração dos serviços
└── package.json            # Gerenciamento do monorepo
```

## 🚀 Início Rápido

### Pré-requisitos

- **Node.js** >= 18.0.0
- **Python** >= 3.11.0
- **Docker** (opcional, para desenvolvimento com containers)

### Instalação

```bash
# 1. Clonar o repositório
git clone https://github.com/luizrocha/orb.git
cd orb

# 2. Instalar todas as dependências
npm run install:all

# 3. Configurar variáveis de ambiente
cp backend/env.example backend/.env
# Editar backend/.env com suas chaves de API
```

### Desenvolvimento

```bash
# Desenvolvimento completo (frontend + backend)
npm run dev

# Ou executar separadamente:
npm run dev:frontend    # Apenas frontend
npm run dev:backend     # Apenas backend
```

### Docker (Desenvolvimento)

```bash
# Subir todos os serviços
npm run docker:up

# Parar serviços
npm run docker:down

# Rebuild containers
npm run docker:build
```

## 🧪 Testes

```bash
# Todos os testes
npm run test

# Testes específicos
npm run test:frontend
npm run test:backend
```

## 🏗️ Build e Deploy

```bash
# Build completo
npm run build

# Build específico
npm run build:frontend
npm run build:backend
```

## 🛠️ Serviços Windows

```bash
# Instalar como serviço Windows
npm run service:install

# Gerenciar serviço
npm run service:start
npm run service:stop
npm run service:uninstall
```

## 📡 API Endpoints

### Health Check
- `GET /health` - Status do serviço

### Agente AI
- `POST /agent/message` - Enviar mensagem para o agente
- `WS /ws` - WebSocket para comunicação em tempo real

### Sistema
- `POST /system/screenshot` - Capturar tela
- `POST /system/toggle-orb` - Alternar visibilidade do orb
- `POST /system/hot-corner` - Configurar hot corner

### Documentação
- `GET /docs` - Swagger UI
- `GET /openapi.json` - Especificação OpenAPI

## 🔧 Configuração

### Variáveis de Ambiente (Backend)

```env
# LLM Configuration
OPENAI_API_KEY=your_openai_key
ANTHROPIC_API_KEY=your_anthropic_key
DEFAULT_MODEL=gpt-4o-mini

# API Configuration
ENVIRONMENT=development
API_HOST=0.0.0.0
API_PORT=8000

# Logging
LOG_LEVEL=INFO
```

### Configuração do Frontend

O frontend se conecta automaticamente ao backend em `http://localhost:8000` por padrão. Para alterar:

```typescript
// shared/config/backend.ts
export const BACKEND_CONFIG = {
  url: process.env.ORB_BACKEND_URL || 'http://localhost:8000',
  // ...
};
```

## 🏗️ Arquitetura

### Frontend (Electron)
- **WindowManager**: Gerencia janelas do orb e chat
- **MouseDetector**: Detecta hot corner para mostrar/ocultar orb
- **ShortcutManager**: Gerencia atalhos de teclado globais
- **ScreenshotService**: Captura tela para análise do LLM
- **LLMManager**: Interface com APIs de LLM (OpenAI/Anthropic)

### Backend (Python/FastAPI)
- **AgenteORB**: Agente principal com pipeline de processamento
- **LLMProvider**: Abstração para diferentes provedores de LLM
- **ToolSelector**: Seleciona ferramentas baseado no contexto
- **SystemAPI**: Endpoints para interação com o sistema

### Comunicação
- **HTTP/REST**: Comunicação principal frontend ↔ backend
- **WebSocket**: Comunicação em tempo real
- **Tipos Compartilhados**: TypeScript types compartilhados via `shared/`

## 🔄 Fluxo de Desenvolvimento

1. **Desenvolvimento**: `npm run dev`
2. **Testes**: `npm run test`
3. **Build**: `npm run build`
4. **Deploy**: `npm run docker:up` ou instalar como serviço Windows

## 📚 Documentação

- [API Documentation](backend/docs/API_DOCUMENTATION.md)
- [Windows Service Guide](backend/docs/WINDOWS_SERVICE.md)
- [Development Guide](docs/DEVELOPMENT.md)

## 🤝 Contribuição

Este é um projeto open source desenvolvido por **Luiz Henrique**. Contribuições da comunidade são muito bem-vindas!

### Como Contribuir

1. **Fork** o projeto
2. **Crie** uma branch para sua feature (`git checkout -b feature/nova-funcionalidade`)
3. **Commit** suas mudanças (`git commit -m 'feat: adiciona nova funcionalidade'`)
4. **Push** para a branch (`git push origin feature/nova-funcionalidade`)
5. **Abra** um Pull Request

### Diretrizes de Contribuição

- 🐛 **Bugs**: Use a label `bug` para reportar problemas
- ✨ **Features**: Use a label `enhancement` para novas funcionalidades
- 📚 **Documentação**: Melhorias na documentação são sempre bem-vindas
- 🧪 **Testes**: Ajude a melhorar a cobertura de testes
- 💡 **Ideias**: Sugestões são apreciadas via Issues

### Contato

- 👨‍💻 **Desenvolvedor**: [Luiz Henrique](https://github.com/luizhcrs)
- 📧 **Issues**: [GitHub Issues](https://github.com/luizhcrs/orb/issues)
- 💬 **Discussões**: [GitHub Discussions](https://github.com/luizhcrs/orb/discussions)

## 📄 Licença

Este projeto está sob a licença MIT. Veja o arquivo [LICENSE](LICENSE) para detalhes.

## 🙏 Agradecimentos

- [Electron](https://electronjs.org/) - Framework para aplicações desktop
- [FastAPI](https://fastapi.tiangolo.com/) - Framework web Python
- [OpenAI](https://openai.com/) - API de linguagem
- [Anthropic](https://anthropic.com/) - Claude API
- **Comunidade open source** - Por todas as bibliotecas e ferramentas incríveis

---

**Desenvolvido com ❤️ por [Luiz Henrique](https://github.com/luizhcrs)**
