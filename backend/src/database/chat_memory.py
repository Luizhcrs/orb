"""
Chat Memory Manager - Compatível com LangChain
Implementa o padrão de memória do LangChain usando SQLite
"""
import sqlite3
import json
from pathlib import Path
from typing import List, Dict, Any, Optional
from datetime import datetime
import uuid


class ChatMessage:
    """Representa uma mensagem de chat compatível com LangChain"""
    
    def __init__(self, role: str, content: str, additional_kwargs: Optional[Dict] = None, created_at: Optional[str] = None):
        """
        Args:
            role: 'user' | 'assistant' | 'system'
            content: Conteúdo da mensagem
            additional_kwargs: Dados adicionais (ex: image_data)
            created_at: Timestamp ISO da criação da mensagem
        """
        self.role = role
        self.content = content
        self.additional_kwargs = additional_kwargs or {}
        self.created_at = created_at or datetime.now().isoformat()
    
    def to_dict(self) -> Dict[str, Any]:
        """Serializa para dict"""
        return {
            'role': self.role,
            'content': self.content,
            'additional_kwargs': self.additional_kwargs,
            'created_at': self.created_at
        }
    
    @classmethod
    def from_dict(cls, data: Dict[str, Any]) -> 'ChatMessage':
        """Deserializa de dict"""
        return cls(
            role=data.get('role', 'user'),
            content=data.get('content', ''),
            additional_kwargs=data.get('additional_kwargs', {}),
            created_at=data.get('created_at')
        )
    
    def to_json(self) -> str:
        """Serializa para JSON"""
        return json.dumps(self.to_dict())
    
    @classmethod
    def from_json(cls, json_str: str) -> 'ChatMessage':
        """Deserializa de JSON"""
        return cls.from_dict(json.loads(json_str))


