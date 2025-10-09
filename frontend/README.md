# Frontend WPF - Orb Agent

## 📋 Configurações Centralizadas

Todas as configurações da aplicação estão centralizadas em `Config/AppSettings.cs`.

### 🔧 Como Configurar

#### Método 1: Variáveis de Ambiente (Recomendado para Dev)

Crie um arquivo `.env` (copie de `env.example`):

```env
ORB_BACKEND_URL=http://127.0.0.1:8000
ORB_BACKEND_PORT=8000
ORB_DEBUG=true
```

#### Método 2: Editar Diretamente no Código

Edite `Config/AppSettings.cs` e altere os valores padrão:

```csharp
public static string BackendBaseUrl { get; set; } = "http://127.0.0.1:9000"; // Nova porta
public static int HttpTimeoutSeconds { get; set; } = 60; // Timeout maior
```

### ⚙️ Configurações Disponíveis

#### Backend
- `BackendBaseUrl` - URL do backend (padrão: `http://127.0.0.1:8000`)
- `BackendPort` - Porta do backend (padrão: `8000`)
- `HttpTimeoutSeconds` - Timeout HTTP (padrão: `30`)
- `BackendExecutablePath` - Caminho do executável Python

#### UI
- `ChatWindowCompactWidth` / `ChatWindowCompactHeight` - Tamanho do chat compacto
- `ChatWindowExpandedWidth` / `ChatWindowExpandedHeight` - Tamanho do chat expandido
- `ConfigWindowWidth` / `ConfigWindowHeight` - Tamanho da tela de config
- `OrbSize` - Tamanho do Orb (padrão: `60`)
- `OrbInactivityTimeoutSeconds` - Tempo antes do Orb desaparecer (padrão: `5`)

#### Hot Corner
- `HotCornerMargin` - Margem de detecção (pixels)
- `HotCornerDelayMs` - Delay de ativação (ms)
- `HotCornerPosition` - Posição (TopLeft, TopRight, etc.)

#### Logging
- `EnableDebugLogging` - Habilitar logs detalhados
- `LogFileName` - Nome do arquivo de log
- `MaxLogFileSizeMB` - Tamanho máximo do log

### 🚀 Build

```bash
# Restaurar dependências
dotnet restore

# Build
dotnet build --configuration Release

# Executar
dotnet run
```

### 🧪 Debug

Logs são gravados em `orb_debug.log` na pasta do executável.

Para habilitar debug via ambiente:

```bash
set ORB_DEBUG=true
dotnet run
```

### 📦 Publish

```bash
dotnet publish --configuration Release --self-contained --runtime win-x64 -p:PublishSingleFile=true
```

---

**Dúvidas? Veja a documentação completa em `/docs`**

