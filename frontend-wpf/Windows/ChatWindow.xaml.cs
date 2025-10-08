using System;
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
            InitializeComponent();
            _backendService = backendService;
            
            // Habilitar transparência real após carregamento
            this.SourceInitialized += OnSourceInitialized;
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
            
            // Adicionar mensagem do usuário (com imagem se houver)
            AddMessage("user", message, _currentScreenshot);
            
            // Mostrar loading
            SendBtn.IsEnabled = false;
            SendBtn.Content = "...";
            
            try
            {
                // Enviar para backend
                var request = new AgentRequest
                {
                    Message = message,
                    ImageData = _currentScreenshot // Incluir imagem se disponível
                };
                
                var response = await _backendService.SendMessageAsync(request);
                
                // Adicionar resposta do agente
                AddMessage("assistant", response.Response);
            }
            catch (Exception ex)
            {
                AddMessage("error", $"Erro: {ex.Message}");
            }
            finally
            {
                // Limpar input e imagem
                MessageInput.Text = string.Empty;
                if (_currentScreenshot != null)
                {
                    _currentScreenshot = null;
                    ImagePreview.Source = null;
                    ImagePreviewContainer.Visibility = Visibility.Collapsed;
                }
                
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

        private void AddMessage(string role, string content, string? imageData = null)
        {
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
                
                // Converter base64 para BitmapImage
                var bitmap = new System.Windows.Media.Imaging.BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = new System.IO.MemoryStream(Convert.FromBase64String(imageData.Split(',')[1]));
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
            var timestamp = new TextBlock
            {
                Text = DateTime.Now.ToString("HH:mm"),
                Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xB3, 0xFF, 0xFF, 0xFF)), // 70% opacity
                FontSize = 11,
                Margin = new Thickness(8, 2, 8, 0),
                HorizontalAlignment = role == "user" ? System.Windows.HorizontalAlignment.Right : System.Windows.HorizontalAlignment.Left
            };
            
            messageContainer.Children.Add(timestamp);
            MessagesPanel.Children.Add(messageContainer);
            
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
            // Armazenar imagem
            _currentScreenshot = base64Image;
            
            // Converter base64 para BitmapImage
            var bitmap = new System.Windows.Media.Imaging.BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = new System.IO.MemoryStream(Convert.FromBase64String(base64Image.Split(',')[1]));
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
                    Text = "✕",
                    Foreground = System.Windows.Media.Brushes.White,
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                    VerticalAlignment = System.Windows.VerticalAlignment.Center
                }
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
    }
}

