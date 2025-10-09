using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace OrbAgent.Frontend.Services
{
    /// <summary>
    /// Serviço de logging para arquivo
    /// </summary>
    public static class LoggingService
    {
        private static readonly string LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "orb_debug.log");
        private static readonly object LockObject = new object();

        /// <summary>
        /// Escreve uma mensagem de log
        /// </summary>
        public static void Log(string message)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var logEntry = $"[{timestamp}] {message}{Environment.NewLine}";

            lock (LockObject)
            {
                try
                {
                    File.AppendAllText(LogFilePath, logEntry, Encoding.UTF8);
                }
                catch (Exception ex)
                {
                    // Se não conseguir escrever no arquivo, pelo menos mostrar no console
                    Console.WriteLine($"Erro ao escrever log: {ex.Message}");
                    Console.WriteLine($"Log message: {message}");
                }
            }

            // Também escrever no Debug Output
            System.Diagnostics.Debug.WriteLine(message);
        }

        /// <summary>
        /// Log de erro com stack trace
        /// </summary>
        public static void LogError(string message, Exception? ex = null)
        {
            var fullMessage = $"❌ ERRO: {message}";
            if (ex != null)
            {
                fullMessage += $"{Environment.NewLine}Exceção: {ex.Message}{Environment.NewLine}Stack Trace: {ex.StackTrace}";
            }
            Log(fullMessage);
        }

        /// <summary>
        /// Log de sucesso
        /// </summary>
        public static void LogSuccess(string message)
        {
            Log($"✅ SUCESSO: {message}");
        }

        /// <summary>
        /// Log de informação
        /// </summary>
        public static void LogInfo(string message)
        {
            Log($"ℹ️ INFO: {message}");
        }

        /// <summary>
        /// Log de debug
        /// </summary>
        public static void LogDebug(string message)
        {
            Log($"🔍 DEBUG: {message}");
        }

        /// <summary>
        /// Limpa o arquivo de log
        /// </summary>
        public static void ClearLog()
        {
            lock (LockObject)
            {
                try
                {
                    if (File.Exists(LogFilePath))
                    {
                        File.WriteAllText(LogFilePath, string.Empty);
                        LogInfo("Log file cleared");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao limpar log: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Retorna o caminho do arquivo de log
        /// </summary>
        public static string GetLogFilePath()
        {
            return LogFilePath;
        }
    }
}
