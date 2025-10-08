using System;
using System.Runtime.InteropServices;
using System.Windows.Threading;

namespace OrbAgent.Frontend.Services
{
    /// <summary>
    /// Detecta quando o mouse entra no "hot corner" (canto superior esquerdo)
    /// </summary>
    public class HotCornerService
    {
        #region Win32 API

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        #endregion

        private readonly DispatcherTimer _timer;
        private bool _wasInHotCorner = false;

        public event EventHandler? HotCornerEntered;
        public event EventHandler? HotCornerLeft;

        /// <summary>
        /// Threshold em pixels para o hot corner (padrão: 5px)
        /// </summary>
        public int HotCornerThreshold { get; set; } = 5;

        public HotCornerService()
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100) // Check a cada 100ms
            };
            _timer.Tick += CheckHotCorner;
        }

        /// <summary>
        /// Inicia a detecção do hot corner
        /// </summary>
        public void Start()
        {
            _timer.Start();
        }

        /// <summary>
        /// Para a detecção do hot corner
        /// </summary>
        public void Stop()
        {
            _timer.Stop();
        }

        private void CheckHotCorner(object? sender, EventArgs e)
        {
            if (!GetCursorPos(out POINT point))
                return;

            // Detectar se está no canto superior esquerdo (0,0)
            bool isInHotCorner = point.X <= HotCornerThreshold && point.Y <= HotCornerThreshold;

            // Disparar eventos apenas nas transições
            if (isInHotCorner && !_wasInHotCorner)
            {
                // Entrou no hot corner
                HotCornerEntered?.Invoke(this, EventArgs.Empty);
            }
            else if (!isInHotCorner && _wasInHotCorner)
            {
                // Saiu do hot corner
                HotCornerLeft?.Invoke(this, EventArgs.Empty);
            }

            _wasInHotCorner = isInHotCorner;
        }
    }
}

