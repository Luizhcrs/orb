using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;

namespace OrbAgent.Frontend.Windows
{
    public partial class AboutWindow : Window
    {
        // Win32 API para efeitos visuais nativos
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private const int DWMWA_SYSTEMBACKDROP_TYPE = 38;
        private const int DWMWCP_ROUND = 2; // Cantos arredondados
        private const int DWMSBT_TRANSIENTWINDOW = 3; // Acrylic backdrop

        public AboutWindow()
        {
            InitializeComponent();
            
            // Configurar eventos de teclado
            KeyDown += AboutWindow_KeyDown;
            Loaded += AboutWindow_Loaded;
            
            // Focar na janela
            Focusable = true;
            Focus();
        }

        private void AboutWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
                
                // Habilitar Dark Mode
                int darkMode = 1;
                DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref darkMode, sizeof(int));
                
                // Habilitar Cantos Arredondados
                int cornerPreference = DWMWCP_ROUND;
                DwmSetWindowAttribute(hwnd, DWMWA_WINDOW_CORNER_PREFERENCE, ref cornerPreference, sizeof(int));
                
                // Habilitar Acrylic Backdrop (blur nativo)
                int backdropType = DWMSBT_TRANSIENTWINDOW;
                DwmSetWindowAttribute(hwnd, DWMWA_SYSTEMBACKDROP_TYPE, ref backdropType, sizeof(int));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao aplicar efeitos visuais: {ex.Message}");
            }
        }

        private void AboutWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // ESC para fechar
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            
            // Garantir que a janela apareça no topo
            var hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            Win32.SetWindowPos(hwnd, Win32.HWND_TOPMOST, 0, 0, 0, 0, Win32.SWP_NOMOVE | Win32.SWP_NOSIZE);
        }
    }

    // Win32 API helpers
    internal static class Win32
    {
        [DllImport("user32.dll")]
        internal static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        internal static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        internal const uint SWP_NOMOVE = 0x0002;
        internal const uint SWP_NOSIZE = 0x0001;
    }
}
