# 📦 Guia Completo - Criar Instalador do Orb Agent

## 🎯 Objetivo

Criar um instalador **único** (`OrbAgent-Setup.exe`) que:
- ✅ Instala frontend WPF
- ✅ Instala backend Python como executável standalone
- ✅ Configura backend como serviço Windows (auto-start)
- ✅ Cria atalhos no Desktop e Menu Iniciar
- ✅ **NÃO requer** Python, .NET Runtime ou outras dependências no PC do usuário

---

## 🛠️ Ferramentas Necessárias

### No PC de Build

1. **.NET 9.0 SDK** - Para compilar frontend WPF
2. **Python 3.11+** - Para criar backend standalone
3. **PyInstaller** - Para empacotar Python
4. **Inno Setup 6+** - Para criar instalador Windows
   - Download: https://jrsoftware.org/isdl.php

---

## 📋 Processo Completo

### Passo 1: Build do Backend Standalone

```bash
cd backend

# Instalar dependências
pip install -r requirements.txt
pip install pyinstaller

# Criar executável standalone
pyinstaller --name=orb-backend ^
    --onefile ^
    --noconsole ^
    --hidden-import=uvicorn.logging ^
    --hidden-import=uvicorn.loops ^
    --hidden-import=uvicorn.protocols ^
    --add-data="src;src" ^
    src/main.py

# Resultado: backend/dist/orb-backend.exe (~60-80 MB)
```

**O que isso faz:**
- Empacota Python 3.11 + FastAPI + OpenAI SDK + todas dependências
- Cria um único `.exe` que roda sem Python instalado
- Inclui todos os arquivos `src/` necessários

### Passo 2: Build do Frontend WPF

```bash
cd ../frontend

# Restaurar e compilar
dotnet restore
dotnet build --configuration Release

# Publicar como self-contained (inclui .NET Runtime)
dotnet publish --configuration Release ^
    --self-contained true ^
    --runtime win-x64 ^
    -p:PublishSingleFile=true ^
    -p:IncludeNativeLibrariesForSelfExtract=true ^
    -p:PublishTrimmed=false

# Resultado: frontend/bin/Release/net9.0-windows/win-x64/publish/Orb.exe (~80-100 MB)
```

**O que isso faz:**
- Compila aplicação WPF
- Inclui .NET 9.0 Runtime completo
- Cria executável único com todas DLLs embutidas
- NÃO requer .NET instalado no PC do usuário

### Passo 3: Criar Scripts de Serviço Windows

Criar `backend/dist/install_service.bat`:

```batch
@echo off
echo Instalando Orb Backend Service...

REM Obter caminho do executável
set BACKEND_PATH=%~dp0orb-backend.exe

REM Criar serviço Windows
sc create OrbBackendService ^
  binPath="%BACKEND_PATH%" ^
  start=auto ^
  DisplayName="Orb Agent Backend"

REM Definir descrição
sc description OrbBackendService "Serviço de backend do Orb Agent - FastAPI + Python"

REM Iniciar serviço
sc start OrbBackendService

echo.
echo Serviço instalado e iniciado com sucesso!
echo O backend agora inicia automaticamente com o Windows.
pause
```

Criar `backend/dist/uninstall_service.bat`:

```batch
@echo off
echo Removendo Orb Backend Service...

REM Parar serviço
sc stop OrbBackendService

REM Deletar serviço
sc delete OrbBackendService

echo.
echo Serviço removido com sucesso!
pause
```

### Passo 4: Criar Script Inno Setup

Criar `installer.iss` na raiz do projeto:

