using System;
using System.Text.Json.Serialization;

namespace OrbAgent.Frontend.Models
{
    /// <summary>
    /// Configurações da aplicação
    /// </summary>
    public class AppConfig
    {
        /// <summary>
        /// Configurações gerais
        /// </summary>
        [JsonPropertyName("general")]
        public GeneralSettings General { get; set; } = new();

        /// <summary>
        /// Configurações do agente/LLM
        /// </summary>
        [JsonPropertyName("agent")]
        public AgentSettings Agent { get; set; } = new();
    }

    /// <summary>
    /// Configurações gerais
    /// </summary>
    public class GeneralSettings
    {
        [JsonPropertyName("theme")]
        public string Theme { get; set; } = "dark";
        
        [JsonPropertyName("language")]
        public string Language { get; set; } = "pt-BR";
        
        [JsonPropertyName("startup")]
        [JsonConverter(typeof(StringToBoolConverter))]
        public bool StartWithWindows { get; set; } = true;
        
        [JsonPropertyName("keep_history")]
        [JsonConverter(typeof(StringToBoolConverter))]
        public bool KeepHistory { get; set; } = true;
    }

    /// <summary>
    /// Configurações do agente
    /// </summary>
    public class AgentSettings
    {
        [JsonPropertyName("provider")]
        public string LlmProvider { get; set; } = "openai";
        
        [JsonPropertyName("api_key")]
        public string ApiKey { get; set; } = string.Empty;
        
        [JsonPropertyName("model")]
        public string Model { get; set; } = "gpt-4o-mini"; // Modelo fixo
        
        [JsonPropertyName("max_tokens")]
        public int MaxTokens { get; set; } = 4000;
        
        [JsonPropertyName("temperature")]
        public double Temperature { get; set; } = 0.7;
    }
    
    /// <summary>
    /// Sessão de histórico de conversa
    /// </summary>
    public class HistorySession
    {
        [JsonPropertyName("session_id")]
        public string SessionId { get; set; } = string.Empty;
        
        [JsonPropertyName("title")]
        public string? Title { get; set; }
        
        [JsonPropertyName("message_count")]
        public int MessageCount { get; set; }
        
        [JsonPropertyName("created_at")]
        [System.Text.Json.Serialization.JsonConverter(typeof(DateTimeConverter))]
        public DateTime CreatedAt { get; set; }
        
        [JsonPropertyName("updated_at")]
        [System.Text.Json.Serialization.JsonConverter(typeof(DateTimeConverter))]
        public DateTime UpdatedAt { get; set; }
    }
    
    /// <summary>
    /// Conversor que aceita string "1"/"0" ou bool true/false
    /// </summary>
    public class StringToBoolConverter : System.Text.Json.Serialization.JsonConverter<bool>
    {
        public override bool Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
        {
            if (reader.TokenType == System.Text.Json.JsonTokenType.String)
            {
                var stringValue = reader.GetString();
                return stringValue == "1" || stringValue?.ToLower() == "true";
            }
            else if (reader.TokenType == System.Text.Json.JsonTokenType.True)
            {
                return true;
            }
            else if (reader.TokenType == System.Text.Json.JsonTokenType.False)
            {
                return false;
            }
            
            return false;
        }

        public override void Write(System.Text.Json.Utf8JsonWriter writer, bool value, System.Text.Json.JsonSerializerOptions options)
        {
            writer.WriteStringValue(value ? "1" : "0");
        }
    }
    
    /// <summary>
    /// Conversor customizado para DateTime
    /// </summary>
    public class DateTimeConverter : System.Text.Json.Serialization.JsonConverter<DateTime>
    {
        public override DateTime Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
        {
            var value = reader.GetString();
            if (string.IsNullOrEmpty(value))
                return DateTime.Now;
                
            if (DateTime.TryParse(value, out var result))
                return result;
                
            // Fallback para formato ISO
            if (DateTime.TryParseExact(value, "yyyy-MM-ddTHH:mm:ss.fffZ", null, System.Globalization.DateTimeStyles.AssumeUniversal, out result))
                return result;
                
            return DateTime.Now;
        }

        public override void Write(System.Text.Json.Utf8JsonWriter writer, DateTime value, System.Text.Json.JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
        }
    }
}
