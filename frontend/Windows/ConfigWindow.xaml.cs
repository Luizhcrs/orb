using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using OrbAgent.Frontend.Services;
using OrbAgent.Frontend.Models;

namespace OrbAgent.Frontend.Windows
{
    public partial class ConfigWindow : Window
    {
        // Win32 API para Acrylic/Blur (Windows 11)
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
        
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
        private const int DWMWA_SYSTEMBACKDROP_TYPE = 38;
        private const int DWMSBT_TRANSIENTWINDOW = 3; // Acrylic
        private const int DWMWCP_ROUND = 2; // Cantos arredondados
        
        public event EventHandler? ConfigClosed;
        
        private BackendService? _backendService;
        private AppConfig _currentConfig = new();

        public ConfigWindow(BackendService? backendService = null)
        {
            InitializeComponent();
            _backendService = backendService;
            
            // Habilitar Acrylic
            this.SourceInitialized += OnSourceInitialized;
            
            // Limpar log anterior e iniciar novo
            LoggingService.ClearLog();
            LoggingService.LogInfo("ConfigWindow inicializado");
            
            // Carregar configurações (fire-and-forget)
            _ = LoadConfigAsync();
            
            // Carregar seção inicial
            LoadGeralSection();
            HighlightTab(GeralBtn);
        }
        
        private void OnSourceInitialized(object? sender, EventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            
            // Habilitar Dark Mode
            int darkMode = 1;
            DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref darkMode, sizeof(int));
            
            // Habilitar Cantos Arredondados
            int cornerPreference = DWMWCP_ROUND;
            DwmSetWindowAttribute(hwnd, DWMWA_WINDOW_CORNER_PREFERENCE, ref cornerPreference, sizeof(int));
            
            // Habilitar Acrylic Backdrop
            int backdropType = DWMSBT_TRANSIENTWINDOW;
            DwmSetWindowAttribute(hwnd, DWMWA_SYSTEMBACKDROP_TYPE, ref backdropType, sizeof(int));
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            CloseConfig();
        }

        private void CloseConfig()
        {
            var fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };
            
            fadeOut.Completed += (s, e) =>
            {
                this.Hide();
                ConfigClosed?.Invoke(this, EventArgs.Empty);
            };
            
