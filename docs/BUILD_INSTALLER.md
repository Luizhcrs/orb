# 📦 Como Criar o Instalador Completo

Este guia mostra como criar um instalador **standalone** que:
- ✅ Instala o backend como **serviço Windows**
- ✅ Instala o frontend como **aplicativo desktop**
- ✅ **NÃO requer** Python ou Node.js instalados
- ✅ **Tudo em um único instalador**

---

## 🔧 Pré-requisitos

### No PC de Desenvolvimento

1. **Node.js 18+** (para build do frontend)
2. **Python 3.11+** (para build do backend)
3. **Windows** (para testar o instalador)

---

## 📋 Processo Completo de Build

### **Passo 1: Preparar Backend**

```bash
cd backend

# 1. Instalar dependências de build
pip install -r requirements.txt
pip install -r requirements-build.txt

# 2. Criar executável standalone
python build_standalone.py
```

**Resultado:** `backend/dist/orb-backend.exe` (executável standalone)

**O que isso faz:**
- Empacota Python + FastAPI + Todas dependências
- Cria um único `.exe` que roda sem Python instalado
- ~50-80 MB (inclui tudo!)

---

### **Passo 2: Preparar Frontend**

```bash
cd frontend

# 1. Instalar dependências
npm install

# 2. Build TypeScript
npm run build
```

---

### **Passo 3: Criar Instalador**

```bash
# Ainda em frontend/
npm run pack:win
```

**Resultado:** `frontend/release/OrbAgent-Setup-1.0.0.exe`

**O que o instalador faz:**
1. ✅ Copia `orb-backend.exe` para `C:\Program Files\Orb Agent\resources\backend\`
2. ✅ Baixa e instala NSSM (Service Manager)
3. ✅ Registra `OrbBackendService` no Windows
4. ✅ Inicia o serviço automaticamente
5. ✅ Instala o frontend Electron
6. ✅ Cria atalhos Desktop e Menu Iniciar

---

## 🎯 Resultado Final

### Para o Desenvolvedor

Você terá em `frontend/release/`:
- `OrbAgent-Setup-1.0.0.exe` - **Instalador completo**
- `OrbAgent-Portable-1.0.0.exe` - Versão portátil (sem instalação)

### Para o Usuário Final

**Instalação:**
1. Baixar `OrbAgent-Setup-1.0.0.exe`
2. Executar (pede admin para instalar serviço)
3. Seguir o wizard de instalação
4. Pronto! ORB está instalado e funcionando

**O que é instalado:**
- `C:\Program Files\Orb Agent\` - Aplicação frontend
- `C:\Program Files\Orb Agent\resources\backend\` - Backend + NSSM
- Serviço Windows: `OrbBackendService` (inicia automaticamente)
- Atalhos: Desktop + Menu Iniciar

**Nenhuma dependência externa necessária!**

---

## 🔍 Verificar Instalação

### Verificar Serviço

```powershell
# Verificar status do serviço
sc query OrbBackendService

# Ver logs
type "C:\Program Files\Orb Agent\resources\backend\logs\stdout.log"
```

### Verificar Backend

```powershell
# Testar API
curl http://127.0.0.1:8000/api/v1/health
```

### Gerenciar Serviço Manualmente

```powershell
# Parar
sc stop OrbBackendService

# Iniciar
sc start OrbBackendService

# Remover
sc delete OrbBackendService
```

---

## 🛠️ Troubleshooting

### Erro: "orb-backend.exe not found"

**Causa:** Backend não foi buildado antes do frontend.

**Solução:**
```bash
cd backend
python build_standalone.py
cd ../frontend
npm run pack:win
```

### Erro: "NSSM download failed"

**Causa:** Sem internet ou bloqueio de firewall.

**Solução:** 
1. Baixe NSSM manualmente: https://nssm.cc/release/nssm-2.24.zip
2. Extraia e copie `nssm.exe` para `frontend/`
3. Edite `installer.nsh` para usar arquivo local

### Serviço não inicia

**Debug:**
```powershell
# Ver logs detalhados
type "C:\Program Files\Orb Agent\resources\backend\logs\stderr.log"

# Testar executável manualmente
cd "C:\Program Files\Orb Agent\resources\backend"
.\orb-backend.exe
```

---

## 📊 Script Automatizado Completo

Para automatizar tudo de uma vez, crie `build-all.bat`:

```batch
@echo off
echo ========================================
echo    ORB - Build Completo
echo ========================================

echo [1/3] Building backend...
cd backend
pip install -r requirements.txt -q
pip install -r requirements-build.txt -q
python build_standalone.py
cd ..

echo [2/3] Building frontend...
cd frontend
call npm install
call npm run build
cd ..

echo [3/3] Creating installer...
cd frontend
call npm run pack:win
cd ..

echo.
echo ========================================
echo    Build concluido com sucesso!
echo ========================================
echo.
echo Instalador: frontend\release\OrbAgent-Setup-1.0.0.exe
echo.
pause
```

**Uso:**
```batch
build-all.bat
```

---

## 🚀 Publicar Release

1. **Teste localmente:**
   - Instale em máquina limpa
   - Verifique serviço rodando
   - Teste funcionalidades

2. **Crie release no GitHub:**
   ```bash
   git tag -a v1.0.0 -m "Release v1.0.0"
   git push origin v1.0.0
   ```

3. **Upload do instalador:**
   - Vá para GitHub Releases
   - Anexe `OrbAgent-Setup-1.0.0.exe`
   - Adicione notas de release

---

**Parabéns! Você tem um instalador profissional completo! 🎉**

