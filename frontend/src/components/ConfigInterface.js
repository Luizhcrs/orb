const { ipcRenderer } = require('electron');

class ConfigInterface {
    constructor() {
        console.log('🔧 ConfigInterface iniciando...');
        this.elements = {};
        this.currentSection = 'general';
        this.historyApiUrl = 'http://localhost:8000/api/v1/history';
        this.configApiUrl = 'http://localhost:8000/api/v1/config';
        
        // Aguardar um pouco para garantir que o DOM está pronto
        setTimeout(() => {
            this.initializeElements();
            this.bindEvents();
            this.loadConfig();
            console.log('🔧 ConfigInterface inicializado com sucesso');
        }, 100);
    }

    initializeElements() {
        console.log('🔧 Inicializando elementos...');
        
        // Elementos de navegação
        this.elements.navItems = document.querySelectorAll('.nav-item');
        this.elements.sections = document.querySelectorAll('.config-section');
        
        console.log('🔧 Elementos encontrados:', {
            navItems: this.elements.navItems.length,
            sections: this.elements.sections.length,
            closeBtn: !!document.querySelector('.close-btn'),
            saveBtn: !!document.querySelector('#save-btn')
        });

        // Verificar se elementos foram encontrados
        if (this.elements.navItems.length === 0) {
            console.error('❌ Nenhum nav-item encontrado!');
        }
        if (this.elements.sections.length === 0) {
            console.error('❌ Nenhuma section encontrada!');
        }

        // Elementos de controle da janela
        this.elements.closeBtn = document.querySelector('.close-btn');
        this.elements.saveBtn = document.querySelector('#save-btn');
        this.elements.resetBtn = document.querySelector('#reset-btn');
        
        // Elementos Geral
        this.elements.themeSelect = document.querySelector('#theme-select');
        this.elements.languageSelect = document.querySelector('#language-select');
        this.elements.startupCheckbox = document.querySelector('#startup-checkbox');
        this.elements.keepHistoryCheckbox = document.querySelector('#keep-history-checkbox');
        
        // Elementos Agente
        this.elements.providerSelect = document.querySelector('#provider-select');
        this.elements.apiKeyInput = document.querySelector('#api-key-input');
        this.elements.modelSelect = document.querySelector('#model-select');
        
        // Elementos Histórico
        this.elements.historyList = document.querySelector('#history-list');
        this.elements.emptyState = document.querySelector('.empty-state');
    }

    bindEvents() {
        console.log('🔧 Bindando eventos...');
        
        // Navegação
        this.elements.navItems.forEach((item, index) => {
            console.log(`🔧 Binding nav item ${index}:`, item.dataset.section);
            
            // Remover event listeners existentes
            item.removeEventListener('click', this.handleNavClick);
            
            // Adicionar novo event listener
            this.handleNavClick = (e) => {
                console.log('🔧 Nav item clicado:', e.currentTarget.dataset.section);
                const section = e.currentTarget.dataset.section;
                this.switchSection(section);
            };
            
            item.addEventListener('click', this.handleNavClick);
            
            // Teste de hover
            item.addEventListener('mouseenter', () => {
                console.log('🔧 Hover em nav item:', item.dataset.section);
            });
        });

        // Botão de fechar
        if (this.elements.closeBtn) {
            console.log('🔧 Binding close button...');
            console.log('🔧 Close button element:', this.elements.closeBtn);
            
            // Remover listeners existentes
            this.elements.closeBtn.removeEventListener('click', this.handleCloseClick);
            
            // Criar novo handler
            this.handleCloseClick = (e) => {
                e.preventDefault();
                e.stopPropagation();
                console.log('🔧 Botão fechar clicado!');
                this.closeWindow();
            };
            
            this.elements.closeBtn.addEventListener('click', this.handleCloseClick);
            
            this.elements.closeBtn.addEventListener('mouseenter', () => {
                console.log('🔧 Hover no botão fechar!');
            });
            
            this.elements.closeBtn.addEventListener('mousedown', () => {
                console.log('🔧 Mouse down no botão fechar!');
            });
        } else {
            console.error('❌ Botão de fechar não encontrado!');
        }

        // Botão salvar
        if (this.elements.saveBtn) {
            this.elements.saveBtn.addEventListener('click', () => {
                console.log('🔧 Botão salvar clicado!');
                this.saveConfig();
            });
        }

        // Botão reset
        if (this.elements.resetBtn) {
            this.elements.resetBtn.addEventListener('click', () => {
                console.log('🔧 Botão reset clicado!');
                this.resetConfig();
            });
        }

        // Provedor LLM - carregar modelos quando mudar
        if (this.elements.providerSelect) {
            this.elements.providerSelect.addEventListener('change', (e) => {
                this.loadModelsForProvider(e.target.value);
            });
        }

        // Atalhos de teclado
        document.addEventListener('keydown', (e) => {
            if (e.ctrlKey) {
                switch (e.key) {
                    case 's':
                        e.preventDefault();
                        this.saveConfig();
                        break;
                    case 'r':
                        e.preventDefault();
                        this.resetConfig();
                        break;
                    case 'w':
                        e.preventDefault();
                        this.closeWindow();
                        break;
                }
            } else if (e.key === 'Escape') {
                e.preventDefault();
                this.closeWindow();
            }
        });
        
        console.log('🔧 Eventos bindados com sucesso');
    }

