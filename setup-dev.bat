@echo off
REM ORB - Script de Setup para Desenvolvimento (Windows)
REM Este script configura o ambiente de desenvolvimento automaticamente

echo.
echo ========================================
echo    ORB - Setup de Desenvolvimento
echo ========================================
echo.

REM Verificar Node.js
where node >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo [ERRO] Node.js nao encontrado. Instale Node.js 18+ primeiro.
    echo        Download: https://nodejs.org/
    exit /b 1
)

echo [OK] Node.js encontrado
node -v

REM Verificar Python
where python >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo [ERRO] Python nao encontrado. Instale Python 3.11+ primeiro.
    echo        Download: https://www.python.org/
    exit /b 1
)

echo [OK] Python encontrado
python --version

REM Verificar npm
where npm >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo [ERRO] npm nao encontrado. Instale npm primeiro.
    exit /b 1
)

echo [OK] npm encontrado
npm -v
echo.

REM 1. Setup do Backend
echo [1/4] Configurando Backend...
cd backend

REM Criar ambiente virtual Python
if not exist "venv" (
    echo    Criando ambiente virtual Python...
    python -m venv venv
)

REM Ativar ambiente virtual
echo    Ativando ambiente virtual...
call venv\Scripts\activate.bat

REM Instalar dependencias Python
echo    Instalando dependencias Python...
python -m pip install --upgrade pip
pip install -r requirements.txt

REM Criar .env se nao existir
if not exist ".env" (
    echo    Criando arquivo .env...
    copy env.example .env
    
    REM Gerar chave Fernet
    echo    Gerando chave de criptografia...
    python -c "from cryptography.fernet import Fernet; f=open('.env', 'a'); f.write('\nFERNET_KEY=' + Fernet.generate_key().decode()); f.close()"
    
    echo.
    echo [AVISO] Configure sua API key no arquivo backend\.env
    echo         Ou use a interface grafica apos iniciar (Ctrl+Shift+O)
    echo.
)

cd ..

REM 2. Setup do Frontend
echo [2/4] Configurando Frontend...
cd frontend

REM Criar .env se nao existir
if not exist ".env" (
    echo    Criando arquivo .env...
    copy env.example .env
)

REM Instalar dependencias Node.js
echo    Instalando dependencias Node.js...
call npm install

REM Build inicial do TypeScript
echo    Compilando TypeScript...
call npm run build

cd ..

REM 3. Inicializar banco de dados
echo [3/4] Inicializando banco de dados...
cd backend
python -c "from src.database.config_manager import ConfigManager; from src.database.chat_memory import ChatMemoryManager; print('   Criando tabelas...'); ConfigManager(); ChatMemoryManager(); print('   [OK] Banco de dados inicializado!')"
cd ..

echo.
echo ========================================
echo    Setup concluido com sucesso!
echo ========================================
echo.
echo Proximos passos:
echo.
echo 1. Configure sua API key:
echo    - Edite backend\.env e adicione OPENAI_API_KEY=sua-chave
echo    - OU use a interface apos iniciar (Ctrl+Shift+O)
echo.
echo 2. Inicie o projeto:
echo    npm run dev
echo.
echo 3. A aplicacao abrira automaticamente!
echo.
echo Comandos uteis:
echo    npm run dev          - Inicia frontend + backend
echo    npm run dev:backend  - Apenas backend
echo    npm run dev:frontend - Apenas frontend
echo    npm run build        - Build de producao
echo.
pause

