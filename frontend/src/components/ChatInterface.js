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

        // Listener para limpar mensagens
        ipcRenderer.on('clear-messages', () => {
            this.clearMessages();
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

        this.elements.chatInput.value = '';
        this.elements.chatInput.style.height = 'auto';
        
        this.showTyping();
        this.elements.sendBtn.disabled = true;

        try {
            // Salvar imagem antes de limpar o preview
            const imageToSend = this.capturedImageData;
            
            // Adicionar mensagem do usuário com imagem (se houver) ANTES de enviar
            this.addMessage(message, 'user', new Date().toISOString(), imageToSend);
            
            // Limpar imagem capturada após adicionar à mensagem
            if (this.capturedImageData) {
                this.removeCapturedImage();
            }
            
            const response = await ipcRenderer.invoke('send-message', message, imageToSend);
            
            this.hideTyping();
            this.addMessage(response.response, 'assistant', response.timestamp);
            
        } catch (error) {
            this.hideTyping();
            this.addMessage('Desculpe, ocorreu um erro ao processar sua mensagem.', 'assistant');
        }

        this.elements.sendBtn.disabled = false;
        this.elements.chatInput.focus();
    }

    addMessage(content, sender, timestamp = null, imageData = null) {
        const messageDiv = document.createElement('div');
        messageDiv.className = `message ${sender}`;

        const contentDiv = document.createElement('div');
        contentDiv.className = 'message-content';
        
        // Adicionar texto da mensagem
        if (content) {
            const textDiv = document.createElement('div');
            textDiv.textContent = content;
            contentDiv.appendChild(textDiv);
        }
        
        // Adicionar imagem se disponível
        if (imageData) {
            const imageContainer = document.createElement('div');
            imageContainer.className = 'message-image-container';
            
            const img = document.createElement('img');
            img.src = `data:image/png;base64,${imageData}`;
            img.className = 'message-image';
            img.alt = 'Imagem da mensagem';
            
            // Adicionar botão para expandir imagem
            const expandBtn = document.createElement('button');
            expandBtn.className = 'expand-image-btn';
            expandBtn.innerHTML = '🔍';
            expandBtn.title = 'Expandir imagem';
            expandBtn.onclick = () => this.expandImage(imageData);
            
            imageContainer.appendChild(img);
            imageContainer.appendChild(expandBtn);
            contentDiv.appendChild(imageContainer);
        }

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

    clearMessages() {
        // Limpa todas as mensagens da interface, exceto o typing indicator
        const messages = this.elements.chatMessages.querySelectorAll('.message');
        messages.forEach(message => message.remove());
        console.log('🧹 Mensagens da interface limpas');
    }

    expandImage(imageData) {
        // Criar modal para expandir imagem
        const modal = document.createElement('div');
        modal.className = 'image-modal';
        modal.innerHTML = `
            <div class="modal-content">
                <span class="close-modal">&times;</span>
                <img src="data:image/png;base64,${imageData}" alt="Imagem expandida" class="expanded-image">
            </div>
        `;
        
        document.body.appendChild(modal);
        
        // Fechar modal ao clicar no X ou fora da imagem
        const closeBtn = modal.querySelector('.close-modal');
        closeBtn.onclick = () => modal.remove();
        modal.onclick = (e) => {
            if (e.target === modal) modal.remove();
        };
        
        console.log('🔍 Imagem expandida');
    }

    scrollToBottom() {
        this.elements.chatMessages.scrollTop = this.elements.chatMessages.scrollHeight;
    }

    closeChat() {
        ipcRenderer.invoke('close-chat');
    }

    showCapturedImage(imageData) {
        this.capturedImageData = imageData;
        this.elements.capturedImage.src = `data:image/png;base64,${imageData}`;
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
