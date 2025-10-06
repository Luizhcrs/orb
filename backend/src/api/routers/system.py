"""
Router para funcionalidades do sistema (screenshot, hot corner, etc.)
"""

from fastapi import APIRouter, HTTPException
from pydantic import BaseModel
from typing import Optional, Dict, Any
import base64
import io
from datetime import datetime
import logging

router = APIRouter(prefix="/system", tags=["system"])

# Modelos Pydantic
class ScreenshotResponse(BaseModel):
    success: bool
    image_data: Optional[str] = None
    error: Optional[str] = None
    timestamp: str

class HotCornerConfig(BaseModel):
    x: int
    y: int
    width: int
    height: int
    hide_delay: int
    show_delay: int

class SystemStatusResponse(BaseModel):
    screenshot_available: bool
    hot_corner_enabled: bool
    orb_visible: bool
    timestamp: str

@router.post("/screenshot", response_model=ScreenshotResponse)
async def capture_screenshot():
    """
    Captura screenshot da tela
    """
    try:
        # Tenta usar mss para captura rápida
        try:
            import mss
            with mss.mss() as sct:
                # Captura a tela principal
                monitor = sct.monitors[1]  # Monitor principal
                screenshot = sct.grab(monitor)
                
                # Converte para base64
                from PIL import Image
                img = Image.frombytes("RGB", screenshot.size, screenshot.bgra, "raw", "BGRX")
                
                # Converte para base64
                buffer = io.BytesIO()
                img.save(buffer, format="PNG")
                img_str = base64.b64encode(buffer.getvalue()).decode()
                
                return ScreenshotResponse(
                    success=True,
                    image_data=img_str,
                    timestamp=datetime.now().isoformat()
                )
                
        except ImportError:
            # Fallback para pyautogui
            import pyautogui
            screenshot = pyautogui.screenshot()
            
            # Converte para base64
            buffer = io.BytesIO()
            screenshot.save(buffer, format="PNG")
            img_str = base64.b64encode(buffer.getvalue()).decode()
            
            return ScreenshotResponse(
                success=True,
                image_data=img_str,
                timestamp=datetime.now().isoformat()
            )
            
    except Exception as e:
        logging.error(f"Erro ao capturar screenshot: {str(e)}")
        return ScreenshotResponse(
            success=False,
            error=str(e),
            timestamp=datetime.now().isoformat()
        )

@router.get("/status", response_model=SystemStatusResponse)
async def get_system_status():
    """
    Retorna status do sistema
    """
    try:
        # Verifica disponibilidade de screenshot
        screenshot_available = False
        try:
            import mss
            screenshot_available = True
        except ImportError:
            try:
                import pyautogui
                screenshot_available = True
            except ImportError:
                pass
        
        return SystemStatusResponse(
            screenshot_available=screenshot_available,
            hot_corner_enabled=False,  # Implementar futuramente
            orb_visible=False,  # Implementar futuramente
            timestamp=datetime.now().isoformat()
        )
        
    except Exception as e:
        raise HTTPException(
            status_code=500,
            detail=f"Erro ao obter status do sistema: {str(e)}"
        )

@router.post("/hot-corner/configure")
async def configure_hot_corner(config: HotCornerConfig):
    """
    Configura hot corner (implementação futura)
    """
    try:
        # Por enquanto, apenas retorna confirmação
        # Futuramente, aqui seria implementada a lógica para configurar hot corner
        
        return {
            "message": "Configuração de hot corner recebida",
            "config": config.dict(),
            "timestamp": datetime.now().isoformat()
        }
        
    except Exception as e:
        raise HTTPException(
            status_code=500,
            detail=f"Erro ao configurar hot corner: {str(e)}"
        )

@router.post("/toggle-orb")
async def toggle_orb():
    """
    Alterna visibilidade do orb (implementação futura)
    """
    try:
        # Por enquanto, apenas retorna confirmação
        # Futuramente, aqui seria implementada a lógica para controlar o orb
        
        return {
            "message": "Comando para alternar orb recebido",
            "timestamp": datetime.now().isoformat()
        }
        
    except Exception as e:
        raise HTTPException(
            status_code=500,
            detail=f"Erro ao alternar orb: {str(e)}"
        )

@router.get("/orb/status")
async def get_orb_status():
    """
    Retorna status do orb (implementação futura)
    """
    try:
        # Por enquanto, retorna status simulado
        return {
            "visible": False,
            "position": {"x": 0, "y": 0},
            "size": {"width": 90, "height": 90},
            "timestamp": datetime.now().isoformat()
        }
        
    except Exception as e:
        raise HTTPException(
            status_code=500,
            detail=f"Erro ao obter status do orb: {str(e)}"
        )
