using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;

namespace OrbAgent.Frontend.Services
{
    public class ScreenshotService
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetDesktopWindow();
        
        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowDC(IntPtr hWnd);
        
        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
        
        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int wDest, int hDest,
            IntPtr hdcSource, int xSrc, int ySrc, CopyPixelOperation rop);

        public event EventHandler<ScreenshotCapturedEventArgs>? ScreenshotCaptured;

        /// <summary>
        /// Captura screenshot da tela inteira
        /// </summary>
        public string CaptureFullScreen()
        {
            // Obter dimensões da tela principal
            var bounds = Screen.PrimaryScreen?.Bounds ?? new Rectangle(0, 0, 1920, 1080);
            
            using var bitmap = new Bitmap(bounds.Width, bounds.Height);
            using var graphics = Graphics.FromImage(bitmap);
            
            // Capturar a tela
            graphics.CopyFromScreen(bounds.Left, bounds.Top, 0, 0, bounds.Size);
            
            // Converter para base64
            using var memoryStream = new MemoryStream();
            bitmap.Save(memoryStream, ImageFormat.Png);
            var imageBytes = memoryStream.ToArray();
            var base64String = Convert.ToBase64String(imageBytes);
            
            return $"data:image/png;base64,{base64String}";
        }

        /// <summary>
        /// Captura screenshot de uma região específica
        /// </summary>
        public string CaptureRegion(int x, int y, int width, int height)
        {
            using var bitmap = new Bitmap(width, height);
            using var graphics = Graphics.FromImage(bitmap);
            
            // Capturar região específica
            graphics.CopyFromScreen(x, y, 0, 0, new System.Drawing.Size(width, height));
            
            // Converter para base64
            using var memoryStream = new MemoryStream();
            bitmap.Save(memoryStream, ImageFormat.Png);
            var imageBytes = memoryStream.ToArray();
            var base64String = Convert.ToBase64String(imageBytes);
            
            return $"data:image/png;base64,{base64String}";
        }

        /// <summary>
        /// Inicia captura interativa (usuário seleciona região)
        /// </summary>
        public void StartInteractiveCapture()
        {
            // Por enquanto, capturar tela inteira
            // TODO: Implementar seleção de região interativa
            var screenshot = CaptureFullScreen();
            ScreenshotCaptured?.Invoke(this, new ScreenshotCapturedEventArgs(screenshot));
        }
    }

    public class ScreenshotCapturedEventArgs : EventArgs
    {
        public string Base64Image { get; }

        public ScreenshotCapturedEventArgs(string base64Image)
        {
            Base64Image = base64Image;
        }
    }
}

