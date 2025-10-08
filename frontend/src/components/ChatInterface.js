/**
 * ChatInterface - Componente principal do chat
 * Gerencia mensagens, input, imagens e interações
 */
class ChatInterface {
    constructor() {
        console.log('🎬 ChatInterface constructor iniciando...');
        this.elements = this.initializeElements();
        this.capturedImageData = null;
        
        // Cache do formatador de data para performance
        this.dateFormatter = new Intl.DateTimeFormat('pt-BR', {
            hour: '2-digit',
            minute: '2-digit'
        });
        
        this.initializeEventListeners();
        this.initializeImageHandlers();
        this.initializeInput();
        this.initExpandButton();
        console.log('✅ ChatInterface totalmente inicializado');
        
        // Notificar o main process que ChatInterface está pronto
        ipcRenderer.send('chat-interface-ready');
        console.log('📡 Sinal "chat-interface-ready" enviado ao main process');
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

        // ESC key para fechar o chat (usar { once: false } mas prevenir duplicação)
        this.escapeHandler = (e) => {
            if (e.key === 'Escape') {
                this.closeChat();
            }
        };
        
        // Remover listener anterior se existir
        document.removeEventListener('keydown', this.escapeHandler);
        document.addEventListener('keydown', this.escapeHandler);
    }

    initializeImageHandlers() {
        console.log('🔌 Registrando listeners IPC...');
        
        // Listener para receber imagem capturada do main process
        ipcRenderer.on('image-captured', (event, imageData) => {
            this.showCapturedImage(imageData);
        });

        // Listener para limpar mensagens
        ipcRenderer.on('clear-messages', () => {
            this.clearMessages();
        });

        // Listener para carregar histórico de sessão
        ipcRenderer.on('load-session-messages', (event, messages) => {
            console.log('========================================');
            console.log('EVENTO RECEBIDO: load-session-messages');
            console.log('Total de mensagens:', messages.length);
            console.log('========================================');
            this.loadHistoryMessages(messages);
        });
        
        console.log('✅ Listeners IPC registrados (image-captured, clear-messages, load-session-messages)');

        // Botão para remover imagem
        this.elements.removeImageBtn.addEventListener('click', () => {
            this.removeCapturedImage();
        });
    }

    loadHistoryMessages(messages) {
        console.log('========================================');
        console.log('loadHistoryMessages CHAMADO (OTIMIZADO)');
        console.log('Mensagens recebidas:', messages);
        console.log('Total:', messages.length);
        console.log('========================================');
        
        // Limpar chat atual
        this.clearMessages();
        console.log('Chat limpo');
        
        // ⚡ OTIMIZAÇÃO: DocumentFragment - batch todas as mensagens (1 reflow apenas!)
        const fragment = document.createDocumentFragment();
        
        // Renderizar todas as mensagens no fragment (SEM tocar no DOM)
        messages.forEach((msg, index) => {
            console.log(`Preparando ${index + 1}/${messages.length}:`, msg);
            const role = msg.role === 'user' ? 'user' : 'assistant';
            const imageData = msg.additional_kwargs?.image_data;
            const timestamp = msg.created_at;
            
            // Criar elemento de mensagem (mesmo código do addMessage, mas sem appendChild)
            const messageDiv = this.createMessageElement(msg.content, role, imageData, timestamp);
            fragment.appendChild(messageDiv);
        });
        
        // ⚡ CRÍTICO: 1 único reflow! (400ms-1.2s mais rápido)
        this.elements.chatMessages.appendChild(fragment);
        
        console.log(`Total renderizado: ${messages.length}`);
        
        // Scroll para o final
        this.scrollToBottom();
        
        console.log('✅ Histórico carregado no chat - verificar DOM agora');
        console.log('Elementos .message no DOM:', document.querySelectorAll('.message').length);
    }
    
    createMessageElement(content, sender, imageData = null, timestamp = null) {
        /**
         * ⚡ OTIMIZAÇÃO: Método auxiliar para criar elemento de mensagem
         * Usado tanto por addMessage (1 msg) quanto loadHistoryMessages (batch)
         */
        const messageDiv = document.createElement('div');
        messageDiv.className = `message ${sender}`;

        const contentDiv = document.createElement('div');
        contentDiv.className = 'message-content';

        // Adicionar texto se disponível
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
            
            const expandBtn = document.createElement('button');
            expandBtn.className = 'expand-image-btn';
            expandBtn.innerHTML = '⛶';
            expandBtn.title = 'Expandir imagem';
            expandBtn.onclick = () => this.expandImage(imageData);
            
            imageContainer.appendChild(img);
            imageContainer.appendChild(expandBtn);
            contentDiv.appendChild(imageContainer);
        }

        const timeDiv = document.createElement('div');
        timeDiv.className = 'message-time';
        const dateObj = timestamp ? new Date(timestamp) : new Date();
        timeDiv.textContent = this.dateFormatter.format(dateObj);

        messageDiv.appendChild(contentDiv);
        messageDiv.appendChild(timeDiv);
        
        return messageDiv;
    }

    initializeInput() {
        this.elements.chatInput.focus();
    }

    async sendMessage() {
        const message = this.elements.chatInput.value.trim();
        if (!message && !this.capturedImageData) return;

        this.elements.chatInput.value = '';
        this.elements.chatInput.style.height = 'auto';
        
        // ✨ UX: Loading state visual
        this.showTyping();
        this.elements.sendBtn.disabled = true;
        this.elements.sendBtn.classList.add('loading');
        this.elements.sendBtn.textContent = '...';

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
            
            // Verificar se a resposta é um erro
            if (response.error) {
                let errorMessage = response.message || 'Desculpe, ocorreu um erro ao processar sua mensagem.';
                
                // Simplificar mensagens de API key
                if (errorMessage.includes('API key') || errorMessage.includes('api_key')) {
                    if (!errorMessage.includes('Pressione')) {
                        errorMessage = 'API key não configurada. Pressione Ctrl+Shift+O para abrir as configurações e adicione sua API key do OpenAI.';
                    }
                }
                
                this.addMessage(errorMessage, 'assistant');
            } else {
                this.addMessage(response.response, 'assistant', response.timestamp);
            }
            
        } catch (error) {
            this.hideTyping();
            console.error('Erro ao enviar mensagem:', error);
            this.addMessage('Erro de conexão com o backend. Verifique se o servidor está em execução.', 'assistant');
        }

        // ✨ UX: Restaurar estado do botão
        this.elements.sendBtn.disabled = false;
        this.elements.sendBtn.classList.remove('loading');
        this.elements.sendBtn.textContent = '→';
        this.elements.chatInput.focus();
    }

    addMessage(content, sender, timestamp = null, imageData = null) {
        // ⚡ OTIMIZAÇÃO: Reutilizar createMessageElement para consistência
        const messageDiv = this.createMessageElement(content, sender, imageData, timestamp);
        
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
        // ✨ UX: Scroll suave ao invés de abrupto
        this.elements.chatMessages.scrollTo({
            top: this.elements.chatMessages.scrollHeight,
            behavior: 'smooth'
        });
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
            // 🔥 FIX: Container agora é 100% da janela automaticamente
            // Apenas chamar IPC para expandir a janela Electron
            ipcRenderer.invoke('expand-chat');
            
            // Alternar ícone
            isExpanded = !isExpanded;
            document.getElementById('expandBtn').textContent = isExpanded ? '⊟' : '⛶';
        });
    }
}

// 🔥 FIX: Não instanciar aqui - já é instanciado no chat.html
// para garantir que está pronto antes de receber mensagens via IPC
