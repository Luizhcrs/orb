using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using OrbAgent.Frontend.Config;

namespace OrbAgent.Frontend.Services
{
    /// <summary>
    /// Gerencia o processo do backend Python
    /// </summary>
    public class BackendProcessManager : IDisposable
    {
        private Process? _backendProcess;
        private readonly string _backendPath;
        private readonly int _port = AppSettings.BackendPort;

        public bool IsRunning => _backendProcess != null && !_backendProcess.HasExited;

        public BackendProcessManager()
        {
            // Caminho relativo ao executável do WPF
            var exePath = AppDomain.CurrentDomain.BaseDirectory;
            _backendPath = Path.Combine(exePath, "..", "..", "..", "..", "backend");
            
            // Debug: mostrar caminho calculado
            System.Diagnostics.Debug.WriteLine($"Backend Path: {_backendPath}");
            System.Diagnostics.Debug.WriteLine($"Backend Path Exists: {Directory.Exists(_backendPath)}");
        }

        /// <summary>
        /// Inicia o backend Python
        /// </summary>
        public async Task<bool> StartBackendAsync()
        {
            if (IsRunning)
            {
                Debug.WriteLine("Backend já está em execução");
                return true;
            }

            try
            {
                // Verificar se o serviço Windows já está rodando
                if (await IsServiceRunningAsync())
                {
                    Debug.WriteLine("Backend está rodando como serviço Windows");
                    return true;
                }

                // Verificar se backend path existe
                if (!Directory.Exists(_backendPath))
                {
                    Debug.WriteLine($"Backend path não encontrado: {_backendPath}");
                    return false;
                }

                // Verificar se main.py existe
                var mainPyPath = Path.Combine(_backendPath, "main.py");
                if (!File.Exists(mainPyPath))
                {
                    Debug.WriteLine($"main.py não encontrado em: {mainPyPath}");
                    return false;
                }

                // Iniciar backend como processo local (para desenvolvimento)
                Debug.WriteLine("Iniciando backend Python local...");
                
                var pythonExe = FindPythonExecutable();
                if (pythonExe == null)
                {
                    Debug.WriteLine("Python não encontrado!");
                    return false;
                }

                Debug.WriteLine($"Usando Python: {pythonExe}");
                Debug.WriteLine($"Working Directory: {_backendPath}");

                var startInfo = new ProcessStartInfo
                {
                    FileName = pythonExe,
                    Arguments = "main.py", // Usar main.py diretamente
                    WorkingDirectory = _backendPath,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                _backendProcess = new Process { StartInfo = startInfo };
                
                // Capturar logs
                _backendProcess.OutputDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        Debug.WriteLine($"[Backend] {e.Data}");
                };
                
                _backendProcess.ErrorDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        Debug.WriteLine($"[Backend ERROR] {e.Data}");
                };

                _backendProcess.Start();
                _backendProcess.BeginOutputReadLine();
                _backendProcess.BeginErrorReadLine();

                Debug.WriteLine("Backend iniciado. Aguardando inicialização...");
                
                // Aguardar backend ficar pronto (até 15 segundos)
                for (int i = 0; i < 15; i++)
                {
                    await Task.Delay(1000);
                    if (await IsServiceRunningAsync())
                    {
                        Debug.WriteLine($"Backend ficou pronto em {i + 1} segundos");
                        return true;
                    }
                }
                
                Debug.WriteLine("Timeout: Backend não respondeu em 15 segundos");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao iniciar backend: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Verifica se o serviço Windows está rodando
        /// </summary>
        private async Task<bool> IsServiceRunningAsync()
        {
            try
            {
                using var client = new System.Net.Http.HttpClient();
                client.Timeout = TimeSpan.FromSeconds(2);
                var response = await client.GetAsync($"{AppSettings.BackendBaseUrl}/");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Encontra o executável do Python
        /// </summary>
        private string? FindPythonExecutable()
        {
            // Tentar python comum
            if (File.Exists("python.exe") || IsCommandAvailable("python"))
                return "python";

            // Tentar python3
            if (IsCommandAvailable("python3"))
                return "python3";

            // Tentar py launcher
            if (IsCommandAvailable("py"))
                return "py";

            return null;
        }

        /// <summary>
        /// Verifica se um comando está disponível no PATH
        /// </summary>
        private bool IsCommandAvailable(string command)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "where",
                        Arguments = command,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true
                    }
                };
                process.Start();
                process.WaitForExit();
                return process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Para o backend
        /// </summary>
        public void StopBackend()
        {
            if (_backendProcess != null && !_backendProcess.HasExited)
            {
                try
                {
                    _backendProcess.Kill();
                    _backendProcess.WaitForExit(5000);
                    Debug.WriteLine("Backend finalizado");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Erro ao finalizar backend: {ex.Message}");
                }
            }
        }

        public void Dispose()
        {
            StopBackend();
            _backendProcess?.Dispose();
        }
    }
}

