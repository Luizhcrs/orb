# 📦 Guia de Release

Este documento descreve o processo completo de release do ORB Agent.

## 📋 Pré-requisitos

- ✅ Todas as features testadas
- ✅ Testes passando (backend e frontend)
- ✅ Documentação atualizada
- ✅ Changelog preparado
- ✅ Version bump no package.json

## 🔄 Processo de Release

### 1. Preparação

```bash
# Certifique-se de estar na branch main
git checkout main
git pull origin main

# Atualize a versão (escolha uma: patch, minor, major)
cd frontend
npm version patch   # 1.0.0 -> 1.0.1
npm version minor   # 1.0.0 -> 1.1.0
npm version major   # 1.0.0 -> 2.0.0

cd ..

# Commit da versão
git add .
git commit -m "chore: bump version to v$(node -p "require('./frontend/package.json').version")"
```

### 2. Testes Finais

```bash
# Backend
cd backend
pytest -v
cd ..

# Frontend build
cd frontend
npm run build
cd ..

# Teste local
npm run dev
```

### 3. Criar Tag

```bash
# Pegar versão do package.json
VERSION=$(node -p "require('./frontend/package.json').version")

# Criar tag
git tag -a "v$VERSION" -m "Release v$VERSION"

# Push tag (isso dispara o CI/CD)
git push origin "v$VERSION"
```

### 4. Acompanhar Build

1. Vá para: https://github.com/seu-usuario/orb/actions
2. Verifique o workflow "Build and Release"
3. Aguarde conclusão (aprox. 15-30 minutos)

### 5. Verificar Release

1. Vá para: https://github.com/seu-usuario/orb/releases
2. Verifique se a release foi criada
3. Teste os downloads:
   - Windows: `.exe` (setup e portable)
   - macOS: `.dmg` e `.zip`
   - Linux: `.AppImage` e `.deb`

### 6. Publicar Release Notes

Edite a release no GitHub e adicione:

```markdown
## 🚀 Novidades

- Feature 1: Descrição
- Feature 2: Descrição

## 🐛 Correções

- Bug 1: Descrição
- Bug 2: Descrição

## 🔧 Melhorias

- Melhoria 1: Descrição
- Melhoria 2: Descrição

## 📝 Breaking Changes

- Mudança 1: Como migrar
- Mudança 2: Como migrar

## 📦 Instalação

### Windows
Baixe `OrbAgent-Setup-X.X.X.exe` e execute.

### macOS
Baixe `OrbAgent-X.X.X.dmg`, abra e arraste para Applications.

### Linux
```bash
chmod +x OrbAgent-X.X.X.AppImage
./OrbAgent-X.X.X.AppImage
```

## 🔗 Links Úteis

- [Documentação](https://github.com/seu-usuario/orb/blob/main/README.md)
- [Guia de Contribuição](https://github.com/seu-usuario/orb/blob/main/CONTRIBUTING.md)
```

## 🔄 Versionamento Semântico

Seguimos [Semantic Versioning](https://semver.org/):

- **MAJOR** (X.0.0): Breaking changes
- **MINOR** (0.X.0): Novas features (backwards compatible)
- **PATCH** (0.0.X): Bug fixes (backwards compatible)

### Exemplos

```
1.0.0 -> 1.0.1  # Bug fix
1.0.1 -> 1.1.0  # Nova feature
1.1.0 -> 2.0.0  # Breaking change
```

## 🐛 Hotfix Release

Para correções urgentes:

```bash
# Criar branch de hotfix
git checkout -b hotfix/v1.0.1

# Fazer correções
# ...

# Bump version
cd frontend && npm version patch && cd ..

# Commit
git add .
git commit -m "fix: correção urgente do bug X"

# Merge para main
git checkout main
git merge hotfix/v1.0.1

# Tag e push
git tag -a v1.0.1 -m "Hotfix v1.0.1"
git push origin v1.0.1
git push origin main

# Limpar
git branch -d hotfix/v1.0.1
```

## 📊 Checklist de Release

### Antes do Release

- [ ] Testes passando (pytest + build)
- [ ] Documentação atualizada
- [ ] CHANGELOG.md atualizado
- [ ] Version bump realizado
- [ ] Breaking changes documentados
- [ ] Migration guide (se necessário)

### Durante o Release

- [ ] Tag criada e pushed
- [ ] CI/CD executado com sucesso
- [ ] Artifacts gerados corretamente
- [ ] Release draft criado no GitHub

### Depois do Release

- [ ] Release notes publicadas
- [ ] Downloads testados (pelo menos 1 plataforma)
- [ ] Comunicação para usuários (se aplicável)
- [ ] Social media announcement (se aplicável)
- [ ] Merge back para develop (se usando gitflow)

## 🔧 Troubleshooting

### Build falhou no CI/CD

1. Verifique os logs em Actions
2. Teste build local: `npm run pack:win` (ou mac/linux)
3. Corrija o problema
4. Delete a tag: `git tag -d v1.0.0 && git push origin :refs/tags/v1.0.0`
5. Recomece o processo

### Artifact missing

- Verifique se electron-builder está configurado corretamente
- Verifique se os paths em `.github/workflows/build.yml` estão corretos
- Execute build local para debug

### Release não aparece

- Tags devem começar com `v` (ex: `v1.0.0`)
- Workflow só executa em tags que correspondem ao padrão
- Verifique se GITHUB_TOKEN tem permissões

## 📞 Ajuda

Se encontrar problemas, consulte:

- [GitHub Actions Docs](https://docs.github.com/en/actions)
- [electron-builder Docs](https://www.electron.build/)
- [Conventional Commits](https://www.conventionalcommits.org/)

---

**Happy Releasing! 🎉**

