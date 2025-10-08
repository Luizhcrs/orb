using System;

namespace OrbAgent.Frontend.Models
{
    /// <summary>
    /// Representa uma mensagem no chat
    /// </summary>
    public class ChatMessage
    {
        public string Role { get; set; } = string.Empty; // "user" ou "assistant"
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string? ImageData { get; set; } // Base64 da imagem capturada (opcional)

        public bool IsUser => Role == "user";
        public bool IsAssistant => Role == "assistant";
    }

    /// <summary>
    /// Request para enviar mensagem ao backend
    /// </summary>
    public class AgentRequest
    {
        public string Message { get; set; } = string.Empty;
        public string? SessionId { get; set; }
        public string? ImageData { get; set; }
    }

    /// <summary>
    /// Response do backend
    /// </summary>
    public class AgentResponse
    {
        public string Response { get; set; } = string.Empty;
        public string Timestamp { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
    }
}