    closeWindow() {
        console.log('🔧 Fechando janela...');
        console.log('🔧 typeof require:', typeof require);
        
        if (typeof require !== 'undefined') {
            try {
                console.log('🔧 Enviando IPC close-config...');
                require('electron').ipcRenderer.send('close-config');
                console.log('🔧 IPC enviado com sucesso');
            } catch (error) {
                console.error('❌ Erro ao enviar IPC:', error);
                console.log('🔧 Tentando window.close()...');
                window.close();
            }
        } else {
            console.log('🔧 require não disponível, usando window.close()...');
            window.close();
        }
    }

    switchSection(sectionName) {
        console.log('🔧 SwitchSection chamado:', sectionName);
        
        // Atualizar navegação
        this.elements.navItems.forEach(item => {
            item.classList.remove('active');
            if (item.dataset.section === sectionName) {
                item.classList.add('active');
                console.log('🔧 Nav item ativado:', item.dataset.section);
            }
        });

        // Atualizar seções
        this.elements.sections.forEach(section => {
            section.classList.remove('active');
            if (section.id === `${sectionName}-section`) {
                section.classList.add('active');
                console.log('🔧 Seção ativada:', section.id);
            }
        });

        this.currentSection = sectionName;
        console.log('🔧 Seção atual:', this.currentSection);
        
        // Carregar histórico ao entrar na aba de histórico
        if (sectionName === 'history') {
            this.loadHistorySessions();
        }
    }

    loadModelsForProvider(provider) {
        console.log('🔧 Carregando modelos para provider:', provider);
        
        if (!this.elements.modelSelect) return;
        
        this.elements.modelSelect.innerHTML = '';
        
        let models = [];
        if (provider === 'openai') {
            models = [
                { value: 'gpt-4o-mini', label: 'GPT-4o Mini' },
                { value: 'gpt-4o', label: 'GPT-4o' },
                { value: 'gpt-3.5-turbo', label: 'GPT-3.5 Turbo' }
            ];
        } else if (provider === 'anthropic') {
            models = [
                { value: 'claude-3-haiku-20240307', label: 'Claude 3 Haiku' },
                { value: 'claude-3-sonnet-20240229', label: 'Claude 3 Sonnet' },
                { value: 'claude-3-opus-20240229', label: 'Claude 3 Opus' }
            ];
        }
        
        models.forEach(model => {
            const option = document.createElement('option');
            option.value = model.value;
            option.textContent = model.label;
            this.elements.modelSelect.appendChild(option);
        });
    }

    async loadConfig() {
        console.log('🔧 Carregando configurações da API...');
        
        try {
            const response = await fetch(this.configApiUrl);
            if (!response.ok) {
                throw new Error(`Erro HTTP: ${response.status}`);
            }
            
            const config = await response.json();
            console.log('🔧 Configurações recebidas:', config);
            
            // Aplicar configurações gerais
            if (config.general) {
                if (this.elements.themeSelect) {
                    this.elements.themeSelect.value = config.general.theme || 'dark';
                }
                if (this.elements.languageSelect) {
                    this.elements.languageSelect.value = config.general.language || 'pt-BR';
                }
                if (this.elements.startupCheckbox) {
                    this.elements.startupCheckbox.checked = config.general.startup || false;
                }
                if (this.elements.keepHistoryCheckbox) {
                    this.elements.keepHistoryCheckbox.checked = config.general.keep_history !== false;
                }
            }
            
            // Aplicar configurações do agente
            if (config.agent) {
                if (this.elements.providerSelect) {
                    this.elements.providerSelect.value = config.agent.provider || 'openai';
                    // Carregar modelos para o provider atual
                    this.loadModelsForProvider(config.agent.provider || 'openai');
                }
                if (this.elements.apiKeyInput) {
                    // API key vem mascarada, apenas mostrar placeholder
                    if (config.agent.api_key) {
                        this.elements.apiKeyInput.placeholder = config.agent.api_key;
                    }
                }
                if (this.elements.modelSelect) {
                    // Aguardar um pouco para modelos carregarem
                    setTimeout(() => {
                        this.elements.modelSelect.value = config.agent.model || 'gpt-4o-mini';
                    }, 100);
                }
            }
            
        } catch (error) {
            console.error('❌ Erro ao carregar configurações:', error);
            // Fallback: aplicar valores padrão
            if (this.elements.themeSelect) this.elements.themeSelect.value = 'dark';
            if (this.elements.languageSelect) this.elements.languageSelect.value = 'pt-BR';
            if (this.elements.startupCheckbox) this.elements.startupCheckbox.checked = false;
            if (this.elements.keepHistoryCheckbox) this.elements.keepHistoryCheckbox.checked = true;
            if (this.elements.providerSelect) this.elements.providerSelect.value = 'openai';
            this.loadModelsForProvider('openai');
            if (this.elements.modelSelect) {
                setTimeout(() => {
                    this.elements.modelSelect.value = 'gpt-4o-mini';
                }, 100);
            }
        }
    }

