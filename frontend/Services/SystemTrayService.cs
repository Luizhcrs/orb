using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows;
using Application = System.Windows.Application;

namespace OrbAgent.Frontend.Services
{
    /// <summary>
    /// Gerencia o ícone do System Tray e seu menu de contexto
    /// </summary>
    public class SystemTrayService : IDisposable
    {
        private NotifyIcon? _notifyIcon;
        private ContextMenuStrip? _contextMenu;

        public event EventHandler? ConfigurationRequested;
        public event EventHandler? AboutRequested;
        public event EventHandler? ExitRequested;

        public SystemTrayService()
        {
            InitializeSystemTray();
        }

        /// <summary>
        /// Inicializa o ícone do System Tray e menu de contexto
        /// </summary>
        private void InitializeSystemTray()
        {
            // Criar menu de contexto com visual personalizado
            _contextMenu = new ContextMenuStrip
            {
                BackColor = ColorTranslator.FromHtml("#141416"), // Fundo escuro (liquid glass)
                ForeColor = Color.White,
                ShowImageMargin = false,
                ShowCheckMargin = false,
                RenderMode = ToolStripRenderMode.Professional,
                Renderer = new CustomMenuRenderer(),
                Padding = new Padding(4), // Padding interno
                AutoSize = true
            };
            
            // Menu: Configurações
            var configItem = new ToolStripMenuItem("Configurações")
            {
                ForeColor = Color.White,
                BackColor = ColorTranslator.FromHtml("#141416"),
                Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular),
                Padding = new Padding(12, 8, 12, 8) // Padding maior
            };
            configItem.Click += (s, e) => ConfigurationRequested?.Invoke(this, EventArgs.Empty);
            _contextMenu.Items.Add(configItem);
            
            _contextMenu.Items.Add(new ToolStripSeparator
            {
                ForeColor = ColorTranslator.FromHtml("#26FFFFFF"),
                BackColor = ColorTranslator.FromHtml("#141416"),
                Padding = new Padding(8, 4, 8, 4)
            });
            
            // Menu: Sobre
            var aboutItem = new ToolStripMenuItem("Sobre")
            {
                ForeColor = Color.White,
                BackColor = ColorTranslator.FromHtml("#141416"),
                Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular),
                Padding = new Padding(12, 8, 12, 8)
            };
            aboutItem.Click += (s, e) => AboutRequested?.Invoke(this, EventArgs.Empty);
            _contextMenu.Items.Add(aboutItem);
            
            _contextMenu.Items.Add(new ToolStripSeparator
            {
                ForeColor = ColorTranslator.FromHtml("#26FFFFFF"),
                BackColor = ColorTranslator.FromHtml("#141416"),
                Padding = new Padding(8, 4, 8, 4)
            });
            
