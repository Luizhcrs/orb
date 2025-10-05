#!/bin/bash

# Script de instalação do Orb Agent

echo "🚀 Instalando Orb Agent..."

# Verificar se Node.js está instalado
if ! command -v node &> /dev/null; then
    echo "❌ Node.js não encontrado. Por favor, instale Node.js 18+ primeiro."
    echo "   Download: https://nodejs.org/"
    exit 1
fi

# Verificar versão do Node.js
NODE_VERSION=$(node -v | cut -d'v' -f2 | cut -d'.' -f1)
if [ "$NODE_VERSION" -lt 18 ]; then
    echo "❌ Node.js versão 18+ é necessária. Versão atual: $(node -v)"
    exit 1
fi

echo "✅ Node.js $(node -v) encontrado"

# Instalar dependências
echo "📦 Instalando dependências..."
npm install

if [ $? -ne 0 ]; then
    echo "❌ Erro ao instalar dependências"
    exit 1
fi

# Criar arquivo .env se não existir
if [ ! -f .env ]; then
    echo "📝 Criando arquivo .env..."
    cp env.example .env
    echo "⚠️  Configure suas chaves de API no arquivo .env"
fi

# Build do projeto
echo "🔨 Compilando projeto..."
npm run build

if [ $? -ne 0 ]; then
    echo "❌ Erro ao compilar projeto"
    exit 1
fi

echo "✅ Orb Agent instalado com sucesso!"
echo ""
echo "🎮 Para executar:"
echo "   npm start"
echo ""
echo "🛠️  Para desenvolvimento:"
echo "   npm run dev"
echo ""
echo "⚙️  Configure suas chaves de API no arquivo .env para usar o assistente AI"
