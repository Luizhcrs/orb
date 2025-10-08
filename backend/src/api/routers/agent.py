"""
Router do Agente ORB
"""

from fastapi import APIRouter, HTTPException, Depends
from pydantic import BaseModel
from typing import Optional, Dict, Any
import uuid
from datetime import datetime

# Importa o agente (lazy import para evitar travar na inicialização)
import sys
import os
sys.path.append(os.path.join(os.path.dirname(__file__), '..', '..'))

router = APIRouter(prefix="/agent", tags=["agent"])

# Instância global do agente (singleton para performance)
_agente_instance = None

def get_agente():
    """Dependency para obter instância do agente (singleton pattern)"""
    global _agente_instance
    if _agente_instance is None:
        try:
            from agentes.orb_agent.agente import AgenteORB
            _agente_instance = AgenteORB()
        except ValueError as e:
            # Erro de configuração (ex: API key não configurada)
            raise HTTPException(
                status_code=400,
                detail=str(e)
            )
        except Exception as e:
            # Outros erros
            raise HTTPException(
                status_code=500,
                detail=f"Erro ao inicializar agente: {str(e)}"
            )
    return _agente_instance

# Modelos Pydantic
class MessageRequest(BaseModel):
    message: str
    session_id: Optional[str] = None
    image_data: Optional[str] = None

class MessageResponse(BaseModel):
    content: str
    session_id: str
    timestamp: str
    model_used: Optional[str] = None
    provider: Optional[str] = None
    tool_used: Optional[str] = None
    reasoning: Optional[str] = None
    error: Optional[str] = None

class AgentStatusResponse(BaseModel):
    name: str
    version: str
    status: str
    initialized: bool
    llm_provider: str
    model: str
    tools_available: list
    active_sessions: int
    timestamp: str
    components: Dict[str, bool]

@router.post("/message", response_model=MessageResponse)
async def send_message(
    request: MessageRequest,
    agente = Depends(get_agente)
):
    """
    Envia mensagem para o agente e recebe resposta
    """
    try:
        # Gera session_id se não fornecido
        session_id = request.session_id or str(uuid.uuid4())
        
        # Processa mensagem
        response = await agente.process_message(
            message=request.message,
            session_id=session_id,
            image_data=request.image_data
        )
        
        # Formata resposta
        return MessageResponse(
            content=response.get('content', ''),
            session_id=session_id,
            timestamp=response.get('timestamp', datetime.now().isoformat()),
            model_used=response.get('model_used'),
            provider=response.get('provider'),
            tool_used=response.get('tool_used'),
            reasoning=response.get('reasoning'),
            error=response.get('error')
        )
        
    except ValueError as e:
        # Erro de configuração (ex: API key não configurada)
        raise HTTPException(
            status_code=400,
            detail=str(e)
        )
    except Exception as e:
        raise HTTPException(
            status_code=500,
            detail=f"Erro ao processar mensagem: {str(e)}"
        )

@router.get("/status", response_model=AgentStatusResponse)
async def get_agent_status(
    agente = Depends(get_agente)
):
    """
    Retorna status do agente
    """
    try:
        status = agente.get_status()
        return AgentStatusResponse(**status)
        
    except Exception as e:
        raise HTTPException(
            status_code=500,
            detail=f"Erro ao obter status: {str(e)}"
        )

@router.post("/reset")
async def reset_agent(
    session_id: str,
    agente = Depends(get_agente)
):
    """
    Reseta o contexto de uma sessão específica
    """
    try:
        # Remove histórico da sessão
        if hasattr(agente, 'conversation_history') and session_id in agente.conversation_history:
            del agente.conversation_history[session_id]
        
        return {
            "message": f"Contexto da sessão {session_id} resetado com sucesso",
            "session_id": session_id,
            "timestamp": datetime.now().isoformat()
        }
        
    except Exception as e:
        raise HTTPException(
            status_code=500,
            detail=f"Erro ao resetar sessão: {str(e)}"
        )

@router.get("/sessions")
async def get_active_sessions(
    agente = Depends(get_agente)
):
    """
    Retorna lista de sessões ativas
    """
    try:
        sessions = {}
        if hasattr(agente, 'conversation_history'):
            for session_id, history in agente.conversation_history.items():
                sessions[session_id] = {
                    "message_count": len(history),
                    "last_activity": history[-1]['timestamp'] if history else None
                }
        
        return {
            "active_sessions": sessions,
            "total_sessions": len(sessions),
            "timestamp": datetime.now().isoformat()
        }
        
    except Exception as e:
        raise HTTPException(
            status_code=500,
            detail=f"Erro ao obter sessões: {str(e)}"
        )

@router.delete("/sessions/{session_id}")
async def delete_session(
    session_id: str,
    agente = Depends(get_agente)
):
    """
    Remove uma sessão específica
    """
    try:
        if hasattr(agente, 'conversation_history') and session_id in agente.conversation_history:
            del agente.conversation_history[session_id]
            message = f"Sessão {session_id} removida com sucesso"
        else:
            message = f"Sessão {session_id} não encontrada"
        
        return {
            "message": message,
            "session_id": session_id,
            "timestamp": datetime.now().isoformat()
        }
        
    except Exception as e:
        raise HTTPException(
            status_code=500,
            detail=f"Erro ao remover sessão: {str(e)}"
        )
