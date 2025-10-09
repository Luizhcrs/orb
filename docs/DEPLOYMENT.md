# 🚀 Guia de Deployment do ORB

Este documento resume tudo que você precisa para fazer deploy do ORB Agent, seja para desenvolvimento ou produção.

## 📦 Para Desenvolvedores

### Setup Rápido (Automatizado)

**Windows:**
```batch
setup-dev.bat
npm run dev
```

**O que o script faz:**
1. ✅ Verifica .NET SDK e Python
2. ✅ Cria ambiente virtual Python
3. ✅ Instala todas as dependências
4. ✅ Gera chave de criptografia Fernet
5. ✅ Cria arquivos `.env`
6. ✅ Inicializa banco de dados SQLite
7. ✅ Build inicial do frontend WPF

### Setup com Docker

Para apenas o backend:

```bash
docker-compose up
```

Isso inicia:
- Backend FastAPI na porta 8000
- Volumes persistentes para banco de dados
- Hot reload automático

**Frontend WPF roda apenas em Windows localmente:**
```bash
cd frontend
dotnet run
```

### Desenvolvimento Local

```bash
# Raiz do projeto (roda backend + frontend)
npm run dev

# Apenas backend
npm run dev:backend

# Apenas frontend
dotnet run --project frontend
```

## 🏭 Para Produção

### Build Local - Windows

```bash
# 1. Build do backend standalone
cd backend
pip install -r requirements.txt
pip install -r requirements-build.txt
python build_standalone.py

# 2. Build do frontend WPF
cd ../frontend
dotnet restore
dotnet build --configuration Release
dotnet publish --configuration Release --self-contained --runtime win-x64 -p:PublishSingleFile=true
```

**Arquivos gerados:**
- `backend/dist/orb-backend.exe` - Backend standalone
- `frontend/bin/Release/net9.0-windows/publish/Orb.exe` - Frontend WPF

### Criar Instalador

Use **Inno Setup** ou **WiX Toolset**:

```bash
# Com script automatizado
build-all.bat

# Manual com Inno Setup
iscc installer.iss
```

---

## 🔧 Configuração de Ambiente

### Backend (.env)

```env
# LLM Provider
LLM_PROVIDER=openai
OPENAI_API_KEY=sua-chave-aqui

# Servidor
HOST=127.0.0.1
PORT=8000
ENVIRONMENT=production

# Banco de Dados
DATABASE_PATH=orb.db
FERNET_KEY=sua-chave-fernet-aqui

# Logging
LOG_LEVEL=INFO
```

### Frontend (Configurado via interface)

- Tema: Dark
- Idioma: pt-BR
- Iniciar com Windows: Sim
- Provider LLM: OpenAI
- API Key: (configurado via UI)

---

## 🐳 Docker (Backend Only)

### Build da Imagem

```bash
cd backend
docker build -t orb-backend:latest .
```

### Executar

```bash
docker run -d \
  --name orb-backend \
  -p 8000:8000 \
  -v orb-data:/app/data \
  -e OPENAI_API_KEY=sua-chave \
  orb-backend:latest
```

### Docker Compose

```bash
docker-compose up -d
```

**Nota:** Frontend WPF não roda em Docker, apenas localmente em Windows.

---

## 📊 Arquitetura de Deploy

```
Windows Desktop
├── Orb.exe (Frontend WPF)
│   ├── .NET 9.0 Runtime (self-contained)
│   ├── WPF UI Libraries
│   └── HttpClient → Backend
│
└── OrbBackendService (Windows Service)
    ├── orb-backend.exe
    ├── Python 3.11 Runtime
    ├── FastAPI + Uvicorn
    └── SQLite Database
```

---

## 🔒 Segurança

### API Keys
- ✅ Armazenadas criptografadas no SQLite (Fernet)
- ✅ Nunca em plaintext
- ✅ Configuradas via interface gráfica

### Comunicação
- ✅ Backend em `127.0.0.1` apenas (não exposto externamente)
- ✅ CORS desabilitado
- ✅ Sem autenticação externa (app local)

---

## 📈 Monitoramento

### Logs

**Backend:**
- `backend/logs/orb_backend.log`
- Nível: INFO (produção), DEBUG (dev)

**Frontend:**
- `frontend/bin/Debug/net9.0-windows/orb_debug.log`
- Apenas em modo debug

### Health Check

```bash
curl http://localhost:8000/health
```

Resposta esperada:
```json
{
  "status": "healthy",
  "service": "ORB Backend API",
  "version": "1.0.0"
}
```

---

## 🔄 Atualizações

### Processo de Update

1. Build nova versão
2. Distribuir instalador
3. Usuário executa novo instalador
4. Instalador atualiza arquivos
5. Reinicia serviço do backend

### Migração de Dados

- ✅ Banco SQLite é preservado automaticamente
- ✅ Configurações mantidas
- ✅ Histórico de conversas preservado

---

## 🐛 Troubleshooting

### Backend não inicia

```bash
# Verificar logs
tail -f backend/logs/orb_backend.log

# Testar manualmente
cd backend/dist
orb-backend.exe
```

### Frontend não conecta

1. Verificar se backend está rodando (`http://localhost:8000/health`)
2. Verificar firewall do Windows
3. Verificar logs: `frontend/orb_debug.log`

### Serviço Windows não inicia

```bash
# Reinstalar serviço
cd "C:\Program Files\Orb Agent\backend"
uninstall_service.bat
install_service.bat

# Verificar status
sc query OrbBackendService
```

---

## 📞 Suporte

- 🐛 **Bugs**: [GitHub Issues](https://github.com/seu-usuario/orb/issues)
- 💬 **Discussões**: [GitHub Discussions](https://github.com/seu-usuario/orb/discussions)
- 📧 **Email**: seu-email@exemplo.com

---

**Feito com ❤️ para tornar a IA mais acessível no desktop Windows**
