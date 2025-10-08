# 🚀 Plano de Otimização de Performance - Inicialização

## 🎯 **PROBLEMAS IDENTIFICADOS PELO USUÁRIO:**

1. ⏱️ **Orb demora pra aparecer no canto** (~500ms-1s)
2. ⏱️ **Tela de configuração demora pra abrir** (~300-800ms)
3. ⏱️ **Atraso geral na renderização**

---

## 🔴 **GARGALOS DE INICIALIZAÇÃO (Frontend):**

### 1. **Carregamento Síncrono de HTML** (CRÍTICO)
**Local:** `WindowManager.ts` - métodos `createOrbWindow()`, `createChatWindow()`, `createConfigWindow()`

**Problema:**
```typescript
this.state.orbWindow.loadFile(path.join(__dirname, '..', 'src', 'orb.html'));
// ↑ Bloqueia a thread até carregar o arquivo!
```

**Impacto:** 200-500ms de bloqueio **POR JANELA**

**Solução:**
- Usar `webContents.loadFile()` é inevitável, mas podemos:
  1. **Pré-compilar os HTMLs** durante o build (já fazemos)
  2. **Lazy load** janelas que não são usadas imediatamente (Config, Chat)
  3. **Usar `show: false`** e só mostrar após `ready-to-show` (já fazemos para Chat)

---

### 2. **Electron App Initialization** (MÉDIO)
**Local:** `main.ts` - `constructor()` e `initializeApp()`

**Problema:**
```typescript
constructor() {
    this.llmManager = new BackendLLMManager();        // ← Instancia TUDO
    this.screenshotService = new ScreenshotService(); // ← Antes do app.whenReady()
    this.windowManager = new WindowManager(...);     // ← Antes do app.whenReady()
    this.mouseDetector = new MouseDetector(...);
    this.shortcutManager = new ShortcutManager(...);
}
```

**Impacto:** Inicialização pesada ANTES do Electron estar pronto

**Solução:**
- Mover inicializações pesadas para **DENTRO de `app.whenReady()`**
- Lazy load `BackendLLMManager` (só instanciar quando abrir chat)
- Lazy load `ScreenshotService` (só instanciar quando usar screenshot)

---

### 3. **MouseDetector Polling** (BAIXO-MÉDIO)
**Local:** `MouseDetector.ts` - `startDetection()`

**Problema:**
```typescript
startDetection(): void {
    this.pollInterval = setInterval(() => {
        const { x, y } = screen.getCursorScreenPoint(); // ← Polling a cada 100ms
        this.checkHotCorner(x, y);
    }, 100);
}
```

**Impacto:** CPU constante (leve, mas desnecessário quando Orb já está visível)

**Solução:**
- Pausar polling quando Orb está visível
- Aumentar intervalo para 150ms (imperceptível para o usuário)

---

### 4. **Config Window Criação On-Demand** (BAIXO)
**Local:** `WindowManager.ts` - `openConfig()`

**Problema:**
```typescript
openConfig(): void {
    if (!this.state.configWindow || this.state.configWindow.isDestroyed()) {
        this.createConfigWindow(); // ← Cria TODA VEZ que abre
    }
    // ...
}
```

**Impacto:** 300-500ms de delay ao abrir config pela primeira vez

**Solução:**
- **Manter a janela criada e oculta** após primeira abertura
- Só destruir quando fechar o app

---

## ✅ **OTIMIZAÇÕES JÁ APLICADAS (Funcionando):**

1. ✅ **Singleton do AgenteORB** (Backend) → -2-5s por requisição
2. ✅ **Connection Pooling SQLite** (Backend) → -500ms-1.5s em queries
3. ✅ **DocumentFragment (loadHistoryMessages)** (Frontend) → -400ms-1.2s ao carregar histórico
4. ✅ **Date Formatter Cache** (Frontend) → -200-700ms em formatação de datas
5. ❌ **~~Remover backdrop-filter~~** (Frontend) → **REVERTIDO pelo usuário** (visual quebrou)

---

## 🚀 **OTIMIZAÇÕES PRIORITÁRIAS (Inicialização):**

### **#1 - Lazy Load de Managers (CRÍTICO)**
**Ganho esperado:** -300-700ms na inicialização do app

