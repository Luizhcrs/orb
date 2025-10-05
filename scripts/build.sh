#!/bin/bash

# Script de build para distribuição do Orb Agent

echo "🔨 Building Orb Agent for distribution..."

# Limpar builds anteriores
echo "🧹 Limpando builds anteriores..."
npm run clean

# Instalar dependências
echo "📦 Instalando dependências..."
npm install

# Build TypeScript
echo "🔧 Compilando TypeScript..."
npm run build

if [ $? -ne 0 ]; then
    echo "❌ Erro ao compilar TypeScript"
    exit 1
fi

# Criar executáveis
echo "📦 Criando executáveis..."
npm run electron:pack

if [ $? -ne 0 ]; then
    echo "❌ Erro ao criar executáveis"
    exit 1
fi

echo "✅ Build concluído com sucesso!"
echo ""
echo "📁 Executáveis disponíveis em: release/"
echo ""
echo "🚀 Para testar localmente:"
echo "   npm start"