            // Menu: Sair (com cor vermelha)
            var exitItem = new ToolStripMenuItem("Sair")
            {
                ForeColor = ColorTranslator.FromHtml("#FF9696"), // Vermelho suave
                BackColor = ColorTranslator.FromHtml("#141416"),
                Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular),
                Padding = new Padding(12, 8, 12, 8)
            };
            exitItem.Click += (s, e) => ExitRequested?.Invoke(this, EventArgs.Empty);
            _contextMenu.Items.Add(exitItem);

            // Criar NotifyIcon
            _notifyIcon = new NotifyIcon
            {
                Icon = CreateOrbIcon(), // Ícone padrão (será substituído por .ico)
                Text = "Orb Agent - Assistente Inteligente",
                Visible = true,
                ContextMenuStrip = _contextMenu
            };

            // Duplo clique abre configurações
            _notifyIcon.DoubleClick += (s, e) => ConfigurationRequested?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Cria um ícone padrão caso não exista arquivo .ico
        /// </summary>
        private Icon CreateOrbIcon()
        {
            // Tentar carregar ícone do arquivo
            var iconPath = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, 
                "Assets", 
                "orb.ico"
            );

            if (System.IO.File.Exists(iconPath))
            {
                return new Icon(iconPath);
            }

            // Fallback: criar ícone com visual do Orb (liquid glass)
            using (var bitmap = new Bitmap(32, 32))
            using (var g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                
                // Glow externo (sutil)
                using (var glowBrush = new System.Drawing.Drawing2D.PathGradientBrush(
                    new PointF[] { 
                        new PointF(16, 16), 
                        new PointF(32, 16), 
                        new PointF(16, 32) 
                    }))
                {
                    glowBrush.CenterColor = Color.FromArgb(40, 255, 255, 255);
                    glowBrush.SurroundColors = new[] { Color.Transparent };
                    g.FillEllipse(glowBrush, 0, 0, 32, 32);
                }
                
                // Círculo principal (liquid glass escuro)
                using (var brush = new SolidBrush(Color.FromArgb(200, 20, 20, 22)))
                {
                    g.FillEllipse(brush, 4, 4, 24, 24);
                }
                
                // Borda sutil
                using (var pen = new Pen(Color.FromArgb(100, 255, 255, 255), 1.5f))
                {
                    g.DrawEllipse(pen, 4, 4, 24, 24);
                }
                
                // Reflexo de vidro (highlight)
                using (var reflectionBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    new PointF(8, 8),
                    new PointF(14, 14),
                    Color.FromArgb(120, 255, 255, 255),
                    Color.Transparent))
                {
                    g.FillEllipse(reflectionBrush, 8, 8, 8, 8);
                }

                return Icon.FromHandle(bitmap.GetHicon());
            }
        }

        /// <summary>
        /// Mostra uma notificação no System Tray
        /// </summary>
        public void ShowNotification(string title, string message, ToolTipIcon icon = ToolTipIcon.Info)
        {
            _notifyIcon?.ShowBalloonTip(3000, title, message, icon);
        }

        /// <summary>
        /// Atualiza o tooltip do ícone
        /// </summary>
        public void UpdateTooltip(string text)
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.Text = text.Length > 63 ? text.Substring(0, 60) + "..." : text;
            }
        }

        /// <summary>
        /// Mostra/oculta o ícone do System Tray
        /// </summary>
        public void SetVisible(bool visible)
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = visible;
            }
        }

        public void Dispose()
        {
            _notifyIcon?.Dispose();
            _contextMenu?.Dispose();
        }
    }

    /// <summary>
    /// Renderer personalizado para menu de contexto com visual liquid glass
    /// </summary>
    public class CustomMenuRenderer : ToolStripProfessionalRenderer
    {
        public CustomMenuRenderer() : base(new CustomColorTable()) { }

        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            // Aplicar cantos arredondados no menu
            if (e.ToolStrip is ContextMenuStrip)
            {
                using (var path = GetRoundedRectPath(e.AffectedBounds, 8))
                {
                    e.ToolStrip.Region = new System.Drawing.Region(path);
                    
                    // Fundo liquid glass
                    using (var brush = new SolidBrush(ColorTranslator.FromHtml("#141416")))
                    {
                        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                        e.Graphics.FillPath(brush, path);
                    }
                    
                    // Borda sutil
                    using (var pen = new Pen(ColorTranslator.FromHtml("#26FFFFFF"), 1.5f))
                    {
                        e.Graphics.DrawPath(pen, path);
                    }
                }
            }
            else
            {
                base.OnRenderToolStripBackground(e);
            }
        }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            if (e.Item.Selected)
            {
                // Fundo ao passar o mouse (liquid glass hover)
                var rect = new Rectangle(2, 0, e.Item.Width - 4, e.Item.Height);
                using (var path = GetRoundedRectPath(rect, 6))
                using (var brush = new SolidBrush(ColorTranslator.FromHtml("#1AFFFFFF")))
                {
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    e.Graphics.FillPath(brush, path);
                }
            }
            else
            {
                base.OnRenderMenuItemBackground(e);
            }
        }
        
        private System.Drawing.Drawing2D.GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)
        {
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            int diameter = radius * 2;
            
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            
            return path;
        }

        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            // Separador personalizado (linha sutil liquid glass)
            using (var pen = new Pen(ColorTranslator.FromHtml("#26FFFFFF"), 1))
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                int y = e.Item.Height / 2;
                e.Graphics.DrawLine(pen, 12, y, e.Item.Width - 12, y);
            }
        }

        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
        {
            // Seta personalizada (branca)
            e.ArrowColor = Color.White;
            base.OnRenderArrow(e);
        }
    }

    /// <summary>
    /// Tabela de cores personalizada para o menu (liquid glass)
    /// </summary>
    public class CustomColorTable : ProfessionalColorTable
    {
        public override Color MenuItemSelected => ColorTranslator.FromHtml("#1AFFFFFF");
        public override Color MenuItemBorder => ColorTranslator.FromHtml("#26FFFFFF");
        public override Color MenuBorder => ColorTranslator.FromHtml("#26FFFFFF");
        public override Color MenuItemSelectedGradientBegin => ColorTranslator.FromHtml("#1AFFFFFF");
        public override Color MenuItemSelectedGradientEnd => ColorTranslator.FromHtml("#1AFFFFFF");
        public override Color MenuItemPressedGradientBegin => ColorTranslator.FromHtml("#0DFFFFFF");
        public override Color MenuItemPressedGradientEnd => ColorTranslator.FromHtml("#0DFFFFFF");
        public override Color SeparatorDark => ColorTranslator.FromHtml("#26FFFFFF");
        public override Color SeparatorLight => ColorTranslator.FromHtml("#1AFFFFFF");
        public override Color ToolStripDropDownBackground => ColorTranslator.FromHtml("#141416");
    }
}

