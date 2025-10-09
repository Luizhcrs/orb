# 🚀 Build Rápido do Instalador

## TL;DR

```batch
build-all.bat
```

**Resultado:** `frontend/bin/Release/net9.0-windows/Orb.exe` + Backend standalone

**Inclui:**
- ✅ Backend como serviço Windows (roda sempre em background)
- ✅ Frontend WPF como aplicativo desktop
- ✅ Instalador completo e standalone
- ✅ **NÃO requer Python ou .NET no PC do usuário!**

---

## 📦 O Que o Usuário Final Recebe

### Instalação
1. Baixa `OrbAgent-Setup-1.0.0.exe`
2. Executa (pede permissão de admin)
3. Instala em `C:\Program Files\Orb Agent\`
4. **Após instalação:** Backend inicia automaticamente como serviço
5. Cria atalhos no Desktop e Menu Iniciar

### Uso
1. Abre o ORB (atalho ou hot corner)
2. Pressiona `Ctrl+Shift+O`
3. Configura API key
4. **Pronto! Funciona!**

### Desinstalação
1. **Primeiro:** Serviço é removido automaticamente
2. **Depois:** Desinstale via Painel de Controle ou Menu Iniciar

---

## 🔧 Para Desenvolvedores

### Requisitos de Build
- Windows 10+
- .NET 9.0 SDK
- Python 3.11+

### Build Passo-a-Passo

```bash
# 1. Backend standalone
cd backend
pip install -r requirements.txt
pip install -r requirements-build.txt
python build_standalone.py

# 2. Frontend WPF
cd ../frontend
dotnet restore
dotnet build --configuration Release
dotnet publish --configuration Release --self-contained
```

### Ou use o script automatizado:

```batch
build-all.bat
```

---

## 📊 Arquitetura do Instalador

```
OrbAgent-Setup-1.0.0.exe
│
├─ Frontend (WPF App)
│  ├─ Aplicativo desktop .NET
│  ├─ Interface do chat (liquid glass)
│  └─ Gerenciamento de config
│
└─ Backend (Windows Service)
   ├─ orb-backend.exe (PyInstaller standalone)
   ├─ FastAPI server em http://127.0.0.1:8000
   ├─ Auto-start no boot do Windows
   └─ Roda em background (sem janela)
```

---

## ✅ Vantagens

**Para o Usuário:**
- ✅ Instalação em 1 clique
- ✅ Não precisa instalar Python, .NET Runtime, nada!
- ✅ Backend roda automaticamente sempre
- ✅ Desinstalação limpa

**Para o Desenvolvedor:**
- ✅ Build automatizado
- ✅ Distribuição profissional
- ✅ Atualizações fáceis
- ✅ Logs centralizados

---

## 🐛 Troubleshooting

### Build falha no backend

**Erro:** "PyInstaller not found"
```bash
cd backend
pip install pyinstaller
```

### Build falha no frontend

**Erro:** "dotnet command not found"
```bash
# Instale .NET 9.0 SDK
# Download: https://dotnet.microsoft.com/download
```

### Instalador muito grande

**Normal!** O instalador tem ~200 MB porque inclui:
- Python runtime completo
- FastAPI + Uvicorn
- OpenAI SDK
- .NET Runtime
- Todas as dependências

---

## 📞 Precisa de Ajuda?

Veja documentação completa em:
- `docs/BUILD_INSTALLER.md` - Detalhes técnicos
- `docs/DEPLOYMENT.md` - Opções de deploy

---

**Pronto para distribuir! 🎉**
