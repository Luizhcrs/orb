"""
ConfigManager - Gerenciador de configurações usando SQLite
Implementação incremental que não quebra funcionalidades existentes
"""
import sqlite3
import os
from pathlib import Path
from typing import Optional, Dict, Any
from cryptography.fernet import Fernet
import base64
from dotenv import load_dotenv

# Carregar variáveis de ambiente
load_dotenv()


class ConfigManager:
    """Gerenciador de configurações com SQLite e fallback para .env"""
    
    def __init__(self, db_path: Optional[str] = None):
        """
        Inicializa o ConfigManager
        
        Args:
            db_path: Caminho para o arquivo SQLite. Se None, usa 'orb.db' na raiz do projeto
        """
        # Definir caminho do banco
        if db_path is None:
            # Usar raiz do projeto (4 níveis acima: database -> src -> backend -> orb)
            project_root = Path(__file__).parent.parent.parent.parent
            db_path = project_root / "orb.db"
        
        self.db_path = str(db_path)
        
        # Inicializar encryption key
        self._init_encryption_key()
        
        # Inicializar banco de dados
        self._init_database()
    
    def _init_encryption_key(self):
        """Inicializa ou cria a chave de criptografia"""
        encryption_key = os.getenv('ENCRYPTION_KEY')
        
        if not encryption_key:
            # Gerar nova chave se não existir
            encryption_key = Fernet.generate_key().decode()
            print(f"AVISO: Nova ENCRYPTION_KEY gerada. Adicione ao .env:")
            print(f"ENCRYPTION_KEY={encryption_key}")
            print("IMPORTANTE: Salve esta chave para nao perder acesso as API keys!")
        
        try:
            self.cipher = Fernet(encryption_key.encode())
        except Exception as e:
            print(f"ERRO: Erro ao inicializar criptografia: {e}")
            # Fallback: gerar nova chave temporária
            self.cipher = Fernet(Fernet.generate_key())
    
    def _init_database(self):
        """Inicializa o banco de dados com o schema"""
        try:
            # Ler schema SQL
            schema_path = Path(__file__).parent / "schema.sql"
            
            if not schema_path.exists():
                print(f"AVISO: Schema SQL nao encontrado em {schema_path}")
                return
            
            with open(schema_path, 'r', encoding='utf-8') as f:
                schema_sql = f.read()
            
            # Executar schema
            with sqlite3.connect(self.db_path) as conn:
                conn.executescript(schema_sql)
                conn.commit()
            
            print(f"OK: Banco de dados inicializado: {self.db_path}")
        
        except Exception as e:
            print(f"ERRO: Erro ao inicializar banco de dados: {e}")
    
    def _encrypt(self, value: str) -> str:
        """Criptografa um valor"""
        if not value:
            return ""
        return self.cipher.encrypt(value.encode()).decode()
    
    def _decrypt(self, encrypted_value: str) -> str:
        """Descriptografa um valor"""
        if not encrypted_value:
            return ""
        try:
            return self.cipher.decrypt(encrypted_value.encode()).decode()
        except Exception as e:
            print(f"ERRO: Erro ao descriptografar: {e}")
            return ""
    
    def get_setting(self, key: str, default: Any = None) -> Optional[str]:
        """
        Obtém uma configuração do banco
        
        Args:
            key: Chave da configuração
            default: Valor padrão se não encontrar
            
        Returns:
            Valor da configuração ou default
        """
        try:
            with sqlite3.connect(self.db_path) as conn:
                cursor = conn.execute(
                    "SELECT value FROM settings WHERE key = ?",
                    (key,)
                )
                result = cursor.fetchone()
                return result[0] if result else default
        
        except Exception as e:
            print(f"ERRO: Erro ao obter configuracao {key}: {e}")
            return default
    
    def set_setting(self, key: str, value: str) -> bool:
        """
        Define uma configuração no banco
        
        Args:
            key: Chave da configuração
            value: Valor da configuração
            
        Returns:
            True se sucesso, False se erro
        """
        try:
            with sqlite3.connect(self.db_path) as conn:
                conn.execute(
                    """
                    INSERT INTO settings (key, value, updated_at)
                    VALUES (?, ?, CURRENT_TIMESTAMP)
                    ON CONFLICT(key) DO UPDATE SET
                        value = excluded.value,
                        updated_at = CURRENT_TIMESTAMP
                    """,
                    (key, value)
                )
                conn.commit()
            return True
        
        except Exception as e:
            print(f"ERRO: Erro ao salvar configuracao {key}: {e}")
            return False
    
    def get_llm_config(self) -> Dict[str, Any]:
        """
        Obtém configuração LLM ativa (com fallback para .env)
        
        Returns:
            Dict com provider, api_key e model
        """
        try:
            with sqlite3.connect(self.db_path) as conn:
                cursor = conn.execute(
                    """
                    SELECT provider, api_key_encrypted, model
                    FROM llm_config
                    WHERE is_active = 1
                    ORDER BY id DESC
                    LIMIT 1
                    """
                )
                result = cursor.fetchone()
                
                if result:
                    provider, encrypted_key, model = result
                    return {
                        'provider': provider,
                        'api_key': self._decrypt(encrypted_key) if encrypted_key else '',
                        'model': model
                    }
        
        except Exception as e:
            print(f"AVISO: Erro ao obter config LLM do banco: {e}")
        
        # Fallback para .env
        print("INFO: Usando configuracao do .env como fallback")
        return {
            'provider': os.getenv('LLM_PROVIDER', 'openai'),
            'api_key': os.getenv('OPENAI_API_KEY', ''),
            'model': os.getenv('LLM_MODEL', 'gpt-4o-mini')
        }
    
    def save_llm_config(self, provider: str, api_key: str, model: str) -> bool:
        """
        Salva configuração LLM no banco
        
        Args:
            provider: Nome do provedor ('openai' | 'anthropic')
            api_key: API key (será criptografada)
            model: Nome do modelo
            
        Returns:
            True se sucesso, False se erro
        """
        try:
            encrypted_key = self._encrypt(api_key)
            
            with sqlite3.connect(self.db_path) as conn:
                # Desativar configurações anteriores
                conn.execute("UPDATE llm_config SET is_active = 0")
                
                # Inserir nova configuração
                conn.execute(
                    """
                    INSERT INTO llm_config (provider, api_key_encrypted, model, is_active)
                    VALUES (?, ?, ?, 1)
                    """,
                    (provider, encrypted_key, model)
                )
                conn.commit()
            
            print(f"OK: Configuracao LLM salva: {provider} - {model}")
            return True
        
        except Exception as e:
            print(f"ERRO: Erro ao salvar config LLM: {e}")
            return False
    
    def get_all_settings(self) -> Dict[str, str]:
        """
        Obtém todas as configurações do banco
        
        Returns:
            Dict com todas as configurações
        """
        try:
            with sqlite3.connect(self.db_path) as conn:
                cursor = conn.execute("SELECT key, value FROM settings")
                return dict(cursor.fetchall())
        
        except Exception as e:
            print(f"ERRO: Erro ao obter todas as configuracoes: {e}")
            return {}


# Instância global (opcional, para facilitar uso)
_config_manager = None


def get_config_manager() -> ConfigManager:
    """Retorna instância singleton do ConfigManager"""
    global _config_manager
    if _config_manager is None:
        _config_manager = ConfigManager()
    return _config_manager


# Exemplo de uso
if __name__ == "__main__":
    # Teste básico
    cm = ConfigManager()
    
    print("\nSTATS: Testando ConfigManager...")
    
    # Testar configurações básicas
    cm.set_setting('theme', 'dark')
    print(f"Tema: {cm.get_setting('theme')}")
    
    # Testar config LLM
    cm.save_llm_config('openai', 'sk-test-key-123', 'gpt-4o-mini')
    config = cm.get_llm_config()
    print(f"LLM Config: {config['provider']} - {config['model']}")
    
    print("\nOK: Testes concluídos!")


