# 🚀 Guia de Deployment do ORB

Este documento resume tudo que você precisa para fazer deploy do ORB Agent, seja para desenvolvimento ou produção.

## 📦 Para Desenvolvedores

### Setup Rápido (Automatizado)

**Linux/macOS:**
```bash
chmod +x setup-dev.sh
./setup-dev.sh
npm run dev
```

**Windows:**
```batch
setup-dev.bat
npm run dev
```

**O que o script faz:**
1. ✅ Verifica Node.js, Python e npm
2. ✅ Cria ambiente virtual Python
3. ✅ Instala todas as dependências
4. ✅ Gera chave de criptografia Fernet
5. ✅ Cria arquivos `.env`
6. ✅ Inicializa banco de dados SQLite
7. ✅ Build inicial do TypeScript

### Setup com Docker

Para apenas o backend:

```bash
docker-compose up
```

Isso inicia:
- Backend FastAPI na porta 8000
- Volumes persistentes para banco de dados
- Hot reload automático

**Frontend ainda precisa rodar localmente** (Electron não roda em Docker):
```bash
cd frontend
npm run dev
```

### Desenvolvimento Local

```bash
# Raiz do projeto (roda backend + frontend)
npm run dev

# Apenas backend
npm run dev:backend

# Apenas frontend
npm run dev:frontend
```

## 🏭 Para Produção

### Build Local

#### 1. Windows

```bash
cd frontend
npm install
npm run build
npm run pack:win
```

**Arquivos gerados em `frontend/release/`:**
- `OrbAgent-Setup-1.0.0.exe` - Instalador completo
- `OrbAgent-Portable-1.0.0.exe` - Versão portátil

#### 2. macOS

```bash
cd frontend
npm install
npm run build
npm run pack:mac
```

**Arquivos gerados:**
- `OrbAgent-1.0.0.dmg` - Instalador DMG
- `OrbAgent-1.0.0-mac.zip` - Versão ZIP

#### 3. Linux

```bash
cd frontend
npm install
npm run build
npm run pack:linux
```

**Arquivos gerados:**
- `OrbAgent-1.0.0.AppImage` - AppImage universal
- `orb-agent_1.0.0_amd64.deb` - Pacote Debian

### Build Multiplataforma

Para criar todos os instaladores de uma vez:

```bash
npm run pack:all
```

**Requer:**
- Windows: VS Build Tools 2019+
- macOS: Xcode Command Line Tools
- Linux: build-essential, rpm

### CI/CD Automatizado (GitHub Actions)

#### Configuração Inicial

1. **Fork o repositório** ou crie seu próprio repo

2. **Configure Secrets** (Settings → Secrets and variables → Actions):
   - Nenhum secret necessário por padrão
   - GITHUB_TOKEN é automático

3. **Push uma tag** para disparar build:
```bash
git tag -a v1.0.0 -m "Release v1.0.0"
git push origin v1.0.0
```

#### Workflows Disponíveis

**`.github/workflows/test.yml`** - Executado em cada push/PR:
- ✅ Lint Python (flake8, black)
- ✅ Type check (mypy)
- ✅ TypeScript build
- ✅ Testes backend (pytest)

**`.github/workflows/build.yml`** - Executado em tags `v*`:
- ✅ Testes completos
- ✅ Build para Windows, macOS e Linux
- ✅ Upload de artifacts
- ✅ Criação automática de release no GitHub

#### Processo de Release Automatizado

```bash
# 1. Atualizar versão
cd frontend
npm version patch  # ou minor/major

# 2. Commit
git add .
git commit -m "chore: bump version to v1.0.1"

# 3. Tag
git tag -a v1.0.1 -m "Release v1.0.1"

# 4. Push (dispara CI/CD automaticamente)
git push origin v1.0.1
```

**O GitHub Actions vai:**
1. Rodar todos os testes
2. Buildar para Windows, macOS e Linux
3. Criar uma release draft
4. Anexar todos os instaladores
5. Gerar release notes automaticamente

## 📋 Checklist de Deploy

### Pré-Deploy

- [ ] Todas as features testadas
- [ ] Testes passando (`pytest` + `npm run build`)
- [ ] Documentação atualizada
- [ ] Version bump (`npm version`)
- [ ] `.env.example` atualizado se necessário

### Deploy Manual

