# 🤝 Contribuindo para o Orb

Obrigado por considerar contribuir para o Orb! Este documento fornece diretrizes para contribuições.

## 🚀 Como Contribuir

### 1. Fork e Clone
```bash
git clone https://github.com/SEU_USUARIO/orb.git
cd orb
```

### 2. Instalar Dependências
```bash
npm install
```

### 3. Configurar Ambiente
```bash
cp env.example .env
# Edite o .env com suas configurações
```

### 4. Executar em Desenvolvimento
```bash
npm run dev
```

## 📋 Processo de Contribuição

### 🔧 Para Bugs
1. Verifique se o bug já foi reportado
2. Use o template de bug report
3. Inclua logs e steps para reproduzir
4. Teste a correção localmente

### ✨ Para Features
1. Abra uma issue discutindo a feature
2. Aguarde aprovação antes de implementar
3. Use o template de feature request
4. Implemente seguindo os padrões do projeto

### 📝 Para Documentação
1. Melhore clareza e precisão
2. Adicione exemplos quando possível
3. Mantenha consistência com o estilo existente

## 🎯 Padrões de Código

### TypeScript
- Use tipagem explícita
- Siga as convenções do ESLint
- Mantenha funções pequenas (< 50 linhas)
- Use nomes descritivos

### CSS
- Use Liquid Glass design system
- Mantenha consistência visual
- Teste em diferentes resoluções
- Use variáveis CSS quando possível

### Commits
Siga o padrão [Conventional Commits](https://www.conventionalcommits.org/):

```
feat: adicionar nova funcionalidade
fix: corrigir bug específico
docs: atualizar documentação
style: ajustes de formatação
refactor: refatoração sem mudança funcional
test: adicionar ou corrigir testes
chore: tarefas de manutenção
```

## 🧪 Testes

### Antes de Enviar PR
- [ ] Código compila sem erros
- [ ] Funcionalidade testada localmente
- [ ] Não quebra funcionalidades existentes
- [ ] Segue padrões de código do projeto

### Testando o Orb
```bash
# Testar hot corner
# Testar abertura/fechamento do chat
# Testar tecla ESC
# Testar expansão do chat
# Testar envio de mensagens
```

## 📋 Pull Request

### Checklist
- [ ] Issue relacionada (se aplicável)
- [ ] Descrição clara das mudanças
- [ ] Screenshots (para mudanças visuais)
- [ ] Testado localmente
- [ ] Não quebra funcionalidades existentes
- [ ] Commits seguem padrão conventional

### Template de PR
```markdown
## 🎯 Descrição
Breve descrição das mudanças realizadas.

## 🔧 Tipo de Mudança
- [ ] Bug fix
- [ ] Nova feature
- [ ] Breaking change
- [ ] Documentação

## ✅ Checklist
- [ ] Testado localmente
- [ ] Não quebra funcionalidades existentes
- [ ] Segue padrões do projeto

## 📸 Screenshots
(Se aplicável)

## 🔗 Issues Relacionadas
Closes #XXX
```

## 🏷️ Labels

### Para Issues
- `bug`: Problemas no código
- `enhancement`: Novas funcionalidades
- `question`: Perguntas e dúvidas
- `help-wanted`: Precisa de ajuda
- `good-first-issue`: Boa para iniciantes
- `needs-triage`: Precisa de análise

### Para PRs
- `ready-for-review`: Pronto para revisão
- `needs-changes`: Precisa de ajustes
- `approved`: Aprovado para merge

## 🎉 Reconhecimento

Contribuidores serão reconhecidos no README e releases do projeto!

## 📞 Contato

- Issues: Use o sistema de issues do GitHub
- Discussões: Use GitHub Discussions
- Email: [seu-email@exemplo.com]

---

**Obrigado por contribuir para o Orb! 🚀**