```iss
; Script do Inno Setup para Orb Agent
#define MyAppName "Orb Agent"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Seu Nome"
#define MyAppURL "https://github.com/seu-usuario/orb"
#define MyAppExeName "Orb.exe"

[Setup]
AppId={{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\Orb Agent
DefaultGroupName=Orb Agent
AllowNoIcons=yes
LicenseFile=LICENSE
OutputDir=release
OutputBaseFilename=OrbAgent-Setup-{#MyAppVersion}
SetupIconFile=frontend\Assets\orb.ico
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

[Languages]
Name: "brazilianportuguese"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"

[Tasks]
Name: "desktopicon"; Description: "Criar atalho na Área de Trabalho"; GroupDescription: "Atalhos adicionais:"
Name: "startup"; Description: "Iniciar com o Windows"; GroupDescription: "Opções:"

[Files]
; Frontend WPF (todos os arquivos publicados)
Source: "frontend\bin\Release\net9.0-windows\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

; Backend Python standalone
Source: "backend\dist\orb-backend.exe"; DestDir: "{app}\backend"; Flags: ignoreversion
Source: "backend\dist\install_service.bat"; DestDir: "{app}\backend"; Flags: ignoreversion
Source: "backend\dist\uninstall_service.bat"; DestDir: "{app}\backend"; Flags: ignoreversion

; Banco de dados (apenas se não existir)
Source: "backend\orb.db"; DestDir: "{app}\backend"; Flags: onlyifdoesntexist uninsneveruninstall

; Assets
Source: "frontend\Assets\*"; DestDir: "{app}\Assets"; Flags: ignoreversion recursesubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Configurações"; Filename: "{app}\{#MyAppExeName}"; Parameters: "--config"
Name: "{group}\Desinstalar {#MyAppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
; Instalar serviço do backend APÓS instalação dos arquivos
Filename: "{app}\backend\install_service.bat"; Description: "Instalar serviço do backend"; Flags: postinstall runascurrentuser waituntilterminated

[UninstallRun]
; Remover serviço ANTES de desinstalar arquivos
Filename: "{app}\backend\uninstall_service.bat"; Flags: runascurrentuser waituntilterminated

[Code]
// Verificar se .NET Runtime está disponível (já está embutido, mas vamos avisar)
function InitializeSetup(): Boolean;
begin
  Result := True;
  MsgBox('Orb Agent será instalado com todos os componentes necessários.' + #13#10 + 
         'Backend Python e .NET Runtime estão incluídos.', mbInformation, MB_OK);
end;
```

### Passo 5: Script de Build Automatizado

Criar `build-installer.bat` na raiz:

```batch
@echo off
echo ========================================
echo    Orb Agent - Build Instalador
echo ========================================
echo.

REM 1. Build Backend
echo [1/3] Criando backend standalone...
cd backend
pip install -r requirements.txt
pip install pyinstaller
pyinstaller --name=orb-backend --onefile --noconsole --hidden-import=uvicorn.logging --hidden-import=uvicorn.loops --hidden-import=uvicorn.protocols --add-data="src;src" src/main.py
if %ERRORLEVEL% NEQ 0 (
    echo ERRO ao criar backend!
    pause
    exit /b 1
)
cd ..

REM 2. Build Frontend
echo [2/3] Compilando frontend WPF...
cd frontend
dotnet restore
dotnet publish --configuration Release --self-contained true --runtime win-x64 -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:PublishTrimmed=false
if %ERRORLEVEL% NEQ 0 (
    echo ERRO ao compilar frontend!
    pause
    exit /b 1
)
cd ..

REM 3. Criar Instalador
echo [3/3] Criando instalador com Inno Setup...
"C:\Program Files (x86)\Inno Setup 6\ISCC.exe" installer.iss
if %ERRORLEVEL% NEQ 0 (
    echo ERRO ao criar instalador!
    echo Certifique-se de que o Inno Setup está instalado.
    pause
    exit /b 1
)

echo.
echo ========================================
echo    Build concluído com sucesso!
echo ========================================
echo.
echo Instalador criado em: release\OrbAgent-Setup-1.0.0.exe
echo Tamanho estimado: ~150-200 MB
echo.
pause
```

---

## 🧪 Testar o Instalador

### Em Máquina Limpa (Recomendado)

1. **VM Windows** ou **PC sem .NET/Python**
2. Execute `OrbAgent-Setup-1.0.0.exe`
3. Siga o wizard
4. **Importante:** Marque "Instalar serviço do backend" no final
5. Verifique:
   - ✅ Frontend abre (`C:\Program Files\Orb Agent\Orb.exe`)
   - ✅ Hot corner funciona
   - ✅ Backend responde (`http://localhost:8000/health`)
   - ✅ Serviço está rodando (`sc query OrbBackendService`)
   - ✅ Chat funciona
   - ✅ Configurações salvam

### Testar Desinstalação

1. Desinstalar via Painel de Controle
2. Verificar se:
   - ✅ Serviço foi removido
   - ✅ Arquivos foram deletados
   - ✅ Banco de dados foi preservado (opcional)

---

## 📊 Estrutura do Instalador

```
OrbAgent-Setup-1.0.0.exe (~150-200 MB)
│
├─ Frontend WPF
│  ├─ Orb.exe (self-contained com .NET Runtime)
│  ├─ *.dll (dependências WPF)
│  └─ Assets/ (orb-icon.svg, etc.)
│
└─ Backend Python
   ├─ orb-backend.exe (PyInstaller standalone)
   ├─ install_service.bat
   ├─ uninstall_service.bat
   └─ orb.db (banco SQLite)
```

