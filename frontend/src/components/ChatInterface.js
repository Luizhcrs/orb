/**
 * ChatInterface - Componente principal do chat
 * Gerencia mensagens, input, imagens e interações
 */
class ChatInterface {
    constructor() {
        this.elements = this.initializeElements();
        this.capturedImageData = null;
        
        this.initializeEventListeners();
        this.initializeImageHandlers();
        this.initializeInput();
        this.initExpandButton();
    }

    initializeElements() {
        return {
            chatMessages: document.getElementById('chatMessages'),
            chatInput: document.getElementById('chatInput'),
            sendBtn: document.getElementById('sendBtn'),
            closeBtn: document.getElementById('closeBtn'),
            typingIndicator: document.getElementById('typingIndicator'),
            capturedImageContainer: document.getElementById('capturedImageContainer'),
            capturedImage: document.getElementById('capturedImage'),
            removeImageBtn: document.getElementById('removeImageBtn')
        };
    }

    initializeEventListeners() {
        this.elements.sendBtn.addEventListener('click', () => this.sendMessage());
        this.elements.closeBtn.addEventListener('click', () => this.closeChat());
        
        this.elements.chatInput.addEventListener('keydown', (e) => {
            if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                this.sendMessage();
            }
        });

        // Auto-resize textarea
        this.elements.chatInput.addEventListener('input', () => {
            this.elements.chatInput.style.height = 'auto';
            this.elements.chatInput.style.height = Math.min(this.elements.chatInput.scrollHeight, 100) + 'px';
        });

        // ESC key para fechar o chat
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape') {
                this.closeChat();
            }
        });
    }

    initializeImageHandlers() {
        // Listener para receber imagem capturada do main process
        ipcRenderer.on('image-captured', (event, imageData) => {
            this.showCapturedImage(imageData);
        });

        // Botão para remover imagem
        this.elements.removeImageBtn.addEventListener('click', () => {
            this.removeCapturedImage();
        });
    }

    initializeInput() {
        this.elements.chatInput.focus();
    }

    async sendMessage() {
        const message = this.elements.chatInput.value.trim();
        if (!message && !this.capturedImageData) return;

        this.addMessage(message, 'user');
        this.elements.chatInput.value = '';
        this.elements.chatInput.style.height = 'auto';
        
        this.showTyping();
        this.elements.sendBtn.disabled = true;

        try {
            const response = await ipcRenderer.invoke('send-message', message, this.capturedImageData);
            this.hideTyping();
            this.addMessage(response.response, 'assistant', response.timestamp);
            
            // Limpar imagem após envio
            if (this.capturedImageData) {
                this.removeCapturedImage();
            }
        } catch (error) {
            this.hideTyping();
            this.addMessage('Desculpe, ocorreu um erro ao processar sua mensagem.', 'assistant');
        }

        this.elements.sendBtn.disabled = false;
        this.elements.chatInput.focus();
    }

    addMessage(content, sender, timestamp = null) {
        const messageDiv = document.createElement('div');
        messageDiv.className = `message ${sender}`;

        const contentDiv = document.createElement('div');
        contentDiv.className = 'message-content';
        contentDiv.textContent = content;

        const timeDiv = document.createElement('div');
        timeDiv.className = 'message-time';
        timeDiv.textContent = timestamp ? 
            new Date(timestamp).toLocaleTimeString('pt-BR', { 
                hour: '2-digit', 
                minute: '2-digit' 
            }) : 
            new Date().toLocaleTimeString('pt-BR', { 
                hour: '2-digit', 
                minute: '2-digit' 
            });

        messageDiv.appendChild(contentDiv);
        messageDiv.appendChild(timeDiv);
        
        // Inserir antes do typing indicator se ele estiver visível
        if (this.elements.typingIndicator.classList.contains('active')) {
            this.elements.chatMessages.insertBefore(messageDiv, this.elements.typingIndicator);
        } else {
            this.elements.chatMessages.appendChild(messageDiv);
        }

        this.scrollToBottom();
    }

    showTyping() {
        // Mover o typing indicator para o final da lista de mensagens
        this.elements.chatMessages.appendChild(this.elements.typingIndicator);
        this.elements.typingIndicator.classList.add('active');
        this.scrollToBottom();
    }

    hideTyping() {
        this.elements.typingIndicator.classList.remove('active');
    }

    scrollToBottom() {
        this.elements.chatMessages.scrollTop = this.elements.chatMessages.scrollHeight;
    }

    closeChat() {
        ipcRenderer.invoke('close-chat');
    }

    showCapturedImage(imageData) {
        this.capturedImageData = imageData;
        this.elements.capturedImage.src = imageData;
        this.elements.capturedImageContainer.style.display = 'block';
        this.scrollToBottom();
        console.log('📸 Imagem capturada exibida no chat');
    }

    removeCapturedImage() {
        this.capturedImageData = null;
        this.elements.capturedImageContainer.style.display = 'none';
        this.elements.capturedImage.src = '';
        ipcRenderer.invoke('clear-captured-image');
        console.log('🗑️ Imagem capturada removida');
    }

    initExpandButton() {
        let isExpanded = false;
        document.getElementById('expandBtn').addEventListener('click', () => {
            // Alternar entre tamanho compacto e expandido
            ipcRenderer.invoke('expand-chat');
            
            // Ajustar altura do container
            const chatContainer = document.querySelector('.chat-container');
            
            if (!isExpanded) {
                chatContainer.style.height = '600px';
                document.getElementById('expandBtn').textContent = '⊟';
                isExpanded = true;
            } else {
                chatContainer.style.height = '440px';
                document.getElementById('expandBtn').textContent = '⛶';
                isExpanded = false;
            }
        });
    }
}

// Inicializar quando o DOM estiver carregado
document.addEventListener('DOMContentLoaded', () => {
    new ChatInterface();
});
