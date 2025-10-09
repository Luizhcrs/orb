# Contribuindo para o ORB

Obrigado pelo interesse em contribuir! Este documento fornece diretrizes para contribuições.

## 🚀 Começando

### 1. Fork e Clone

```bash
git clone https://github.com/seu-usuario/orb.git
cd orb
```

### 2. Setup do Ambiente

Execute o script de setup:

**Linux/macOS:**
```bash
chmod +x setup-dev.sh
./setup-dev.sh
```

**Windows:**
```batch
setup-dev.bat
```

### 3. Crie uma Branch

```bash
git checkout -b feature/minha-feature
# ou
git checkout -b fix/meu-bugfix
```

## 📋 Diretrizes

### Commits

Use [Conventional Commits](https://www.conventionalcommits.org/):

```
feat: adiciona suporte para Gemini
fix: corrige erro ao salvar histórico
docs: atualiza README com instruções de deploy
style: aplica formatação no código
refactor: reorganiza estrutura de pastas
test: adiciona testes para ConfigManager
chore: atualiza dependências
```

### Código

#### Python (Backend)

- Siga PEP 8
- Use type hints
- Docstrings em funções públicas
- Máximo 88 caracteres por linha (black)

```python
def processar_mensagem(texto: str, contexto: dict) -> str:
    """
    Processa uma mensagem do usuário.
    
    Args:
        texto: Mensagem do usuário
        contexto: Contexto da conversa
        
    Returns:
        Resposta processada
    """
    ...
```

#### C# (Frontend WPF)

- Use nullable reference types
- Evite `dynamic`, prefira tipos específicos
- Siga convenções WPF para MVVM (se aplicável)

```csharp
public class ChatMessage
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime? Timestamp { get; set; }
}
```

### Testes

- Backend: Use pytest
- Frontend: Use MSTest ou xUnit (se configurado)
- Cobertura mínima: 70%

```bash
# Backend
cd backend
pytest

# Com cobertura
pytest --cov=src --cov-report=html
```

### Documentação

- Atualize README.md se adicionar features
- Documente funções públicas
- Adicione comentários para lógica complexa

## 🔄 Fluxo de Trabalho

1. **Desenvolvimento**
   ```bash
   npm run dev
   ```

2. **Testes**
   ```bash
   # Backend
   cd backend && pytest
   
   # Build frontend
   cd frontend && npm run build
   ```

3. **Commit e Push**
   ```bash
   git add .
   git commit -m "feat: minha nova feature"
   git push origin feature/minha-feature
   ```

4. **Pull Request**
   - Vá para GitHub
   - Crie PR de `feature/minha-feature` para `develop`
   - Preencha o template de PR
   - Aguarde review

## 📝 Template de Pull Request

```markdown
## Descrição
[Descreva as mudanças]

## Tipo de Mudança
- [ ] Bug fix
- [ ] Nova feature
- [ ] Breaking change
- [ ] Documentação

## Checklist
- [ ] Código segue as diretrizes do projeto
- [ ] Self-review realizado
- [ ] Comentários adicionados em código complexo
- [ ] Documentação atualizada
- [ ] Testes adicionados/atualizados
- [ ] Testes passando localmente
- [ ] Build sem warnings

## Screenshots (se aplicável)
```

## 🐛 Reportando Bugs

Use o template de issue:

```markdown
## Descrição do Bug
[Descrição clara do problema]

## Como Reproduzir
1. Vá para '...'
2. Clique em '....'
3. Role até '....'
4. Veja o erro

## Comportamento Esperado
[O que deveria acontecer]

## Screenshots
[Se aplicável]

## Ambiente
- OS: [Windows/macOS/Linux]
- Versão do ORB: [1.0.0]
- .NET SDK: [9.0.x]
- Python: [3.11]
```

## 💡 Sugerindo Features

```markdown
## Descrição da Feature
[Descrição clara da sugestão]

## Problema Resolvido
[Que problema isso resolve]

## Solução Proposta
[Como você imagina que funcione]

## Alternativas Consideradas
[Outras abordagens pensadas]
```

## 🎨 Padrões de Código

### Estrutura de Arquivos

```
nova-feature/
├── __init__.py
├── service.py       # Lógica de negócio
├── models.py        # Modelos de dados
└── tests/
    └── test_service.py
```

### Nomenclatura

- **Arquivos**: `snake_case.py`, `kebab-case.ts`
- **Classes**: `PascalCase`
- **Funções**: `snake_case` (Python), `PascalCase` (C# métodos públicos), `camelCase` (C# variáveis locais)
- **Constantes**: `UPPER_SNAKE_CASE`
- **Variáveis privadas**: `_leading_underscore`

## 🔍 Code Review

Todos os PRs passam por review. Esperamos:

- ✅ Código limpo e legível
- ✅ Testes adequados
- ✅ Documentação atualizada
- ✅ Sem breaking changes (ou documentados)
- ✅ CI/CD passando

## 📞 Dúvidas?

- 💬 Discussions: Para perguntas gerais
- 🐛 Issues: Para bugs e features
- 📧 Email: para questões privadas

---

**Obrigado por contribuir! 🎉**

