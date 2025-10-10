# 🌐 ORB - Agente IA Flutuante para Desktop

![Version](https://img.shields.io/badge/version-1.0.0-blue.svg)
![License](https://img.shields.io/badge/license-MIT-green.svg)
![Platform](https://img.shields.io/badge/platform-Windows-lightgrey.svg)

ORB é um assistente de IA flutuante para desktop Windows que utiliza modelos de linguagem (LLM) para fornecer ajuda contextual enquanto você trabalha. Com uma interface minimalista em "liquid glass" desenvolvida em WPF, o ORB fica disponível através de hot corners e atalhos globais.

## ✨ Características

- 🎯 **Hot Corner**: Ative o ORB movendo o mouse para o canto superior esquerdo
- ⌨️ **Atalhos Globais**: 
  - `Ctrl+Shift+Space`: Abrir chat
  - `Ctrl+Shift+O`: Abrir configurações
  - `Ctrl+Shift+S`: Capturar screenshot
- 🔒 **Privacidade**: Todas as conversas e configurações são armazenadas localmente em SQLite
- 🎨 **Interface Moderna**: Design "liquid glass" com glassmorphism
- 📸 **Capturas de Tela**: Analise imagens com visão computacional integrada
- 💾 **Histórico Persistente**: Acesse e retome conversas anteriores
- 🤖 **OpenAI GPT**: Suporte para modelos GPT-4o e GPT-4o-mini
- 🚀 **Zero Dependências**: Instalador inclui .NET Runtime e Python - nada mais necessário!

## 📥 Instalação

### Usuários Finais

1. **Baixe o instalador** da [página de releases](https://github.com/Luizhcrs/orb/releases/latest)
   - `OrbAgent-Setup-1.0.0.exe` (~114 MB)

2. **Execute o instalador** e siga as instruções

3. **Configure sua API key da OpenAI**:
   - Clique no ícone do Orb na bandeja do sistema
   - Selecione "Configurações" ou pressione `Ctrl+Shift+O`
   - Insira sua API key da OpenAI
   - Clique em "Salvar"

4. **Comece a usar**:
   - Mova o mouse para o canto superior esquerdo da tela
   - O ORB aparecerá - clique nele para abrir o chat

### Desenvolvedores

#### Requisitos

- .NET 9.0 SDK
- Python 3.11+
- Inno Setup 6 (para criar instaladores)

#### Setup

1. **Clone o repositório:**
```bash
git clone https://github.com/Luizhcrs/orb.git
cd orb
```

2. **Configure o Backend:**
```bash
cd backend
python -m venv venv
venv\Scripts\activate
pip install -r requirements.txt
```

3. **Configure o Frontend:**
```bash
cd frontend
dotnet restore
dotnet build
```

4. **Execute em modo desenvolvimento:**
```bash
# A partir da pasta frontend
dotnet run
```

O backend será iniciado automaticamente junto com o frontend.

## 🏗️ Arquitetura

```
orb/
├── frontend/                    # Aplicação WPF (.NET 9)
│   ├── Windows/                # Janelas (Chat, Config, Orb, About)
│   ├── Services/               # Serviços (Backend, Screenshot, System Tray)
│   ├── Models/                 # Modelos de dados
│   ├── Config/                 # Configurações
│   └── Assets/                 # Recursos (ícones, SVG)
│
├── backend/                    # API FastAPI
│   ├── src/
│   │   ├── api/               # Routers FastAPI
│   │   ├── agentes/           # Pipeline do agente LLM
│   │   ├── database/          # SQLite + Config Manager
│   │   └── config/            # Configurações
│   ├── backend_service.py     # Entry point do executável
│   └── build_standalone.py    # Script de build PyInstaller
│
├── docs/                       # Documentação
├── build-installer.bat         # Script master de build
└── installer.iss              # Configuração Inno Setup
```

## 🛠️ Build do Instalador

Para criar um instalador completo:

```bash
# Windows (executar como Administrador)
.\build-installer.bat
```

O instalador será criado em `release\OrbAgent-Setup-1.0.0.exe`

## 📂 Localização dos Dados

Após a instalação, os dados do usuário são armazenados em:

- **Banco de dados**: `%APPDATA%\OrbAgent\data\orb.db`
- **Logs**: `%APPDATA%\OrbAgent\logs\orb-backend.log`
- **Chave de criptografia**: `%APPDATA%\OrbAgent\data\.encryption_key`

## 🔧 Configuração

### Via Interface (Recomendado)

Pressione `Ctrl+Shift+O` e configure:

- **Geral**: Tema e idioma
- **Agente**: API key da OpenAI
- **Histórico**: Visualizar e retomar conversas anteriores

### Via Banco de Dados

O banco SQLite pode ser acessado diretamente em `%APPDATA%\OrbAgent\data\orb.db`

## 🤝 Contribuindo

Contribuições são bem-vindas! Por favor:

1. Fork o projeto
2. Crie uma branch para sua feature (`git checkout -b feature/MinhaFeature`)
3. Commit suas mudanças (`git commit -m 'Adiciona MinhaFeature'`)
4. Push para a branch (`git push origin feature/MinhaFeature`)
5. Abra um Pull Request

Veja [CONTRIBUTING.md](CONTRIBUTING.md) para mais detalhes.

## 📝 Licença

Este projeto está sob a licença MIT. Veja o arquivo [LICENSE](LICENSE) para mais detalhes.

## 🙏 Créditos

- **Desenvolvedor**: Luiz Cavalcanti
- **Framework Frontend**: [WPF](https://docs.microsoft.com/pt-br/dotnet/desktop/wpf/)
- **Framework Backend**: [FastAPI](https://fastapi.tiangolo.com/)
- **LLM**: [OpenAI](https://openai.com/)

## 📞 Suporte

- 🐛 **Bugs e Issues**: [GitHub Issues](https://github.com/Luizhcrs/orb/issues)
- 💬 **Discussões**: [GitHub Discussions](https://github.com/Luizhcrs/orb/discussions)

## 🗺️ Roadmap

- [ ] Suporte para Anthropic Claude
- [ ] Suporte para Google Gemini
- [ ] Plugins e extensões
- [ ] Themes customizáveis
- [ ] Sincronização em nuvem (opcional)
- [ ] Comandos de voz
- [ ] Suporte para Linux e macOS

---

**Desenvolvido com ❤️ para tornar a IA mais acessível no desktop**