- [ ] Build executado com sucesso
- [ ] Instaladores testados (pelo menos 1 plataforma)
- [ ] Configuração inicial funciona (API key, etc)
- [ ] Hot corner funcionando
- [ ] Atalhos globais funcionando
- [ ] Histórico persistindo

### Deploy Automatizado (CI/CD)

- [ ] Tag criada com padrão `v*`
- [ ] GitHub Actions executou com sucesso
- [ ] Release criada no GitHub
- [ ] Todos os artifacts presentes
- [ ] Release notes publicadas

## 🔧 Configuração de Produção

### Variáveis de Ambiente

**Backend (obrigatórias):**
```env
FERNET_KEY=<gerado-automaticamente-pelo-setup>
```

**Backend (opcionais):**
```env
LLM_PROVIDER=openai
OPENAI_API_KEY=<sua-chave>  # Pode ser configurado via UI
HOST=0.0.0.0
PORT=8000
ENVIRONMENT=production
MAX_TOKENS=1000
TEMPERATURE=0.7
```

**Frontend:**
```env
BACKEND_URL=http://localhost:8000
VITE_APP_TITLE=Orb Agent
```

### Configuração via Interface

**Recomendado**: Configure via UI após instalação:

1. Instale o ORB
2. Abra (ele aparece automaticamente ou via hot corner)
3. Pressione `Ctrl+Shift+O`
4. Na seção "Agente":
   - Selecione o Provider: OpenAI
   - Insira sua API Key
   - Escolha o modelo: gpt-4o-mini
5. Salve

## 📦 Distribuição

### Para Usuários Finais

**Recomendações:**
1. Hospedar releases no GitHub Releases
2. Fornecer checksums (SHA256) para verificação
3. Assinar executáveis (code signing)
4. Manter changelog atualizado

**Estrutura de Release:**
```
Release v1.0.0
├── OrbAgent-Setup-1.0.0.exe          (Windows Installer)
├── OrbAgent-Portable-1.0.0.exe       (Windows Portable)
├── OrbAgent-1.0.0.dmg                (macOS DMG)
├── OrbAgent-1.0.0-mac.zip            (macOS ZIP)
├── OrbAgent-1.0.0.AppImage           (Linux AppImage)
└── orb-agent_1.0.0_amd64.deb         (Debian Package)
```

### Code Signing (Opcional)

**Windows:**
```bash
# Requer certificado de code signing
electron-builder --win --publish never
```

**macOS:**
```bash
# Requer Apple Developer ID
export CSC_LINK=<path-to-cert>
export CSC_KEY_PASSWORD=<password>
electron-builder --mac --publish never
```

## 🐛 Troubleshooting

### Build Falha

**Erro:** `Cannot find module '@electron/rebuild'`
```bash
cd frontend
npm install --save-dev @electron/rebuild
npm rebuild
```

**Erro:** Python não encontrado
```bash
# Windows
npm config set python C:\Python311\python.exe

# Linux/macOS
npm config set python /usr/bin/python3
```

### Backend não inicia

**Erro:** `FERNET_KEY not set`
```bash
cd backend
python3 -c "from cryptography.fernet import Fernet; print('FERNET_KEY=' + Fernet.generate_key().decode())" >> .env
```

**Erro:** `Module not found`
```bash
cd backend
source venv/bin/activate  # ou venv\Scripts\activate (Windows)
pip install -r requirements.txt
```

### Frontend não compila

**Erro:** TypeScript errors
```bash
cd frontend
rm -rf node_modules package-lock.json
npm install
npm run build
```

## 📊 Monitoramento

### Logs

**Backend:**
- Logs em: `backend/logs/` (se configurado)
- Stdout/stderr do processo

**Frontend:**
- Electron DevTools: `Ctrl+Shift+I`
- Logs do sistema: `%APPDATA%/orb-agent/logs/` (Windows)

### Performance

- Uso de memória: ~100-200 MB (backend + frontend)
- CPU: <5% idle, <30% durante processamento
- Disco: ~500 MB instalação + histórico variável

## 🔗 Links Úteis

- [GitHub Releases](https://github.com/seu-usuario/orb/releases)
- [Documentação Completa](../README.md)
- [Guia de Release](./RELEASE.md)
- [Contribuindo](../CONTRIBUTING.md)

---

**Deploy com confiança! 🚀**

