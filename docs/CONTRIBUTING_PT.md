# 🤝 Guia de Contribuição - ORB Agent

Obrigado por seu interesse em contribuir com o ORB Agent! Este documento fornece diretrizes para diferentes tipos de contribuição.

## 🐛 Reportando Bugs

Encontrou um bug? Ajude-nos a melhorar!

### Antes de Reportar

1. Verifique se o bug já foi reportado em [Issues](https://github.com/Luizhcrs/orb/issues)
2. Certifique-se de estar usando a versão mais recente
3. Tente reproduzir o bug em modo limpo (sem configurações customizadas)

### Como Reportar

Abra uma [nova issue](https://github.com/Luizhcrs/orb/issues/new) com:

**Título**: Descrição curta e clara (ex: "Chat trava ao enviar screenshot grande")

**Descrição deve incluir**:
- **Versão do ORB**: v1.0.0 (veja em Configurações > Sobre)
- **Sistema Operacional**: Windows 10/11 (build número)
- **Passos para reproduzir**:
  1. Abra o chat
  2. Capture screenshot
  3. Envie mensagem
  4. Aplicação trava
- **Comportamento esperado**: Chat deve funcionar normalmente
- **Comportamento atual**: Chat trava e não responde
- **Screenshots/GIFs**: Se possível, anexe evidências visuais
- **Logs**: Copie logs de `%APPDATA%\OrbAgent\logs\orb-backend.log`

### Classificação de Bugs

Ajude-nos marcando a severidade:
- 🔴 **Crítico**: Aplicação quebra completamente
- 🟠 **Alto**: Funcionalidade principal não funciona
- 🟡 **Médio**: Funcionalidade secundária com problemas
- 🟢 **Baixo**: Problemas cosméticos ou edge cases

## ✨ Sugerindo Features

Tem uma ideia? Queremos ouvir!

### Como Sugerir

1. Abra uma [Discussion](https://github.com/Luizhcrs/orb/discussions) na categoria "Ideas"
2. Descreva:
   - **Problema que resolve**: Por que essa feature é necessária?
   - **Solução proposta**: Como você imagina que funcione?
   - **Alternativas**: Considerou outras abordagens?
   - **Contexto adicional**: Screenshots, mockups, exemplos

## 💻 Contribuindo com Código

### Setup de Desenvolvimento

```bash
# Clone o repositório
git clone https://github.com/Luizhcrs/orb.git
cd orb

# Backend
cd backend
python -m venv venv
venv\Scripts\activate
pip install -r requirements.txt
cd ..

# Frontend
cd frontend
dotnet restore
dotnet build
cd ..

# Executar
cd frontend
dotnet run
```

### Workflow de Contribuição

1. **Fork** o repositório
2. **Clone** seu fork: `git clone https://github.com/SEU-USUARIO/orb.git`
3. **Crie uma branch**: `git checkout -b feature/minha-feature`
4. **Faça suas alterações**
5. **Teste** localmente
6. **Commit**: `git commit -m "feat: adiciona X funcionalidade"`
7. **Push**: `git push origin feature/minha-feature`
8. **Abra um Pull Request**

### Convenções de Commit

Usamos [Conventional Commits](https://www.conventionalcommits.org/):

- `feat:` Nova funcionalidade
- `fix:` Correção de bug
- `docs:` Mudanças na documentação
- `style:` Formatação, ponto e vírgula faltando, etc
- `refactor:` Refatoração de código
- `perf:` Melhorias de performance
- `test:` Adicionar testes
- `chore:` Tarefas de manutenção

**Exemplos**:
```
feat: adiciona suporte para Claude AI
fix: corrige travamento ao enviar screenshot grande
docs: atualiza README com instruções de instalação
style: formata código seguindo padrão C#
refactor: simplifica lógica de criptografia
perf: otimiza carregamento do histórico
test: adiciona testes para ConfigManager
chore: atualiza dependências NuGet
```

### Checklist do Pull Request

Antes de enviar seu PR, certifique-se de:

- [ ] Código compila sem erros
- [ ] Aplicação funciona corretamente
- [ ] Sem warnings de compilação
- [ ] Código formatado corretamente
- [ ] Comentários em código complexo
- [ ] Sem API keys ou dados sensíveis
- [ ] .gitignore atualizado se necessário
- [ ] README atualizado se mudou comportamento público
- [ ] Screenshots/GIFs se mudou UI

## 🎨 Contribuindo com Design

### Áreas que Precisam de Ajuda

- **Ícones**: Melhorar ícones da aplicação
- **Animações**: Tornar transições mais suaves
- **UX**: Melhorar fluxos de usuário
- **Temas**: Criar temas alternativos (light mode?)
- **Acessibilidade**: Melhorar contraste, leitores de tela

### Como Contribuir

1. Abra uma Discussion mostrando seus mockups/designs
2. Receba feedback da comunidade
3. Implemente ou ajude desenvolvedores a implementar

## 📚 Contribuindo com Documentação

### O Que Ajudaria Muito

- Tutoriais em vídeo
- Guias de uso para iniciantes
- Troubleshooting de problemas comuns
- Tradução para outros idiomas
- Exemplos de uso avançado

### Onde Contribuir

- `README.md`: Documentação principal
- `docs/`: Documentação técnica
- Wiki do GitHub: Tutoriais e guias
- YouTube: Vídeos de demonstração

## 🧪 Testando (Para QA)

### O Que Testar

1. **Instalação**:
   - Instala corretamente?
   - Todos os atalhos são criados?
   - Backend inicia junto com frontend?

2. **Funcionalidades Core**:
   - Chat funciona?
   - Screenshots funcionam?
   - Histórico salva e carrega corretamente?
   - API key é salva de forma segura?

3. **Edge Cases**:
   - Mensagens muito longas
   - Screenshots muito grandes
   - Muitas conversas no histórico
   - Sem internet (backend deve funcionar offline)
   - Múltiplas instâncias da aplicação

4. **Performance**:
   - Uso de memória
   - Tempo de resposta
   - Lag na UI

5. **Compatibilidade**:
   - Windows 10 (diferentes builds)
   - Windows 11
   - Diferentes resoluções de tela
   - Múltiplos monitores

### Como Reportar Resultados de Testes

Abra uma issue com template:

```markdown
## Ambiente de Teste
- OS: Windows 11 Build 22621
- RAM: 16 GB
- Resolução: 1920x1080

## Testes Realizados
✅ Instalação - OK
✅ Chat básico - OK
⚠️ Screenshot grande (>10MB) - Lento (15s)
❌ Histórico com 100+ conversas - Trava ao abrir

## Logs
[Cole logs relevantes aqui]

## Screenshots
[Anexe evidências]
```

## 🌟 Áreas que Mais Precisam de Ajuda

### Alta Prioridade
1. **Testes de QA**: Precisamos quebrar o app para fortalecê-lo
2. **Performance**: Otimizar carregamento e uso de memória
3. **Documentação**: Vídeos, tutoriais, exemplos

### Média Prioridade
4. **Novos LLMs**: Implementar Claude, Gemini
5. **Temas**: Light mode e temas customizáveis
6. **i18n**: Internacionalização completa

### Features Futuras
7. **Plugins**: Sistema de extensões
8. **Sincronização**: Backup em nuvem opcional
9. **Mobile**: Companion app para iOS/Android

## 💬 Comunicação

- **Issues**: Bugs e feature requests
- **Discussions**: Ideias, perguntas, ajuda
- **Pull Requests**: Contribuições de código
- **Discord/Telegram**: (criar se houver demanda)

## 🏆 Reconhecimento

Todos os contribuidores serão:
- Listados no README.md
- Mencionados nas Release Notes
- Adicionados ao arquivo CONTRIBUTORS.md

## 📜 Código de Conduta

Seja respeitoso, inclusivo e colaborativo. Não toleramos:
- Assédio ou discriminação
- Spam ou autopromoção excessiva
- Comportamento não profissional

---

**Obrigado por ajudar a tornar o ORB Agent melhor! 🚀**

Dúvidas? Abra uma [Discussion](https://github.com/Luizhcrs/orb/discussions/new)!

