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
                BackColor = ColorTranslator.FromHtml("#1A1A1C"), // Fundo escuro
                ForeColor = Color.White,
                ShowImageMargin = false, // Remover margem de ícones
                ShowCheckMargin = false,
                RenderMode = ToolStripRenderMode.Professional,
                Renderer = new CustomMenuRenderer() // Renderer personalizado
            };
            
            // Menu: Configurações
            var configItem = new ToolStripMenuItem("Configurações")
            {
                ForeColor = Color.White,
                BackColor = ColorTranslator.FromHtml("#1A1A1C"),
                Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular)
            };
            configItem.Click += (s, e) => ConfigurationRequested?.Invoke(this, EventArgs.Empty);
            _contextMenu.Items.Add(configItem);
            
            _contextMenu.Items.Add(new ToolStripSeparator
            {
                ForeColor = ColorTranslator.FromHtml("#333333"),
                BackColor = ColorTranslator.FromHtml("#1A1A1C")
            });
            
            // Menu: Sobre
            var aboutItem = new ToolStripMenuItem("Sobre")
            {
                ForeColor = Color.White,
                BackColor = ColorTranslator.FromHtml("#1A1A1C"),
                Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular)
            };
            aboutItem.Click += (s, e) => AboutRequested?.Invoke(this, EventArgs.Empty);
            _contextMenu.Items.Add(aboutItem);
            
            _contextMenu.Items.Add(new ToolStripSeparator
            {
                ForeColor = ColorTranslator.FromHtml("#333333"),
                BackColor = ColorTranslator.FromHtml("#1A1A1C")
            });
            
            // Menu: Sair (com cor vermelha)
            var exitItem = new ToolStripMenuItem("Sair")
            {
                ForeColor = ColorTranslator.FromHtml("#FF6666"), // Vermelho suave
                BackColor = ColorTranslator.FromHtml("#1A1A1C"),
                Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular)
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

            // Fallback: criar ícone simples em memória
            using (var bitmap = new Bitmap(32, 32))
            using (var g = Graphics.FromImage(bitmap))
            {
                // Fundo transparente
                g.Clear(Color.Transparent);
                
                // Desenhar círculo roxo (#290060)
                using (var brush = new SolidBrush(ColorTranslator.FromHtml("#290060")))
                {
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    g.FillEllipse(brush, 2, 2, 28, 28);
                }
                
                // Borda branca
                using (var pen = new Pen(Color.White, 2))
                {
                    g.DrawEllipse(pen, 2, 2, 28, 28);
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

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            if (e.Item.Selected)
            {
                // Fundo ao passar o mouse (liquid glass hover)
                using (var brush = new SolidBrush(ColorTranslator.FromHtml("#2A2A2C")))
                {
                    e.Graphics.FillRectangle(brush, 0, 0, e.Item.Width, e.Item.Height);
                }
                
                // Borda sutil
                using (var pen = new Pen(ColorTranslator.FromHtml("#444444"), 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, e.Item.Width - 1, e.Item.Height - 1);
                }
            }
            else
            {
                base.OnRenderMenuItemBackground(e);
            }
        }

        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            // Separador personalizado (linha sutil)
            using (var pen = new Pen(ColorTranslator.FromHtml("#333333"), 1))
            {
                int y = e.Item.Height / 2;
                e.Graphics.DrawLine(pen, 8, y, e.Item.Width - 8, y);
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
    /// Tabela de cores personalizada para o menu
    /// </summary>
    public class CustomColorTable : ProfessionalColorTable
    {
        public override Color MenuItemSelected => ColorTranslator.FromHtml("#2A2A2C");
        public override Color MenuItemBorder => ColorTranslator.FromHtml("#444444");
        public override Color MenuBorder => ColorTranslator.FromHtml("#333333");
        public override Color MenuItemSelectedGradientBegin => ColorTranslator.FromHtml("#2A2A2C");
        public override Color MenuItemSelectedGradientEnd => ColorTranslator.FromHtml("#2A2A2C");
        public override Color MenuItemPressedGradientBegin => ColorTranslator.FromHtml("#1A1A1C");
        public override Color MenuItemPressedGradientEnd => ColorTranslator.FromHtml("#1A1A1C");
        public override Color SeparatorDark => ColorTranslator.FromHtml("#333333");
        public override Color SeparatorLight => ColorTranslator.FromHtml("#444444");
    }
}

