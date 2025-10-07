const { ipcRenderer } = require('electron');

class ConfigInterface {
    constructor() {
        console.log('🔧 ConfigInterface iniciando...');
        this.elements = {};
        this.currentSection = 'general';
        
        // Aguardar um pouco para garantir que o DOM está pronto
        setTimeout(() => {
            this.initializeElements();
            this.bindEvents();
            this.loadConfig();
            this.updateSliderValues();
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
        
        // Outros elementos
        this.elements.temperatureSlider = document.querySelector('#temperature-slider');
        this.elements.temperatureValue = document.querySelector('#temperature-value');
        this.elements.maxTokensSlider = document.querySelector('#max-tokens-slider');
        this.elements.maxTokensValue = document.querySelector('#max-tokens-value');
        this.elements.orbSizeSlider = document.querySelector('#orb-size-slider');
        this.elements.orbSizeValue = document.querySelector('#orb-size-value');
        this.elements.chatOpacitySlider = document.querySelector('#chat-opacity-slider');
        this.elements.chatOpacityValue = document.querySelector('#chat-opacity-value');
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

        // Sliders
        if (this.elements.temperatureSlider) {
            this.elements.temperatureSlider.addEventListener('input', () => this.updateSliderValues());
        }
        if (this.elements.maxTokensSlider) {
            this.elements.maxTokensSlider.addEventListener('input', () => this.updateSliderValues());
        }
        if (this.elements.orbSizeSlider) {
            this.elements.orbSizeSlider.addEventListener('input', () => this.updateSliderValues());
        }
        if (this.elements.chatOpacitySlider) {
            this.elements.chatOpacitySlider.addEventListener('input', () => this.updateSliderValues());
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
    }

    updateSliderValues() {
        if (this.elements.temperatureValue && this.elements.temperatureSlider) {
            this.elements.temperatureValue.textContent = this.elements.temperatureSlider.value;
        }
        if (this.elements.maxTokensValue && this.elements.maxTokensSlider) {
            this.elements.maxTokensValue.textContent = this.elements.maxTokensSlider.value;
        }
        if (this.elements.orbSizeValue && this.elements.orbSizeSlider) {
            this.elements.orbSizeValue.textContent = `${this.elements.orbSizeSlider.value}px`;
        }
        if (this.elements.chatOpacityValue && this.elements.chatOpacitySlider) {
            this.elements.chatOpacityValue.textContent = `${this.elements.chatOpacitySlider.value}%`;
        }
    }

    async loadConfig() {
        console.log('🔧 Carregando configurações...');
        // Configurações padrão
        const config = {
            general: {
                theme: 'system',
                language: 'pt-BR',
                startup: false,
                notifications: true
            },
            agent: {
                provider: 'openai',
                model: 'gpt-4o-mini',
                temperature: 0.7,
                maxTokens: 1000
            },
            interface: {
                orbSize: 50,
                chatOpacity: 90,
                alwaysOnTop: false,
                autoHideOrb: true
            },
            history: {
                saveHistory: true,
                historyDays: '30'
            }
        };

        // Aplicar configurações aos elementos da UI
        if (this.elements.temperatureSlider) this.elements.temperatureSlider.value = config.agent.temperature;
        if (this.elements.maxTokensSlider) this.elements.maxTokensSlider.value = config.agent.maxTokens;
        if (this.elements.orbSizeSlider) this.elements.orbSizeSlider.value = config.interface.orbSize;
        if (this.elements.chatOpacitySlider) this.elements.chatOpacitySlider.value = config.interface.chatOpacity;

        this.updateSliderValues();
        console.log('🔧 Configurações carregadas');
    }

    async saveConfig() {
        console.log('🔧 Salvando configurações...');
        try {
            // Simular salvamento
            await new Promise(resolve => setTimeout(resolve, 500));
            
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
            console.error('Erro ao salvar configurações:', error);
        }
    }

    resetConfig() {
        console.log('🔧 Resetando configurações...');
        if (confirm('Tem certeza que deseja redefinir todas as configurações para o padrão?')) {
            this.loadConfig();
            alert('Configurações redefinidas para o padrão.');
        }
    }
}

// Inicialização mais robusta
function initializeConfig() {
    console.log('🔧 Inicializando ConfigInterface...');
    try {
        new ConfigInterface();
    } catch (error) {
        console.error('❌ Erro ao inicializar ConfigInterface:', error);
        // Tentar novamente após um delay
        setTimeout(() => {
            console.log('🔧 Tentando novamente...');
            new ConfigInterface();
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