### Instalação no PC do Usuário

```
C:\Program Files\Orb Agent\
├── Orb.exe                    # Frontend WPF
├── Orb.dll                    # Bibliotecas WPF
├── Microsoft.*.dll            # Dependências .NET
├── Assets\
│   ├── orb-icon.svg
│   └── README.md
└── backend\
    ├── orb-backend.exe       # Backend standalone
    ├── orb.db                # Banco SQLite
    ├── install_service.bat   # Script de instalação
    └── uninstall_service.bat # Script de remoção
```

---

## 🚀 Processo de Build Completo

### Opção 1: Script Automatizado (Recomendado)

```batch
build-installer.bat
```

Isso faz tudo automaticamente:
1. Build backend → `backend/dist/orb-backend.exe`
2. Build frontend → `frontend/bin/Release/.../publish/Orb.exe`
3. Cria instalador → `release/OrbAgent-Setup-1.0.0.exe`

### Opção 2: Manual

```bash
# Backend
cd backend
pip install pyinstaller
pyinstaller --name=orb-backend --onefile --noconsole --add-data="src;src" src/main.py

# Frontend
cd ../frontend
dotnet publish --configuration Release --self-contained --runtime win-x64

# Instalador
iscc installer.iss
```

---

## 🔧 Customizações Avançadas

### Tamanho do Instalador

**Reduzir tamanho (arriscado):**
```bash
# Frontend com trimming
dotnet publish -p:PublishTrimmed=true

# Backend comprimido com UPX
upx --best backend/dist/orb-backend.exe
```

⚠️ **Aviso:** Pode causar problemas com antivírus e quebrar funcionalidades.

### Adicionar Logo e Tema

No `installer.iss`:
```iss
SetupIconFile=frontend\Assets\orb.ico
WizardImageFile=installer-assets\wizard-image.bmp
WizardSmallImageFile=installer-assets\wizard-small.bmp
```

### Auto-Update

Adicione no `[Code]` do `installer.iss`:
```pascal
function InitializeSetup(): Boolean;
var
  Version: String;
begin
  // Verificar versão instalada
  if RegQueryStringValue(HKLM, 'Software\Orb Agent', 'Version', Version) then
  begin
    if CompareStr(Version, '{#MyAppVersion}') >= 0 then
    begin
      MsgBox('Versão mais recente já está instalada.', mbInformation, MB_OK);
      Result := False;
    end
    else
    begin
      Result := True;
    end;
  end
  else
  begin
    Result := True;
  end;
end;
```

---

## 📦 Distribuição

### GitHub Releases

1. Criar tag:
```bash
git tag -a v1.0.0 -m "Release 1.0.0"
git push origin v1.0.0
```

2. Upload no GitHub:
- `OrbAgent-Setup-1.0.0.exe` (~150-200 MB)
- Checksums (SHA256)

### Download Direto

Hospede em:
- GitHub Releases (grátis, recomendado)
- Google Drive / Dropbox
- Servidor próprio

---

## 🐛 Troubleshooting

### Backend não compila

**Erro:** "Module not found"
```bash
# Adicionar imports ocultos
pyinstaller ... --hidden-import=nome_do_modulo
```

### Frontend muito grande

**Normal!** Inclui:
- .NET Runtime (~60 MB)
- WPF Libraries (~20 MB)
- Dependências (~10 MB)

### Instalador não funciona

1. Testar em VM Windows limpa
2. Verificar logs do Inno Setup
3. Executar instalador como Admin

### Serviço não inicia

```bash
# Verificar status
sc query OrbBackendService

# Ver logs
eventvwr.msc
```

---

## ✅ Checklist Final

Antes de distribuir:

- [ ] Backend standalone funciona (`orb-backend.exe`)
- [ ] Frontend self-contained funciona (`Orb.exe`)
- [ ] Scripts de serviço funcionam (`install_service.bat`)
- [ ] Instalador cria todos os arquivos
- [ ] Serviço Windows inicia automaticamente
- [ ] Aplicação funciona em máquina limpa
- [ ] Desinstalação remove tudo corretamente
- [ ] Banco de dados é preservado na desinstalação
- [ ] Atalhos funcionam
- [ ] Hot corner funciona
- [ ] Configurações salvam

---

**Próximo passo: Quer que eu crie os arquivos agora? (installer.iss, build-installer.bat, scripts de serviço)**

