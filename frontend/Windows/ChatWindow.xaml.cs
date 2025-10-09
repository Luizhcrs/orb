using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using OrbAgent.Frontend.Models;
using OrbAgent.Frontend.Services;

namespace OrbAgent.Frontend.Windows
{
    public partial class ChatWindow : Window
    {
        private readonly BackendService _backendService;
        private bool _isExpanded = false;
        private string? _currentScreenshot;
        private string? _currentSessionId;
        private const int COMPACT_WIDTH = 380;
        private const int COMPACT_HEIGHT = 512;
        private const int EXPANDED_WIDTH = 660;
        private const int EXPANDED_HEIGHT = 792;
        
        // Win32 API para Acrylic/Blur (Windows 11)
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
        
        [DllImport("dwmapi.dll")]
        private static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS margins);
        
        // Win32 API para controlar Z-Index da janela
        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_SHOWWINDOW = 0x0040;
        
        [StructLayout(LayoutKind.Sequential)]
        private struct MARGINS
        {
            public int cxLeftWidth;
            public int cxRightWidth;
            public int cyTopHeight;
            public int cyBottomHeight;
        }
        
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
        private const int DWMWA_SYSTEMBACKDROP_TYPE = 38;
        private const int DWMSBT_MAINWINDOW = 2; // Mica
        private const int DWMSBT_TRANSIENTWINDOW = 3; // Acrylic
        private const int DWMSBT_TABBEDWINDOW = 4; // Tabbed
        private const int DWMWCP_ROUND = 2; // Cantos arredondados

        public event EventHandler? ChatClosed;

        public ChatWindow(BackendService backendService)
        {
            Services.LoggingService.LogInfo(" ChatWindow construtor chamado");
            InitializeComponent();
            _backendService = backendService;
            
            Services.LoggingService.LogDebug("InitializeComponent concluído");
            
            // FORÇAR VISIBILIDADE IMEDIATA (sem animação)
            ChatContainer.Opacity = 1.0;
            Services.LoggingService.LogDebug("ChatContainer.Opacity definido como 1.0");
            
            // Habilitar transparência real após carregamento
            this.SourceInitialized += OnSourceInitialized;
            this.Loaded += ChatWindow_Loaded;
        }
        
        private void ChatWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Services.LoggingService.LogInfo(" ChatWindow.Loaded evento disparado");
            Services.LoggingService.LogDebug($"ChatContainer é null? {ChatContainer == null}");
            Services.LoggingService.LogDebug($"MessagesPanel é null? {MessagesPanel == null}");
            Services.LoggingService.LogDebug($"MessagesScrollViewer é null? {MessagesScrollViewer == null}");
            Services.LoggingService.LogDebug($"ExpandBtn é null? {ExpandBtn == null}");
            Services.LoggingService.LogDebug($"CloseBtn é null? {CloseBtn == null}");
            
            // Verificar tamanho da janela
            Services.LoggingService.LogDebug($"Window Width: {this.Width}, Height: {this.Height}");
            Services.LoggingService.LogDebug($"Window Position: Left={this.Left}, Top={this.Top}");
            Services.LoggingService.LogDebug($"WindowState: {this.WindowState}");
            
