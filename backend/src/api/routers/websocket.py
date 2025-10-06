"""
Router WebSocket para comunicação em tempo real com o ORB
"""

from fastapi import APIRouter, WebSocket, WebSocketDisconnect, Depends
from typing import Dict, Any
import json
import uuid
from datetime import datetime
import logging

# Importa o agente (lazy import para evitar travar na inicialização)
import sys
import os
sys.path.append(os.path.join(os.path.dirname(__file__), '..', '..'))

router = APIRouter(tags=["websocket"])

def get_agente():
    """Dependency para obter instância do agente (nova instância a cada request)"""
    from agentes.orb_agent.agente import AgenteORB
    return AgenteORB()

# Gerenciador de conexões WebSocket
class ConnectionManager:
    def __init__(self):
        self.active_connections: Dict[str, WebSocket] = {}
        self.logger = logging.getLogger(__name__)
    
    async def connect(self, websocket: WebSocket, client_id: str):
        """Aceita conexão WebSocket"""
        await websocket.accept()
        self.active_connections[client_id] = websocket
        self.logger.info(f"Cliente {client_id} conectado via WebSocket")
        
        # Envia mensagem de boas-vindas
        await self.send_message(client_id, {
            "type": "connection",
            "message": "Conectado ao ORB Backend",
            "client_id": client_id,
            "timestamp": datetime.now().isoformat()
        })
    
    def disconnect(self, client_id: str):
        """Remove conexão WebSocket"""
        if client_id in self.active_connections:
            del self.active_connections[client_id]
            self.logger.info(f"Cliente {client_id} desconectado")
    
    async def send_message(self, client_id: str, message: Dict[str, Any]):
        """Envia mensagem para cliente específico"""
        if client_id in self.active_connections:
            try:
                await self.active_connections[client_id].send_text(json.dumps(message))
            except Exception as e:
                self.logger.error(f"Erro ao enviar mensagem para {client_id}: {str(e)}")
                # Remove conexão problemática
                self.disconnect(client_id)
    
    async def broadcast(self, message: Dict[str, Any]):
        """Envia mensagem para todos os clientes conectados"""
        for client_id in list(self.active_connections.keys()):
            await self.send_message(client_id, message)

# Instância global do gerenciador
manager = ConnectionManager()

@router.websocket("/ws")
async def websocket_endpoint(
    websocket: WebSocket,
    agente = Depends(get_agente)
):
    """
    Endpoint WebSocket principal para comunicação em tempo real
    """
    client_id = str(uuid.uuid4())
    
    try:
        # Conecta cliente
        await manager.connect(websocket, client_id)
        
        while True:
            # Recebe mensagem do cliente
            data = await websocket.receive_text()
            message_data = json.loads(data)
            
            message_type = message_data.get("type", "message")
            
            if message_type == "message":
                # Processa mensagem do agente
                await handle_agent_message(client_id, message_data, agente)
            
            elif message_type == "ping":
                # Responde ping
                await manager.send_message(client_id, {
                    "type": "pong",
                    "timestamp": datetime.now().isoformat()
                })
            
            elif message_type == "toggle_orb":
                # Comando para alternar visibilidade do orb
                await handle_toggle_orb(client_id, message_data)
            
            elif message_type == "screenshot":
                # Comando para capturar tela
                await handle_screenshot(client_id, message_data)
            
            else:
                # Tipo de mensagem não reconhecido
                await manager.send_message(client_id, {
                    "type": "error",
                    "message": f"Tipo de mensagem não reconhecido: {message_type}",
                    "timestamp": datetime.now().isoformat()
                })
    
    except WebSocketDisconnect:
        manager.disconnect(client_id)
    except Exception as e:
        logging.error(f"Erro no WebSocket {client_id}: {str(e)}")
        manager.disconnect(client_id)

async def handle_agent_message(client_id: str, message_data: Dict[str, Any], agente):
    """Processa mensagem do agente via WebSocket"""
    try:
        # Envia indicador de processamento
        await manager.send_message(client_id, {
            "type": "processing",
            "message": "Processando sua mensagem...",
            "timestamp": datetime.now().isoformat()
        })
        
        # Extrai dados da mensagem
        user_message = message_data.get("message", "")
        session_id = message_data.get("session_id", str(uuid.uuid4()))
        image_data = message_data.get("image_data")
        
        if not user_message:
            await manager.send_message(client_id, {
                "type": "error",
                "message": "Mensagem vazia",
                "timestamp": datetime.now().isoformat()
            })
            return
        
        # Processa mensagem com o agente
        response = await agente.process_message(
            message=user_message,
            session_id=session_id,
            image_data=image_data
        )
        
        # Envia resposta
        await manager.send_message(client_id, {
            "type": "response",
            "content": response.get("content", ""),
            "session_id": session_id,
            "model_used": response.get("model_used"),
            "provider": response.get("provider"),
            "tool_used": response.get("tool_used"),
            "reasoning": response.get("reasoning"),
            "timestamp": response.get("timestamp", datetime.now().isoformat())
        })
        
    except Exception as e:
        logging.error(f"Erro ao processar mensagem do agente: {str(e)}")
        await manager.send_message(client_id, {
            "type": "error",
            "message": f"Erro ao processar mensagem: {str(e)}",
            "timestamp": datetime.now().isoformat()
        })

async def handle_toggle_orb(client_id: str, message_data: Dict[str, Any]):
    """Processa comando para alternar visibilidade do orb"""
    try:
        # Por enquanto, apenas confirma o comando
        # Futuramente, aqui seria implementada a lógica para controlar o orb
        
        await manager.send_message(client_id, {
            "type": "orb_toggled",
            "message": "Comando para alternar orb recebido",
            "timestamp": datetime.now().isoformat()
        })
        
    except Exception as e:
        logging.error(f"Erro ao processar toggle orb: {str(e)}")
        await manager.send_message(client_id, {
            "type": "error",
            "message": f"Erro ao alternar orb: {str(e)}",
            "timestamp": datetime.now().isoformat()
        })

async def handle_screenshot(client_id: str, message_data: Dict[str, Any]):
    """Processa comando para capturar tela"""
    try:
        # Por enquanto, apenas confirma o comando
        # Futuramente, aqui seria implementada a lógica para capturar tela
        
        await manager.send_message(client_id, {
            "type": "screenshot_taken",
            "message": "Comando para capturar tela recebido",
            "timestamp": datetime.now().isoformat()
        })
        
    except Exception as e:
        logging.error(f"Erro ao processar screenshot: {str(e)}")
        await manager.send_message(client_id, {
            "type": "error",
            "message": f"Erro ao capturar tela: {str(e)}",
            "timestamp": datetime.now().isoformat()
        })

@router.get("/connections")
async def get_active_connections():
    """Retorna informações sobre conexões WebSocket ativas"""
    return {
        "active_connections": len(manager.active_connections),
        "client_ids": list(manager.active_connections.keys()),
        "timestamp": datetime.now().isoformat()
    }
