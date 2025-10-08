# 🐍 Empacotando o Backend Python

Existem 3 abordagens para distribuir o backend com o instalador:

## **Opção 1: PyInstaller (Recomendado)**

Cria um executável standalone do backend.

### Setup

```bash
cd backend
pip install pyinstaller
python build_standalone.py
```

Isso gera `backend/dist/orb-backend.exe` (Windows) ou `orb-backend` (Linux/Mac).

### Configurar electron-builder

Em `frontend/package.json`, atualize:

```json
{
  "build": {
    "extraResources": [
      {
        "from": "../backend/dist/orb-backend${.exe}",
        "to": "backend/"
      }
    ]
  }
}
```

### Atualizar BackendProcessManager

```typescript
private getPythonPath(): string {
  if (app.isPackaged) {
    // Usar executável standalone
    const ext = process.platform === 'win32' ? '.exe' : '';
    return path.join(process.resourcesPath, 'backend', `orb-backend${ext}`);
  }
  // Desenvolvimento
  return process.platform === 'win32' ? 'python' : 'python3';
}
```

---

## **Opção 2: Python Portable (Mais Simples)**

Incluir uma instalação portátil do Python.

### Windows

1. Baixe Python Embeddable: https://www.python.org/downloads/windows/
2. Extraia em `backend/python-embed/`
3. Instale dependências: `python-embed/python -m pip install -r requirements.txt`

### Configurar electron-builder

```json
{
  "build": {
    "extraResources": [
      {
        "from": "../backend",
        "to": "backend",
        "filter": ["**/*", "!venv/**", "!__pycache__/**"]
      }
    ]
  }
}
```

---

## **Opção 3: Requerer Python do Sistema (Mais Fácil de Desenvolver)**

O usuário precisa ter Python instalado.

### Adicionar ao instalador (NSIS)

```json
{
  "build": {
    "nsis": {
      "include": "installer.nsh"
    }
  }
}
```

`installer.nsh`:
```nsis
!macro customInit
  ; Verificar Python
  nsExec::ExecToStack 'python --version'
  Pop $0
  ${If} $0 != 0
    MessageBox MB_OK "Python 3.11+ é necessário. Instale de python.org"
    Abort
  ${EndIf}
!macroend
```

---

## **Recomendação Atual**

**Para desenvolvimento rápido:**
- Use Opção 3 (usuário instala Python)
- Adicione verificação no instalador
- Documente no README

**Para produção profissional:**
- Use Opção 1 (PyInstaller)
- Executável standalone
- Melhor UX (usuário não precisa de nada)

---

## **Por Agora (Solução Temporária)**

Mantenha o backend rodando separadamente durante desenvolvimento:

```bash
# Terminal 1 - Backend
cd backend
python scripts/dev.py

# Terminal 2 - Frontend
cd frontend
npm run dev
```

Para o instalador, adicione no README que Python 3.11+ é necessário.

