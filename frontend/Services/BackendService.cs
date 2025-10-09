using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using OrbAgent.Frontend.Models;
using OrbAgent.Frontend.Config;

namespace OrbAgent.Frontend.Services
{
    /// <summary>
    /// Serviço para comunicação HTTP com o backend Python FastAPI
    /// </summary>
    public class BackendService
    {
        private readonly HttpClient _httpClient;

        public BackendService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(AppSettings.BackendBaseUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(AppSettings.HttpTimeoutSeconds);
        }

        /// <summary>
        /// Envia mensagem para o agente ORB
        /// </summary>
        public async Task<AgentResponse> SendMessageAsync(AgentRequest request)
        {
            try
            {
                var hasImage = request.ImageData != null;
                var imageLength = hasImage ? request.ImageData!.Length : 0;
                
                LoggingService.Log($" SendMessageAsync INICIADO");
                LoggingService.Log($" Message: {request.Message}");
                LoggingService.Log($" SessionId: {request.SessionId}");
                LoggingService.Log($" Tem imagem? {hasImage} (tamanho: {imageLength} chars)");
                
                if (hasImage)
                {
                    var prefix = request.ImageData!.Substring(0, Math.Min(100, request.ImageData.Length));
                    LoggingService.Log($" Imagem prefix: {prefix}...");
                }
                
                // FORMATO SIMPLES QUE O BACKEND ESPERA (igual ao Electron)
                var backendRequest = new
                {
                    message = request.Message,
                    session_id = request.SessionId,
                    image_data = request.ImageData
                };

                LoggingService.Log($" Enviando POST para /api/v1/agent/message...");

                var response = await _httpClient.PostAsJsonAsync("/api/v1/agent/message", backendRequest);
                
                LoggingService.Log($" Response Status: {response.StatusCode}");
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    LoggingService.Log($" ERRO DO BACKEND - Status: {response.StatusCode}");
                    LoggingService.Log($" ERRO DO BACKEND - Content: {errorContent}");
                    throw new Exception($"Backend retornou {response.StatusCode}: {errorContent}");
                }
                
                var result = await response.Content.ReadFromJsonAsync<AgentResponse>();
                var preview = result?.Content?.Substring(0, Math.Min(100, result.Content?.Length ?? 0)) ?? "";
                LoggingService.Log($" Response recebida: {preview}...");
                
                return result ?? throw new Exception("Response was null");
            }
            catch (HttpRequestException ex)
            {
                LoggingService.Log($" HttpRequestException: {ex.Message}");
                throw new Exception($"Erro ao conectar com backend: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                LoggingService.Log($" Exception: {ex.Message}");
                LoggingService.Log($" StackTrace: {ex.StackTrace}");
                throw new Exception($"Erro ao processar mensagem: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Verifica se o backend está online
        /// </summary>
        public async Task<bool> IsBackendOnlineAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Carrega configurações do backend
        /// </summary>
        public async Task<object?> GetConfigAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine(" BackendService.GetConfigAsync chamado");
                System.Diagnostics.Debug.WriteLine($"URL: {_httpClient.BaseAddress}/api/v1/config");
                
                var response = await _httpClient.GetAsync("/api/v1/config");
                
                System.Diagnostics.Debug.WriteLine($"Response Status: {response.StatusCode}");
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($" Erro do backend: {errorContent}");
                    throw new Exception($"Backend retornou {response.StatusCode}: {errorContent}");
                }
                
                var jsonString = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($" JSON response: {jsonString}");
                
                var result = System.Text.Json.JsonSerializer.Deserialize<object>(jsonString);
                System.Diagnostics.Debug.WriteLine($" Deserialização OK: {result != null}");
                
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($" EXCEÇÃO em GetConfigAsync: {ex.Message}");
                throw new Exception($"Erro ao carregar configurações: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Salva configurações no backend
        /// </summary>
        public async Task<bool> SaveConfigAsync(object config)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine(" BackendService.SaveConfigAsync chamado");
                System.Diagnostics.Debug.WriteLine($"URL: {_httpClient.BaseAddress}/api/v1/config");
                
                var response = await _httpClient.PostAsJsonAsync("/api/v1/config", config);
                
                System.Diagnostics.Debug.WriteLine($"Response Status: {response.StatusCode}");
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($" Erro do backend: {errorContent}");
                }
                
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($" EXCEÇÃO: {ex.Message}");
                throw new Exception($"Erro ao salvar configurações: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Testa conexão com API key
        /// </summary>
        public async Task<bool> TestApiKeyAsync(string apiKey)
        {
            try
            {
                var testRequest = new { message = "Teste de conexão", apiKey = apiKey };
                var response = await _httpClient.PostAsJsonAsync("/api/v1/agent/message", testRequest);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}

