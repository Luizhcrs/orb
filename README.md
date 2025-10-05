# 🌟 Orb - Assistente AI Flutuante

[![CI/CD](https://github.com/SEU_USUARIO/orb/actions/workflows/ci.yml/badge.svg)](https://github.com/SEU_USUARIO/orb/actions/workflows/ci.yml)
[![GitHub issues](https://img.shields.io/github/issues/SEU_USUARIO/orb)](https://github.com/SEU_USUARIO/orb/issues)
[![GitHub stars](https://img.shields.io/github/stars/SEU_USUARIO/orb)](https://github.com/SEU_USUARIO/orb/stargazers)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

Um assistente de inteligência artificial flutuante e minimalista para desktop, construído com Electron e TypeScript.

## ✨ Características

- **Orb Flutuante**: Interface minimalista que flutua sobre todas as janelas
- **Sempre Visível**: Permanece no topo de todas as aplicações
- **Chat Intuitivo**: Interface de chat moderna que aparece ao clicar no orb
- **Integração LLM**: Suporte para OpenAI GPT e Anthropic Claude
- **Atalhos Globais**: Acesso rápido via teclado
- **Multiplataforma**: Windows, macOS e Linux

## 🚀 Instalação

### Pré-requisitos
- Node.js 18+ 
- npm ou yarn

### Setup
```bash
# Clone o repositório
git clone <seu-repo>
cd orb

# Instale as dependências
npm install

# Configure as variáveis de ambiente
cp env.example .env
# Edite o .env com suas chaves de API
```

## 🔧 Configuração

Edite o arquivo `.env` com suas configurações:

```env
# OpenAI
OPENAI_API_KEY=sua_chave_openai_aqui

# Anthropic Claude  
ANTHROPIC_API_KEY=sua_chave_anthropic_aqui

# Modelo padrão
DEFAULT_MODEL=gpt-3.5-turbo
```

## 🎮 Como Usar

### Desenvolvimento
```bash
npm run dev
```

### Produção
```bash
npm run build
npm start
```

### Atalhos Globais
- `Ctrl+Shift+O`: Mostrar/ocultar orb
- `Ctrl+Shift+C`: Abrir/fechar chat
- **Clique no orb**: Abrir chat

## 🏗️ Arquitetura

### Estrutura do Projeto
```
src/
├── main.ts          # Processo principal do Electron
├── orb.html         # Interface do orb flutuante
└── chat.html        # Interface do chat
```

### Componentes Principais
- **OrbApp**: Classe principal que gerencia as janelas
- **Orb Window**: Janela flutuante transparente
- **Chat Window**: Interface de conversação
- **LLM Integration**: Processamento de mensagens via API

## 🎨 Personalização

### Posição do Orb
Edite `src/main.ts` para alterar a posição padrão:
```typescript
x: width - 100,  // Posição X
y: 50,           // Posição Y
```

### Cores e Estilo
Modifique `src/orb.html` para personalizar a aparência do orb:
```css
background: radial-gradient(circle at 30% 30%, #290060, #1a0038);
```

## 📦 Build e Distribuição

```bash
# Build para produção
npm run build

# Criar executável
npm run electron:pack
```

Os executáveis serão gerados na pasta `release/`.

## 🤝 Contribuição

1. Fork o projeto
2. Crie uma branch para sua feature (`git checkout -b feature/nova-feature`)
3. Commit suas mudanças (`git commit -m 'feat: adiciona nova feature'`)
4. Push para a branch (`git push origin feature/nova-feature`)
5. Abra um Pull Request

## 🤝 Contribuindo

Contribuições são bem-vindas! Por favor:

1. **Fork** o projeto
2. **Crie** uma branch para sua feature (`git checkout -b feature/nova-funcionalidade`)
3. **Commit** suas mudanças (`git commit -m 'feat: adicionar nova funcionalidade'`)
4. **Push** para a branch (`git push origin feature/nova-funcionalidade`)
5. **Abra** um Pull Request

Veja nosso [Guia de Contribuição](.github/CONTRIBUTING.md) para mais detalhes.

### 🏷️ Labels Disponíveis
- `bug`: Problemas no código
- `enhancement`: Novas funcionalidades  
- `question`: Perguntas e dúvidas
- `help-wanted`: Precisa de ajuda
- `good-first-issue`: Boa para iniciantes

## 🐛 Reportar Bugs

Use nosso [template de bug report](.github/ISSUE_TEMPLATE/bug_report.md) para reportar problemas.

## ✨ Sugerir Features

Use nosso [template de feature request](.github/ISSUE_TEMPLATE/feature_request.md) para sugerir novas funcionalidades.

## 📞 Suporte

Para dúvidas, sugestões ou problemas:
- 📋 [Issues](https://github.com/SEU_USUARIO/orb/issues)
- 💬 [Discussions](https://github.com/SEU_USUARIO/orb/discussions)
- 📧 Email: [seu-email@exemplo.com]

## 📄 Licença

Este projeto está sob a licença MIT. Veja o arquivo `LICENSE` para mais detalhes.

---

**Feito com ❤️ para a comunidade de desenvolvedores**

## 🐛 Problemas Conhecidos

- Em alguns sistemas, o orb pode não aparecer sobre jogos em tela cheia
- Transparência pode não funcionar em todas as configurações de GPU

## 🔮 Roadmap

- [ ] Integração com mais provedores de LLM
- [ ] Temas personalizáveis
- [ ] Comandos de voz
- [ ] Plugins e extensões
- [ ] Integração com calendário e tarefas
- [ ] Modo escuro/claro automático
