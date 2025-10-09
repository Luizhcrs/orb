# 📦 Como Criar o Instalador Completo

Este guia mostra como criar um instalador **standalone** que:
- ✅ Instala o backend como **serviço Windows**
- ✅ Instala o frontend WPF como **aplicativo desktop**
- ✅ **NÃO requer** Python ou .NET Runtime instalados
- ✅ **Tudo em um único instalador**

---

## 🔧 Pré-requisitos

### No PC de Desenvolvimento

1. **.NET 9.0 SDK** (para build do frontend WPF)
2. **Python 3.11+** (para build do backend)
3. **Windows 10+** (para testar o instalador)
4. **Visual Studio 2022** ou **VS Code** (recomendado)

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

### **Passo 2: Compilar Frontend WPF**

```bash
cd frontend

# 1. Restaurar dependências NuGet
dotnet restore

# 2. Build em Release
dotnet build --configuration Release

# 3. Publicar como self-contained
dotnet publish --configuration Release --self-contained --runtime win-x64 -p:PublishSingleFile=true
```

**Resultado:** `frontend/bin/Release/net9.0-windows/publish/Orb.exe`

**O que isso faz:**
- Compila aplicação WPF
- Inclui .NET Runtime embutido
- Cria executável único com todas dependências
- ~80-100 MB (inclui .NET Runtime)

---

### **Passo 3: Criar Instalador (Avançado)**

**Opção A: Inno Setup (Recomendado)**

1. Instale [Inno Setup](https://jrsoftware.org/isdl.php)
2. Crie script `installer.iss`:

```iss
[Setup]
AppName=Orb Agent
AppVersion=1.0.0
DefaultDirName={pf}\Orb Agent
DefaultGroupName=Orb Agent
OutputDir=release
OutputBaseFilename=OrbAgent-Setup-1.0.0

[Files]
Source: "frontend\bin\Release\net9.0-windows\publish\*"; DestDir: "{app}"; Flags: recursesubdirs
Source: "backend\dist\orb-backend.exe"; DestDir: "{app}\backend"
Source: "backend\orb.db"; DestDir: "{app}\backend"; Flags: onlyifdoesntexist

[Icons]
Name: "{group}\Orb Agent"; Filename: "{app}\Orb.exe"
Name: "{commondesktop}\Orb Agent"; Filename: "{app}\Orb.exe"

[Run]
Filename: "{app}\backend\install_service.bat"; Description: "Instalar serviço do backend"; Flags: runascurrentuser

[UninstallRun]
Filename: "{app}\backend\uninstall_service.bat"; Flags: runascurrentuser
```

3. Compile:
```bash
iscc installer.iss
```

**Opção B: Script Automatizado**

```batch
build-all.bat
```

---

## 📦 Estrutura do Instalador Final

```
C:\Program Files\Orb Agent\
├── Orb.exe                    # Frontend WPF
├── Orb.dll                    # Bibliotecas da aplicação
├── *.dll                      # Dependências .NET
├── backend\
│   ├── orb-backend.exe       # Backend standalone
│   ├── orb.db                # Banco SQLite
│   ├── install_service.bat   # Instalar serviço
│   └── uninstall_service.bat # Remover serviço
└── Assets\                   # Recursos da aplicação
```

---

## 🧪 Testar o Build

### Teste Local (Sem Instalador)

```bash
# 1. Executar backend manualmente
cd backend\dist
orb-backend.exe

# 2. Em outro terminal, executar frontend
cd frontend\bin\Release\net9.0-windows\publish
Orb.exe
```

### Teste do Instalador

1. Execute `OrbAgent-Setup-1.0.0.exe` em uma **máquina limpa**
2. Verifique se:
   - ✅ Frontend abre corretamente
   - ✅ Hot corner funciona
   - ✅ Atalhos globais funcionam
   - ✅ Backend responde (teste com chat)
   - ✅ Configurações salvam corretamente
   - ✅ Histórico persiste

---

## 🚀 Build Automatizado (CI/CD)

### GitHub Actions

```yaml
name: Build Instalador

on:
  push:
    tags:
      - 'v*'

jobs:
  build:
    runs-on: windows-latest
    
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '9.0.x'
      
      - name: Setup Python
        uses: actions/setup-python@v4
        with:
          python-version: '3.11'
      
      - name: Build Backend
        run: |
          cd backend
          pip install -r requirements.txt
          pip install -r requirements-build.txt
          python build_standalone.py
      
      - name: Build Frontend
        run: |
          cd frontend
          dotnet restore
          dotnet publish --configuration Release --self-contained --runtime win-x64 -p:PublishSingleFile=true
      
      - name: Upload Artifacts
        uses: actions/upload-artifact@v3
        with:
          name: orb-agent-build
          path: |
            frontend/bin/Release/net9.0-windows/publish/
            backend/dist/
```

---

## 📊 Tamanhos Estimados

| Componente | Tamanho | Observação |
|------------|---------|------------|
| Frontend WPF (self-contained) | ~80-100 MB | Inclui .NET Runtime |
| Backend (PyInstaller) | ~50-80 MB | Inclui Python + FastAPI |
| **Total do Instalador** | **~150-200 MB** | Standalone completo |

---

## 🐛 Troubleshooting

### Backend não compila

**Erro:** "PyInstaller failed"
```bash
pip install --upgrade pyinstaller
python build_standalone.py
```

### Frontend não compila

**Erro:** "dotnet command not found"
```bash
# Instale .NET 9.0 SDK
# Download: https://dotnet.microsoft.com/download
```

**Erro:** "CS0103" ou outras falhas de compilação
```bash
cd frontend
dotnet clean
dotnet restore
dotnet build
```

### Instalador muito grande

**Normal!** O instalador é grande porque inclui:
- .NET 9.0 Runtime completo
- Python 3.11 runtime
- FastAPI + Uvicorn
- OpenAI SDK
- Todas as dependências WPF

**Otimizações possíveis:**
- Use `PublishTrimmed=true` (pode causar problemas)
- Use `PublishSingleFile=true` (já implementado)
- Comprima com UPX (risco de antivírus)

---

## 📞 Precisa de Ajuda?

Veja também:
- `DEPLOYMENT.md` - Opções de deploy
- `RELEASE.md` - Processo de release
- `QUICK_BUILD.md` - Build rápido para testes

---

**Pronto para distribuir! 🎉**
