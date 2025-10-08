"""
Router de Histórico de Conversas
Endpoints para gerenciar sessões e mensagens
"""
from fastapi import APIRouter, HTTPException
from pydantic import BaseModel
from typing import List, Optional, Dict, Any
import sys
from pathlib import Path

# Adicionar database ao path
database_path = Path(__file__).parent.parent.parent / "database"
if str(database_path) not in sys.path:
    sys.path.insert(0, str(database_path))

from database.chat_memory import ChatMemoryManager, ChatMessage

router = APIRouter(prefix="/history", tags=["history"])

# Instância global do ChatMemoryManager
chat_memory = ChatMemoryManager()


# ===== MODELS =====

class SessionResponse(BaseModel):
    """Resposta com informações de uma sessão"""
    session_id: str
    title: str
    created_at: str
    updated_at: str
    message_count: int


class MessageResponse(BaseModel):
    """Resposta com uma mensagem"""
    role: str
    content: str
    additional_kwargs: Dict[str, Any] = {}


class SessionWithMessagesResponse(BaseModel):
    """Resposta com sessão e suas mensagens"""
    session: SessionResponse
    messages: List[MessageResponse]


# ===== ENDPOINTS =====

@router.get("/sessions", response_model=List[SessionResponse])
async def list_sessions(limit: int = 50):
    """
    Lista todas as sessões de chat (mais recentes primeiro)
    
    Args:
        limit: Número máximo de sessões a retornar (padrão: 50)
    
    Returns:
        Lista de sessões
    """
    try:
        sessions = chat_memory.list_sessions(limit=limit)
        return sessions
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Erro ao listar sessões: {str(e)}")


@router.get("/sessions/{session_id}", response_model=SessionResponse)
async def get_session(session_id: str):
    """
    Obtém informações de uma sessão específica
    
    Args:
        session_id: ID da sessão
    
    Returns:
        Informações da sessão
    """
    try:
        session_info = chat_memory.get_session_info(session_id)
        
        if not session_info:
            raise HTTPException(status_code=404, detail="Sessão não encontrada")
        
        return session_info
    except HTTPException:
        raise
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Erro ao obter sessão: {str(e)}")


@router.get("/sessions/{session_id}/messages", response_model=List[MessageResponse])
async def get_session_messages(session_id: str, limit: Optional[int] = None):
    """
    Obtém todas as mensagens de uma sessão
    
    Args:
        session_id: ID da sessão
        limit: Limite de mensagens (None = todas)
    
    Returns:
        Lista de mensagens
    """
    try:
        messages = chat_memory.get_messages(session_id, limit=limit)
        
        # Converter ChatMessage para dict
        messages_dict = []
        for msg in messages:
            messages_dict.append({
                'role': msg.role,
                'content': msg.content,
                'additional_kwargs': msg.additional_kwargs,
                'created_at': msg.created_at
            })
        
        return messages_dict
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Erro ao obter mensagens: {str(e)}")


@router.get("/sessions/{session_id}/full", response_model=SessionWithMessagesResponse)
async def get_session_with_messages(session_id: str, limit: Optional[int] = None):
    """
    Obtém sessão completa com todas as mensagens
    
    Args:
        session_id: ID da sessão
        limit: Limite de mensagens (None = todas)
    
    Returns:
        Sessão com suas mensagens
    """
    try:
        # Obter info da sessão
        session_info = chat_memory.get_session_info(session_id)
        
        if not session_info:
            raise HTTPException(status_code=404, detail="Sessão não encontrada")
        
        # Obter mensagens
        messages = chat_memory.get_messages(session_id, limit=limit)
        
        # Converter ChatMessage para dict
        messages_dict = []
        for msg in messages:
            messages_dict.append({
                'role': msg.role,
                'content': msg.content,
                'additional_kwargs': msg.additional_kwargs
            })
        
        return {
            'session': session_info,
            'messages': messages_dict
        }
    except HTTPException:
        raise
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Erro ao obter sessão completa: {str(e)}")


@router.delete("/sessions/{session_id}")
async def delete_session(session_id: str):
    """
    Deleta uma sessão e todas as suas mensagens
    
    Args:
        session_id: ID da sessão
    
    Returns:
        Confirmação de deleção
    """
    try:
        success = chat_memory.delete_session(session_id)
        
        if not success:
            raise HTTPException(status_code=500, detail="Erro ao deletar sessão")
        
        return {"message": "Sessão deletada com sucesso", "session_id": session_id}
    except HTTPException:
        raise
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Erro ao deletar sessão: {str(e)}")


@router.post("/sessions/{session_id}/clear")
async def clear_session_messages(session_id: str):
    """
    Limpa todas as mensagens de uma sessão (mantém a sessão)
    
    Args:
        session_id: ID da sessão
    
    Returns:
        Confirmação de limpeza
    """
    try:
        success = chat_memory.clear_all_messages(session_id)
        
        if not success:
            raise HTTPException(status_code=500, detail="Erro ao limpar mensagens")
        
        return {"message": "Mensagens limpas com sucesso", "session_id": session_id}
    except HTTPException:
        raise
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Erro ao limpar mensagens: {str(e)}")


@router.get("/stats")
async def get_history_stats():
    """
    Obtém estatísticas do histórico
    
    Returns:
        Estatísticas gerais
    """
    try:
        sessions = chat_memory.list_sessions(limit=1000)
        
        total_sessions = len(sessions)
        total_messages = sum(s['message_count'] for s in sessions)
        
        return {
            'total_sessions': total_sessions,
            'total_messages': total_messages,
            'sessions': sessions[:10]  # Últimas 10 sessões
        }
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Erro ao obter estatísticas: {str(e)}")


