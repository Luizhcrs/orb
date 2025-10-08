@echo off
REM Script para instalar ORB Backend como servico Windows
REM Usa sc.exe (nativo do Windows, sem NSSM)
REM Execute como Administrador!

echo ========================================
echo    ORB Backend - Instalacao de Servico
echo ========================================
echo.

REM Verificar se esta rodando como admin
net session >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo [ERRO] Execute como Administrador!
    echo        Clique com botao direito e selecione "Executar como administrador"
    pause
    exit /b 1
)

set "EXE_PATH=%~dp0dist\orb-backend.exe"
set "SERVICE_NAME=OrbBackendService"

REM Verificar se executavel existe
if not exist "%EXE_PATH%" (
    echo [ERRO] Executavel nao encontrado: %EXE_PATH%
    echo        Execute 'python build_standalone.py' primeiro
    pause
    exit /b 1
)

echo [1/3] Removendo servico anterior (se existir)...
sc stop %SERVICE_NAME% >nul 2>&1
sc delete %SERVICE_NAME% >nul 2>&1

echo [2/3] Criando servico Windows...
sc create %SERVICE_NAME% binPath= "%EXE_PATH%" start= auto DisplayName= "ORB Backend Service"

if %ERRORLEVEL% NEQ 0 (
    echo [ERRO] Falha ao criar servico!
    pause
    exit /b 1
)

echo [3/3] Configurando e iniciando servico...
sc description %SERVICE_NAME% "Backend API para ORB Agent - Assistente IA Desktop"
sc start %SERVICE_NAME%

echo.
echo ========================================
echo    Servico instalado com sucesso!
echo ========================================
echo.
echo Nome do servico: %SERVICE_NAME%
echo Status: Rodando
echo Porta: http://127.0.0.1:8000
echo.
echo Aguardando backend iniciar (10 segundos)...
timeout /t 10 >nul

echo.
echo Testando conexao...
curl http://127.0.0.1:8000/api/v1/health 2>nul
if %ERRORLEVEL% EQU 0 (
    echo.
    echo [OK] Backend respondendo!
) else (
    echo.
    echo [AVISO] Backend pode estar iniciando ainda...
    echo         Aguarde mais alguns segundos.
)

echo.
echo Para gerenciar o servico:
echo   - Ver status:  sc query %SERVICE_NAME%
echo   - Parar:       sc stop %SERVICE_NAME%
echo   - Iniciar:     sc start %SERVICE_NAME%
echo   - Remover:     sc delete %SERVICE_NAME%
echo.
pause

