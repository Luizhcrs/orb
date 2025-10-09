@echo off
REM ORB - Script de Build Completo
REM Cria instalador standalone com backend como servico Windows

echo.
echo ========================================
echo    ORB - Build Completo do Instalador
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

REM Verificar Python
where python >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo [ERRO] Python nao encontrado!
    pause
    exit /b 1
)

echo [1/3] Instalando dependencias do backend...
cd backend
python -m pip install --upgrade pip -q
pip install -r requirements.txt -q
pip install -r requirements-build.txt -q
cd ..

echo [2/3] Criando executavel standalone do backend...
cd backend
python build_standalone.py
if %ERRORLEVEL% NEQ 0 (
    echo [ERRO] Falha ao criar executavel do backend!
    cd ..
    pause
    exit /b 1
)
cd ..

echo [3/3] Compilando frontend WPF...
cd frontend
dotnet restore
if %ERRORLEVEL% NEQ 0 (
    echo [ERRO] Falha ao restaurar dependencias do frontend!
    cd ..
    pause
    exit /b 1
)

dotnet build --configuration Release
if %ERRORLEVEL% NEQ 0 (
    echo [ERRO] Falha ao compilar frontend!
    cd ..
    pause
    exit /b 1
)

dotnet publish --configuration Release --self-contained
if %ERRORLEVEL% NEQ 0 (
    echo [ERRO] Falha ao publicar frontend!
    cd ..
    pause
    exit /b 1
)
cd ..

echo.
echo ========================================
echo    Build concluido com sucesso!
echo ========================================
echo.
echo Frontend compilado em: frontend\bin\Release\net9.0-windows\
echo Backend compilado em: backend\dist\orb-backend.exe
echo.
echo Tamanho estimado: ~150-250 MB
echo Inclui: Backend (servico Windows) + Frontend (app WPF)
echo Nao requer: Python, .NET Runtime ou outras dependencias
echo.
echo Para testar: Execute Orb.exe na pasta de release
echo.
pause