    async saveConfig() {
        console.log('🔧 Salvando configurações...');
        try {
            // Coletar dados dos campos
            const configData = {
                general: {
                    theme: this.elements.themeSelect?.value,
                    language: this.elements.languageSelect?.value,
                    startup: this.elements.startupCheckbox?.checked,
                    keep_history: this.elements.keepHistoryCheckbox?.checked
                },
                agent: {
                    provider: this.elements.providerSelect?.value,
                    api_key: this.elements.apiKeyInput?.value,
                    model: this.elements.modelSelect?.value
                }
            };
            
            console.log('🔧 Enviando configurações:', configData);
            
            // Enviar para API
            const response = await fetch(this.configApiUrl, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(configData)
            });
            
            if (!response.ok) {
                throw new Error(`Erro HTTP: ${response.status}`);
            }
            
            const result = await response.json();
            console.log('✅ Configurações salvas:', result);
            
            // Limpar campo de API key após salvar
            if (this.elements.apiKeyInput) {
                this.elements.apiKeyInput.value = '';
                this.elements.apiKeyInput.placeholder = '***' + (configData.agent.api_key?.slice(-4) || '');
            }
            
            // Mostrar feedback visual
            if (this.elements.saveBtn) {
                const originalText = this.elements.saveBtn.textContent;
                this.elements.saveBtn.textContent = 'Salvo!';
                this.elements.saveBtn.style.background = 'linear-gradient(90deg, rgba(76, 175, 80, 0.3) 0%, rgba(69, 160, 73, 0.3) 100%)';
                
                setTimeout(() => {
                    this.elements.saveBtn.textContent = originalText;
                    this.elements.saveBtn.style.background = '';
                }, 2000);
            }
            
        } catch (error) {
            console.error('❌ Erro ao salvar configurações:', error);
            
            // Mostrar feedback de erro
            if (this.elements.saveBtn) {
                const originalText = this.elements.saveBtn.textContent;
                this.elements.saveBtn.textContent = 'Erro!';
                this.elements.saveBtn.style.background = 'linear-gradient(90deg, rgba(244, 67, 54, 0.3) 0%, rgba(229, 57, 53, 0.3) 100%)';
                
                setTimeout(() => {
                    this.elements.saveBtn.textContent = originalText;
                    this.elements.saveBtn.style.background = '';
                }, 2000);
            }
        }
    }

    resetConfig() {
        console.log('🔧 Resetando configurações...');
        if (confirm('Tem certeza que deseja redefinir todas as configurações para o padrão?')) {
            this.loadConfig();
            alert('Configurações redefinidas para o padrão.');
        }
    }

    async loadHistorySessions() {
        console.log('📚 Carregando histórico de sessões...');
        
        if (!this.elements.historyList) {
            console.error('❌ historyList element não encontrado');
            return;
        }
        
        try {
            // Buscar sessões da API
            const response = await fetch(`${this.historyApiUrl}/sessions?limit=50`);
            
            if (!response.ok) {
                throw new Error(`Erro HTTP: ${response.status}`);
            }
            
            const sessions = await response.json();
            console.log('📊 Sessões carregadas:', sessions.length);
            
            // Limpar lista atual
            this.elements.historyList.innerHTML = '';
            
            if (sessions.length === 0) {
                // Mostrar empty state
                if (this.elements.emptyState) {
                    this.elements.emptyState.style.display = 'block';
                }
            } else {
                // Esconder empty state
                if (this.elements.emptyState) {
                    this.elements.emptyState.style.display = 'none';
                }
                
                // Renderizar sessões
                sessions.forEach(session => {
                    this.renderHistoryItem(session);
                });
            }
        } catch (error) {
            console.error('❌ Erro ao carregar histórico:', error);
            
            // Mostrar erro na interface
            this.elements.historyList.innerHTML = `
                <div class="error-message">
                    <p>❌ Erro ao carregar histórico</p>
                    <p class="error-hint">${error.message}</p>
                    <button onclick="location.reload()" class="action-btn">Tentar Novamente</button>
                </div>
            `;
        }
    }

    renderHistoryItem(session) {
        const item = document.createElement('div');
        item.className = 'history-item';
        item.dataset.sessionId = session.session_id;
        
        // Formatar data
        const date = this.formatDate(session.updated_at);
        
        // Criar preview (usaremos as primeiras mensagens depois)
        const preview = session.title || 'Nova Conversa';
        
        item.innerHTML = `
            <div class="history-header">
                <span class="history-date">${date}</span>
                <button class="history-delete-btn" onclick="event.stopPropagation(); configInterface.deleteHistorySession('${session.session_id}')">×</button>
            </div>
            <div class="history-title">${preview}</div>
            <div class="history-meta">${session.message_count} mensagen${session.message_count !== 1 ? 's' : ''}</div>
        `;
        
        // Click para abrir sessão (futuro: abrir no chat)
        item.addEventListener('click', () => {
            this.openHistorySession(session.session_id);
        });
        
        this.elements.historyList.appendChild(item);
    }

    formatDate(dateString) {
        const date = new Date(dateString);
        const now = new Date();
        const diff = now.getTime() - date.getTime();
        const days = Math.floor(diff / (1000 * 60 * 60 * 24));
        
        const timeStr = date.toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' });
        
        if (days === 0) {
            return `Hoje, ${timeStr}`;
        } else if (days === 1) {
            return `Ontem, ${timeStr}`;
        } else if (days < 7) {
            const weekday = date.toLocaleDateString('pt-BR', { weekday: 'long' });
            return `${weekday}, ${timeStr}`;
        } else {
            return date.toLocaleDateString('pt-BR', { day: '2-digit', month: 'short' }) + `, ${timeStr}`;
        }
    }

    async deleteHistorySession(sessionId) {
        if (!confirm('Tem certeza que deseja deletar esta conversa?')) {
            return;
        }
        
        try {
            const response = await fetch(`${this.historyApiUrl}/sessions/${sessionId}`, {
                method: 'DELETE'
            });
            
            if (!response.ok) {
                throw new Error('Erro ao deletar sessão');
            }
            
            console.log('✅ Sessão deletada:', sessionId);
            
            // Recarregar lista
            this.loadHistorySessions();
        } catch (error) {
            console.error('❌ Erro ao deletar sessão:', error);
            alert('Erro ao deletar conversa.');
        }
    }

    async openHistorySession(sessionId) {
        console.log('📖 Abrindo sessão:', sessionId);
        
        try {
            // Carregar histórico via IPC
            const result = await require('electron').ipcRenderer.invoke('load-session-history', sessionId);
            
            if (!result.success) {
                throw new Error(result.error || 'Erro ao carregar sessão');
            }
            
            console.log('✅ Sessão carregada:', result.messages.length, 'mensagens');
            
            // Fechar janela de configuração
            this.closeWindow();
            
            // Aguardar um pouco e então abrir o chat com histórico
            setTimeout(() => {
                // O chat será aberto automaticamente pelo main process
                // que enviará 'load-session-messages' para o ChatInterface
                console.log('💬 Chat será aberto com histórico da sessão');
            }, 100);
            
        } catch (error) {
            console.error('❌ Erro ao abrir sessão:', error);
            alert('Erro ao abrir conversa: ' + error.message);
        }
    }
}

// Variável global para acesso aos métodos
let configInterface;

// Inicialização mais robusta
function initializeConfig() {
    console.log('🔧 Inicializando ConfigInterface...');
    try {
        configInterface = new ConfigInterface();
    } catch (error) {
        console.error('❌ Erro ao inicializar ConfigInterface:', error);
        // Tentar novamente após um delay
        setTimeout(() => {
            console.log('🔧 Tentando novamente...');
            configInterface = new ConfigInterface();
        }, 500);
    }
}

// Múltiplas formas de inicializar
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initializeConfig);
} else {
    initializeConfig();
}

// Fallback adicional
setTimeout(initializeConfig, 1000);