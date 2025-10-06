# Dockerfile para ORB Frontend (Node.js/Electron) - Desenvolvimento
FROM node:18-slim

# Instalar dependências do sistema para Electron
RUN apt-get update && apt-get install -y \
    libnss3 \
    libatk-bridge2.0-0 \
    libdrm2 \
    libxkbcommon0 \
    libxcomposite1 \
    libxdamage1 \
    libxrandr2 \
    libgbm1 \
    libxss1 \
    libasound2 \
    && rm -rf /var/lib/apt/lists/*

# Definir diretório de trabalho
WORKDIR /app

# Copiar package.json primeiro (para cache de layers)
COPY package*.json ./

# Instalar dependências
RUN npm ci

# Copiar código fonte
COPY src/ ./src/
COPY tsconfig.json ./
COPY shared/ ./shared/

# Expor porta (para desenvolvimento com hot reload)
EXPOSE 3000

# Comando padrão para desenvolvimento
CMD ["npm", "run", "dev"]
