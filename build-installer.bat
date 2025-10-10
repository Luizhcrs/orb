@echo off
REM Orb Agent - Build Completo do Instalador
REM Cria instalador standalone com backend + frontend

echo.
echo ========================================
echo    Orb Agent - Build Instalador
echo ========================================
echo.

REM Verificar .NET SDK
where dotnet >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo [ERRO] .NET SDK nao encontrado!
    echo        Instale .NET 9.0 SDK: https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

echo [OK] .NET SDK encontrado
dotnet --version

REM Verificar Python
where python >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo [ERRO] Python nao encontrado!
    echo        Instale Python 3.11+: https://www.python.org/
    pause
    exit /b 1
)

echo [OK] Python encontrado
python --version

REM Verificar Inno Setup
set "INNO_PATH=C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
if not exist "%INNO_PATH%" (
    echo [ERRO] Inno Setup nao encontrado!
    echo        Instale Inno Setup 6: https://jrsoftware.org/isdl.php
    pause
    exit /b 1
)

echo [OK] Inno Setup encontrado
echo.

REM ========================================
REM Passo 1: Build Backend Standalone
REM ========================================
echo [1/3] Criando backend standalone...
echo.

cd backend

echo   Instalando dependencias...
python -m pip install --upgrade pip -q
pip install -r requirements.txt -q
pip install pyinstaller -q

echo   Executando PyInstaller...
python build_standalone.py

if %ERRORLEVEL% NEQ 0 (
    echo [ERRO] Falha ao criar executavel do backend!
    cd ..
    pause
    exit /b 1
)

REM Verificar se o executável foi criado
if not exist "dist\orb-backend.exe" (
    echo [ERRO] orb-backend.exe nao foi criado!
    cd ..
    pause
    exit /b 1
)

echo   [OK] Backend criado: dist\orb-backend.exe
for %%A in (dist\orb-backend.exe) do echo   Tamanho: %%~zA bytes

cd ..

REM ========================================
REM Passo 2: Build Frontend WPF
REM ========================================
echo.
echo [2/3] Compilando frontend WPF...
echo.

cd frontend

echo   Restaurando dependencias NuGet...
dotnet restore -v quiet

echo   Compilando em Release mode...
dotnet build --configuration Release -v quiet

echo   Publicando como self-contained...
dotnet publish --configuration Release ^
    --self-contained true ^
    --runtime win-x64 ^
    -p:PublishSingleFile=true ^
    -p:IncludeNativeLibrariesForSelfExtract=true ^
    -p:PublishTrimmed=false ^
    -v quiet

if %ERRORLEVEL% NEQ 0 (
    echo [ERRO] Falha ao compilar frontend!
    cd ..
    pause
    exit /b 1
)

REM Verificar se o executável foi criado
if not exist "bin\Release\net9.0-windows\win-x64\publish\Orb.exe" (
    echo [ERRO] Orb.exe nao foi criado!
    cd ..
    pause
    exit /b 1
)

echo   [OK] Frontend compilado: bin\Release\net9.0-windows\win-x64\publish\Orb.exe
for %%A in (bin\Release\net9.0-windows\win-x64\publish\Orb.exe) do echo   Tamanho: %%~zA bytes

cd ..

REM ========================================
REM Passo 3: Criar Instalador com Inno Setup
REM ========================================
echo.
echo [3/3] Criando instalador com Inno Setup...
echo.

REM Criar pasta release se não existir
if not exist "release" mkdir release

REM Executar Inno Setup
"%INNO_PATH%" installer.iss

if %ERRORLEVEL% NEQ 0 (
    echo [ERRO] Falha ao criar instalador!
    pause
    exit /b 1
)

REM Verificar se o instalador foi criado
if not exist "release\OrbAgent-Setup-1.0.0.exe" (
    echo [ERRO] Instalador nao foi criado!
    pause
    exit /b 1
)

echo.
echo ========================================
echo    Build concluido com sucesso!
echo ========================================
echo.
echo Instalador criado: release\OrbAgent-Setup-1.0.0.exe
for %%A in (release\OrbAgent-Setup-1.0.0.exe) do (
    echo Tamanho: %%~zA bytes
    set /a SIZE_MB=%%~zA/1024/1024
    echo Tamanho: !SIZE_MB! MB
)
echo.
echo O que o instalador inclui:
echo - Frontend WPF com .NET Runtime embutido
echo - Backend Python com FastAPI embutido
echo - Instalacao automatica do servico Windows
echo - Atalhos no Desktop e Menu Iniciar
echo - NAO requer Python ou .NET no PC do usuario
echo.
echo Para testar: Execute em uma maquina limpa (VM recomendado)
echo.
pause

