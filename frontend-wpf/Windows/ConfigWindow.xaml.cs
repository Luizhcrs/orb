using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;

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

        public ConfigWindow()
        {
            InitializeComponent();
            
            // Habilitar Acrylic
            this.SourceInitialized += OnSourceInitialized;
            
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

        private void AgenteBtn_Click(object sender, RoutedEventArgs e)
        {
            LoadAgenteSection();
            HighlightTab(AgenteBtn);
        }

        private void HistoricoBtn_Click(object sender, RoutedEventArgs e)
        {
            LoadHistoricoSection();
            HighlightTab(HistoricoBtn);
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
            
            AddSpacer();
            
            // Manter histórico
            AddCheckBox("Manter histórico de conversas", true);
        }

        private void LoadAgenteSection()
        {
            ContentPanel.Children.Clear();
            
            AddSectionTitle("Configurações do Agente");
            
            // Provedor LLM (somente OpenAI)
            AddLabel("Provedor de LLM");
            AddComboBox(new[] { "OpenAI" }, 0);
            
            AddSpacer();
            
            // API Key
            AddLabel("API Key");
            var apiKeyBox = AddPasswordBox("Insira sua API key...");
            
            AddSpacer();
            
            // Modelo
            AddLabel("Modelo");
            AddComboBox(new[] { "gpt-4", "gpt-4-turbo", "gpt-3.5-turbo" }, 0);
        }

        private void LoadHistoricoSection()
        {
            ContentPanel.Children.Clear();
            
            AddSectionTitle("Histórico de Conversas");
            
            AddLabel("Nenhuma conversa salva ainda.");
        }

        // Helpers para criar elementos UI
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

        private System.Windows.Controls.TextBox AddPasswordBox(string placeholder)
        {
            var textBox = new System.Windows.Controls.TextBox
            {
                Style = (Style)FindResource("GlassTextBoxStyle"),
                Text = placeholder,
                Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x66, 0xFF, 0xFF, 0xFF)),
                Margin = new Thickness(0, 0, 0, 16)
            };
            
            textBox.GotFocus += (s, e) =>
            {
                if (textBox.Text == placeholder)
                {
                    textBox.Text = "";
                    textBox.Foreground = System.Windows.Media.Brushes.White;
                }
            };
            
            textBox.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    textBox.Text = placeholder;
                    textBox.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x66, 0xFF, 0xFF, 0xFF));
                }
            };
            
            ContentPanel.Children.Add(textBox);
            return textBox;
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
    }
}

