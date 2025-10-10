# 🔧 Configuração do GitHub para Colaboração

Este guia explica como configurar o repositório GitHub para receber contribuições.

## 🏷️ Labels Recomendadas

Acesse: `https://github.com/Luizhcrs/orb/labels`

### Tipo de Issue
- `bug` 🐛 - Cor: `#d73a4a` - Algo não está funcionando
- `enhancement` ✨ - Cor: `#a2eeef` - Nova feature ou request
- `documentation` 📚 - Cor: `#0075ca` - Melhorias na documentação
- `question` ❓ - Cor: `#d876e3` - Perguntas

### Prioridade
- `priority: critical` 🔴 - Cor: `#b60205` - Precisa de atenção imediata
- `priority: high` 🟠 - Cor: `#d93f0b` - Importante
- `priority: medium` 🟡 - Cor: `#fbca04` - Moderado
- `priority: low` 🟢 - Cor: `#0e8a16` - Pode esperar

### Status
- `status: investigating` 🔍 - Cor: `#f9d0c4` - Investigando o problema
- `status: in progress` 🚧 - Cor: `#c5def5` - Alguém está trabalhando
- `status: blocked` 🚫 - Cor: `#e99695` - Bloqueado por algo
- `status: ready` ✅ - Cor: `#c2e0c6` - Pronto para trabalhar

### Área
- `area: frontend` 🖥️ - Cor: `#5319e7` - Frontend WPF
- `area: backend` ⚙️ - Cor: `#1d76db` - Backend Python
- `area: installer` 📦 - Cor: `#0052cc` - Build e instalador
- `area: ui/ux` 🎨 - Cor: `#ee0701` - Design e experiência

### Dificuldade (para novos contribuidores)
- `good first issue` 👋 - Cor: `#7057ff` - Bom para iniciantes
- `help wanted` 🙋 - Cor: `#008672` - Precisa de ajuda
- `easy` 🟢 - Cor: `#c2e0c6` - Fácil de resolver
- `medium` 🟡 - Cor: `#fbca04` - Dificuldade média
- `hard` 🔴 - Cor: `#b60205` - Desafiador

### Especiais
- `duplicate` ♊ - Cor: `#cfd3d7` - Já existe issue similar
- `invalid` ❌ - Cor: `#e4e669` - Não é válido ou não será implementado
- `wontfix` 🚫 - Cor: `#ffffff` - Não será trabalhado

## 🔒 Proteção de Branches

Acesse: `Settings` > `Branches` > `Add branch protection rule`

### Para `main`:
```
Branch name pattern: main

✅ Require a pull request before merging
  └─ Required approvals: 1
  └─ Dismiss stale pull request approvals when new commits are pushed

✅ Require status checks to pass before merging
  └─ (Adicionar quando configurar CI/CD)

✅ Require conversation resolution before merging

✅ Require linear history

✅ Include administrators (opcional - desmarque se quiser poder fazer emergency push)
```

## 📊 Projects (Kanban)

Crie um Project para organizar o trabalho:

1. Acesse: `Projects` > `New project`
2. Escolha template: **Team backlog**
3. Nome: **ORB Development**
4. Adicione colunas:
   - 📋 Backlog
   - 🔍 Triagem
   - 📝 To Do
   - 🚧 In Progress
   - 👀 Review
   - ✅ Done

## 🤖 GitHub Actions (Opcional para v1.1)

Crie workflows para:
- Build automático em cada PR
- Testes automatizados
- Geração de instalador em cada release
- Verificação de código (linting)

## 📢 Discussions

Habilite Discussions e crie categorias:

1. Acesse: `Settings` > `Features` > Marque **Discussions**
2. Categorias sugeridas:
   - 💡 **Ideas** - Sugestões de features
   - 🙋 **Q&A** - Perguntas e respostas
   - 📣 **Announcements** - Novidades do projeto
   - 🎉 **Show and Tell** - Mostre o que você fez
   - 🛠️ **Development** - Discussões técnicas

## 🌟 Sobre o Repositório

Adicione no repositório (Settings > General):

### About Section
- **Description**: `Assistente de IA flutuante para Windows com design liquid glass`
- **Website**: [seu site/portfolio se tiver]
- **Topics/Tags**:
  - `ai-assistant`
  - `desktop-app`
  - `wpf`
  - `openai`
  - `gpt`
  - `windows`
  - `chatbot`
  - `python`
  - `fastapi`
  - `sqlite`
  - `desktop-assistant`

### Social Preview Image
Crie uma imagem 1280x640 mostrando o ORB em ação.

## 📝 Wiki (Opcional)

Use a Wiki para:
- Tutoriais detalhados
- FAQs
- Troubleshooting comum
- Guias de desenvolvimento avançado

---

Essas configurações vão tornar muito mais fácil para colaboradores encontrarem e contribuírem com o projeto!

