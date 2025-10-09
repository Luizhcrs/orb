using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace OrbAgent.Frontend.Windows
{
    /// <summary>
    /// Janela de confirmação personalizada com visual liquid glass
    /// </summary>
    public partial class ConfirmationWindow : Window
    {
        // Win32 API para Acrylic/Blur (Windows 11)
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
        
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
        private const int DWMWCP_ROUND = 2; // Cantos arredondados
        private const int DWMWA_SYSTEMBACKDROP_TYPE = 38;
        private const int DWMSBT_TRANSIENTWINDOW = 3; // Acrylic effect

        public bool Result { get; private set; }

        public ConfirmationWindow()
        {
            InitializeComponent();
            Loaded += ConfirmationWindow_Loaded;
        }

        public ConfirmationWindow(string title, string message) : this()
        {
            TitleText.Text = title;
            MessageText.Text = message;
        }

        private void ConfirmationWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Aplicar Acrylic/Blur effect
            var helper = new WindowInteropHelper(this);
            var hwnd = helper.Handle;

            // Ativar dark mode
            int useDarkMode = 1;
            DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useDarkMode, sizeof(int));

            // Ativar cantos arredondados nativos
            int cornerPreference = DWMWCP_ROUND;
            DwmSetWindowAttribute(hwnd, DWMWA_WINDOW_CORNER_PREFERENCE, ref cornerPreference, sizeof(int));

            // Ativar Acrylic effect
            int backdropType = DWMSBT_TRANSIENTWINDOW;
            DwmSetWindowAttribute(hwnd, DWMWA_SYSTEMBACKDROP_TYPE, ref backdropType, sizeof(int));
        }

        private void Cancel_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Result = false;
            Close();
        }

        private void Exit_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Result = true;
            Close();
        }

        /// <summary>
        /// Mostra a janela de confirmação e retorna o resultado
        /// </summary>
        public static bool Show(string title, string message)
        {
            var window = new ConfirmationWindow(title, message);
            window.ShowDialog();
            return window.Result;
        }

        /// <summary>
        /// Mostra confirmação de saída do aplicativo
        /// </summary>
        public static bool ShowExitConfirmation()
        {
            return Show("Confirmar Saída", "Deseja realmente sair do Orb Agent?");
        }
    }
}

