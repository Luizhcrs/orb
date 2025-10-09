# 🎉 Guia de Release

Este documento descreve o processo completo de release do ORB Agent.

## 📋 Checklist de Release

### Pré-Release

- [ ] Todos os testes passando
- [ ] Documentação atualizada
- [ ] Changelog atualizado
- [ ] Versão atualizada em:
  - [ ] `frontend/OrbAgent.Frontend.csproj`
  - [ ] `backend/src/api/config/api_config.py`
  - [ ] `README.md`

### Build

- [ ] Build do backend standalone (`python build_standalone.py`)
- [ ] Build do frontend WPF (`dotnet publish --configuration Release`)
- [ ] Testar em máquina limpa
- [ ] Verificar tamanho do instalador

### Distribuição

- [ ] Criar release no GitHub
- [ ] Upload dos binários
- [ ] Publicar release notes
- [ ] Atualizar documentação

---

## 🔢 Versionamento

Seguimos **Semantic Versioning** (SemVer): `MAJOR.MINOR.PATCH`

### Incrementar Versão

**MAJOR** (1.0.0 → 2.0.0):
- Mudanças incompatíveis (breaking changes)
- Reescritas maiores

**MINOR** (1.0.0 → 1.1.0):
- Novas funcionalidades compatíveis
- Melhorias significativas

**PATCH** (1.0.0 → 1.0.1):
- Correção de bugs
- Melhorias de performance

---

## 🏗️ Processo de Build

### 1. Atualizar Versão

**Frontend (OrbAgent.Frontend.csproj):**
```xml
<PropertyGroup>
  <AssemblyVersion>1.1.0.0</AssemblyVersion>
  <FileVersion>1.1.0.0</FileVersion>
  <Version>1.1.0</Version>
</PropertyGroup>
```

**Backend (api_config.py):**
```python
VERSION = "1.1.0"
```

### 2. Build do Backend

```bash
cd backend
pip install -r requirements.txt
pip install -r requirements-build.txt
python build_standalone.py
```

**Verifica:**
- [ ] `backend/dist/orb-backend.exe` criado
- [ ] Tamanho: ~50-80 MB
- [ ] Teste: `cd dist && orb-backend.exe`

### 3. Build do Frontend

```bash
cd frontend
dotnet clean
dotnet restore
dotnet build --configuration Release
dotnet publish --configuration Release --self-contained --runtime win-x64 -p:PublishSingleFile=true
```

**Verifica:**
- [ ] `frontend/bin/Release/net9.0-windows/publish/Orb.exe` criado
- [ ] Tamanho: ~80-100 MB
- [ ] Teste: Execute `Orb.exe` em máquina de teste

### 4. Criar Instalador (Opcional)

```bash
# Com Inno Setup
iscc installer.iss

# Com script automatizado
build-all.bat
```

---

## 📝 Changelog

### Exemplo de Release Notes

```markdown
## [1.1.0] - 2025-10-09

### Adicionado
- ✨ Suporte para GPT-4o-mini
- 📸 Captura de tela com expansão de imagem
- 🎨 Novo visual liquid glass

### Modificado
- 🚀 Migração de Electron para WPF
- ⚡ Performance melhorada em 40%
- 🎯 UX aprimorado no chat

### Corrigido
- 🐛 Timestamps em histórico
- 🔧 Sessões de chat misturando contexto
- 💾 API key não salvando corretamente
```

---

## 🚀 Distribuição

### GitHub Releases

1. **Criar Tag:**
```bash
git tag -a v1.1.0 -m "Release 1.1.0"
git push origin v1.1.0
```

2. **Criar Release no GitHub:**
- Acesse: `https://github.com/seu-usuario/orb/releases/new`
- Tag: `v1.1.0`
- Título: `ORB Agent v1.1.0`
- Descrição: Cole as release notes
- Anexos:
  - `Orb.exe` (Frontend)
  - `orb-backend.exe` (Backend standalone)
  - `OrbAgent-Setup-1.0.0.exe` (Instalador completo)

### Download Links

```markdown
**Windows:**
- [Instalador Completo](link) - Recomendado
- [Portable](link) - Sem instalação
- [Frontend Only](link) - Requer backend já instalado
```

---

## 🧪 Testes Pré-Release

### Checklist de Testes

**Frontend:**
- [ ] Hot corner funciona
- [ ] Atalhos globais (`Ctrl+Shift+Space`, `Ctrl+Shift+O`, `Ctrl+Shift+S`)
- [ ] Chat envia e recebe mensagens
- [ ] Screenshot captura e anexa
- [ ] Configurações salvam
- [ ] Histórico carrega conversas antigas
- [ ] Timestamps corretos em mensagens históricas

**Backend:**
- [ ] Health check responde (`/health`)
- [ ] API `/api/v1/agent/message` funciona
- [ ] Configurações persistem no SQLite
- [ ] Histórico persiste entre sessões
- [ ] Serviço Windows inicia automaticamente

**Integração:**
- [ ] Frontend conecta ao backend
- [ ] Sessões não se misturam
- [ ] Imagens são salvas e carregadas
- [ ] API key criptografada funciona

---

## 📊 Métricas de Release

### Tamanhos

| Componente | Tamanho |
|------------|---------|
| Frontend (self-contained) | ~80-100 MB |
| Backend (standalone) | ~50-80 MB |
| Instalador completo | ~150-200 MB |

### Performance

| Métrica | Target |
|---------|--------|
| Startup do frontend | < 2s |
| Resposta do backend | < 100ms |
| Resposta LLM (GPT-4o) | 1-3s |
| Uso de memória | < 200 MB |

---

## 🔄 Processo de Update

### Para Usuários

1. Baixar novo instalador
2. Executar (sobrescreve instalação antiga)
3. Reiniciar aplicação
4. ✅ Configurações e histórico preservados

### Compatibilidade de Dados

- ✅ Banco SQLite é retrocompatível
- ✅ Configurações migram automaticamente
- ✅ Histórico preservado entre versões

---

## 📞 Suporte Pós-Release

### Monitoramento

- GitHub Issues para bugs reportados
- Analytics de uso (se implementado)
- Feedback de usuários

### Hotfix

Para correções críticas:

```bash
# 1. Criar branch hotfix
git checkout -b hotfix/v1.0.1

# 2. Corrigir bug

# 3. Incrementar PATCH version
# (1.0.0 → 1.0.1)

# 4. Build e release
build-all.bat

# 5. Merge de volta
git checkout main
git merge hotfix/v1.0.1
git push
```

---

## 📦 Artefatos de Release

### Estrutura do ZIP

```
orb-agent-v1.1.0-windows.zip
├── Orb.exe
├── orb-backend.exe
├── README.txt
├── LICENSE.txt
└── install_service.bat
```

### Checksums

Sempre gere e publique checksums:

```bash
# SHA256
certutil -hashfile Orb.exe SHA256
certutil -hashfile orb-backend.exe SHA256
```

---

**Pronto para Release! 🚀**
