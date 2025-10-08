"""
Router para gerenciamento de configurações
"""

from fastapi import APIRouter, HTTPException
from pydantic import BaseModel
from typing import Optional, Dict, Any
import os
from pathlib import Path

# Importar ConfigManager
import sys
backend_path = Path(__file__).parent.parent.parent
sys.path.insert(0, str(backend_path))

from database.config_manager import ConfigManager

router = APIRouter(prefix="/config", tags=["config"])

# Helper para obter ConfigManager (singleton pattern)
_config_manager_instance = None

def get_config_manager():
    """Retorna instância singleton do ConfigManager"""
    global _config_manager_instance
    if _config_manager_instance is None:
        try:
            print("INIT: Tentando inicializar ConfigManager...")
            _config_manager_instance = ConfigManager()
            print("OK: ConfigManager inicializado com sucesso!")
        except Exception as e:
            print(f"ERRO: ao inicializar ConfigManager: {type(e).__name__}: {e}")
            import traceback
            traceback.print_exc()
            raise HTTPException(
                status_code=500,
                detail=f"Erro ao inicializar ConfigManager: {str(e)}"
            )
    return _config_manager_instance

# Models
class GeneralConfig(BaseModel):
    theme: Optional[str] = None
    language: Optional[str] = None
    startup: Optional[bool] = None
    keep_history: Optional[bool] = None

class AgentConfig(BaseModel):
    provider: Optional[str] = None
    api_key: Optional[str] = None
    model: Optional[str] = None

class ConfigUpdate(BaseModel):
    general: Optional[GeneralConfig] = None
    agent: Optional[AgentConfig] = None

class ConfigResponse(BaseModel):
    general: Dict[str, Any]
    agent: Dict[str, Any]

@router.get("", response_model=ConfigResponse)
@router.get("/", response_model=ConfigResponse)
async def get_config():
    """
    Obter todas as configurações
    """
    print("DEBUG: GET /config chamado!")
    try:
        print("DEBUG: Tentando obter ConfigManager...")
        config_manager = get_config_manager()
        print("DEBUG: ConfigManager obtido com sucesso!")
        
        # Configurações gerais
        print("DEBUG: Obtendo configuracoes gerais...")
        try:
            theme = config_manager.get_setting('theme', 'dark')
            print(f"DEBUG: theme = {theme}")
            language = config_manager.get_setting('language', 'pt-BR')
            print(f"DEBUG: language = {language}")
            startup = config_manager.get_setting('startup', False)
            print(f"DEBUG: startup = {startup}")
            keep_history = config_manager.get_setting('keep_history', True)
            print(f"DEBUG: keep_history = {keep_history}")
            
            general = {
                'theme': theme,
                'language': language,
                'startup': startup,
                'keep_history': keep_history
            }
            print(f"DEBUG: General config: {general}")
        except Exception as e:
            print(f"DEBUG: ERRO ao obter general config: {type(e).__name__}: {e}")
            import traceback
            traceback.print_exc()
            raise
        
        # Configurações do agente
        print("DEBUG: Obtendo config LLM...")
        llm_config = config_manager.get_llm_config()
        print(f"DEBUG: LLM config obtido: {llm_config}")
        
        agent = {
            'provider': llm_config.get('provider', 'openai'),
            'api_key': '***' + llm_config.get('api_key', '')[-4:] if llm_config.get('api_key') else '',
            'model': llm_config.get('model', 'gpt-4o-mini')
        }
        print(f"DEBUG: Agent config: {agent}")
        
        print("DEBUG: Criando ConfigResponse...")
        response = ConfigResponse(general=general, agent=agent)
        print(f"DEBUG: Response criado: {response}")
        return response
    
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Erro ao obter configurações: {str(e)}")

@router.post("")
@router.post("/")
async def update_config(config: ConfigUpdate):
    """
    Atualizar configurações
    """
    print("DEBUG: POST /config chamado!")
    print(f"DEBUG: Config recebido: {config}")
    try:
        print("DEBUG: Tentando obter ConfigManager...")
        config_manager = get_config_manager()
        print("DEBUG: ConfigManager obtido com sucesso!")
        updated = []
        
        # Atualizar configurações gerais
        if config.general:
            if config.general.theme is not None:
                config_manager.set_setting('theme', config.general.theme)
                updated.append('theme')
            
            if config.general.language is not None:
                config_manager.set_setting('language', config.general.language)
                updated.append('language')
            
            if config.general.startup is not None:
                config_manager.set_setting('startup', config.general.startup)
                updated.append('startup')
            
            if config.general.keep_history is not None:
                config_manager.set_setting('keep_history', config.general.keep_history)
                updated.append('keep_history')
        
        # Atualizar configurações do agente
        if config.agent:
            provider = config.agent.provider
            api_key = config.agent.api_key
            model = config.agent.model
            print(f"DEBUG: Atualizando LLM config - provider={provider}, model={model}, api_key={'***' if api_key else 'None'}")
            
            if provider and api_key and model:
                print("DEBUG: Salvando LLM config...")
                try:
                    config_manager.save_llm_config(provider, api_key, model)
                    updated.extend(['provider', 'api_key', 'model'])
                    print("DEBUG: LLM config salvo!")
                except Exception as e:
                    print(f"DEBUG: ERRO ao salvar LLM config: {type(e).__name__}: {e}")
                    import traceback
                    traceback.print_exc()
                    raise
        
        print(f"DEBUG: Itens atualizados: {updated}")
        result = {
            "status": "success",
            "message": "Configurações atualizadas com sucesso",
            "updated": updated
        }
        print(f"DEBUG: Retornando resultado: {result}")
        return result
    
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Erro ao atualizar configurações: {str(e)}")

@router.get("/general/{key}")
async def get_general_config(key: str):
    """
    Obter uma configuração geral específica
    """
    try:
        config_manager = get_config_manager()
        value = config_manager.get(key)
        if value is None:
            raise HTTPException(status_code=404, detail=f"Configuração '{key}' não encontrada")
        
        return {"key": key, "value": value}
    
    except HTTPException:
        raise
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Erro ao obter configuração: {str(e)}")

@router.post("/general/{key}")
async def set_general_config(key: str, value: Any):
    """
    Definir uma configuração geral específica
    """
    try:
        config_manager.set(key, value)
        return {
            "status": "success",
            "message": f"Configuração '{key}' atualizada",
            "key": key,
            "value": value
        }
    
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Erro ao definir configuração: {str(e)}")

@router.get("/agent")
async def get_agent_config():
    """
    Obter configuração do agente (LLM)
    """
    try:
        llm_config = config_manager.get_llm_config()
        
        # Mascarar API key
        if llm_config.get('api_key'):
            llm_config['api_key'] = '***' + llm_config['api_key'][-4:]
        
        return llm_config
    
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Erro ao obter configuração do agente: {str(e)}")

@router.post("/agent")
async def set_agent_config(provider: str, model: str, api_key: str):
    """
    Definir configuração do agente (LLM)
    """
    try:
        config_manager.set_llm_config(provider, model, api_key)
        return {
            "status": "success",
            "message": "Configuração do agente atualizada",
            "provider": provider,
            "model": model
        }
    
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Erro ao definir configuração do agente: {str(e)}")