            ConfigContainer.BeginAnimation(OpacityProperty, fadeOut);
        }

        public void ShowConfig()
        {
            ConfigContainer.Opacity = 0;
            this.Show();
            
            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new PowerEase { EasingMode = EasingMode.EaseOut, Power = 2 }
            };
            
            ConfigContainer.BeginAnimation(OpacityProperty, fadeIn);
        }

        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Escape)
            {
                CloseConfig();
            }
        }

        // Eventos dos Tabs
        private void GeralBtn_Click(object sender, RoutedEventArgs e)
        {
            LoadGeralSection();
            HighlightTab(GeralBtn);
        }

        private async void AgenteBtn_Click(object sender, RoutedEventArgs e)
        {
            // Aguardar configurações carregarem antes de mostrar a seção
            await LoadConfigAsync();
            LoadAgenteSection();
            HighlightTab(AgenteBtn);
        }

        private void HistoricoBtn_Click(object sender, RoutedEventArgs e)
        {
            LoggingService.LogDebug("HistoricoBtn_Click foi chamado!");
            try
            {
                LoadHistoricoSection();
                HighlightTab(HistoricoBtn);
                LoggingService.LogSuccess("HistoricoBtn_Click executado com sucesso!");
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Erro em HistoricoBtn_Click", ex);
            }
        }

        private void HighlightTab(System.Windows.Controls.Button activeButton)
        {
            // Reset all tabs
            GeralBtn.Background = System.Windows.Media.Brushes.Transparent;
            GeralBtn.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x99, 0xFF, 0xFF, 0xFF));
            AgenteBtn.Background = System.Windows.Media.Brushes.Transparent;
            AgenteBtn.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x99, 0xFF, 0xFF, 0xFF));
            HistoricoBtn.Background = System.Windows.Media.Brushes.Transparent;
            HistoricoBtn.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x99, 0xFF, 0xFF, 0xFF));
            
            // Highlight active
            activeButton.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x26, 0xFF, 0xFF, 0xFF));
            activeButton.Foreground = System.Windows.Media.Brushes.White;
        }

        // Carregar Seções
        private void LoadGeralSection()
        {
            ContentPanel.Children.Clear();
            
            AddSectionTitle("Configurações Gerais");
            
            // Tema (somente dark)
            AddLabel("Tema");
            AddComboBox(new[] { "Escuro" }, 0);
            
            AddSpacer();
            
            // Idioma
            AddLabel("Idioma");
            AddComboBox(new[] { "Português (BR)", "English" }, 0);
            
            AddSpacer();
            
            // Iniciar com Windows
            AddCheckBox("Iniciar com o Windows", false);
        }

        private void LoadAgenteSection()
        {
            LoggingService.LogInfo("LoadAgenteSection iniciado!");
            LoggingService.LogDebug($"_currentConfig é null? {_currentConfig == null}");
            
            if (_currentConfig != null)
            {
                LoggingService.LogDebug($"API Key: {_currentConfig.Agent?.ApiKey ?? "(null)"}");
                LoggingService.LogDebug($"Provider: {_currentConfig.Agent?.LlmProvider ?? "(null)"}");
            }
            
            ContentPanel.Children.Clear();
            
            AddSectionTitle("Configurações do Agente");
            
            // Provedor LLM (somente OpenAI, desabilitado)
            AddLabel("Provedor LLM");
            var providerCombo = AddComboBox(new[] { "OpenAI" }, 0);
            providerCombo.IsEnabled = false;
            providerCombo.Opacity = 0.5;
            providerCombo.Tag = "llm_provider";
            
            AddSpacer();
            
            // API Key com campo mascarado e botões Salvar/Redefinir
            AddApiKeyField();
            
            LoggingService.LogInfo("LoadAgenteSection concluído!");
        }

        private async void LoadHistoricoSection()
        {
            LoggingService.LogDebug("LoadHistoricoSection iniciado!");
            LoggingService.LogDebug($"ContentPanel é null? {ContentPanel == null}");
            LoggingService.LogDebug($"ContentPanel.Children.Count ANTES de limpar: {ContentPanel?.Children.Count ?? -1}");
            
            if (ContentPanel == null) return;
            
            ContentPanel.Children.Clear();
            LoggingService.LogDebug($"ContentPanel.Children.Count DEPOIS de limpar: {ContentPanel.Children.Count}");
            
            AddSectionTitle("Histórico de Conversas");
            LoggingService.LogDebug($"ContentPanel.Children.Count DEPOIS do título: {ContentPanel.Children.Count}");
            
            // Carregar histórico do backend
            try
            {
                LoggingService.LogDebug("Chamando LoadHistoryFromBackend...");
                await LoadHistoryFromBackend();
                LoggingService.LogSuccess("LoadHistoryFromBackend concluído!");
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Erro ao carregar histórico", ex);
                ShowEmptyHistoryState($"Erro ao carregar: {ex.Message}");
            }
        }
        
        private async Task LoadHistoryFromBackend()
        {
            try
            {
                if (_backendService == null)
                {
                    ShowEmptyHistoryState("Backend não disponível");
                    return;
                }
                
                System.Diagnostics.Debug.WriteLine(" Carregando sessões do histórico...");
                
                // Buscar sessões da API (igual ao Electron)
                using var httpClient = new System.Net.Http.HttpClient();
                httpClient.BaseAddress = new Uri(Config.AppSettings.BackendBaseUrl);
                
                var response = await httpClient.GetAsync("/api/v1/history/sessions?limit=50");
                
                if (!response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($" Erro ao buscar histórico: {response.StatusCode}");
                    ShowEmptyHistoryState("Erro ao carregar histórico");
                    return;
                }
                
                var jsonString = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($" JSON recebido: {jsonString.Substring(0, Math.Min(200, jsonString.Length))}...");
                
                // Deserializar de forma mais simples para evitar crashes
                var sessions = new List<HistorySession>();
                try
                {
                    var options = new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        Converters = { new DateTimeConverter() }
                    };
                    
                    sessions = System.Text.Json.JsonSerializer.Deserialize<List<HistorySession>>(jsonString, options) ?? new List<HistorySession>();
                }
                catch (Exception deserializeEx)
                {
                    System.Diagnostics.Debug.WriteLine($" Erro na deserialização: {deserializeEx.Message}");
                    
                    // Fallback: tentar deserializar sem conversor customizado
                    try
                    {
                        var fallbackOptions = new System.Text.Json.JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        };
                        sessions = System.Text.Json.JsonSerializer.Deserialize<List<HistorySession>>(jsonString, fallbackOptions) ?? new List<HistorySession>();
                    }
                    catch (Exception fallbackEx)
                    {
                        System.Diagnostics.Debug.WriteLine($" Fallback também falhou: {fallbackEx.Message}");
                        sessions = new List<HistorySession>();
                    }
                }
                
                if (sessions == null || sessions.Count == 0)
                {
                    LoggingService.LogInfo("Nenhuma conversa encontrada - mostrando empty state");
                    ShowEmptyHistoryState("Nenhuma conversa encontrada");
                    return;
                }
                
                LoggingService.LogSuccess($"{sessions.Count} sessões carregadas!");
                
                // Renderizar sessões
                LoggingService.LogDebug($"Iniciando renderização de {sessions.Count} sessões...");
                foreach (var session in sessions)
                {
                    LoggingService.LogDebug($"Renderizando sessão: {session.SessionId} - {session.Title}");
                    RenderHistoryItem(session);
                }
                LoggingService.LogSuccess($"Renderização completa! ContentPanel.Children.Count = {ContentPanel.Children.Count}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($" Erro ao carregar histórico: {ex.Message}");
                ShowEmptyHistoryState($"Erro: {ex.Message}");
            }
        }
        
        private void ShowEmptyHistoryState(string message)
        {
            var emptyMessage = new TextBlock
            {
                Text = message,
                Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x99, 0xFF, 0xFF, 0xFF)),
                FontSize = 14,
                Margin = new Thickness(0, 40, 0, 0),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };
            
            ContentPanel.Children.Add(emptyMessage);
        }
        
        private void RenderHistoryItem(HistorySession session)
        {
            LoggingService.LogDebug($" RenderHistoryItem INICIADO para {session.SessionId}");
            
            var item = new Border
            {
                Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x08, 0xFF, 0xFF, 0xFF)),
                BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x14, 0xFF, 0xFF, 0xFF)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(16),
                Margin = new Thickness(0, 0, 0, 12),
                Cursor = System.Windows.Input.Cursors.Hand
            };
            
            var itemContent = new StackPanel();
            
            // Header com data e botão delete
            var header = new Grid();
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            
            var dateText = new TextBlock
            {
                Text = FormatDate(session.UpdatedAt),
                FontSize = 12,
                Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x80, 0xFF, 0xFF, 0xFF)),
                FontWeight = FontWeights.Medium
            };
            Grid.SetColumn(dateText, 0);
            header.Children.Add(dateText);
            
            // Botão deletar com bordas arredondadas e sem hover azul
            var deleteBtn = CreateRoundedButton("×",
                System.Windows.Media.Brushes.Transparent,
                System.Windows.Media.Brushes.Transparent,
                new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x4D, 0xFF, 0xFF, 0xFF)));
            
            deleteBtn.Width = 24;
            deleteBtn.Height = 24;
            deleteBtn.FontSize = 16;
            
            // Hover effect (liquid glass - vermelho suave, não azul)
            deleteBtn.MouseEnter += (s, e) => {
                deleteBtn.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xFF, 0x96, 0x96));
                deleteBtn.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x1A, 0xFF, 0x64, 0x64));
            };
            deleteBtn.MouseLeave += (s, e) => {
                deleteBtn.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x4D, 0xFF, 0xFF, 0xFF));
                deleteBtn.Background = System.Windows.Media.Brushes.Transparent;
            };
            
            deleteBtn.Click += async (s, e) => {
                e.Handled = true;
                await DeleteHistorySession(session.SessionId);
            };
            Grid.SetColumn(deleteBtn, 1);
            header.Children.Add(deleteBtn);
            
            itemContent.Children.Add(header);
            
            // Título da conversa
            var title = new TextBlock
            {
                Text = session.Title ?? "Nova Conversa",
                FontSize = 14,
                Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xE6, 0xFF, 0xFF, 0xFF)),
                FontWeight = FontWeights.Medium,
                Margin = new Thickness(0, 12, 0, 6),
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            itemContent.Children.Add(title);
            
            // Metadados
            var meta = new TextBlock
            {
                Text = $"{session.MessageCount} mensagem{(session.MessageCount != 1 ? "ns" : "")}",
                FontSize = 12,
                Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x80, 0xFF, 0xFF, 0xFF))
            };
            itemContent.Children.Add(meta);
            
            item.Child = itemContent;
            
            // Hover effect
            item.MouseEnter += (s, e) => {
                item.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x0F, 0xFF, 0xFF, 0xFF));
                item.BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x26, 0xFF, 0xFF, 0xFF));
            };
            item.MouseLeave += (s, e) => {
                item.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x08, 0xFF, 0xFF, 0xFF));
                item.BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x14, 0xFF, 0xFF, 0xFF));
            };
            
            // Click para abrir sessão
            item.MouseLeftButtonDown += (s, e) => {
                e.Handled = true;
                try
                {
                    OpenHistorySession(session.SessionId);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($" Erro ao abrir sessão: {ex.Message}");
                    ShowErrorNotification("Erro ao abrir conversa");
                }
            };
            
            LoggingService.LogDebug($" Adicionando item ao ContentPanel...");
            ContentPanel.Children.Add(item);
            LoggingService.LogSuccess($" Item {session.SessionId} adicionado! Total de children: {ContentPanel.Children.Count}");
        }
        
        private string FormatDate(DateTime date)
        {
            var now = DateTime.Now;
            var diff = now - date;
            var timeStr = date.ToString("HH:mm");
            
            if (diff.TotalDays < 1)
            {
                return $"Hoje, {timeStr}";
            }
            else if (diff.TotalDays < 2)
            {
                return $"Ontem, {timeStr}";
            }
            else if (diff.TotalDays < 7)
            {
                var weekday = date.ToString("dddd", new System.Globalization.CultureInfo("pt-BR"));
                return $"{char.ToUpper(weekday[0]) + weekday.Substring(1)}, {timeStr}";
            }
            else
            {
                return date.ToString("dd MMM, HH:mm", new System.Globalization.CultureInfo("pt-BR"));
            }
        }
        
        private async Task DeleteHistorySession(string sessionId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($" Deletando sessão: {sessionId}");
                
                using var httpClient = new System.Net.Http.HttpClient();
                httpClient.BaseAddress = new Uri(Config.AppSettings.BackendBaseUrl);
                
                var response = await httpClient.DeleteAsync($"/api/v1/history/sessions/{sessionId}");
                
                if (response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine(" Sessão deletada!");
                    // Recarregar histórico
                    LoadHistoricoSection();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($" Erro ao deletar: {ex.Message}");
                ShowErrorNotification("Erro ao deletar conversa");
            }
        }
        
        private async void OpenHistorySession(string sessionId)
        {
            LoggingService.LogInfo($" Abrindo sessão no chat: {sessionId}");
            
            try
            {
                // Carregar histórico da sessão do backend
                LoggingService.LogDebug("Buscando histórico da sessão...");
                using var httpClient = new System.Net.Http.HttpClient();
                var response = await httpClient.GetAsync($"{Config.AppSettings.BackendBaseUrl}/api/v1/history/sessions/{sessionId}/messages");
                
                if (!response.IsSuccessStatusCode)
                {
                    LoggingService.LogError($"Erro ao buscar sessão: {response.StatusCode}");
                    ShowErrorNotification("Erro ao carregar conversa");
                    return;
                }
                
                var jsonString = await response.Content.ReadAsStringAsync();
                LoggingService.LogDebug($"Resposta recebida: {jsonString.Substring(0, Math.Min(200, jsonString.Length))}...");
                
                var sessionData = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(jsonString);
                
                // Fechar janela de configurações
                LoggingService.LogDebug("Fechando ConfigWindow...");
                this.Close();
                
                // Abrir ChatWindow
                LoggingService.LogInfo("Abrindo ChatWindow...");
                if (_backendService == null)
                {
                    LoggingService.LogError("BackendService é null! Não é possível abrir chat.");
                    return;
                }
                var chatWindow = new ChatWindow(_backendService);
                
                // Centralizar a janela ANTES de mostrar
                LoggingService.LogDebug("Centralizando ChatWindow...");
                chatWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                
                // Mostrar a janela PRIMEIRO
                chatWindow.Show();
                LoggingService.LogDebug("ChatWindow.Show() chamado");
                
                // Aguardar a animação de fade-in completar (800ms + margem de segurança)
                LoggingService.LogDebug("Aguardando animação de fade-in (1000ms)...");
                await System.Threading.Tasks.Task.Delay(1000);
                
                // AGORA carregar o histórico no chat (janela já está completamente visível!)
                LoggingService.LogInfo("Carregando histórico no chat APÓS animação...");
                await chatWindow.LoadHistorySession(sessionId, sessionData);
                
                LoggingService.LogSuccess($"Chat aberto com sessão {sessionId}!");
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Erro ao abrir sessão", ex);
                ShowErrorNotification($"Erro: {ex.Message}");
            }
        }

        // Helpers para criar elementos UI
        
        /// <summary>
        /// Adiciona campo de API Key (igual ao Electron - usa placeholder)
        /// </summary>
        private void AddApiKeyField()
        {
            AddLabel("API Key");
            
            // Debug: verificar estado atual
            LoggingService.LogDebug($"AddApiKeyField - _currentConfig é null: {_currentConfig == null}");
            if (_currentConfig != null)
            {
                LoggingService.LogDebug($"API Key length: {_currentConfig.Agent.ApiKey?.Length ?? 0}");
                LoggingService.LogDebug($"API Key is empty: {string.IsNullOrEmpty(_currentConfig.Agent.ApiKey)}");
            }
            
            // Verificar se já existe API Key (backend retorna mascarada como "***XXXX")
            var hasApiKey = _currentConfig != null && 
                           !string.IsNullOrEmpty(_currentConfig.Agent.ApiKey) && 
                           (_currentConfig.Agent.ApiKey.Length > 4 || _currentConfig.Agent.ApiKey.StartsWith("***"));
            
            LoggingService.LogDebug($"AddApiKeyField - hasApiKey: {hasApiKey}");
            
            string placeholder;
            if (hasApiKey && _currentConfig?.Agent?.ApiKey != null)
            {
                // Se já vem mascarada do backend, usar direto
                if (_currentConfig.Agent.ApiKey.StartsWith("***"))
                {
                    placeholder = _currentConfig.Agent.ApiKey;
                }
                else
                {
                    var last4 = _currentConfig.Agent.ApiKey.Substring(_currentConfig.Agent.ApiKey.Length - 4);
                    placeholder = "***" + last4;
                }
                LoggingService.LogDebug($"Placeholder: {placeholder}");
            }
            else
            {
                placeholder = "Insira sua API key...";
                LoggingService.LogDebug($"Placeholder: {placeholder}");
            }
            
            // Campo de API Key
            var apiKeyBox = AddPasswordBox(placeholder);
            apiKeyBox.Tag = "api_key";
            
            // Se já tem API key, DESABILITAR o campo e adicionar botões Redefinir + Salvar
            if (hasApiKey)
            {
                apiKeyBox.IsEnabled = false; // DESABILITAR completamente
                apiKeyBox.Password = ""; // Deixar vazio - só salva se usuário digite novo valor
                LoggingService.LogInfo("Campo API Key DESABILITADO (já existe API key)");
                
                // Botões Redefinir + Salvar
                AddResetApiKeyButton(apiKeyBox);
            }
            else
            {
                apiKeyBox.IsEnabled = true; // HABILITAR para edição
                LoggingService.LogInfo("Campo API Key HABILITADO (sem API key)");
                
                // Apenas botão Salvar (sem Redefinir, pois não há key para redefinir)
                AddSaveApiKeyButton();
            }
        }
        
        /// <summary>
        /// Adiciona apenas botão Salvar (quando não tem API key configurada)
        /// </summary>
        private void AddSaveApiKeyButton()
        {
            var saveButton = CreateRoundedButton("Salvar",
                new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x40, 0xFF, 0xFF, 0xFF)),
                new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x4D, 0xFF, 0xFF, 0xFF)),
                System.Windows.Media.Brushes.White);
            
            saveButton.Height = 40;
            saveButton.Margin = new Thickness(0, 8, 0, 16);
            
            saveButton.MouseEnter += (s, e) =>
            {
                saveButton.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x59, 0xFF, 0xFF, 0xFF));
                saveButton.BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x66, 0xFF, 0xFF, 0xFF));
            };
            
            saveButton.MouseLeave += (s, e) =>
            {
                saveButton.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x40, 0xFF, 0xFF, 0xFF));
                saveButton.BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x4D, 0xFF, 0xFF, 0xFF));
            };
            
            saveButton.Click += async (s, e) =>
            {
                try
                {
                    saveButton.IsEnabled = false;
                    saveButton.Content = "Salvando...";
                    
                    await SaveConfigAsync();
                    
                    saveButton.Content = "Salvo!";
                    await Task.Delay(1000);
                    
                    // Recarregar a seção para mostrar os botões corretos
                    LoadAgenteSection();
                }
                catch (Exception ex)
                {
                    LoggingService.LogError($"Erro ao salvar: {ex.Message}");
                    saveButton.Content = "Erro";
                    await Task.Delay(2000);
                    saveButton.Content = "Salvar";
                    saveButton.IsEnabled = true;
                }
            };
            
            ContentPanel.Children.Add(saveButton);
        }
        
        /// <summary>
        /// Adiciona botões Salvar e Redefinir lado a lado
        /// </summary>
        private void AddResetApiKeyButton(System.Windows.Controls.PasswordBox apiKeyBox)
        {
            // Container para os botões
            var buttonsContainer = new System.Windows.Controls.Grid
            {
                Margin = new Thickness(0, 8, 0, 16)
            };
            
            buttonsContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            buttonsContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) }); // Espaçamento
            buttonsContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            
            // Botão Redefinir
            var resetButton = CreateRoundedButton("Redefinir", 
                new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x33, 0xFF, 0x64, 0x64)),
                new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x4D, 0xFF, 0x64, 0x64)),
                new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xFF, 0x96, 0x96)));
            
            resetButton.MouseEnter += (s, e) =>
            {
                resetButton.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x4D, 0xFF, 0x64, 0x64));
                resetButton.BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x66, 0xFF, 0x64, 0x64));
            };
            
            resetButton.MouseLeave += (s, e) =>
            {
                resetButton.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x33, 0xFF, 0x64, 0x64));
                resetButton.BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x4D, 0xFF, 0x64, 0x64));
            };
            
            resetButton.Click += async (s, e) =>
            {
                try
                {
                    if (_currentConfig != null)
                    {
                        _currentConfig.Agent.ApiKey = string.Empty;
                    }
                    
                    await SaveConfigAsync();
                    LoadAgenteSection();
                    
                    LoggingService.LogInfo("API Key redefinida com sucesso!");
                }
                catch (Exception ex)
                {
                    LoggingService.LogError($"Erro ao redefinir API Key: {ex.Message}");
                }
            };
            
            Grid.SetColumn(resetButton, 0);
            buttonsContainer.Children.Add(resetButton);
            
            // Botão Salvar
            var saveButton = CreateRoundedButton("Salvar",
                new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x40, 0xFF, 0xFF, 0xFF)),
                new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x4D, 0xFF, 0xFF, 0xFF)),
                System.Windows.Media.Brushes.White);
            
            saveButton.MouseEnter += (s, e) =>
            {
                saveButton.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x59, 0xFF, 0xFF, 0xFF));
                saveButton.BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x66, 0xFF, 0xFF, 0xFF));
            };
            
            saveButton.MouseLeave += (s, e) =>
            {
                saveButton.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x40, 0xFF, 0xFF, 0xFF));
                saveButton.BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x4D, 0xFF, 0xFF, 0xFF));
            };
            
            saveButton.Click += async (s, e) =>
            {
                try
                {
                    saveButton.IsEnabled = false;
                    saveButton.Content = "Salvando...";
                    
                    await SaveConfigAsync();
                    
                    saveButton.Content = "Salvo!";
                    await Task.Delay(1000);
                    saveButton.Content = "Salvar";
                }
                catch (Exception ex)
                {
                    LoggingService.LogError($"Erro ao salvar: {ex.Message}");
                    saveButton.Content = "Erro";
                    await Task.Delay(2000);
                    saveButton.Content = "Salvar";
                }
                finally
                {
                    saveButton.IsEnabled = true;
                }
            };
            
            Grid.SetColumn(saveButton, 2);
            buttonsContainer.Children.Add(saveButton);
            
            ContentPanel.Children.Add(buttonsContainer);
        }
        
        /// <summary>
        /// Cria botão com bordas arredondadas e sem hover azul
        /// </summary>
        private System.Windows.Controls.Button CreateRoundedButton(string content, System.Windows.Media.Brush background, System.Windows.Media.Brush border, System.Windows.Media.Brush foreground)
        {
            var button = new System.Windows.Controls.Button
            {
                Content = content,
                Height = 40,
                FontSize = 14,
                FontWeight = FontWeights.Medium,
                Background = background,
                BorderBrush = border,
                BorderThickness = new Thickness(1),
                Foreground = foreground,
                Cursor = System.Windows.Input.Cursors.Hand
            };
            
            // Template customizado para bordas arredondadas e sem hover azul
            var template = new ControlTemplate(typeof(System.Windows.Controls.Button));
            var borderFactory = new FrameworkElementFactory(typeof(Border));
            borderFactory.Name = "border";
            borderFactory.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(System.Windows.Controls.Button.BackgroundProperty));
            borderFactory.SetValue(Border.BorderBrushProperty, new TemplateBindingExtension(System.Windows.Controls.Button.BorderBrushProperty));
            borderFactory.SetValue(Border.BorderThicknessProperty, new TemplateBindingExtension(System.Windows.Controls.Button.BorderThicknessProperty));
            borderFactory.SetValue(Border.CornerRadiusProperty, new CornerRadius(8));
            
            var contentPresenterFactory = new FrameworkElementFactory(typeof(ContentPresenter));
            contentPresenterFactory.SetValue(ContentPresenter.HorizontalAlignmentProperty, System.Windows.HorizontalAlignment.Center);
            contentPresenterFactory.SetValue(ContentPresenter.VerticalAlignmentProperty, System.Windows.VerticalAlignment.Center);
            
            borderFactory.AppendChild(contentPresenterFactory);
            template.VisualTree = borderFactory;
            
            button.Template = template;
            
            return button;
        }
        
        private void AddSectionTitle(string title)
        {
            var textBlock = new TextBlock
            {
                Text = title,
                FontSize = 20,
                FontWeight = FontWeights.SemiBold,
                Foreground = System.Windows.Media.Brushes.White,
                Margin = new Thickness(0, 0, 0, 20)
            };
            ContentPanel.Children.Add(textBlock);
        }

        private void AddLabel(string text)
        {
            var label = new TextBlock
            {
                Text = text,
                FontSize = 14,
                Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xCC, 0xFF, 0xFF, 0xFF)),
                Margin = new Thickness(0, 0, 0, 8)
            };
            ContentPanel.Children.Add(label);
        }

        private System.Windows.Controls.ComboBox AddComboBox(string[] items, int selectedIndex)
        {
            var combo = new System.Windows.Controls.ComboBox
            {
                Style = (Style)FindResource("GlassComboBoxStyle"),
                Margin = new Thickness(0, 0, 0, 16)
            };
            
            foreach (var item in items)
            {
                combo.Items.Add(item);
            }
            
            if (items.Length > 0)
            {
                combo.SelectedIndex = selectedIndex;
            }
            
            ContentPanel.Children.Add(combo);
            return combo;
        }

        private PasswordBox AddPasswordBox(string placeholder)
        {
            var passwordBox = new PasswordBox
            {
                // Usar estilo genérico ou criar um específico para PasswordBox
                Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x26, 0xFF, 0xFF, 0xFF)),
                BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x4D, 0xFF, 0xFF, 0xFF)),
                BorderThickness = new Thickness(1),
                Foreground = System.Windows.Media.Brushes.White,
                FontSize = 14,
                Padding = new Thickness(12, 8, 12, 8),
                Margin = new Thickness(0, 0, 0, 16)
            };
            
            // Criar TextBlock para placeholder
            var placeholderText = new TextBlock
            {
                Text = placeholder,
                Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x66, 0xFF, 0xFF, 0xFF)),
                IsHitTestVisible = false,
                Margin = new Thickness(12, 8, 0, 0) // Ajustar para alinhar com o conteúdo
            };
            
            // Adicionar placeholder como overlay
            var grid = new Grid();
            grid.Children.Add(passwordBox);
            grid.Children.Add(placeholderText);
            
            // Event handlers para mostrar/esconder placeholder
            passwordBox.GotFocus += (s, e) =>
            {
                placeholderText.Visibility = Visibility.Collapsed;
            };
            
            passwordBox.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(passwordBox.Password))
                {
                    placeholderText.Visibility = Visibility.Visible;
                }
            };
            
            ContentPanel.Children.Add(grid);
            return passwordBox;
        }

        private System.Windows.Controls.CheckBox AddCheckBox(string content, bool isChecked)
        {
            var checkBox = new System.Windows.Controls.CheckBox
            {
                Content = content,
                IsChecked = isChecked,
                Style = (Style)FindResource("GlassCheckBoxStyle"),
                Margin = new Thickness(0, 0, 0, 16)
            };
            
            ContentPanel.Children.Add(checkBox);
            return checkBox;
        }

        private void AddSpacer()
        {
            var spacer = new Border
            {
                Height = 20
            };
            ContentPanel.Children.Add(spacer);
        }

        /// <summary>
        /// Adiciona uma linha de configuração (igual ao Electron)
        /// </summary>
        private void AddSettingRow(string label, Func<FrameworkElement> controlFactory)
        {
            var row = new Grid
            {
                Margin = new Thickness(0, 0, 0, 0)
            };
            
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200) });
            
            // Label
            var labelText = new TextBlock
            {
                Text = label,
                Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xE6, 0xFF, 0xFF, 0xFF)),
                FontSize = 14,
                FontWeight = FontWeights.Medium,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 16, 0, 16)
            };
            Grid.SetColumn(labelText, 0);
            row.Children.Add(labelText);
            
            // Control
            var control = controlFactory();
            control.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(control, 1);
            row.Children.Add(control);
            
            // Linha separadora
            var separator = new Border
            {
                Height = 1,
                Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x0D, 0xFF, 0xFF, 0xFF)),
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(0, 0, 0, 0)
            };
            Grid.SetColumnSpan(separator, 2);
            row.Children.Add(separator);
            
            ContentPanel.Children.Add(row);
        }

        /// <summary>
        /// Adiciona botões do footer (igual ao Electron)
        /// </summary>
        private void AddFooterButtons()
        {
            // Footer container
            var footer = new Border
            {
                Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x33, 0x00, 0x00, 0x00)),
                BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x0D, 0xFF, 0xFF, 0xFF)),
                BorderThickness = new Thickness(0, 1, 0, 0),
                Padding = new Thickness(30, 20, 30, 20),
                Margin = new Thickness(-30, 20, -30, -20) // Estender para as bordas
            };
            
            var footerGrid = new Grid();
            footerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            footerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            footerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
            footerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            
            // Botão Redefinir com Border arredondado
            var resetBorder = new Border
            {
                Width = 100,
                Height = 40,
                Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x0D, 0xFF, 0xFF, 0xFF)),
                BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x1A, 0xFF, 0xFF, 0xFF)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10)
            };
            
            var resetBtn = new System.Windows.Controls.Button
            {
                Content = "Redefinir",
                Background = System.Windows.Media.Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xCC, 0xFF, 0xFF, 0xFF)),
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Cursor = System.Windows.Input.Cursors.Hand
            };
            
            // Efeitos hover Redefinir
            resetBorder.MouseEnter += (s, e) => {
                resetBorder.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x14, 0xFF, 0xFF, 0xFF));
                resetBorder.BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x33, 0xFF, 0xFF, 0xFF));
            };
            resetBorder.MouseLeave += (s, e) => {
                resetBorder.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x0D, 0xFF, 0xFF, 0xFF));
                resetBorder.BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x1A, 0xFF, 0xFF, 0xFF));
            };
            
            resetBorder.Child = resetBtn;
            Grid.SetColumn(resetBorder, 1);
            footerGrid.Children.Add(resetBorder);
            
            // Botão Salvar com Border arredondado
            var saveBorder = new Border
            {
                Width = 100,
                Height = 40,
                Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x1F, 0xFF, 0xFF, 0xFF)),
                BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x33, 0xFF, 0xFF, 0xFF)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10)
            };
            
            var saveBtn = new System.Windows.Controls.Button
            {
                Content = "Salvar",
                Background = System.Windows.Media.Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Foreground = System.Windows.Media.Brushes.White,
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Cursor = System.Windows.Input.Cursors.Hand
            };
            
            // Efeitos hover Salvar
            saveBorder.MouseEnter += (s, e) => {
                saveBorder.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x28, 0xFF, 0xFF, 0xFF));
                saveBorder.BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x40, 0xFF, 0xFF, 0xFF));
            };
            saveBorder.MouseLeave += (s, e) => {
                saveBorder.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x1F, 0xFF, 0xFF, 0xFF));
                saveBorder.BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x33, 0xFF, 0xFF, 0xFF));
            };
            
            saveBorder.Child = saveBtn;
            Grid.SetColumn(saveBorder, 3);
            footerGrid.Children.Add(saveBorder);
            
            // Event handlers
            saveBtn.Click += async (s, e) =>
            {
                try
                {
                    saveBtn.IsEnabled = false;
                    saveBtn.Content = "Salvando...";
                    
                    await SaveConfigAsync();
                    
                    saveBtn.Content = "Salvo!";
                    await Task.Delay(1000);
                    saveBtn.Content = "Salvar";
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erro ao salvar: {ex.Message}");
                    saveBtn.Content = "Erro";
                    await Task.Delay(2000);
                    saveBtn.Content = "Salvar";
                }
                finally
                {
                    saveBtn.IsEnabled = true;
                }
            };
            
            footer.Child = footerGrid;
            ContentPanel.Children.Add(footer);
        }

        /// <summary>
        /// Adiciona botão de salvar configurações (método antigo - mantido para compatibilidade)
        /// </summary>
        private System.Windows.Controls.Button AddSaveButton()
        {
            var saveButton = CreateRoundedButton("Salvar",
                new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x40, 0xFF, 0xFF, 0xFF)),
                new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x4D, 0xFF, 0xFF, 0xFF)),
                System.Windows.Media.Brushes.White);
            
            saveButton.Height = 45;
            saveButton.FontSize = 15;
            saveButton.FontWeight = FontWeights.SemiBold;
            saveButton.Margin = new Thickness(0, 0, 0, 16);

            // Efeitos hover (liquid glass - não azul)
            saveButton.MouseEnter += (s, e) =>
            {
                saveButton.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x59, 0xFF, 0xFF, 0xFF));
                saveButton.BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x66, 0xFF, 0xFF, 0xFF));
            };

            saveButton.MouseLeave += (s, e) =>
            {
                saveButton.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x40, 0xFF, 0xFF, 0xFF));
                saveButton.BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x4D, 0xFF, 0xFF, 0xFF));
            };

            // Event handler para salvar
            saveButton.Click += async (s, e) =>
            {
                try
                {
                    saveButton.IsEnabled = false;
                    saveButton.Content = "Salvando...";
                    
                    await SaveConfigAsync();
                    
                    saveButton.Content = "Salvo!";
                    await Task.Delay(1000);
                    saveButton.Content = "Salvar";
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erro ao salvar: {ex.Message}");
                    saveButton.Content = "Erro ao salvar";
                    await Task.Delay(2000);
                    saveButton.Content = "Salvar";
                }
                finally
                {
                    saveButton.IsEnabled = true;
                }
            };

            ContentPanel.Children.Add(saveButton);
            return saveButton;
        }

        /// <summary>
        /// Carrega configurações do backend
        /// </summary>
        private async Task LoadConfigAsync()
        {
            if (_backendService == null) return;

            try
            {
                System.Diagnostics.Debug.WriteLine(" Carregando configurações do backend...");
                var configJson = await _backendService.GetConfigAsync();
                
                if (configJson != null)
                {
                    // Parsear JSON para AppConfig
                    var jsonString = System.Text.Json.JsonSerializer.Serialize(configJson);
                    LoggingService.LogDebug($"JSON recebido: {jsonString}");
                    
                    var loadedConfig = System.Text.Json.JsonSerializer.Deserialize<AppConfig>(jsonString);
                    
                    if (loadedConfig != null)
                    {
                        _currentConfig = loadedConfig;
                        var hasApiKey = !string.IsNullOrEmpty(_currentConfig.Agent.ApiKey);
                        LoggingService.LogInfo($"Config carregada! API Key: {(hasApiKey ? "SIM (" + _currentConfig.Agent.ApiKey.Length + " chars)" : "NÃO")}");
                        
                        if (hasApiKey)
                        {
                            var last4 = _currentConfig.Agent.ApiKey.Substring(_currentConfig.Agent.ApiKey.Length - 4);
                            LoggingService.LogDebug($"Últimos 4 chars: {last4}");
                        }
                    }
                    else
                    {
                        LoggingService.LogError("Falha ao deserializar configuração - loadedConfig é null");
                    }
                }
                else
                {
                    LoggingService.LogError("configJson é null - backend não retornou configurações");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($" Erro ao carregar configurações: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($" Stack trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Salva configurações no backend
        /// </summary>
        private async Task SaveConfigAsync()
        {
            if (_backendService == null) return;

            try
            {
                System.Diagnostics.Debug.WriteLine(" INICIANDO SALVAMENTO...");
                
                // Coletar configurações da UI
                CollectConfigFromUI();
                
                System.Diagnostics.Debug.WriteLine($"API Key coletada: {(_currentConfig.Agent.ApiKey?.Length > 0 ? "SIM (" + _currentConfig.Agent.ApiKey.Length + " chars)" : "NÃO")}");
                System.Diagnostics.Debug.WriteLine($"Provider: {_currentConfig.Agent.LlmProvider}");
                System.Diagnostics.Debug.WriteLine($"Model: {_currentConfig.Agent.Model}");
                
                // Salvar no backend
                System.Diagnostics.Debug.WriteLine(" Enviando para backend...");
                var success = await _backendService.SaveConfigAsync(_currentConfig);
                if (success)
                {
                    System.Diagnostics.Debug.WriteLine(" Configurações salvas com sucesso!");
                    ShowSuccessNotification("Configurações salvas com sucesso!");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine(" Backend retornou FALSE");
                    ShowErrorNotification("Backend não confirmou o salvamento");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($" ERRO: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack: {ex.StackTrace}");
                ShowErrorNotification($"Erro ao salvar: {ex.Message}");
            }
        }

        /// <summary>
        /// Coleta configurações da UI
        /// </summary>
        private void CollectConfigFromUI()
        {
            // Coletar configurações do agente (API Key, etc.)
            foreach (var child in ContentPanel.Children)
            {
                if (child is Grid grid)
                {
                    // Procurar controles dentro do Grid (podem estar dentro de Border)
                    foreach (var gridChild in grid.Children)
                    {
                        // Caso 1: PasswordBox direto
                        if (gridChild is PasswordBox passwordBox && passwordBox.Tag?.ToString() == "api_key")
                        {
                            _currentConfig.Agent.ApiKey = passwordBox.Password;
                            System.Diagnostics.Debug.WriteLine($"API Key coletada: {passwordBox.Password.Substring(0, Math.Min(10, passwordBox.Password.Length))}...");
                        }
                        // Caso 2: PasswordBox dentro de Border
                        else if (gridChild is Border border && border.Child is PasswordBox borderPasswordBox && borderPasswordBox.Tag?.ToString() == "api_key")
                        {
                            _currentConfig.Agent.ApiKey = borderPasswordBox.Password;
                            System.Diagnostics.Debug.WriteLine($"API Key coletada (border): {borderPasswordBox.Password.Substring(0, Math.Min(10, borderPasswordBox.Password.Length))}...");
                        }
                        // Caso 3: ComboBox direto (llm_provider)
                        else if (gridChild is System.Windows.Controls.ComboBox comboBox && comboBox.Tag?.ToString() == "llm_provider")
                        {
                            _currentConfig.Agent.LlmProvider = comboBox.SelectedItem?.ToString() ?? "OpenAI";
                        }
                        // Caso 4: ComboBox dentro de Border (llm_provider)
                        else if (gridChild is Border borderCombo && borderCombo.Child is System.Windows.Controls.ComboBox borderComboBox && borderComboBox.Tag?.ToString() == "llm_provider")
                        {
                            _currentConfig.Agent.LlmProvider = borderComboBox.SelectedItem?.ToString() ?? "OpenAI";
                        }
                        // Modelo removido - só provider e API key
                    }
                }
            }
        }

        /// <summary>
        /// Testa API key
        /// </summary>
        private async Task<bool> TestApiKeyAsync(string apiKey)
        {
            if (_backendService == null) return false;

            try
            {
                return await _backendService.TestApiKeyAsync(apiKey);
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Mostra notificação de sucesso com estilo liquid glass
        /// </summary>
        private void ShowSuccessNotification(string message)
        {
            var notification = new Border
            {
                Width = 300,
                Height = 80,
                Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xD9, 0x14, 0x14, 0x16)),
                BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x4D, 0x00, 0xFF, 0x00)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(12),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Top,
                Margin = new Thickness(0, 20, 0, 0),
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = System.Windows.Media.Colors.Black,
                    BlurRadius = 20,
                    ShadowDepth = 0,
                    Opacity = 0.5
                }
            };
            
            var text = new TextBlock
            {
                Text = message,
                Foreground = System.Windows.Media.Brushes.White,
                FontSize = 14,
                FontWeight = FontWeights.Medium,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(20)
            };
            
            notification.Child = text;
            
            // Adicionar ao painel principal (pode ser Grid, StackPanel, etc.)
            if (this.Content is System.Windows.Controls.Panel panel)
            {
                panel.Children.Add(notification);
                System.Windows.Controls.Panel.SetZIndex(notification, 1000);
                
                // Auto-remover após 2 segundos
                var timer = new System.Windows.Threading.DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(2)
                };
                timer.Tick += (s, e) =>
                {
                    panel.Children.Remove(notification);
                    timer.Stop();
                };
                timer.Start();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine(" Não foi possível adicionar notificação de sucesso - Content não é Panel");
            }
        }
        
        /// <summary>
        /// Mostra notificação de erro com estilo liquid glass
        /// </summary>
        private void ShowErrorNotification(string message)
        {
            var notification = new Border
            {
                Width = 300,
                MinHeight = 80,
                Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xD9, 0x14, 0x14, 0x16)),
                BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x4D, 0xFF, 0x64, 0x64)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(12),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Top,
                Margin = new Thickness(0, 20, 0, 0),
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = System.Windows.Media.Colors.Black,
                    BlurRadius = 20,
                    ShadowDepth = 0,
                    Opacity = 0.5
                }
            };
            
            var text = new TextBlock
            {
                Text = message,
                Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0xFF, 0xAA, 0xAA)),
                FontSize = 14,
                FontWeight = FontWeights.Medium,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(20)
            };
            
            notification.Child = text;
            
            // Adicionar ao painel principal (pode ser Grid, StackPanel, etc.)
            if (this.Content is System.Windows.Controls.Panel panel)
            {
                panel.Children.Add(notification);
                System.Windows.Controls.Panel.SetZIndex(notification, 1000);
                
                // Auto-remover após 3 segundos (mais tempo para ler erro)
                var timer = new System.Windows.Threading.DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(3)
                };
                timer.Tick += (s, e) =>
                {
                    panel.Children.Remove(notification);
                    timer.Stop();
                };
                timer.Start();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine(" Não foi possível adicionar notificação de erro - Content não é Panel");
            }
        }
    }
}