**Implementação:**
```typescript
// main.ts - ANTES (ruim)
constructor() {
    this.llmManager = new BackendLLMManager();        // ← Instancia SEMPRE
    this.screenshotService = new ScreenshotService(); // ← Instancia SEMPRE
}

// main.ts - DEPOIS (bom)
constructor() {
    this._llmManager: BackendLLMManager | null = null;
    this._screenshotService: ScreenshotService | null = null;
}

private get llmManager(): BackendLLMManager {
    if (!this._llmManager) {
        this._llmManager = new BackendLLMManager();
    }
    return this._llmManager;
}
```

---

### **#2 - Orb Window com `show: false` + `ready-to-show`**
**Ganho esperado:** -200-400ms (Orb aparece instantaneamente quando pronto)

**Implementação:**
```typescript
// WindowManager.ts - createOrbWindow()
this.state.orbWindow = new BrowserWindow({
    ...orbConfig,
    show: false, // ← NÃO mostrar imediatamente
    webPreferences: { ... }
});

this.state.orbWindow.loadFile(path.join(__dirname, '..', 'src', 'orb.html'));

// Mostrar APENAS quando estiver pronto
this.state.orbWindow.once('ready-to-show', () => {
    this.state.orbWindow?.show();
    console.log('✅ Orb window pronto e visível');
});
```

---

### **#3 - Config Window Persistente (Cache)**
**Ganho esperado:** -300-500ms ao abrir config pela 2ª+ vez

**Implementação:**
```typescript
// WindowManager.ts - openConfig()
openConfig(): void {
    // Se já existe, apenas mostrar (instantâneo!)
    if (this.state.configWindow && !this.state.configWindow.isDestroyed()) {
        this.state.configWindow.show();
        this.state.configWindow.focus();
        return;
    }
    
    // Só criar se NÃO existe
    this.createConfigWindow();
    this.state.configWindow!.show();
}

closeConfig(): void {
    if (this.state.configWindow && !this.state.configWindow.isDestroyed()) {
        this.state.configWindow.hide(); // ← HIDE ao invés de destroy!
    }
}
```

---

### **#4 - MouseDetector Smart Polling**
**Ganho esperado:** -5-10% uso de CPU

**Implementação:**
```typescript
// MouseDetector.ts
pauseDetection(): void {
    if (this.pollInterval) {
        clearInterval(this.pollInterval);
        this.pollInterval = null;
    }
}

resumeDetection(): void {
    if (!this.pollInterval) {
        this.startDetection();
    }
}

// main.ts - handleOrbShow()
private handleOrbShow() {
    this.mouseDetector.pauseDetection(); // ← Pausar quando Orb visível
}

// main.ts - handleOrbHide()
private handleOrbHide() {
    this.mouseDetector.resumeDetection(); // ← Retomar quando Orb oculto
}
```

---

## 📊 **GANHO TOTAL ESPERADO:**

| Otimização | Ganho (ms) | Prioridade |
|------------|-----------|-----------|
| Lazy Load Managers | 300-700ms | 🔴 CRÍTICO |
| Orb `ready-to-show` | 200-400ms | 🔴 CRÍTICO |
| Config Window Cache | 300-500ms | 🟡 ALTA |
| Smart Polling | -5-10% CPU | 🟢 MÉDIA |
| **TOTAL** | **800-1600ms** | ⚡ |

---

## 🎯 **PRÓXIMOS PASSOS:**

1. Implementar **Lazy Load** dos managers (`BackendLLMManager`, `ScreenshotService`)
2. Adicionar `show: false` + `ready-to-show` no `createOrbWindow()`
3. Modificar `closeConfig()` para usar `.hide()` ao invés de `.destroy()`
4. Adicionar `pauseDetection()` e `resumeDetection()` no `MouseDetector`
5. Testar e medir ganhos reais

---

## ⚠️ **OTIMIZAÇÕES DESCARTADAS:**

- ❌ **Remover backdrop-filter:** Visual quebrou (usuário reverteu)
- ❌ **Code splitting:** Electron não se beneficia tanto disso
- ❌ **Web Workers:** Overhead maior que ganho para esse caso

