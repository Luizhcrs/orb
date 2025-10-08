@echo off
REM ========================================
REM ORB - Desinstalar Backend Service
REM ========================================

echo ========================================
echo  ORB - Desinstalar Backend Service
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

echo [1/3] Parando servico...
sc stop OrbBackendService
timeout /t 3 /nobreak >nul

echo [2/3] Removendo servico...
sc delete OrbBackendService
if %errorLevel% neq 0 (
    echo [ERRO] Falha ao remover servico!
    pause
    exit /b 1
)

echo [3/3] Limpando arquivos...
if exist "data\orb.db" (
    echo Deseja remover o banco de dados? (S/N)
    set /p remove_db=
    if /i "%remove_db%"=="S" (
        del /F /Q "data\orb.db"
        echo Banco de dados removido.
    )
)

echo.
echo ========================================
echo  Servico removido com sucesso!
echo ========================================
echo.
echo Agora voce pode desinstalar o Orb Agent
echo pelo Painel de Controle normalmente.
echo.
pause

