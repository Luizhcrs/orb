# Release Notes - ORB Agent

## v1.0.0 - Primeira Release Pública (2025-10-10)

### 🎉 Destaques

Primeira versão pública do ORB Agent! Um assistente de IA flutuante para Windows com design moderno "liquid glass" e integração completa com OpenAI GPT.

### ✨ Funcionalidades

#### Interface
- **ORB Flutuante**: Orbe animado que aparece no hot corner (canto superior esquerdo)
- **Chat com Liquid Glass**: Interface de chat com efeito glassmorphism
- **Tela de Configurações**: Gerenciamento completo de configurações
- **System Tray**: Ícone na bandeja do sistema com menu contextual

#### Funcionalidades do Chat
- Conversas com OpenAI GPT-4o e GPT-4o-mini
- Envio de screenshots para análise visual
- Histórico de conversas persistente
- Cada chat é uma sessão individual
- Carregamento e retomada de conversas antigas

#### Atalhos Globais
- `Ctrl+Shift+Space`: Abrir chat
- `Ctrl+Shift+O`: Abrir configurações
- `Ctrl+Shift+S`: Capturar e enviar screenshot

#### Configurações
- **Geral**: Tema (dark) e idioma (PT-BR/EN)
- **Agente**: Configuração de API key OpenAI
- **Histórico**: Visualizar, retomar e excluir conversas antigas

### 🔒 Privacidade e Segurança

- **Armazenamento Local**: Todas as conversas e configurações ficam no seu computador
- **Criptografia**: API keys são criptografadas usando Fernet (AES-256)
- **Banco de Dados**: SQLite local em `%APPDATA%\OrbAgent\data\orb.db`
- **Sem Telemetria**: Nenhum dado é enviado para servidores externos (exceto OpenAI para LLM)

### 🚀 Instalação e Deploy

- **Instalador Único**: Um arquivo `.exe` de ~114 MB
- **Zero Dependências**: Instalador inclui .NET 9 Runtime e Python FastAPI
- **Backend Integrado**: Backend Python roda como processo gerenciado pelo frontend
- **Lifecycle Management**: Backend fecha automaticamente quando o Orb é fechado

### 📦 O que está incluído

- Frontend WPF com .NET 9.0 Runtime embutido
- Backend Python com FastAPI, SQLite e dependências
- Ícone personalizado do Orb
- Atalho na Área de Trabalho
- Atalhos no Menu Iniciar

### 🐛 Problemas Conhecidos

1. **Performance**: Primeira mensagem pode demorar ~2-3 segundos (inicialização do LLM)
2. **Screenshot**: Imagens grandes podem levar alguns segundos para enviar
3. **Windows 10**: Efeito Acrylic pode não funcionar em versões antigas do Windows 10

### 📋 Notas de Upgrade

Como esta é a primeira versão pública, não há processo de upgrade. Instalações futuras preservarão:
- Banco de dados em `%APPDATA%\OrbAgent\data\`
- Chave de criptografia em `%APPDATA%\OrbAgent\data\.encryption_key`
- Histórico de conversas
- Configurações salvas

### 🔄 Próximos Passos

Planejado para v1.1.0:
- Suporte para Anthropic Claude
- Melhorias de performance
- Temas customizáveis
- Backup e restauração de histórico

---

**Notas Técnicas**

- **Frontend**: WPF (.NET 9.0) com design liquid glass
- **Backend**: FastAPI (Python 3.11) empacotado com PyInstaller
- **Banco de Dados**: SQLite com schema LangChain-compatible
- **Criptografia**: Fernet (AES-256) para API keys
- **Build**: Inno Setup 6 para instalador Windows

**Tamanhos**
- Instalador: ~114 MB
- Instalado: ~240 MB
- Runtime: Frontend ~40 MB RAM, Backend ~80 MB RAM

**Compatibilidade**
- Windows 10 (build 19041+) / Windows 11
- x64 only
- Requer ~300 MB de espaço em disco

