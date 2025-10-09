# 🌐 ORB - Agente LLM Flutuante para Desktop

![Version](https://img.shields.io/badge/version-1.0.0-blue.svg)
![License](https://img.shields.io/badge/license-MIT-green.svg)
![Platform](https://img.shields.io/badge/platform-Windows-lightgrey.svg)

ORB é um assistente de IA flutuante para desktop que utiliza modelos de linguagem (LLM) para fornecer ajuda contextual enquanto você trabalha. Com uma interface minimalista em "liquid glass" desenvolvida em WPF, o ORB fica disponível através de hot corners e atalhos globais.

## ✨ Características

- 🎯 **Hot Corner**: Ative o ORB movendo o mouse para o canto superior esquerdo
- ⌨️ **Atalhos Globais**: `Ctrl+Shift+Space` para chat, `Ctrl+Shift+O` para configurações
- 🔒 **Privacidade**: Todas as conversas e configurações são armazenadas localmente em SQLite
- 🎨 **Interface Moderna**: Design "liquid glass" com glassmorphism
- 📸 **Capturas de Tela**: Analise imagens com visão computacional
- 💾 **Histórico Persistente**: Acesse e retome conversas anteriores
- 🔌 **Multi-LLM**: Suporte para OpenAI (GPT-4o, GPT-4o-mini) e Anthropic Claude

## 🚀 Início Rápido

### Para Usuários Finais

1. **Baixe o instalador** para Windows:
   - Windows: `OrbAgent-Setup-1.0.0.exe` ou `OrbAgent-Portable-1.0.0.exe`

2. **Instale e execute**

3. **Configure sua API key**:
   - Pressione `Ctrl+Shift+O` para abrir as configurações
   - Na seção "Agente", insira sua API key da OpenAI
   - Salve as configurações

4. **Comece a usar**:
   - Mova o mouse para o canto superior esquerdo OU
   - Pressione `Ctrl+Shift+Space` para abrir o chat

### Para Desenvolvedores

#### Requisitos

- .NET 9.0 SDK
- Python 3.11+
- Visual Studio 2022 ou VS Code (recomendado)

#### Setup Automatizado

**Windows:**
```batch
setup-dev.bat
```

#### Setup Manual

1. **Clone o repositório:**
```bash
git clone https://github.com/seu-usuario/orb.git
cd orb
```

2. **Configure o Backend:**
```bash
cd backend

# Criar ambiente virtual
python3 -m venv venv

# Ativar (Linux/macOS)
source venv/bin/activate
# Ativar (Windows)
venv\Scripts\activate

# Instalar dependências
pip install -r requirements.txt

# Criar .env
cp env.example .env

# Gerar chave de criptografia
python3 -c "from cryptography.fernet import Fernet; print('FERNET_KEY=' + Fernet.generate_key().decode())" >> .env

cd ..
```

3. **Configure o Frontend:**
```bash
cd frontend

# Restaurar dependências NuGet
dotnet restore

# Build inicial
dotnet build

cd ..
```

4. **Inicie o projeto:**
```bash
# Na raiz do projeto
npm run dev
# ou
dotnet run --project frontend
```

## 🏗️ Arquitetura

```
orb/
├── frontend/                 # Aplicação WPF (.NET)
│   ├── Windows/             # Janelas da aplicação (Chat, Config, Orb)
│   ├── Services/            # Serviços (Backend, Screenshot, System Tray)
│   ├── Models/              # Modelos de dados
│   ├── Helpers/             # Utilitários
│   └── OrbAgent.Frontend.csproj
│
├── backend/                 # API FastAPI
│   ├── src/
│   │   ├── api/            # Routers FastAPI
│   │   ├── agentes/        # Pipeline do agente LLM
│   │   ├── database/       # SQLite + Managers
│   │   └── config/         # Configurações
│   └── main.py
│
├── docs/                    # Documentação
├── scripts/                 # Scripts de automação
└── docker-compose.yml      # Docker para desenvolvimento
```

## 🛠️ Comandos Disponíveis

### Desenvolvimento

```bash
# Iniciar modo desenvolvimento (frontend + backend)
npm run dev

# Apenas backend
npm run dev:backend

# Apenas frontend
npm run dev:frontend

# Build do .NET
npm run build

# Limpar builds
npm run clean
```

### Produção

```bash
# Build completo
npm run build

# Criar instalador Windows
npm run pack:win

# Criar instalador macOS
npm run pack:mac

# Criar instalador Linux
npm run pack:linux

# Criar para todas as plataformas
npm run pack:all
```

### Docker

```bash
# Iniciar backend com Docker
docker-compose up

# Build fresh
docker-compose up --build

# Parar serviços
docker-compose down
```

## 🔧 Configuração

### Variáveis de Ambiente

**Backend (`backend/.env`):**
```env
# LLM Provider
LLM_PROVIDER=openai
OPENAI_API_KEY=sua-chave-aqui

# Servidor
HOST=0.0.0.0
PORT=8000
ENVIRONMENT=development

# Banco de Dados
DATABASE_PATH=orb.db
FERNET_KEY=sua-chave-fernet-aqui

# Opções
MAX_TOKENS=1000
TEMPERATURE=0.7
```

**Frontend (`frontend/.env`):**
```env
BACKEND_URL=http://localhost:8000
VITE_APP_TITLE=Orb Agent
```

### Configuração via Interface

Todas as configurações podem ser ajustadas através da interface gráfica (`Ctrl+Shift+O`):

- **Geral**: Tema, idioma, iniciar com Windows
- **Agente**: Provider LLM, API key, modelo
- **Histórico**: Gerenciar conversas anteriores

## 📦 Build e Release

### Pré-requisitos para Build

- **Windows**: .NET 9.0 SDK, Visual Studio 2022 ou VS Code

### Processo de Build

1. **Prepare o ambiente:**
```bash
# Instalar dependências de build
npm install

# Build do backend (se necessário)
cd backend && pip install -r requirements.txt && cd ..
```

2. **Crie o instalador:**
```bash
# Windows
npm run pack:win
```

3. **Artefatos gerados em** `frontend/release/`:
   - Windows: `.exe` (setup) e `.exe` (portable)

### Versionamento

Atualize a versão em `frontend/OrbAgent.Frontend.csproj`:
```xml
<PropertyGroup>
  <AssemblyVersion>1.1.0.0</AssemblyVersion>
  <FileVersion>1.1.0.0</FileVersion>
</PropertyGroup>
```

## 🧪 Testes

```bash
# Backend
cd backend
pytest

# Com cobertura
pytest --cov=src --cov-report=html

# Testes específicos
pytest backend/tests/test_agent_integration.py
```

## 🤝 Contribuindo

1. Fork o projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

## 📝 Licença

Este projeto está sob a licença MIT. Veja o arquivo [LICENSE](LICENSE) para mais detalhes.

## 🙏 Agradecimentos

- [WPF](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/) - Framework para aplicações desktop .NET
- [FastAPI](https://fastapi.tiangolo.com/) - Framework web Python moderno
- [OpenAI](https://openai.com/) - APIs de LLM

## 📞 Suporte

- 🐛 **Bugs**: Abra uma [issue](https://github.com/seu-usuario/orb/issues)
- 💬 **Discussões**: Use [Discussions](https://github.com/seu-usuario/orb/discussions)
- 📧 **Email**: seu-email@exemplo.com

## 🗺️ Roadmap

- [ ] Suporte para mais LLMs (Gemini, Llama)
- [ ] Plugins e extensões
- [ ] Themes customizáveis
- [ ] Sincronização em nuvem (opcional)
- [ ] Comandos de voz
- [ ] Backend em C# (.NET Core)

---

**Desenvolvido com ❤️ para tornar a IA mais acessível no desktop**
