-- Schema SQLite para ORB
-- Versão: 1.0.0
-- Compatível com LangChain memory pattern
-- Criação incremental, sem quebrar funcionalidades existentes

-- Tabela de configurações gerais
CREATE TABLE IF NOT EXISTS settings (
    key TEXT PRIMARY KEY,
    value TEXT NOT NULL,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Tabela de configurações LLM (com API key criptografada)
CREATE TABLE IF NOT EXISTS llm_config (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    provider TEXT NOT NULL,        -- 'openai' | 'anthropic'
    api_key_encrypted TEXT,        -- API key criptografada
    model TEXT NOT NULL,
    is_active BOOLEAN DEFAULT 1,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- ===== LANGCHAIN MEMORY PATTERN =====
-- Baseado em: langchain.memory.chat_message_histories.sql

-- Tabela de sessões de chat (message_store sessions)
CREATE TABLE IF NOT EXISTS message_store (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    session_id TEXT NOT NULL,
    message TEXT NOT NULL,           -- JSON serializado da mensagem
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Índice para buscar mensagens por sessão
CREATE INDEX IF NOT EXISTS idx_message_store_session 
ON message_store(session_id);

-- Tabela adicional para metadados de sessão (extensão do padrão LangChain)
CREATE TABLE IF NOT EXISTS chat_sessions (
    session_id TEXT PRIMARY KEY,
    title TEXT,                       -- Título gerado da primeira mensagem
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    message_count INTEGER DEFAULT 0
);

-- Trigger para atualizar updated_at quando nova mensagem é adicionada
CREATE TRIGGER IF NOT EXISTS update_session_timestamp
AFTER INSERT ON message_store
BEGIN
    UPDATE chat_sessions 
    SET updated_at = CURRENT_TIMESTAMP,
        message_count = message_count + 1
    WHERE session_id = NEW.session_id;
END;

-- Índices para melhor performance
CREATE INDEX IF NOT EXISTS idx_settings_key ON settings(key);
CREATE INDEX IF NOT EXISTS idx_llm_active ON llm_config(is_active);
CREATE INDEX IF NOT EXISTS idx_sessions_updated ON chat_sessions(updated_at DESC);

-- Inserir configurações padrão (se não existirem)
INSERT OR IGNORE INTO settings (key, value) VALUES ('theme', 'dark');
INSERT OR IGNORE INTO settings (key, value) VALUES ('language', 'pt-BR');
INSERT OR IGNORE INTO settings (key, value) VALUES ('startup', 'false');
INSERT OR IGNORE INTO settings (key, value) VALUES ('keep_history', 'true');

