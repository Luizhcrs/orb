using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using OrbAgent.Frontend.Helpers;

namespace OrbAgent.Frontend.Windows
{
    public partial class OrbWindow : Window
    {
        public event EventHandler? OrbClicked;
        
        private DispatcherTimer? _autoHideTimer;
        private bool _isMouseOver = false;

        public OrbWindow()
        {
            InitializeComponent();
            
            // Posicionar no canto superior esquerdo (hot corner)
            this.Left = 0;
            this.Top = 0;
            
            // Configurar timer de auto-hide (5 segundos)
            _autoHideTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            _autoHideTimer.Tick += (s, e) =>
            {
                if (!_isMouseOver)
                {
                    HideOrb();
                    _autoHideTimer.Stop();
                }
            };
            
            // Eventos de mouse
            this.MouseLeftButtonDown += (s, e) =>
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    this.DragMove();
                }
            };
            
            this.MouseLeftButtonUp += (s, e) =>
            {
                OrbClicked?.Invoke(this, EventArgs.Empty);
            };
            
            // Detectar mouse enter/leave
            this.MouseEnter += (s, e) =>
            {
                _isMouseOver = true;
                _autoHideTimer?.Stop(); // Parar timer se mouse estiver sobre o Orb
            };
            
            this.MouseLeave += (s, e) =>
            {
                _isMouseOver = false;
                _autoHideTimer?.Start(); // Reiniciar timer quando mouse sair
            };
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Não aplicar blur aqui - o Orb já tem transparência no XAML
            // O blur é aplicado via CSS backdrop-filter dentro do próprio círculo
            
            // Iniciar timer de auto-hide
            _autoHideTimer?.Start();
        }

        /// <summary>
        /// Mostra o Orb com animação
        /// </summary>
        public void ShowOrb()
        {
            this.Show();
            this.Opacity = 0;
            
            // Fade in
            var fadeIn = new System.Windows.Media.Animation.DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(300)
            };
            this.BeginAnimation(Window.OpacityProperty, fadeIn);
            
            // Resetar e iniciar timer
            _autoHideTimer?.Stop();
            _autoHideTimer?.Start();
        }

        /// <summary>
        /// Esconde o Orb com animação
        /// </summary>
        public void HideOrb()
        {
            // Fade out
            var fadeOut = new System.Windows.Media.Animation.DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300)
            };
            fadeOut.Completed += (s, e) => this.Hide();
            this.BeginAnimation(Window.OpacityProperty, fadeOut);
        }
    }
}