            // NOVA SESSÃO A CADA ABERTURA (se não veio do histórico)
            if (string.IsNullOrEmpty(_currentSessionId))
            {
                _currentSessionId = Guid.NewGuid().ToString();
                Services.LoggingService.LogInfo($" Nova sessão criada no Loaded: {_currentSessionId}");
            }
            else
            {
                Services.LoggingService.LogInfo($" Usando sessão existente: {_currentSessionId}");
            }
        }
        
        // INDICADOR DE DIGITAÇÃO (3 pontos animados)
        private void ShowTypingIndicator()
        {
            try
            {
                Services.LoggingService.LogDebug(" Mostrando indicador de digitação...");
                
                // CRIAR INDICADOR PROGRAMATICAMENTE (sempre no final)
                var typingIndicator = CreateTypingIndicator();
                
                // Adicionar ao FINAL do MessagesPanel
                MessagesPanel.Children.Add(typingIndicator);
                
                Services.LoggingService.LogDebug($"Indicador adicionado ao MessagesPanel. Total children: {MessagesPanel.Children.Count}");
                
                // Iniciar animação dos pontos
                StartTypingAnimation(typingIndicator);
                
                // Scroll para o final para mostrar o indicador
                MessagesScrollViewer.ScrollToEnd();
                
                Services.LoggingService.LogSuccess(" Indicador de digitação mostrado com sucesso!");
            }
            catch (Exception ex)
            {
                Services.LoggingService.LogError("Erro ao mostrar indicador de digitação", ex);
            }
        }
        
        private void HideTypingIndicator()
        {
            try
            {
                Services.LoggingService.LogDebug(" Escondendo indicador de digitação...");
                
                // REMOVER INDICADOR DO PANEL (se existir)
                var typingIndicator = MessagesPanel.Children.OfType<Border>()
                    .FirstOrDefault(b => b.Name == "TypingIndicator");
                
                if (typingIndicator != null)
                {
                    MessagesPanel.Children.Remove(typingIndicator);
                    Services.LoggingService.LogDebug("Indicador removido do MessagesPanel");
                }
                else
                {
                    Services.LoggingService.LogDebug("Nenhum indicador encontrado para remover");
                }
            }
            catch (Exception ex)
            {
                Services.LoggingService.LogError("Erro ao esconder indicador de digitação", ex);
            }
        }
        
        private Border CreateTypingIndicator()
        {
            var typingIndicator = new Border
            {
                Name = "TypingIndicator",
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                MaxWidth = 200,
                Margin = new Thickness(0, 8, 0, 0),
                Background = new LinearGradientBrush
                {
                    StartPoint = new System.Windows.Point(0, 0),
                    EndPoint = new System.Windows.Point(1, 1),
                    GradientStops = new GradientStopCollection
                    {
                        new GradientStop(System.Windows.Media.Color.FromArgb(46, 255, 255, 255), 0),
                        new GradientStop(System.Windows.Media.Color.FromArgb(38, 255, 255, 255), 1)
                    }
                },
                BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(64, 255, 255, 255)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(18),
                Effect = new DropShadowEffect
                {
                    Color = System.Windows.Media.Colors.Black,
                    BlurRadius = 8,
                    ShadowDepth = 2,
                    Opacity = 0.15
                }
            };
            
            var dotsPanel = new StackPanel
            {
                Orientation = System.Windows.Controls.Orientation.Horizontal,
                Margin = new Thickness(12),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center
            };
            
            // 3 PONTOS ANIMADOS
            var dot1 = new System.Windows.Shapes.Ellipse
            {
                Name = "TypingDot1",
                Width = 8,
                Height = 8,
                Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(204, 255, 255, 255)),
                Margin = new Thickness(0, 0, 4, 0),
                RenderTransform = new ScaleTransform(0.8, 0.8)
            };
            
            var dot2 = new System.Windows.Shapes.Ellipse
            {
                Name = "TypingDot2",
                Width = 8,
                Height = 8,
                Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(204, 255, 255, 255)),
                Margin = new Thickness(0, 0, 4, 0),
                RenderTransform = new ScaleTransform(0.8, 0.8)
            };
            
            var dot3 = new System.Windows.Shapes.Ellipse
            {
                Name = "TypingDot3",
                Width = 8,
                Height = 8,
                Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(204, 255, 255, 255)),
                RenderTransform = new ScaleTransform(0.8, 0.8)
            };
            
            dotsPanel.Children.Add(dot1);
            dotsPanel.Children.Add(dot2);
            dotsPanel.Children.Add(dot3);
            
            typingIndicator.Child = dotsPanel;
            
            return typingIndicator;
        }
        
        private void StartTypingAnimation(Border typingIndicator)
        {
            try
            {
                // Encontrar os pontos dentro do indicador
                var dotsPanel = typingIndicator.Child as StackPanel;
                if (dotsPanel?.Children.Count >= 3)
                {
                    var dot1 = dotsPanel.Children[0] as System.Windows.Shapes.Ellipse;
                    var dot2 = dotsPanel.Children[1] as System.Windows.Shapes.Ellipse;
                    var dot3 = dotsPanel.Children[2] as System.Windows.Shapes.Ellipse;
                    
                    if (dot1?.RenderTransform is ScaleTransform scale1 &&
                        dot2?.RenderTransform is ScaleTransform scale2 &&
                        dot3?.RenderTransform is ScaleTransform scale3)
                    {
                        // Animação do ponto 1 (delay 0s)
                        var storyboard1 = new System.Windows.Media.Animation.Storyboard();
                        var animation1 = new System.Windows.Media.Animation.DoubleAnimation
                        {
                            From = 0.8,
                            To = 1.0,
                            Duration = TimeSpan.FromMilliseconds(700),
                            RepeatBehavior = System.Windows.Media.Animation.RepeatBehavior.Forever,
                            AutoReverse = true
                        };
                        System.Windows.Media.Animation.Storyboard.SetTarget(animation1, scale1);
                        System.Windows.Media.Animation.Storyboard.SetTargetProperty(animation1, new PropertyPath("ScaleX"));
                        storyboard1.Children.Add(animation1);
                        
                        var animation1Y = new System.Windows.Media.Animation.DoubleAnimation
                        {
                            From = 0.8,
                            To = 1.0,
                            Duration = TimeSpan.FromMilliseconds(700),
                            RepeatBehavior = System.Windows.Media.Animation.RepeatBehavior.Forever,
                            AutoReverse = true
                        };
                        System.Windows.Media.Animation.Storyboard.SetTarget(animation1Y, scale1);
                        System.Windows.Media.Animation.Storyboard.SetTargetProperty(animation1Y, new PropertyPath("ScaleY"));
                        storyboard1.Children.Add(animation1Y);
                        storyboard1.Begin();
                        
                        // Animação do ponto 2 (delay 0.16s)
                        var storyboard2 = new System.Windows.Media.Animation.Storyboard();
                        var animation2 = new System.Windows.Media.Animation.DoubleAnimation
                        {
                            From = 0.8,
                            To = 1.0,
                            Duration = TimeSpan.FromMilliseconds(700),
                            BeginTime = TimeSpan.FromMilliseconds(160),
                            RepeatBehavior = System.Windows.Media.Animation.RepeatBehavior.Forever,
                            AutoReverse = true
                        };
                        System.Windows.Media.Animation.Storyboard.SetTarget(animation2, scale2);
                        System.Windows.Media.Animation.Storyboard.SetTargetProperty(animation2, new PropertyPath("ScaleX"));
                        storyboard2.Children.Add(animation2);
                        
                        var animation2Y = new System.Windows.Media.Animation.DoubleAnimation
                        {
                            From = 0.8,
                            To = 1.0,
                            Duration = TimeSpan.FromMilliseconds(700),
                            BeginTime = TimeSpan.FromMilliseconds(160),
                            RepeatBehavior = System.Windows.Media.Animation.RepeatBehavior.Forever,
                            AutoReverse = true
                        };
                        System.Windows.Media.Animation.Storyboard.SetTarget(animation2Y, scale2);
                        System.Windows.Media.Animation.Storyboard.SetTargetProperty(animation2Y, new PropertyPath("ScaleY"));
                        storyboard2.Children.Add(animation2Y);
                        storyboard2.Begin();
                        
                        // Animação do ponto 3 (delay 0.32s)
                        var storyboard3 = new System.Windows.Media.Animation.Storyboard();
                        var animation3 = new System.Windows.Media.Animation.DoubleAnimation
                        {
                            From = 0.8,
                            To = 1.0,
                            Duration = TimeSpan.FromMilliseconds(700),
                            BeginTime = TimeSpan.FromMilliseconds(320),
                            RepeatBehavior = System.Windows.Media.Animation.RepeatBehavior.Forever,
                            AutoReverse = true
                        };
                        System.Windows.Media.Animation.Storyboard.SetTarget(animation3, scale3);
                        System.Windows.Media.Animation.Storyboard.SetTargetProperty(animation3, new PropertyPath("ScaleX"));
                        storyboard3.Children.Add(animation3);
                        
                        var animation3Y = new System.Windows.Media.Animation.DoubleAnimation
                        {
                            From = 0.8,
                            To = 1.0,
                            Duration = TimeSpan.FromMilliseconds(700),
                            BeginTime = TimeSpan.FromMilliseconds(320),
                            RepeatBehavior = System.Windows.Media.Animation.RepeatBehavior.Forever,
                            AutoReverse = true
                        };
                        System.Windows.Media.Animation.Storyboard.SetTarget(animation3Y, scale3);
                        System.Windows.Media.Animation.Storyboard.SetTargetProperty(animation3Y, new PropertyPath("ScaleY"));
                        storyboard3.Children.Add(animation3Y);
                        storyboard3.Begin();
                    }
                }
            }
            catch (Exception ex)
            {
                Services.LoggingService.LogError("Erro ao iniciar animação de digitação", ex);
            }
        }
        
        
        private void OnSourceInitialized(object? sender, EventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            
            // 1. Habilitar Dark Mode
            int darkMode = 1;
            DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref darkMode, sizeof(int));
            
            // 2. Habilitar Cantos Arredondados NATIVOS do Windows 11
            int cornerPreference = DWMWCP_ROUND;
            DwmSetWindowAttribute(hwnd, DWMWA_WINDOW_CORNER_PREFERENCE, ref cornerPreference, sizeof(int));
            
            // 3. Habilitar Acrylic Backdrop (Liquid Glass REAL do Windows 11)
            int backdropType = DWMSBT_TRANSIENTWINDOW; // Acrylic nativo
            DwmSetWindowAttribute(hwnd, DWMWA_SYSTEMBACKDROP_TYPE, ref backdropType, sizeof(int));
            
            // NOTA: NÃO usar DwmExtendFrameIntoClientArea pois faz cliques passarem através
            // O Acrylic funciona sem extend frame quando há um background adequado
        }


        private void CenterWindow()
        {
            // Usar SystemParameters do WPF para garantir valores corretos
            var screenWidth = SystemParameters.WorkArea.Width;
            var screenHeight = SystemParameters.WorkArea.Height;
            
            // Usar ActualWidth/ActualHeight se já renderizado, senão usar Width/Height
            var windowWidth = this.ActualWidth > 0 ? this.ActualWidth : this.Width;
            var windowHeight = this.ActualHeight > 0 ? this.ActualHeight : this.Height;
            
            this.Left = (screenWidth - windowWidth) / 2;
            this.Top = (screenHeight - windowHeight) / 2;
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void ExpandBtn_Click(object sender, RoutedEventArgs e)
        {
            // IMPORTANTE: Cancelar animações anteriores primeiro
            this.BeginAnimation(WidthProperty, null);
            this.BeginAnimation(HeightProperty, null);
            this.BeginAnimation(LeftProperty, null);
            this.BeginAnimation(TopProperty, null);
            ChatContainer.BeginAnimation(OpacityProperty, null);
            
            // Guardar valores ATUAIS (após cancelar animações)
            var currentWidth = this.Width;
            var currentHeight = this.Height;
            var currentLeft = this.Left;
            var currentTop = this.Top;
            
            // Calcular centro atual
            var centerX = currentLeft + (currentWidth / 2);
            var centerY = currentTop + (currentHeight / 2);
            
            // Alternar estado
            _isExpanded = !_isExpanded;
            
            // FIXAR tamanhos alvo
            var targetWidth = _isExpanded ? EXPANDED_WIDTH : COMPACT_WIDTH;
            var targetHeight = _isExpanded ? EXPANDED_HEIGHT : COMPACT_HEIGHT;
            
            // Calcular nova posição (mantendo centro)
            var targetLeft = centerX - (targetWidth / 2);
            var targetTop = centerY - (targetHeight / 2);
            
            // UX MELHORADA: Animação suave com micro-feedback
            var duration = TimeSpan.FromMilliseconds(300);
            var easing = new CubicEase { EasingMode = EasingMode.EaseInOut };
            
            // 1. Micro fade out (feedback visual)
            var microFade = new DoubleAnimation
            {
                From = 1,
                To = 0.95,
                Duration = TimeSpan.FromMilliseconds(100),
                AutoReverse = true,
                EasingFunction = new PowerEase { EasingMode = EasingMode.EaseOut }
            };
            ChatContainer.BeginAnimation(OpacityProperty, microFade);
            
            // 2. Definir tamanhos FIXOS imediatamente
            this.Width = targetWidth;
            this.Height = targetHeight;
            
            // 3. Animar posição com easing suave
            this.BeginAnimation(LeftProperty, new DoubleAnimation
            {
                From = currentLeft,
                To = targetLeft,
                Duration = duration,
                EasingFunction = easing
            });
            
            this.BeginAnimation(TopProperty, new DoubleAnimation
            {
                From = currentTop,
                To = targetTop,
                Duration = duration,
                EasingFunction = easing
            });
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            CloseChat();
        }

        private async void SendBtn_Click(object sender, RoutedEventArgs e)
        {
            await SendMessage();
        }

        private async void MessageInput_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Enter envia, Shift+Enter quebra linha
            if (e.Key == Key.Enter && !Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
            {
                e.Handled = true;
                await SendMessage();
            }
        }

        private async System.Threading.Tasks.Task SendMessage()
        {
            var message = MessageInput.Text.Trim();
            if (string.IsNullOrEmpty(message) && _currentScreenshot == null)
                return;
            
            // Guardar screenshot antes de limpar
            var screenshot = _currentScreenshot;
            
            // Adicionar mensagem do usuário (com imagem se houver)
            AddMessage("user", message, screenshot);
            
            // LIMPAR INPUT E IMAGEM IMEDIATAMENTE
            MessageInput.Text = string.Empty;
            if (_currentScreenshot != null)
            {
                _currentScreenshot = null;
                ImagePreview.Source = null;
                ImagePreviewContainer.Visibility = Visibility.Collapsed;
            }
            
            // Mostrar loading
            SendBtn.IsEnabled = false;
            SendBtn.Content = "...";
            
            try
            {
                // MOSTRAR INDICADOR DE DIGITAÇÃO
                ShowTypingIndicator();
                
                // Enviar para backend (continuando sessão se existir)
                Services.LoggingService.LogDebug($"Enviando mensagem com SessionId: {_currentSessionId}");
                
                var request = new AgentRequest
                {
                    Message = message,
                    ImageData = screenshot, // Usar screenshot guardado
                    SessionId = _currentSessionId // Continuar sessão histórica se carregada
                };
                
                var response = await _backendService.SendMessageAsync(request);
                
                // Atualizar session ID se veio do backend
                if (!string.IsNullOrEmpty(response.SessionId))
                {
                    Services.LoggingService.LogDebug($"Backend retornou SessionId: {response.SessionId}");
                    _currentSessionId = response.SessionId;
                }
                else
                {
                    Services.LoggingService.LogDebug("Backend não retornou SessionId");
                }
                
                // ESCONDER INDICADOR DE DIGITAÇÃO
                HideTypingIndicator();
                
                // Adicionar resposta do agente
                AddMessage("assistant", response.Content);
            }
            catch (Exception ex)
            {
                // ESCONDER INDICADOR DE DIGITAÇÃO EM CASO DE ERRO
                HideTypingIndicator();
                AddMessage("error", $"Erro: {ex.Message}");
            }
            finally
            {
                // Restaurar botão
                SendBtn.IsEnabled = true;
                SendBtn.Content = new TextBlock
                {
                    Text = "→",
                    FontSize = 20,
                    FontWeight = FontWeights.Bold,
                    Foreground = System.Windows.Media.Brushes.White
                };
                
                // Focar de volta no input
                MessageInput.Focus();
            }
        }

        private void AddMessage(string role, string content, string? imageData = null, DateTime? timestamp = null)
        {
            Services.LoggingService.LogDebug($" AddMessage chamado - Role: {role}, Content: {(string.IsNullOrEmpty(content) ? "(vazio)" : content.Substring(0, Math.Min(50, content.Length)))}...");
            
            // Container para mensagem + timestamp
            var messageContainer = new StackPanel
            {
                Margin = new Thickness(0, 0, 0, 5),
                MaxWidth = 320
            };
            
            if (role == "user")
            {
                messageContainer.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            }
            else
            {
                messageContainer.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            }
            
            // Balão da mensagem
            var messageBlock = new Border
            {
                Background = role == "user" 
                    ? new LinearGradientBrush
                    {
                        StartPoint = new System.Windows.Point(0, 0),
                        EndPoint = new System.Windows.Point(1, 1),
                        GradientStops = new GradientStopCollection
                        {
                            new GradientStop(System.Windows.Media.Color.FromArgb(0x40, 0xFF, 0xFF, 0xFF), 0),
                            new GradientStop(System.Windows.Media.Color.FromArgb(0x33, 0xFF, 0xFF, 0xFF), 1)
                        }
                    }
                    : new LinearGradientBrush
                    {
                        StartPoint = new System.Windows.Point(0, 0),
                        EndPoint = new System.Windows.Point(1, 1),
                        GradientStops = new GradientStopCollection
                        {
                            new GradientStop(System.Windows.Media.Color.FromArgb(0x2E, 0xFF, 0xFF, 0xFF), 0),
                            new GradientStop(System.Windows.Media.Color.FromArgb(0x26, 0xFF, 0xFF, 0xFF), 1)
                        }
                    },
                BorderBrush = role == "user"
                    ? new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x4D, 0xFF, 0xFF, 0xFF))
                    : new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x40, 0xFF, 0xFF, 0xFF)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(18),
                Padding = new Thickness(12, 8, 16, 8),
                Effect = role == "user"
                    ? new DropShadowEffect { Color = Colors.Black, BlurRadius = 16, ShadowDepth = 4, Opacity = 0.2 }
                    : new DropShadowEffect { Color = Colors.Black, BlurRadius = 8, ShadowDepth = 2, Opacity = 0.15 }
            };
            
            // Container para texto e imagem dentro do balão
            var contentStack = new StackPanel();
            
            // Adicionar imagem se disponível
            if (!string.IsNullOrEmpty(imageData))
            {
                var imageContainer = new Border
                {
                    Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x20, 0xFF, 0xFF, 0xFF)),
                    BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x30, 0xFF, 0xFF, 0xFF)),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(8),
                    Margin = new Thickness(0, 0, 0, 8),
                    Padding = new Thickness(4),
                    // ZIndex será definido após a criação
                };
                
                // Remover prefixo se existir (data:image/png;base64,)
                var base64Only = imageData.Contains(",") ? imageData.Split(',')[1] : imageData;
                
                // Converter base64 para BitmapImage
                var bitmap = new System.Windows.Media.Imaging.BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = new System.IO.MemoryStream(Convert.FromBase64String(base64Only));
                bitmap.EndInit();
                bitmap.Freeze();
                
                var messageImage = new System.Windows.Controls.Image
                {
                    Source = bitmap,
                    MaxWidth = 200,
                    MaxHeight = 150,
                    Stretch = System.Windows.Media.Stretch.Uniform,
                    Cursor = System.Windows.Input.Cursors.Hand
                };
                
                // Adicionar evento de clique para expandir
                messageImage.MouseLeftButtonDown += (s, e) => ShowImageExpanded(bitmap);
                
                imageContainer.Child = messageImage;
                System.Windows.Controls.Panel.SetZIndex(imageContainer, 10); // Garantir que a imagem fique na frente
                contentStack.Children.Add(imageContainer);
            }
            
            // Adicionar texto se não estiver vazio
            if (!string.IsNullOrEmpty(content))
            {
                var textBlock = new TextBlock
                {
                    Text = content,
                    Foreground = System.Windows.Media.Brushes.White,
                    FontSize = 14,
                    TextWrapping = TextWrapping.Wrap,
                    LineHeight = 19.6
                };
                
                contentStack.Children.Add(textBlock);
            }
            
            messageBlock.Child = contentStack;
            messageContainer.Children.Add(messageBlock);
            
            // Timestamp
            var timestampText = new TextBlock
            {
                Text = (timestamp ?? DateTime.Now).ToString("HH:mm"),
                Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xB3, 0xFF, 0xFF, 0xFF)), // 70% opacity
                FontSize = 11,
                Margin = new Thickness(8, 2, 8, 0),
                HorizontalAlignment = role == "user" ? System.Windows.HorizontalAlignment.Right : System.Windows.HorizontalAlignment.Left
            };
            
            messageContainer.Children.Add(timestampText);
            
            Services.LoggingService.LogDebug($" Adicionando mensagem ao MessagesPanel. Total antes: {MessagesPanel.Children.Count}");
            MessagesPanel.Children.Add(messageContainer);
            Services.LoggingService.LogSuccess($" Mensagem adicionada! Total agora: {MessagesPanel.Children.Count}");
            
            // Scroll para o final
            MessagesScrollViewer.ScrollToEnd();
        }

        private void CloseChat()
        {
            // Animação de saída mais rápida e suave
            var fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };
            
            fadeOut.Completed += (s, e) =>
            {
                // Limpar histórico da sessão ao fechar
                ClearSession();
                
                this.Hide();
                ChatClosed?.Invoke(this, EventArgs.Empty);
            };
            
            ChatContainer.BeginAnimation(OpacityProperty, fadeOut);
        }
        
        private void ClearSession()
        {
            // Limpar todas as mensagens do painel
            MessagesPanel.Children.Clear();
            
            // Limpar input
            MessageInput.Text = string.Empty;
        }
        
        /// <summary>
        /// Anexa screenshot capturado ao chat
        /// </summary>
        public void AttachScreenshot(string base64Image)
        {
            // Remover prefixo "data:image/png;base64," se existir
            var base64Only = base64Image.Contains(",") ? base64Image.Split(',')[1] : base64Image;
            
            // Armazenar apenas o base64 puro (sem prefixo)
            _currentScreenshot = base64Only;
            
            LoggingService.Log($" Screenshot anexado - tamanho: {base64Only.Length} chars");
            
            // Converter base64 para BitmapImage
            var bitmap = new System.Windows.Media.Imaging.BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = new System.IO.MemoryStream(Convert.FromBase64String(base64Only));
            bitmap.EndInit();
            bitmap.Freeze();
            
            // Mostrar preview
            ImagePreview.Source = bitmap;
            ImagePreviewContainer.Visibility = Visibility.Visible;
            
            // Focar no input
            MessageInput.Focus();
        }
        
        /// <summary>
        /// Remove imagem anexada
        /// </summary>
        private void RemoveImage_Click(object sender, RoutedEventArgs e)
        {
            _currentScreenshot = null;
            ImagePreview.Source = null;
            ImagePreviewContainer.Visibility = Visibility.Collapsed;
            MessageInput.Focus();
        }
        
        /// <summary>
        /// Mostra imagem expandida em uma janela modal
        /// </summary>
        private void ShowImageExpanded(System.Windows.Media.Imaging.BitmapImage bitmap)
        {
            // Esconder temporariamente o chat para não interferir
            this.Hide();
            
            var expandedWindow = new Window
            {
                Title = "Imagem Expandida",
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                Background = System.Windows.Media.Brushes.Transparent,
                Topmost = true,
                ResizeMode = ResizeMode.NoResize,
                ShowInTaskbar = false,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            
            // Forçar a janela a ficar na frente de tudo
            expandedWindow.Loaded += (s, e) =>
            {
                expandedWindow.Activate();
                expandedWindow.Focus();
                expandedWindow.BringIntoView();
                
                // Definir Z-Index alto via Win32 API
                var helper = new WindowInteropHelper(expandedWindow);
                SetWindowPos(helper.Handle, new IntPtr(-1), 0, 0, 0, 0, 
                    SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW); // HWND_TOPMOST
            };
            
            // Calcular tamanho da janela baseado na imagem
            var maxWidth = SystemParameters.WorkArea.Width * 0.8;
            var maxHeight = SystemParameters.WorkArea.Height * 0.8;
            
            var aspectRatio = (double)bitmap.PixelWidth / bitmap.PixelHeight;
            double windowWidth, windowHeight;
            
            if (aspectRatio > maxWidth / maxHeight)
            {
                windowWidth = maxWidth;
                windowHeight = windowWidth / aspectRatio;
            }
            else
            {
                windowHeight = maxHeight;
                windowWidth = windowHeight * aspectRatio;
            }
            
            expandedWindow.Width = windowWidth;
            expandedWindow.Height = windowHeight + 40; // Espaço para controles
            
            // Container principal com cantos arredondados
            var mainBorder = new Border
            {
                Background = System.Windows.Media.Brushes.Black,
                CornerRadius = new CornerRadius(12),
                BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x40, 0xFF, 0xFF, 0xFF)),
                BorderThickness = new Thickness(1),
                Effect = new DropShadowEffect 
                { 
                    Color = Colors.Black, 
                    BlurRadius = 20, 
                    ShadowDepth = 8, 
                    Opacity = 0.3 
                }
            };
            
            var mainGrid = new Grid();
            mainBorder.Child = mainGrid;
            expandedWindow.Content = mainBorder;
            
            // Barra de título com cantos arredondados
            var titleBar = new Border
            {
                Height = 40,
                Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x80, 0x00, 0x00, 0x00)),
                CornerRadius = new CornerRadius(12, 12, 0, 0),
                VerticalAlignment = VerticalAlignment.Top
            };
            
            var titleStack = new StackPanel
            {
                Orientation = System.Windows.Controls.Orientation.Horizontal,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                Margin = new Thickness(0, 8, 8, 0)
            };
            
            var closeButton = new Border
            {
                Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x40, 0xFF, 0xFF, 0xFF)),
                BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x60, 0xFF, 0xFF, 0xFF)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(14),
                Width = 28,
                Height = 28,
                Cursor = System.Windows.Input.Cursors.Hand,
                Child = new TextBlock
                {
                    Text = "×",
                    Foreground = System.Windows.Media.Brushes.White,
                    FontSize = 20,
                    FontWeight = FontWeights.Normal,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                    VerticalAlignment = System.Windows.VerticalAlignment.Center
                }
            };
            
            // Hover effect
            closeButton.MouseEnter += (s, e) =>
            {
                closeButton.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x66, 0xFF, 0x64, 0x64));
                closeButton.BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x80, 0xFF, 0x96, 0x96));
            };
            
            closeButton.MouseLeave += (s, e) =>
            {
                closeButton.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x40, 0xFF, 0xFF, 0xFF));
                closeButton.BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x60, 0xFF, 0xFF, 0xFF));
            };
            
            closeButton.MouseLeftButtonDown += (s, e) => 
            {
                expandedWindow.Close();
                this.Show(); // Mostrar chat novamente
            };
            titleStack.Children.Add(closeButton);
            titleBar.Child = titleStack;
            
            // Imagem expandida
            var expandedImage = new System.Windows.Controls.Image
            {
                Source = bitmap,
                Stretch = System.Windows.Media.Stretch.Uniform,
                Margin = new Thickness(8, 48, 8, 8) // Margem para não cortar nos cantos arredondados
            };
            
            mainGrid.Children.Add(expandedImage);
            mainGrid.Children.Add(titleBar);
            
            // Fechar com ESC ou clique fora
            expandedWindow.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape) 
                {
                    expandedWindow.Close();
                    this.Show(); // Mostrar chat novamente
                }
            };
            
            expandedWindow.MouseLeftButtonDown += (s, e) =>
            {
                if (e.OriginalSource == expandedWindow || e.OriginalSource == expandedImage)
                {
                    expandedWindow.Close();
                    this.Show(); // Mostrar chat novamente
                }
            };
            
            // Mostrar chat quando janela fechar (qualquer forma)
            expandedWindow.Closed += (s, e) => this.Show();
            
            expandedWindow.Show();
            expandedWindow.Activate();
            expandedWindow.Focus();
        }

        /// <summary>
        /// Exibe a janela de chat
        /// </summary>
        public void ShowChat()
        {
            // 1. CANCELAR TODAS AS ANIMAÇÕES
            this.BeginAnimation(WidthProperty, null);
            this.BeginAnimation(HeightProperty, null);
            this.BeginAnimation(LeftProperty, null);
            this.BeginAnimation(TopProperty, null);
            ChatContainer.BeginAnimation(OpacityProperty, null);
            
            // 2. RESETAR ESTADO COMPLETO
            _isExpanded = false;
            
            // 3. FORÇAR TAMANHO COMPACTO
            this.Width = COMPACT_WIDTH;
            this.Height = COMPACT_HEIGHT;
            
            // 4. FORÇAR CENTRALIZAÇÃO
            var screenWidth = SystemParameters.WorkArea.Width;
            var screenHeight = SystemParameters.WorkArea.Height;
            this.Left = (screenWidth - COMPACT_WIDTH) / 2;
            this.Top = (screenHeight - COMPACT_HEIGHT) / 2;
            
            // 5. RESETAR OPACIDADE
            ChatContainer.Opacity = 0;
            
            // 6. MOSTRAR JANELA
            this.Show();
            
            // 7. ANIMAR ENTRADA
            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new PowerEase { EasingMode = EasingMode.EaseOut, Power = 2 }
            };
            
            ChatContainer.BeginAnimation(OpacityProperty, fadeIn);
            MessageInput.Focus();
        }
        
        /// <summary>
        /// Carrega uma sessão histórica no chat
        /// </summary>
        public System.Threading.Tasks.Task LoadHistorySession(string sessionId, System.Text.Json.JsonElement sessionData)
        {
            Services.LoggingService.LogInfo($" Carregando sessão histórica: {sessionId}");
            
            try
            {
                // Guardar ID da sessão para continuar conversando
                _currentSessionId = sessionId;
                
                // LIMPAR MENSAGENS EXISTENTES (incluindo o indicador se estiver visível)
                MessagesPanel.Children.Clear();
                Services.LoggingService.LogDebug("MessagesPanel limpo para carregar histórico");
                
                // O backend retorna um ARRAY de mensagens diretamente (não um objeto com propriedade "messages")
                if (sessionData.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    var messagesCount = sessionData.GetArrayLength();
                    Services.LoggingService.LogDebug($"Encontradas {messagesCount} mensagens");
                    
                    foreach (var messageElement in sessionData.EnumerateArray())
                    {
                        // Log da estrutura completa da mensagem para debug
                        Services.LoggingService.LogDebug($" Estrutura da mensagem: {messageElement}");
                        
                        var role = messageElement.GetProperty("role").GetString();
                        var content = messageElement.GetProperty("content").GetString();
                        
                        // Extrair timestamp se existir
                        DateTime? messageTimestamp = null;
                        if (messageElement.TryGetProperty("created_at", out var timestampProp))
                        {
                            if (timestampProp.ValueKind == System.Text.Json.JsonValueKind.String)
                            {
                                var timestampStr = timestampProp.GetString();
                                Services.LoggingService.LogDebug($" Tentando parse do timestamp: '{timestampStr}'");
                                
                                if (DateTime.TryParse(timestampStr, out var parsedDt))
                                {
                                    messageTimestamp = parsedDt;
                                    Services.LoggingService.LogDebug($" Timestamp parseado com sucesso: {parsedDt:HH:mm:ss}");
                                }
                                else
                                {
                                    Services.LoggingService.LogDebug($" Falha ao fazer parse do timestamp: '{timestampStr}'");
                                }
                            }
                        }
                        
                        // Extrair imageData se existir
                        string? imageData = null;
                        if (messageElement.TryGetProperty("additional_kwargs", out var additionalKwargs))
                        {
                            if (additionalKwargs.TryGetProperty("image_data", out var imageDataProp))
                            {
                                imageData = imageDataProp.GetString();
                            }
                        }
                        
                        Services.LoggingService.LogDebug($" Timestamp da mensagem: {(messageTimestamp?.ToString("HH:mm:ss") ?? "não encontrado")}");
                        
                        if (role == "user")
                        {
                            AddMessage("user", content ?? "", imageData, messageTimestamp);
                        }
                        else if (role == "assistant")
                        {
                            AddMessage("assistant", content ?? "", null, messageTimestamp);
                        }
                    }
                    
                    Services.LoggingService.LogSuccess($" {messagesCount} mensagens carregadas!");
                }
                else
                {
                    Services.LoggingService.LogError($"sessionData não é um array! Tipo: {sessionData.ValueKind}");
                }
            }
            catch (Exception ex)
            {
                Services.LoggingService.LogError("Erro ao carregar sessão histórica", ex);
            }
            
            return System.Threading.Tasks.Task.CompletedTask;
        }
    }
}

