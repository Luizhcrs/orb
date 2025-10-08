@echo off
REM ========================================
REM ORB - Atualizar Backend Service
REM ========================================

echo ========================================
echo  ORB - Atualizar Backend Service
echo ========================================
echo.

REM Verificar se esta rodando como Admin
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo [ERRO] Este script precisa rodar como Administrador!
    echo Clique com botao direito e selecione "Executar como administrador"
    pause
    exit /b 1
)

echo [1/5] Parando servico...
sc stop OrbBackendService
timeout /t 5 /nobreak >nul

echo [2/5] Deletando executavel antigo...
del /F /Q "C:\Program Files\Orb Agent\resources\backend\orb-backend.exe"
if %errorLevel% neq 0 (
    echo [AVISO] Nao foi possivel deletar o arquivo antigo, tentando continuar...
)
timeout /t 2 /nobreak >nul

echo [3/5] Copiando novo executavel...
copy /Y "dist\orb-backend.exe" "C:\Program Files\Orb Agent\resources\backend\orb-backend.exe"
if %errorLevel% neq 0 (
    echo [ERRO] Falha ao copiar executavel!
    pause
    exit /b 1
)

echo [4/5] Iniciando servico...
sc start OrbBackendService
if %errorLevel% neq 0 (
    echo [ERRO] Falha ao iniciar servico!
    pause
    exit /b 1
)

echo [5/5] Testando conexao...
timeout /t 3 /nobreak >nul
powershell -Command "try { Invoke-WebRequest -Uri http://127.0.0.1:8000/ -UseBasicParsing | Out-Null; Write-Host '[OK] Backend rodando!' -ForegroundColor Green } catch { Write-Host '[ERRO] Backend nao esta respondendo!' -ForegroundColor Red; exit 1 }"

echo.
echo ========================================
echo  Atualizacao concluida!
echo ========================================
echo.
pause

