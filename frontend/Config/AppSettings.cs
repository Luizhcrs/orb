using System;
using System.IO;

namespace OrbAgent.Frontend.Config
{
    /// <summary>
    /// Configurações centralizadas da aplicação
    /// </summary>
    public static class AppSettings
    {
        #region Backend Configuration
        
        /// <summary>
        /// URL base do backend (sem barra final)
        /// </summary>
        public static string BackendBaseUrl { get; set; } = "http://127.0.0.1:8000";
        
        /// <summary>
        /// Timeout padrão para requisições HTTP (em segundos)
        /// </summary>
        public static int HttpTimeoutSeconds { get; set; } = 30;
        
        /// <summary>
        /// Caminho relativo para o executável do backend
        /// </summary>
        public static string BackendExecutablePath { get; set; } = Path.Combine("..", "backend", "src", "main.py");
        
        /// <summary>
        /// Porta do backend
        /// </summary>
        public static int BackendPort { get; set; } = 8000;
        
        #endregion

        #region UI Configuration
        
        /// <summary>
        /// Largura da janela de chat no modo compacto
        /// </summary>
        public static double ChatWindowCompactWidth { get; set; } = 400;
        
        /// <summary>
        /// Altura da janela de chat no modo compacto
        /// </summary>
        public static double ChatWindowCompactHeight { get; set; } = 500;
        
        /// <summary>
        /// Largura da janela de chat no modo expandido
        /// </summary>
        public static double ChatWindowExpandedWidth { get; set; } = 600;
        
        /// <summary>
        /// Altura da janela de chat no modo expandido
        /// </summary>
        public static double ChatWindowExpandedHeight { get; set; } = 700;
        
        /// <summary>
        /// Largura da janela de configuração
        /// </summary>
        public static double ConfigWindowWidth { get; set; } = 800;
        
        /// <summary>
        /// Altura da janela de configuração
        /// </summary>
        public static double ConfigWindowHeight { get; set; } = 600;
        
        /// <summary>
        /// Tamanho do Orb (raio)
        /// </summary>
        public static double OrbSize { get; set; } = 60;
        
        /// <summary>
        /// Tempo de inatividade antes do Orb desaparecer (em segundos)
        /// 0 = nunca desaparece
        /// </summary>
        public static int OrbInactivityTimeoutSeconds { get; set; } = 5;
        
        #endregion

        #region Hot Corner Configuration
        
        /// <summary>
        /// Margem de detecção do hot corner (em pixels)
        /// </summary>
        public static int HotCornerMargin { get; set; } = 10;
        
        /// <summary>
        /// Tempo de permanência no hot corner para ativar (em ms)
        /// </summary>
        public static int HotCornerDelayMs { get; set; } = 300;
        
        /// <summary>
        /// Canto do hot corner (TopLeft, TopRight, BottomLeft, BottomRight)
        /// </summary>
        public static string HotCornerPosition { get; set; } = "TopLeft";
        
        #endregion

        #region Logging Configuration
        
        /// <summary>
        /// Habilitar logs detalhados (debug)
        /// </summary>
        public static bool EnableDebugLogging { get; set; } = true;
        
        /// <summary>
        /// Nome do arquivo de log
        /// </summary>
        public static string LogFileName { get; set; } = "orb_debug.log";
        
        /// <summary>
        /// Tamanho máximo do arquivo de log (em MB)
        /// </summary>
        public static int MaxLogFileSizeMB { get; set; } = 10;
        
        #endregion

        #region Application Info
        
        /// <summary>
        /// Nome da aplicação
        /// </summary>
        public static string AppName { get; } = "Orb Agent";
        
        /// <summary>
        /// Versão da aplicação
        /// </summary>
        public static string AppVersion { get; } = "1.0.0";
        
        /// <summary>
        /// Nome do processo do frontend
        /// </summary>
        public static string ProcessName { get; } = "Orb";
        
        #endregion

        #region Paths
        
        /// <summary>
        /// Caminho base da aplicação
        /// </summary>
        public static string AppBasePath { get; } = AppDomain.CurrentDomain.BaseDirectory;
        
        /// <summary>
        /// Caminho do arquivo de log
        /// </summary>
        public static string LogFilePath => Path.Combine(AppBasePath, LogFileName);
        
        #endregion

        #region Validation
        
        /// <summary>
        /// Valida as configurações da aplicação
        /// </summary>
        /// <returns>True se todas as configurações são válidas</returns>
        public static bool Validate()
        {
            try
            {
                // Validar URL do backend
                if (string.IsNullOrWhiteSpace(BackendBaseUrl))
                {
                    throw new InvalidOperationException("BackendBaseUrl não pode ser vazio");
                }

                if (!Uri.TryCreate(BackendBaseUrl, UriKind.Absolute, out _))
                {
                    throw new InvalidOperationException($"BackendBaseUrl inválida: {BackendBaseUrl}");
                }

                // Validar porta
                if (BackendPort < 1 || BackendPort > 65535)
                {
                    throw new InvalidOperationException($"BackendPort inválida: {BackendPort}");
                }

                // Validar timeouts
                if (HttpTimeoutSeconds < 1)
                {
                    throw new InvalidOperationException($"HttpTimeoutSeconds deve ser >= 1");
                }

                if (OrbInactivityTimeoutSeconds < 0)
                {
                    throw new InvalidOperationException($"OrbInactivityTimeoutSeconds deve ser >= 0");
                }

                // Validar dimensões
                if (ChatWindowCompactWidth <= 0 || ChatWindowCompactHeight <= 0)
                {
                    throw new InvalidOperationException("Dimensões da janela de chat (compacto) inválidas");
                }

                if (ChatWindowExpandedWidth <= 0 || ChatWindowExpandedHeight <= 0)
                {
                    throw new InvalidOperationException("Dimensões da janela de chat (expandido) inválidas");
                }

                if (ConfigWindowWidth <= 0 || ConfigWindowHeight <= 0)
                {
                    throw new InvalidOperationException("Dimensões da janela de configuração inválidas");
                }

                if (OrbSize <= 0)
                {
                    throw new InvalidOperationException("OrbSize deve ser > 0");
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($" Erro na validação de configurações: {ex.Message}");
                return false;
            }
        }
        
        #endregion

        #region Load from Environment (Optional)
        
        /// <summary>
        /// Carrega configurações de variáveis de ambiente (se existirem)
        /// Útil para desenvolvimento e testes
        /// </summary>
        public static void LoadFromEnvironment()
        {
            var backendUrl = Environment.GetEnvironmentVariable("ORB_BACKEND_URL");
            if (!string.IsNullOrWhiteSpace(backendUrl))
            {
                BackendBaseUrl = backendUrl;
            }

            var backendPort = Environment.GetEnvironmentVariable("ORB_BACKEND_PORT");
            if (!string.IsNullOrWhiteSpace(backendPort) && int.TryParse(backendPort, out var port))
            {
                BackendPort = port;
            }

            var debug = Environment.GetEnvironmentVariable("ORB_DEBUG");
            if (!string.IsNullOrWhiteSpace(debug))
            {
                EnableDebugLogging = debug.ToLower() == "true" || debug == "1";
            }

            System.Diagnostics.Debug.WriteLine($" Configurações carregadas:");
            System.Diagnostics.Debug.WriteLine($"   Backend URL: {BackendBaseUrl}");
            System.Diagnostics.Debug.WriteLine($"   Backend Port: {BackendPort}");
            System.Diagnostics.Debug.WriteLine($"   Debug Logging: {EnableDebugLogging}");
        }
        
        #endregion
    }
}

