#!/bin/bash
# ORB - Script de Setup para Desenvolvimento
# Este script configura o ambiente de desenvolvimento automaticamente

set -e

echo "🚀 ORB - Setup de Desenvolvimento"
echo "=================================="
echo ""

# Verificar Node.js
if ! command -v node &> /dev/null; then
    echo "❌ Node.js não encontrado. Instale Node.js 18+ primeiro."
    echo "   Download: https://nodejs.org/"
    exit 1
fi

echo "✅ Node.js $(node -v) encontrado"

# Verificar Python
if ! command -v python3 &> /dev/null; then
    echo "❌ Python 3 não encontrado. Instale Python 3.11+ primeiro."
    echo "   Download: https://www.python.org/"
    exit 1
fi

echo "✅ Python $(python3 --version) encontrado"

# Verificar npm
if ! command -v npm &> /dev/null; then
    echo "❌ npm não encontrado. Instale npm primeiro."
    exit 1
fi

echo "✅ npm $(npm -v) encontrado"
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

# 2. Setup do Frontend
echo "📦 Configurando Frontend..."
cd frontend

# Criar .env se não existir
if [ ! -f ".env" ]; then
    echo "   Criando arquivo .env..."
    cp env.example .env
fi

# Instalar dependências Node.js
echo "   Instalando dependências Node.js..."
npm install

# Build inicial do TypeScript
echo "   Compilando TypeScript..."
npm run build

cd ..

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
echo "   npm run dev"
echo ""
echo "3. A aplicação abrirá automaticamente!"
echo ""
echo "🔧 Comandos úteis:"
echo "   npm run dev          - Inicia frontend + backend"
echo "   npm run dev:backend  - Apenas backend"
echo "   npm run dev:frontend - Apenas frontend"
echo "   npm run build        - Build de produção"
echo ""

