@echo off
REM ORB - Script de Build Completo
REM Cria instalador standalone com backend como servico Windows

echo.
echo ========================================
echo    ORB - Build Completo do Instalador
echo ========================================
echo.

REM Verificar Node.js
where node >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo [ERRO] Node.js nao encontrado!
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

echo [1/4] Instalando dependencias do backend...
cd backend
python -m pip install --upgrade pip -q
pip install -r requirements.txt -q
pip install -r requirements-build.txt -q
cd ..

echo [2/4] Criando executavel standalone do backend...
cd backend
python build_standalone.py
if %ERRORLEVEL% NEQ 0 (
    echo [ERRO] Falha ao criar executavel do backend!
    cd ..
    pause
    exit /b 1
)
cd ..

echo [3/4] Preparando frontend...
cd frontend
call npm install
if %ERRORLEVEL% NEQ 0 (
    echo [ERRO] Falha ao instalar dependencias do frontend!
    cd ..
    pause
    exit /b 1
)

call npm run build
if %ERRORLEVEL% NEQ 0 (
    echo [ERRO] Falha ao compilar frontend!
    cd ..
    pause
    exit /b 1
)

echo [4/4] Criando instalador...
call npm run pack:win
if %ERRORLEVEL% NEQ 0 (
    echo [ERRO] Falha ao criar instalador!
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
echo Instalador criado em:
dir /b frontend\release\*.exe
echo.
echo Tamanho estimado: ~150-250 MB
echo Inclui: Backend (servico Windows) + Frontend (app desktop)
echo Nao requer: Python, Node.js ou outras dependencias
echo.
echo Para testar: Execute o instalador em uma maquina limpa
echo.
pause

