using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using OrbAgent.Frontend.Config;

namespace OrbAgent.Frontend.Services
{
    /// <summary>
    /// Gerencia o processo do backend Python
    /// </summary>
    public class BackendProcessManager : IDisposable
    {
        // Win32 API para Job Objects (garante que o backend feche com o frontend)
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr CreateJobObject(IntPtr lpJobAttributes, string? name);

        [DllImport("kernel32.dll")]
        private static extern bool SetInformationJobObject(IntPtr job, int infoClass,
            IntPtr lpJobObjectInfo, uint cbJobObjectInfoLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AssignProcessToJobObject(IntPtr job, IntPtr process);

        [StructLayout(LayoutKind.Sequential)]
        private struct JOBOBJECT_BASIC_LIMIT_INFORMATION
        {
            public long PerProcessUserTimeLimit;
            public long PerJobUserTimeLimit;
            public uint LimitFlags;
            public UIntPtr MinimumWorkingSetSize;
            public UIntPtr MaximumWorkingSetSize;
            public uint ActiveProcessLimit;
            public UIntPtr Affinity;
            public uint PriorityClass;
            public uint SchedulingClass;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct IO_COUNTERS
        {
            public ulong ReadOperationCount;
            public ulong WriteOperationCount;
            public ulong OtherOperationCount;
            public ulong ReadTransferCount;
            public ulong WriteTransferCount;
            public ulong OtherTransferCount;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct JOBOBJECT_EXTENDED_LIMIT_INFORMATION
        {
            public JOBOBJECT_BASIC_LIMIT_INFORMATION BasicLimitInformation;
            public IO_COUNTERS IoInfo;
            public UIntPtr ProcessMemoryLimit;
            public UIntPtr JobMemoryLimit;
            public UIntPtr PeakProcessMemoryUsed;
            public UIntPtr PeakJobMemoryUsed;
        }

        private const int JobObjectExtendedLimitInformation = 9;
        private const uint JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE = 0x2000;

        private Process? _backendProcess;
        private readonly string _backendPath;
        private readonly int _port = AppSettings.BackendPort;
        private IntPtr _jobHandle = IntPtr.Zero;

        public bool IsRunning => _backendProcess != null && !_backendProcess.HasExited;

        public BackendProcessManager()
        {
            // Caminho relativo ao executável do WPF
            var exePath = AppDomain.CurrentDomain.BaseDirectory;
            
            // Verificar se está em modo desenvolvimento ou instalado
            // Instalado: backend\ ao lado do executável
            // Desenvolvimento: backend\ na raiz do projeto
            var installedBackendPath = Path.Combine(exePath, "backend", "dist");
            var devBackendPath = Path.Combine(exePath, "..", "..", "..", "..", "backend", "dist");
            
            if (Directory.Exists(installedBackendPath))
            {
                _backendPath = installedBackendPath;
            }
            else if (Directory.Exists(devBackendPath))
            {
                _backendPath = Path.GetFullPath(devBackendPath);
            }
            else
            {
                _backendPath = installedBackendPath; // Fallback
            }
            
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
                // Verificar se backend path existe
                if (!Directory.Exists(_backendPath))
                {
                    Debug.WriteLine($"Backend path não encontrado: {_backendPath}");
                    return false;
                }

                // Procurar pelo executável standalone
                var backendExe = Path.Combine(_backendPath, "orb-backend.exe");
                if (!File.Exists(backendExe))
                {
                    Debug.WriteLine($"orb-backend.exe não encontrado em: {backendExe}");
                    return false;
                }

                // Iniciar backend como processo
                Debug.WriteLine($"Iniciando backend: {backendExe}");
                Debug.WriteLine($"Working Directory: {_backendPath}");

                var startInfo = new ProcessStartInfo
                {
                    FileName = backendExe,
                    WorkingDirectory = _backendPath,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                _backendProcess = new Process { StartInfo = startInfo };
                
                // IMPORTANTE: Garantir que o backend seja finalizado quando o frontend fechar
                _backendProcess.EnableRaisingEvents = true;
                
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
                
                _backendProcess.Exited += (s, e) =>
                {
                    Debug.WriteLine("Backend finalizado");
                };

                _backendProcess.Start();
                _backendProcess.BeginOutputReadLine();
                _backendProcess.BeginErrorReadLine();

                // Criar Job Object para garantir que o backend feche com o frontend
                try
                {
                    _jobHandle = CreateJobObject(IntPtr.Zero, null);
                    if (_jobHandle != IntPtr.Zero)
                    {
                        var info = new JOBOBJECT_EXTENDED_LIMIT_INFORMATION();
                        info.BasicLimitInformation.LimitFlags = JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE;

                        int length = Marshal.SizeOf(typeof(JOBOBJECT_EXTENDED_LIMIT_INFORMATION));
                        IntPtr extendedInfoPtr = Marshal.AllocHGlobal(length);
                        Marshal.StructureToPtr(info, extendedInfoPtr, false);

                        if (SetInformationJobObject(_jobHandle, JobObjectExtendedLimitInformation, extendedInfoPtr, (uint)length))
                        {
                            AssignProcessToJobObject(_jobHandle, _backendProcess.Handle);
                            Debug.WriteLine("Backend vinculado ao Job Object - será finalizado automaticamente");
                        }

                        Marshal.FreeHGlobal(extendedInfoPtr);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Aviso: Não foi possível criar Job Object: {ex.Message}");
                }

                Debug.WriteLine("Backend iniciado. Aguardando inicialização...");
                
                // Aguardar backend ficar pronto (até 15 segundos)
                for (int i = 0; i < 15; i++)
                {
                    await Task.Delay(1000);
                    if (await IsBackendRespondingAsync())
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
        /// Verifica se o backend está respondendo
        /// </summary>
        private async Task<bool> IsBackendRespondingAsync()
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

