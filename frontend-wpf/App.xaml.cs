using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Microsoft.Extensions.DependencyInjection;
using OrbAgent.Frontend.Services;
using OrbAgent.Frontend.Windows;

namespace OrbAgent.Frontend;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    // Win32 API para hotkey global
    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
    
    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    
    private const int HOTKEY_ID_CONFIG = 9000;
    private const int HOTKEY_ID_SCREENSHOT = 9001;
    private const uint MOD_CONTROL = 0x0002;
    private const uint MOD_SHIFT = 0x0004;
    private const uint VK_O = 0x4F;
    private const uint VK_S = 0x53;
    
    private ServiceProvider? _serviceProvider;
    private OrbWindow? _orbWindow;
    private ChatWindow? _chatWindow;
    private ConfigWindow? _configWindow;
    private HotCornerService? _hotCornerService;
    private SystemTrayService? _systemTrayService;
    private BackendProcessManager? _backendProcessManager;
    private HwndSource? _hwndSource;
    private Window? _invisibleWindow; // Janela auxiliar para hotkey

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Iniciar backend Python
        InitializeBackendAsync();

        // Configurar Dependency Injection
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        // Criar janela Orb (inicialmente escondida)
        _orbWindow = new OrbWindow();
        _orbWindow.OrbClicked += OnOrbClicked;
        
        // Inicialmente esconder o Orb (ele aparece no hot corner)
        _orbWindow.Hide();

        // Configurar Hot Corner Detection
        _hotCornerService = new HotCornerService();
        _hotCornerService.HotCornerEntered += OnHotCornerEntered;
        _hotCornerService.Start();
        
        // Configurar System Tray
        _systemTrayService = new SystemTrayService();
        _systemTrayService.ConfigurationRequested += OnConfigRequest;
        _systemTrayService.AboutRequested += OnAboutRequest;
        _systemTrayService.ExitRequested += OnExitRequest;
        
        // Criar janela invisível para receber hotkeys globais
        CreateInvisibleWindow();
    }

    private async void InitializeBackendAsync()
    {
        System.Diagnostics.Debug.WriteLine("🚀 Iniciando backend...");
        _backendProcessManager = new BackendProcessManager();
        var started = await _backendProcessManager.StartBackendAsync();
        
        if (!started)
        {
            System.Diagnostics.Debug.WriteLine("❌ FALHA ao iniciar backend Python");
            System.Windows.MessageBox.Show(
                "Falha ao conectar com o backend Python.\n\n" +
                "Verifique:\n" +
                "1. Python está instalado\n" +
                "2. Backend está em: backend/\n" +
                "3. Veja o Debug Output para mais detalhes",
                "Erro - Backend",
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("✅ Backend Python inicializado com sucesso!");
            // Notificação removida - não é necessária
        }
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Registrar HttpClient para BackendService
        services.AddHttpClient<BackendService>();
        
        // Registrar serviços
        services.AddSingleton<BackendService>();
        services.AddSingleton<HotCornerService>();
    }

    private void OnHotCornerEntered(object? sender, EventArgs e)
    {
        // Mouse entrou no hot corner - mostrar Orb
        _orbWindow?.ShowOrb();
    }

    private void OnOrbClicked(object? sender, EventArgs e)
    {
        // SEMPRE criar nova instância para nova sessão
        var backendService = _serviceProvider?.GetService<BackendService>();
        if (backendService != null)
        {
            // Fechar janela anterior se existir
            _chatWindow?.Close();
            
            // Criar nova instância
            _chatWindow = new ChatWindow(backendService);
            _chatWindow.ChatClosed += OnChatClosed;
        }
        
        _chatWindow?.ShowChat();
        
        // Esconder o Orb quando o chat abrir
        _orbWindow?.HideOrb();
    }
    
    private void OnChatClosed(object? sender, EventArgs e)
    {
        // Mostrar o Orb de volta quando fechar o chat
        _orbWindow?.ShowOrb();
    }

    private void CreateInvisibleWindow()
    {
        // Criar janela invisível para receber hotkeys
        _invisibleWindow = new Window
        {
            Width = 1,
            Height = 1,
            WindowStyle = WindowStyle.None,
            ShowInTaskbar = false,
            ResizeMode = ResizeMode.NoResize,
            Left = -10000,
            Top = -10000,
            Visibility = Visibility.Hidden
        };
        
        // IMPORTANTE: Mostrar a janela para criar o handle
        _invisibleWindow.Show();
        
        // Registrar hotkey após janela estar visível
        var helper = new WindowInteropHelper(_invisibleWindow);
        _hwndSource = HwndSource.FromHwnd(helper.Handle);
        _hwndSource?.AddHook(HwndHook);
        
        // Registrar Ctrl+Shift+O (Config)
        bool successConfig = RegisterHotKey(helper.Handle, HOTKEY_ID_CONFIG, MOD_CONTROL | MOD_SHIFT, VK_O);
        
        // Registrar Ctrl+Shift+S (Screenshot)
        bool successScreenshot = RegisterHotKey(helper.Handle, HOTKEY_ID_SCREENSHOT, MOD_CONTROL | MOD_SHIFT, VK_S);
        
        if (!successConfig)
        {
            System.Diagnostics.Debug.WriteLine("ERRO: Falha ao registrar hotkey Ctrl+Shift+O");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("✅ Hotkey Ctrl+Shift+O registrado com sucesso!");
        }
        
        if (!successScreenshot)
        {
            System.Diagnostics.Debug.WriteLine("ERRO: Falha ao registrar hotkey Ctrl+Shift+S");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("✅ Hotkey Ctrl+Shift+S registrado com sucesso!");
        }
    }
    
    private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        const int WM_HOTKEY = 0x0312;
        
        if (msg == WM_HOTKEY)
        {
            int hotkeyId = wParam.ToInt32();
            
            if (hotkeyId == HOTKEY_ID_CONFIG)
            {
                // Hotkey Ctrl+Shift+O pressionado → Abrir Config
                OnConfigRequest(null, EventArgs.Empty);
                handled = true;
            }
            else if (hotkeyId == HOTKEY_ID_SCREENSHOT)
            {
                // Hotkey Ctrl+Shift+S pressionado → Capturar Screenshot
                OnScreenshotRequest();
                handled = true;
            }
        }
        
        return IntPtr.Zero;
    }
    
    private void OnScreenshotRequest()
    {
        System.Diagnostics.Debug.WriteLine("📸 Screenshot capturado via Ctrl+Shift+S");
        
        // Capturar screenshot
        var screenshotService = new ScreenshotService();
        var base64Image = screenshotService.CaptureFullScreen();
        
        // SEMPRE criar nova instância para nova sessão
        var backendService = _serviceProvider?.GetService<BackendService>();
        if (backendService != null)
        {
            // Fechar janela anterior se existir
            _chatWindow?.Close();
            
            // Criar nova instância
            _chatWindow = new ChatWindow(backendService);
            _chatWindow.ChatClosed += OnChatClosed;
        }
        
        // Mostrar chat e adicionar imagem
        _chatWindow?.ShowChat();
        _chatWindow?.AttachScreenshot(base64Image);
        
        // Esconder o Orb
        _orbWindow?.HideOrb();
    }

    private void OnConfigRequest(object? sender, EventArgs e)
    {
        // Criar ou mostrar janela de configuração
        if (_configWindow == null || !_configWindow.IsLoaded)
        {
            var backendService = _serviceProvider?.GetService<BackendService>();
            _configWindow = new ConfigWindow(backendService);
            _configWindow.ConfigClosed += OnConfigClosed;
        }
        
        _configWindow?.ShowConfig();
    }
    
    private void OnConfigClosed(object? sender, EventArgs e)
    {
        // Config fechado - sem ação necessária por enquanto
    }
    
    private void OnAboutRequest(object? sender, EventArgs e)
    {
        // Mostrar modal "Sobre" com visual liquid glass
        var aboutWindow = new Windows.AboutWindow();
        aboutWindow.ShowDialog();
    }
    
    private void OnExitRequest(object? sender, EventArgs e)
    {
        // Confirmação antes de sair com janela personalizada
        if (ConfirmationWindow.ShowExitConfirmation())
        {
            System.Windows.Application.Current.Shutdown();
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        // Desregistrar hotkeys globais
        if (_hwndSource != null && _invisibleWindow != null)
        {
            var helper = new WindowInteropHelper(_invisibleWindow);
            UnregisterHotKey(helper.Handle, HOTKEY_ID_CONFIG);
            UnregisterHotKey(helper.Handle, HOTKEY_ID_SCREENSHOT);
            _hwndSource.RemoveHook(HwndHook);
        }
        
        _invisibleWindow?.Close();
        _hotCornerService?.Stop();
        _systemTrayService?.Dispose();
        _backendProcessManager?.Dispose();
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}