class ChatMemoryManager:
    """
    Gerenciador de memória de chat compatível com LangChain
    
    Baseado em: langchain.memory.chat_message_histories.sql.SQLChatMessageHistory
    """
    
    def __init__(self, db_path: Optional[str] = None):
        """
        Args:
            db_path: Caminho para o banco SQLite. Se None, usa orb.db na raiz
        """
        if db_path is None:
            project_root = Path(__file__).parent.parent.parent.parent
            db_path = project_root / "orb.db"
        
        self.db_path = str(db_path)
        self._connection = None  # Connection persistente para performance
    
    @property
    def connection(self):
        """Retorna connection persistente (pooling pattern)"""
        if self._connection is None:
            self._connection = sqlite3.connect(self.db_path, check_same_thread=False)
            self._connection.row_factory = sqlite3.Row
        return self._connection
    
    def create_session(self, session_id: Optional[str] = None, title: Optional[str] = None) -> str:
        """
        Cria uma nova sessão de chat
        
        Args:
            session_id: ID customizado ou None para gerar UUID
            title: Título da sessão
            
        Returns:
            session_id criado
        """
        if session_id is None:
            session_id = str(uuid.uuid4())
        
        try:
            self.connection.execute(
                """
                INSERT OR IGNORE INTO chat_sessions (session_id, title)
                VALUES (?, ?)
                """,
                (session_id, title or 'Nova Conversa')
            )
            self.connection.commit()
            
            print(f"OK: Sessão criada: {session_id}")
            return session_id
        
        except Exception as e:
            print(f"ERRO: Erro ao criar sessão: {e}")
            return session_id
    
    def add_message(self, session_id: str, message: ChatMessage) -> bool:
        """
        Adiciona mensagem à sessão (padrão LangChain)
        
        Args:
            session_id: ID da sessão
            message: ChatMessage para adicionar
            
        Returns:
            True se sucesso
        """
        try:
            # Garantir que a sessão existe
            self.create_session(session_id)
            
            # Adicionar mensagem
            self.connection.execute(
                """
                INSERT INTO message_store (session_id, message)
                VALUES (?, ?)
                """,
                (session_id, message.to_json())
            )
            self.connection.commit()
            
            return True
        
        except Exception as e:
            print(f"ERRO: Erro ao adicionar mensagem: {e}")
            return False
    
    def add_user_message(self, session_id: str, content: str, image_data: Optional[str] = None) -> bool:
        """Helper: Adiciona mensagem do usuário"""
        additional_kwargs = {}
        if image_data:
            additional_kwargs['image_data'] = image_data
        
        message = ChatMessage('user', content, additional_kwargs)
        return self.add_message(session_id, message)
    
    def add_assistant_message(self, session_id: str, content: str) -> bool:
        """Helper: Adiciona mensagem do assistente"""
        message = ChatMessage('assistant', content)
        return self.add_message(session_id, message)
    
    def get_messages(self, session_id: str, limit: Optional[int] = None) -> List[ChatMessage]:
        """
        Obtém mensagens da sessão (padrão LangChain)
        
        Args:
            session_id: ID da sessão
            limit: Limite de mensagens (None = todas)
            
        Returns:
            Lista de ChatMessage
        """
        try:
            if limit:
                query = """
                    SELECT message, created_at FROM message_store
                    WHERE session_id = ?
                    ORDER BY created_at DESC
                    LIMIT ?
                """
                cursor = self.connection.execute(query, (session_id, limit))
            else:
                query = """
                    SELECT message, created_at FROM message_store
                    WHERE session_id = ?
                    ORDER BY created_at ASC
                """
                cursor = self.connection.execute(query, (session_id,))
            
            messages = []
            for row in cursor.fetchall():
                message = ChatMessage.from_json(row[0])
                message.created_at = row[1]  # Usar timestamp do banco
                messages.append(message)
            
            return messages
        
        except Exception as e:
            print(f"ERRO: Erro ao obter mensagens: {e}")
            return []
    
    def update_session_title(self, session_id: str, title: str) -> bool:
        """
        Atualiza o título de uma sessão
        
        Args:
            session_id: ID da sessão
            title: Novo título
            
        Returns:
            True se sucesso
        """
        try:
            self.connection.execute(
                """
                UPDATE chat_sessions
                SET title = ?
                WHERE session_id = ?
                """,
                (title, session_id)
            )
            self.connection.commit()
            
            return True
        
        except Exception as e:
            print(f"ERRO: Erro ao atualizar titulo da sessao: {e}")
            return False
    
    def get_session_info(self, session_id: str) -> Optional[Dict[str, Any]]:
        """
        Obtém informações da sessão
        
        Returns:
            Dict com session_id, title, created_at, updated_at, message_count
        """
        try:
            cursor = self.connection.execute(
                """
                SELECT session_id, title, created_at, updated_at, message_count
                FROM chat_sessions
                WHERE session_id = ?
                """,
                (session_id,)
            )
            row = cursor.fetchone()
            
            if row:
                return {
                    'session_id': row[0],
                    'title': row[1],
                    'created_at': row[2],
                    'updated_at': row[3],
                    'message_count': row[4]
                }
            return None
        
        except Exception as e:
            print(f"ERRO: Erro ao obter info da sessão: {e}")
            return None
    
    def list_sessions(self, limit: int = 50) -> List[Dict[str, Any]]:
        """
        Lista todas as sessões (mais recentes primeiro)
        
        Args:
            limit: Número máximo de sessões
            
        Returns:
            Lista de dicts com info das sessões
        """
        try:
            cursor = self.connection.execute(
                """
                SELECT session_id, title, created_at, updated_at, message_count
                FROM chat_sessions
                ORDER BY updated_at DESC
                LIMIT ?
                """,
                (limit,)
            )
            
            sessions = []
            for row in cursor.fetchall():
                sessions.append({
                    'session_id': row[0],
                    'title': row[1],
                    'created_at': row[2],
                    'updated_at': row[3],
                    'message_count': row[4]
                })
            
            return sessions
        
        except Exception as e:
            print(f"ERRO: Erro ao listar sessões: {e}")
            return []
    
    def delete_session(self, session_id: str) -> bool:
        """
        Deleta uma sessão e todas as suas mensagens
        
        Args:
            session_id: ID da sessão
            
        Returns:
            True se sucesso
        """
        try:
            with sqlite3.connect(self.db_path) as conn:
                # Deletar mensagens
                conn.execute(
                    "DELETE FROM message_store WHERE session_id = ?",
                    (session_id,)
                )
                # Deletar sessão
                conn.execute(
                    "DELETE FROM chat_sessions WHERE session_id = ?",
                    (session_id,)
                )
                conn.commit()
            
            print(f"OK: Sessão deletada: {session_id}")
            return True
        
        except Exception as e:
            print(f"ERRO: Erro ao deletar sessão: {e}")
            return False
    
    def clear_all_messages(self, session_id: str) -> bool:
        """
        Limpa todas as mensagens de uma sessão (mantém a sessão)
        
        Args:
            session_id: ID da sessão
            
        Returns:
            True se sucesso
        """
        try:
            with sqlite3.connect(self.db_path) as conn:
                conn.execute(
                    "DELETE FROM message_store WHERE session_id = ?",
                    (session_id,)
                )
                # Resetar contador
                conn.execute(
                    "UPDATE chat_sessions SET message_count = 0 WHERE session_id = ?",
                    (session_id,)
                )
                conn.commit()
            
            return True
        
        except Exception as e:
            print(f"ERRO: Erro ao limpar mensagens: {e}")
            return False


# Exemplo de uso
if __name__ == "__main__":
    print(" Testando ChatMemoryManager...\n")
    
    cm = ChatMemoryManager()
    
    # Criar sessão
    session_id = cm.create_session(title="Teste de Memória")
    print(f"Session ID: {session_id}\n")
    
    # Adicionar mensagens
    cm.add_user_message(session_id, "Olá! Como você está?")
    cm.add_assistant_message(session_id, "Olá! Estou bem, obrigado. Como posso ajudar?")
    cm.add_user_message(session_id, "Me explique sobre Python")
    cm.add_assistant_message(session_id, "Python é uma linguagem de programação...")
    
    # Obter mensagens
    messages = cm.get_messages(session_id)
    print(f"NOTE: Total de mensagens: {len(messages)}\n")
    
    for i, msg in enumerate(messages, 1):
        print(f"{i}. [{msg.role}] {msg.content[:50]}...")
    
    # Info da sessão
    info = cm.get_session_info(session_id)
    print(f"\nSTATS: Info da sessão:")
    print(f"   Título: {info['title']}")
    print(f"   Mensagens: {info['message_count']}")
    print(f"   Criada em: {info['created_at']}")
    
    # Listar todas as sessões
    sessions = cm.list_sessions()
    print(f"\nDOCS: Total de sessões: {len(sessions)}")
    
    print("\nOK: Testes concluídos!")


