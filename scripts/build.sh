#!/bin/bash

# Build script for ORB project
set -e

echo "🔨 Building ORB project..."

# Build frontend (WPF)
echo "📦 Building frontend..."
cd frontend
dotnet restore
dotnet build --configuration Release
cd ..

# Build backend
echo "🐍 Building backend..."
cd backend
python -m pip install -r requirements.txt
cd ..

echo "✅ Build completed successfully!"