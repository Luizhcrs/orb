#!/bin/bash

# Installation script for ORB project
set -e

echo "🚀 Installing ORB project..."

# Check if .NET SDK is installed
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET SDK not found. Please install .NET 9.0 SDK first."
    echo "Visit: https://dotnet.microsoft.com/download"
    exit 1
fi

# Check if Python is installed
if ! command -v python3 &> /dev/null; then
    echo "❌ Python not found. Please install Python 3.11+ first."
    exit 1
fi

# Install backend dependencies
echo "🐍 Installing backend dependencies..."
cd backend
python3 -m pip install -r requirements.txt

# Create .env if it doesn't exist
if [ ! -f .env ]; then
    echo "📝 Creating .env file..."
    cp env.example .env
    
    # Generate Fernet key
    echo "🔑 Generating encryption key..."
    python3 -c "from cryptography.fernet import Fernet; print('FERNET_KEY=' + Fernet.generate_key().decode())" >> .env
fi

cd ..

# Build frontend
echo "📦 Building frontend..."
cd frontend
dotnet restore
dotnet build --configuration Release
cd ..

echo "✅ Installation completed successfully!"
echo "🎉 You can now run the project with: npm run dev"