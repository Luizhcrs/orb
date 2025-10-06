# ORB Backend - Serviço Windows

Este documento explica como instalar e gerenciar o ORB Backend como um serviço Windows.

## 🎯 **Por que usar como Serviço Windows?**

- **Sempre Disponível**: Inicia automaticamente com o Windows
- **Execução em Background**: Roda sem interface gráfica
- **Confiabilidade**: Restart automático em caso de falha
- **Integração**: Melhor acesso a recursos do sistema
- **Persistência**: Continua rodando após logout
- **Logs Centralizados**: Integração com Event Viewer

## 📋 **Pré-requisitos**

1. **Python 3.8+** instalado
2. **Dependências instaladas**:
   ```bash
   pip install -r requirements.txt
   ```

## 🚀 **Instalação do Serviço**

### Método 1: Script Automático (Recomendado)

```bash
# Navegue para o diretório do backend
cd backend

# Execute o instalador
python scripts/install_service.py
```

### Método 2: Comando Direto

```bash
# Instalar serviço
python src/services/windows_service.py install

# Iniciar serviço
python scripts/start_service.py
```

## 🎮 **Gerenciamento do Serviço**

### Scripts de Controle

```bash
# Iniciar serviço
python scripts/start_service.py

# Parar serviço
python scripts/stop_service.py

# Gerenciador interativo
python scripts/service_manager.py

# Remover serviço
python scripts/uninstall_service.py
```

### Comandos Windows

```cmd
# Iniciar serviço
net start ORBBackend

# Parar serviço
net stop ORBBackend

# Ver status
sc query ORBBackend
```

### Via Services.msc

1. Pressione `Win + R`
2. Digite `services.msc`
3. Procure por "ORB Backend API Service"
4. Clique com botão direito para gerenciar

## 📊 **Monitoramento**

### Logs do Event Viewer

1. Pressione `Win + R`
2. Digite `eventvwr.msc`
3. Navegue para: `Windows Logs > Application`
4. Procure por eventos com fonte "ORB Backend"

### Teste da API

```bash
# Verificar se está funcionando
curl http://localhost:8000/health/

# Ou acesse no navegador
http://localhost:8000/docs
```

## 🔧 **Configuração**

### Variáveis de Ambiente

Crie um arquivo `.env` na raiz do backend:

```env
# LLM Providers
OPENAI_API_KEY=your_openai_api_key_here
ANTHROPIC_API_KEY=your_anthropic_api_key_here

# Configurações do serviço
HOST=0.0.0.0
PORT=8000
DEBUG=false

# CORS settings
CORS_ORIGINS=http://localhost:3000
```

### Configurações do Serviço

O serviço é configurado com:
- **Nome**: `ORBBackend`
- **Nome de Exibição**: `ORB Backend API Service`
- **Descrição**: `Serviço backend para o assistente ORB - Assistente de IA flutuante`
- **Tipo**: `Automático` (inicia com o Windows)

## 🛠️ **Desenvolvimento vs Produção**

### Modo Desenvolvimento

```bash
# Executa normalmente (não como serviço)
python main.py
```

### Modo Serviço

```bash
# Instala e inicia como serviço
python scripts/install_service.py
python scripts/start_service.py
```

## 🐛 **Solução de Problemas**

### Serviço não inicia

1. **Verifique logs** no Event Viewer
2. **Verifique permissões** - execute como Administrador
3. **Verifique dependências**:
   ```bash
   pip install pywin32
   ```

### API não responde

1. **Verifique se o serviço está rodando**:
   ```cmd
   sc query ORBBackend
   ```

2. **Teste a conexão**:
   ```bash
   curl http://localhost:8000/health/
   ```

3. **Verifique firewall** - porta 8000 deve estar liberada

### Erro de permissão

1. **Execute como Administrador**
2. **Verifique se o usuário tem permissão** para instalar serviços

### Reinstalar serviço

```bash
# Remover serviço existente
python scripts/uninstall_service.py

# Instalar novamente
python scripts/install_service.py
```

## 📱 **Integração com Frontend**

### WebSocket Connection

```javascript
// Conecta ao WebSocket do serviço
const ws = new WebSocket('ws://localhost:8000/ws');

ws.onopen = () => {
    console.log('Conectado ao ORB Backend');
};

ws.onmessage = (event) => {
    const data = JSON.parse(event.data);
    console.log('Mensagem recebida:', data);
};
```

### API REST

```javascript
// Enviar mensagem para o agente
const response = await fetch('http://localhost:8000/agent/message', {
    method: 'POST',
    headers: {
        'Content-Type': 'application/json',
    },
    body: JSON.stringify({
        message: 'Olá, como você pode me ajudar?',
        session_id: 'user-session-123'
    })
});

const data = await response.json();
console.log('Resposta do agente:', data);
```

## 🔄 **Atualizações**

Para atualizar o serviço:

1. **Pare o serviço**:
   ```bash
   python scripts/stop_service.py
   ```

2. **Atualize o código**

3. **Reinicie o serviço**:
   ```bash
   python scripts/start_service.py
   ```

## 🗑️ **Desinstalação**

```bash
# Parar serviço
python scripts/stop_service.py

# Remover serviço
python scripts/uninstall_service.py
```

## 📞 **Suporte**

- **Logs**: Event Viewer > Windows Logs > Application
- **Status**: `sc query ORBBackend`
- **Teste**: http://localhost:8000/health/
- **Documentação**: http://localhost:8000/docs
