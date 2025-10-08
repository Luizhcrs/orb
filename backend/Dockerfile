# Dockerfile para Backend ORB
FROM python:3.11-slim

# Definir diretório de trabalho
WORKDIR /app

# Instalar dependências do sistema
RUN apt-get update && apt-get install -y \
    gcc \
    python3-dev \
    && rm -rf /var/lib/apt/lists/*

# Copiar requirements
COPY requirements.txt .

# Instalar dependências Python
RUN pip install --no-cache-dir --upgrade pip && \
    pip install --no-cache-dir -r requirements.txt

# Copiar código do backend
COPY . .

# Criar diretório para banco de dados
RUN mkdir -p /app/data

# Expor porta
EXPOSE 8000

# Comando padrão (será sobrescrito pelo docker-compose)
CMD ["python", "scripts/dev.py"]
