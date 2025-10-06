"""
Router de Health Check para ORB API
"""

from fastapi import APIRouter, HTTPException
from datetime import datetime
import psutil
import sys
import os

router = APIRouter(prefix="/health", tags=["health"])

@router.get("/")
async def health_check():
    """
    Health check básico
    """
    return {
        "status": "healthy",
        "timestamp": datetime.now().isoformat(),
        "service": "ORB Backend API",
        "version": "1.0.0"
    }

@router.get("/detailed")
async def detailed_health():
    """
    Health check detalhado com métricas do sistema
    """
    try:
        # Informações do sistema
        cpu_percent = psutil.cpu_percent(interval=1)
        memory = psutil.virtual_memory()
        disk = psutil.disk_usage('/')
        
        # Informações do Python
        python_info = {
            "version": sys.version,
            "platform": sys.platform,
            "executable": sys.executable
        }
        
        # Informações do processo
        process = psutil.Process()
        process_info = {
            "pid": process.pid,
            "memory_mb": round(process.memory_info().rss / 1024 / 1024, 2),
            "cpu_percent": process.cpu_percent(),
            "create_time": datetime.fromtimestamp(process.create_time()).isoformat()
        }
        
        return {
            "status": "healthy",
            "timestamp": datetime.now().isoformat(),
            "service": "ORB Backend API",
            "version": "1.0.0",
            "system": {
                "cpu_percent": cpu_percent,
                "memory": {
                    "total_gb": round(memory.total / 1024 / 1024 / 1024, 2),
                    "available_gb": round(memory.available / 1024 / 1024 / 1024, 2),
                    "percent": memory.percent
                },
                "disk": {
                    "total_gb": round(disk.total / 1024 / 1024 / 1024, 2),
                    "free_gb": round(disk.free / 1024 / 1024 / 1024, 2),
                    "percent": round((disk.used / disk.total) * 100, 2)
                }
            },
            "python": python_info,
            "process": process_info
        }
        
    except Exception as e:
        raise HTTPException(
            status_code=500,
            detail=f"Erro ao obter informações detalhadas: {str(e)}"
        )

@router.get("/ready")
async def readiness_check():
    """
    Readiness check - verifica se o serviço está pronto para receber tráfego
    """
    try:
        # Verifica se as dependências estão disponíveis
        dependencies = {
            "openai": False,
            "anthropic": False,
            "fastapi": True,
            "websockets": True
        }
        
        # Verifica OpenAI
        try:
            import openai
            api_key = os.getenv('OPENAI_API_KEY')
            dependencies["openai"] = bool(api_key)
        except ImportError:
            pass
        
        # Verifica Anthropic
        try:
            import anthropic
            api_key = os.getenv('ANTHROPIC_API_KEY')
            dependencies["anthropic"] = bool(api_key)
        except ImportError:
            pass
        
        # Determina se está pronto
        is_ready = any([dependencies["openai"], dependencies["anthropic"]])
        
        return {
            "status": "ready" if is_ready else "not_ready",
            "timestamp": datetime.now().isoformat(),
            "dependencies": dependencies,
            "message": "Serviço pronto" if is_ready else "Configure pelo menos uma API key (OpenAI ou Anthropic)"
        }
        
    except Exception as e:
        raise HTTPException(
            status_code=500,
            detail=f"Erro no readiness check: {str(e)}"
        )

@router.get("/live")
async def liveness_check():
    """
    Liveness check - verifica se o serviço está vivo
    """
    return {
        "status": "alive",
        "timestamp": datetime.now().isoformat(),
        "uptime": "running"
    }
