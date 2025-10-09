# Assets - Recursos do Orb Agent

## Ícone do Sistema (orb.ico)

Para personalizar o ícone do Orb Agent:

1. **Criar o ícone:**
   - Tamanho: 256x256, 128x128, 64x64, 32x32, 16x16 (multi-size)
   - Formato: `.ico`
   - Design sugerido: Círculo roxo (#290060) com efeito de brilho/gradiente

2. **Adicionar ao projeto:**
   - Salvar o arquivo como `orb.ico` nesta pasta (`frontend-wpf/Assets/`)
   - Descomentar a linha `<ApplicationIcon>Assets\orb.ico</ApplicationIcon>` em `OrbAgent.Frontend.csproj`
   - O arquivo será automaticamente incluído no build

3. **Onde o ícone é usado:**
   - **System Tray** → Ícone na bandeja do Windows
   - **Executável** → Ícone do arquivo `Orb.exe`
   - **Barra de Tarefas** → Quando a aplicação está em execução

## Ícone Padrão

Se nenhum arquivo `orb.ico` for encontrado, o sistema usará um ícone padrão gerado em código:
- Círculo roxo (#290060)
- Borda branca
- Fundo transparente

## Ferramentas Recomendadas

Para criar/editar ícones `.ico`:
- **Online:** [ICO Convert](https://icoconvert.com/)
- **Windows:** [IcoFX](https://icofx.ro/) (gratuito)
- **Cross-platform:** [GIMP](https://www.gimp.org/) (gratuito)

## Exemplo de Design

```
┌─────────────┐
│   ◯◯◯◯◯◯   │  ← Círculo principal (#290060)
│  ◯      ◯  │  ← Brilho/gradiente central
│ ◯   ORB  ◯ │  ← Opcional: texto ou símbolo
│  ◯      ◯  │
│   ◯◯◯◯◯◯   │
└─────────────┘
```

## Notas

- O sistema de System Tray irá redimensionar automaticamente para 16x16
- Para melhor qualidade, inclua todos os tamanhos no arquivo .ico
- Use fundo transparente para melhor integração visual

