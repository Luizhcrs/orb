# 🐍 Backend Packaging - Criar Executável Standalone

Este documento explica como criar um executável standalone do backend Python, que pode rodar **sem Python instalado**.

---

## 🎯 Objetivo

Criar `orb-backend.exe` que:
- ✅ Roda sem Python instalado
- ✅ Inclui todas as dependências (FastAPI, Uvicorn, OpenAI SDK, etc.)
- ✅ Pode ser instalado como serviço Windows
- ✅ É distribuído junto com o frontend WPF

---

## 🔧 Ferramentas

Usamos **PyInstaller** para empacotar Python em executável.

---

## 📋 Processo

### 1. Instalar Dependências de Build

```bash
cd backend
pip install -r requirements.txt
pip install -r requirements-build.txt
```

**requirements-build.txt:**
```txt
pyinstaller==6.3.0
```

### 2. Criar Script de Build

**`build_standalone.py`:**
```python
import PyInstaller.__main__
import os

PyInstaller.__main__.run([
    'src/main.py',
    '--name=orb-backend',
    '--onefile',
    '--noconsole',
    '--hidden-import=uvicorn',
    '--hidden-import=uvicorn.logging',
    '--hidden-import=uvicorn.loops',
    '--hidden-import=uvicorn.protocols',
    '--hidden-import=fastapi',
    '--hidden-import=pydantic',
    '--hidden-import=sqlalchemy',
    '--hidden-import=openai',
    '--add-data=src;src',
    '--distpath=dist',
    '--workpath=build',
    '--specpath=.',
])
```

### 3. Executar Build

```bash
python build_standalone.py
```

**Resultado:**
- `backend/dist/orb-backend.exe` (~50-80 MB)

---

## 🧪 Testar Executável

### Teste Básico

```bash
cd backend/dist
orb-backend.exe
```

Deve iniciar servidor em `http://127.0.0.1:8000`

### Teste de Health Check

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

## 🔄 Instalação como Serviço Windows

### Criar Serviço

```batch
sc create OrbBackendService binPath="C:\Program Files\Orb Agent\backend\orb-backend.exe" start=auto
sc description OrbBackendService "Orb Agent Backend API Service"
sc start OrbBackendService
```

### Script de Instalação

**`install_service.bat`:**
```batch
@echo off
echo Instalando Orb Backend Service...

sc create OrbBackendService ^
  binPath="%~dp0orb-backend.exe" ^
  start=auto ^
  DisplayName="Orb Agent Backend"

sc description OrbBackendService "Servico de backend do Orb Agent - FastAPI + Python"
sc start OrbBackendService

echo.
echo Servico instalado e iniciado com sucesso!
pause
```

### Script de Remoção

**`uninstall_service.bat`:**
```batch
@echo off
echo Removendo Orb Backend Service...

sc stop OrbBackendService
sc delete OrbBackendService

echo.
echo Servico removido com sucesso!
pause
```

---

## 🐛 Troubleshooting

### Erro: "Module not found"

Adicione ao `hidden-import`:
```python
'--hidden-import=nome_do_modulo',
```

### Executável muito grande

**Normal!** Inclui:
- Python runtime (~15 MB)
- FastAPI + Uvicorn (~10 MB)
- OpenAI SDK (~5 MB)
- SQLite + SQLAlchemy (~5 MB)
- Outras dependências (~15 MB)
- **Total:** ~50-80 MB

### Erro ao iniciar serviço

```bash
# Verificar logs do Windows Event Viewer
eventvwr.msc

# Testar executável manualmente primeiro
orb-backend.exe
```

---

## 📦 Estrutura Final

```
backend/dist/
├── orb-backend.exe          # Executável standalone
├── install_service.bat      # Instalar como serviço
├── uninstall_service.bat    # Remover serviço
└── orb.db                   # Banco de dados (criado em runtime)
```

---

## 🚀 Integração com Frontend WPF

O frontend WPF gerencia o backend através de:

1. **BackendProcessManager.cs** - Inicia processo do backend
2. **BackendService.cs** - Comunica via HTTP com backend
3. **SystemTrayService.cs** - Notifica status do backend

---

**Backend Standalone Pronto! 🎉**
