#!/bin/bash
# ORB - Script de Setup para Desenvolvimento
# Este script configura o ambiente de desenvolvimento automaticamente

set -e

echo "🚀 ORB - Setup de Desenvolvimento"
echo "=================================="
echo ""

# Verificar .NET SDK (apenas para Windows/WSL)
if command -v dotnet &> /dev/null; then
    echo "✅ .NET SDK $(dotnet --version) encontrado"
else
    echo "⚠️  .NET SDK não encontrado. Necessário para frontend WPF."
    echo "   Download: https://dotnet.microsoft.com/download"
    echo "   Nota: WPF é apenas para Windows"
fi

# Verificar Python
if ! command -v python3 &> /dev/null; then
    echo "❌ Python 3 não encontrado. Instale Python 3.11+ primeiro."
    echo "   Download: https://www.python.org/"
    exit 1
fi

echo "✅ Python $(python3 --version) encontrado"
echo ""

# 1. Setup do Backend
echo "📦 Configurando Backend..."
cd backend

# Criar ambiente virtual Python
if [ ! -d "venv" ]; then
    echo "   Criando ambiente virtual Python..."
    python3 -m venv venv
fi

# Ativar ambiente virtual
echo "   Ativando ambiente virtual..."
source venv/bin/activate || source venv/Scripts/activate

# Instalar dependências Python
echo "   Instalando dependências Python..."
pip install --upgrade pip
pip install -r requirements.txt

# Criar .env se não existir
if [ ! -f ".env" ]; then
    echo "   Criando arquivo .env..."
    cp env.example .env
    
    # Gerar chave Fernet
    echo "   Gerando chave de criptografia..."
    FERNET_KEY=$(python3 -c "from cryptography.fernet import Fernet; print(Fernet.generate_key().decode())")
    
    # Adicionar ao .env
    echo "FERNET_KEY=$FERNET_KEY" >> .env
    
    echo ""
    echo "⚠️  IMPORTANTE: Configure sua API key no arquivo backend/.env"
    echo "   Ou use a interface gráfica após iniciar (Ctrl+Shift+O)"
    echo ""
fi

cd ..

# 2. Setup do Frontend (WPF - apenas Windows)
if command -v dotnet &> /dev/null; then
    echo "📦 Configurando Frontend WPF..."
    cd frontend
    
    # Restaurar dependências NuGet
    echo "   Restaurando dependências NuGet..."
    dotnet restore
    
    # Build inicial
    echo "   Compilando aplicação WPF..."
    dotnet build --configuration Release
    
    cd ..
else
    echo "⏭️  Pulando setup do frontend (WPF requer Windows e .NET SDK)"
fi

# 3. Inicializar banco de dados
echo "🗄️  Inicializando banco de dados..."
cd backend
python3 -c "
from src.database.config_manager import ConfigManager
from src.database.chat_memory import ChatMemoryManager

print('   Criando tabelas de configuração...')
config_manager = ConfigManager()

print('   Criando tabelas de histórico...')
memory_manager = ChatMemoryManager()

print('   ✅ Banco de dados inicializado!')
"
cd ..

echo ""
echo "✅ Setup concluído com sucesso!"
echo ""
echo "📚 Próximos passos:"
echo ""
echo "1. Configure sua API key:"
echo "   - Edite backend/.env e adicione OPENAI_API_KEY=sua-chave"
echo "   - OU use a interface após iniciar (Ctrl+Shift+O)"
echo ""
echo "2. Inicie o projeto:"
echo "   npm run dev (ou dotnet run --project frontend)"
echo ""
echo "3. A aplicação abrirá automaticamente!"
echo ""
echo "🔧 Comandos úteis:"
echo "   npm run dev          - Inicia frontend + backend"
echo "   npm run dev:backend  - Apenas backend"
echo "   dotnet run --project frontend - Apenas frontend WPF"
echo "   npm run build        - Build de produção"
echo ""
