using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using OrbAgent.Frontend.Models;

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
            _httpClient.BaseAddress = new Uri("http://127.0.0.1:8000");
            _httpClient.Timeout = TimeSpan.FromSeconds(120); // LLM pode demorar
        }

        /// <summary>
        /// Envia mensagem para o agente ORB
        /// </summary>
        public async Task<AgentResponse> SendMessageAsync(AgentRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/v1/agent/message", request);
                response.EnsureSuccessStatusCode();
                
                var result = await response.Content.ReadFromJsonAsync<AgentResponse>();
                return result ?? throw new Exception("Response was null");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Erro ao conectar com backend: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
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
    }
}

