"""
Seletor de Tools baseado em LangChain para o Agente ORB
Decide qual ferramenta usar baseado na entrada do usuário usando prompts YAML
"""

import json
import logging
import yaml
import os
from typing import Dict, Any, List, Optional
from datetime import datetime

# Carrega variáveis do .env
try:
    from dotenv import load_dotenv
    load_dotenv()
except ImportError:
    pass

try:
    from langchain_openai import ChatOpenAI
    from langchain_core.messages import HumanMessage, SystemMessage
    LANGCHAIN_AVAILABLE = True
except ImportError:
    LANGCHAIN_AVAILABLE = False

class ToolSelector:
    """Seletor inteligente de ferramentas usando LangChain e prompts YAML"""
    
    def __init__(self, tool_registry, prompt_config_path: str = None):
        """
        Inicializa o seletor de tools
        
        Args:
            tool_registry: Registro de ferramentas
            prompt_config_path: Caminho para arquivo de configuração do prompt
        """
        self.tool_registry = tool_registry
        self.logger = logging.getLogger(__name__)
        
        if not LANGCHAIN_AVAILABLE:
            self.logger.warning("LangChain não está instalado. Usando fallback simples.")
            self.llm = None
        else:
            # Define caminho padrão se não fornecido
            if prompt_config_path is None:
                prompt_config_path = os.path.join(os.path.dirname(__file__), "..", "prompts", "tool_selector.yaml")
            
            # Carrega configuração do prompt
            self.prompt_config = self._load_prompt_config(prompt_config_path)
            
            # Inicializa LLM com configurações do prompt
            self.llm = self._init_llm()
        
        self.logger.info("Tool Selector inicializado")
    
    def _load_prompt_config(self, config_path: str) -> Dict[str, Any]:
        """Carrega configuração do prompt do arquivo YAML"""
        try:
            if os.path.exists(config_path):
                with open(config_path, 'r', encoding='utf-8') as file:
                    config = yaml.safe_load(file)
                self.logger.info(f"Configuração do prompt carregada: {config_path}")
                return config
            else:
                self.logger.warning(f"Arquivo de configuração não encontrado: {config_path}")
                return self._get_default_config()
        except Exception as e:
            self.logger.error(f"Erro ao carregar configuração do prompt: {str(e)}")
            return self._get_default_config()
    
    def _get_default_config(self) -> Dict[str, Any]:
        """Configuração padrão de fallback"""
        return {
            'generation_params': {
                'temperature': 0.1,
                'max_tokens': 200,
                'top_p': 0.9
            },
            'prompt_templates': {
                'default': 'Selecione a ferramenta apropriada para: {user_input}'
            }
        }
    
    def _init_llm(self):
        """Inicializa LLM com configurações do prompt YAML"""
        if not LANGCHAIN_AVAILABLE:
            return None
            
        try:
            llm_config = self.prompt_config.get('llm_config', {})
            generation_params = self.prompt_config.get('generation_params', {})
            
            provider = llm_config.get('provider', 'openai')
            model = llm_config.get('model', 'gpt-3.5-turbo')
            
            self.logger.info(f"Configurações LLM carregadas: {provider}/{model}")
            
            if provider == 'openai':
                llm = ChatOpenAI(
                    model=model,
                    temperature=generation_params.get('temperature', 0.1),
                    max_tokens=generation_params.get('max_tokens', 200),
                    top_p=generation_params.get('top_p', 0.9)
                )
            else:
                # Fallback para OpenAI padrão
                self.logger.warning(f"Provider '{provider}' não suportado, usando OpenAI")
                llm = ChatOpenAI(
                    model='gpt-3.5-turbo',
                    temperature=generation_params.get('temperature', 0.1),
                    max_tokens=generation_params.get('max_tokens', 200)
                )
            
            self.logger.info(f"LLM inicializado com sucesso: {provider}/{model}")
            return llm
            
        except Exception as e:
            self.logger.error(f"Erro ao inicializar LLM: {str(e)}")
            return None
    
    def select_tool(self, user_input: str, context: Optional[Dict] = None) -> Dict[str, Any]:
        """
        Seleciona a ferramenta mais apropriada para a entrada do usuário
        
        Args:
            user_input: Entrada do usuário
            context: Contexto adicional (opcional)
            
        Returns:
            Decisão contendo tool e input
        """
        try:
            if self.llm is None:
                # Fallback sem LangChain
                return self._get_fallback_decision(user_input)
            
            # Prepara prompt usando template YAML
            prompt = self._build_prompt(user_input, context)
            
            # Gera resposta usando LangChain
            response = self.llm.invoke(prompt)
            
            # Extrai conteúdo da resposta
            if hasattr(response, 'content'):
                decision_text = response.content
            else:
                decision_text = str(response)
            
            # Parse da resposta JSON
            decision = self._parse_decision(decision_text)
            
            # Valida decisão
            if self._validate_decision(decision):
                self.logger.info(f"Tool selecionada: {decision.get('tool', 'none')}")
                return decision
            else:
                self.logger.warning("Decisão inválida, usando fallback")
                return self._get_fallback_decision(user_input)
                
        except Exception as e:
            self.logger.error(f"Erro na seleção de tool: {str(e)}")
            return self._get_fallback_decision(user_input)
    
    def _build_prompt(self, user_input: str, context: Optional[Dict]) -> str:
        """Constrói prompt usando template YAML"""
        
        # Lista ferramentas disponíveis
        if self.tool_registry and hasattr(self.tool_registry, 'list_tools'):
            available_tools = self.tool_registry.list_tools(enabled_only=True)
        else:
            # Fallback: lista básica de ferramentas
            available_tools = []
        
        # Formata ferramentas
        tools_description = ""
        if available_tools:
            for i, tool in enumerate(available_tools, 1):
                tools_description += f"{i}. **{tool.name}**: {tool.description}\n"
                if hasattr(tool, 'trigger_keywords') and tool.trigger_keywords:
                    tools_description += f"   - Palavras-chave: {', '.join(tool.trigger_keywords)}\n"
                tools_description += "\n"
        else:
            tools_description = "Nenhuma ferramenta específica disponível no momento.\n"
        
        # Usa template do YAML
        template = self.prompt_config.get('prompt_templates', {}).get('default', '')
        
        if not template:
            # Fallback se template não estiver disponível
            template = """Você é um assistente especializado em seleção de ferramentas para o ORB.

Ferramentas disponíveis:
{available_tools}

Entrada do usuário: "{user_input}"

Responda APENAS com um JSON válido no formato:
{{
    "tool": "NomeDaFerramenta",
    "input": {{"parametro": "valor"}},
    "reasoning": "Explicação da escolha"
}}

Se nenhuma ferramenta for apropriada, use:
{{
    "tool": "none",
    "input": {{}},
    "reasoning": "Nenhuma ferramenta apropriada"
}}"""
        
        # Substitui variáveis no template
        prompt = template.format(
            available_tools=tools_description,
            user_input=user_input
        )
        
        self.logger.debug(f"Prompt construído: {len(prompt)} caracteres")
        return prompt
    
    def _parse_decision(self, decision_text: str) -> Dict[str, Any]:
        """Parse da resposta do LLM para extrair decisão JSON"""
        try:
            # Tenta extrair JSON da resposta
            if "```json" in decision_text:
                json_start = decision_text.find("```json") + 7
                json_end = decision_text.find("```", json_start)
                json_text = decision_text[json_start:json_end].strip()
            elif "{" in decision_text and "}" in decision_text:
                json_start = decision_text.find("{")
                json_end = decision_text.rfind("}") + 1
                json_text = decision_text[json_start:json_end]
            else:
                # Fallback: retorna decisão padrão
                return self._get_fallback_decision("")
            
            decision = json.loads(json_text)
            
            # Valida estrutura básica
            if not isinstance(decision, dict):
                return self._get_fallback_decision("")
            
            # Garante que 'tool' não seja null
            if decision.get('tool') is None:
                decision['tool'] = 'none'
            
            return decision
            
        except json.JSONDecodeError as e:
            self.logger.warning(f"Erro ao parsear JSON: {str(e)}")
            return self._get_fallback_decision("")
    
    def _validate_decision(self, decision: Dict[str, Any]) -> bool:
        """Valida se a decisão é válida"""
        try:
            # Verifica estrutura básica
            if not isinstance(decision, dict):
                return False
            
            if 'tool' not in decision or 'input' not in decision:
                return False
            
            tool_name = decision['tool']
            
            # Se for "none", é válido
            if tool_name == "none" or tool_name is None:
                return True
            
            # Verifica se a ferramenta existe
            if not self.tool_registry or not hasattr(self.tool_registry, 'get_tool_metadata'):
                # Fallback: aceita qualquer ferramenta
                return True
                
            tool_metadata = self.tool_registry.get_tool_metadata(tool_name)
            if not tool_metadata:
                return False
            
            # Valida entrada básica
            input_data = decision.get('input', {})
            if not isinstance(input_data, dict):
                return False
            
            return True
            
        except Exception as e:
            self.logger.error(f"Erro na validação: {str(e)}")
            return False
    
    def _get_fallback_decision(self, user_input: str) -> Dict[str, Any]:
        """Decisão de fallback quando a seleção automática falha"""
        
        # Por enquanto, sempre retorna "none" pois não temos ferramentas específicas
        return {
            "tool": "none",
            "input": {},
            "reasoning": "Fallback: nenhuma ferramenta específica configurada no momento"
        }
    
    def get_selection_stats(self) -> Dict[str, Any]:
        """Retorna estatísticas de seleção"""
        available_tools = []
        if self.tool_registry and hasattr(self.tool_registry, 'list_tools'):
            available_tools = self.tool_registry.list_tools(enabled_only=True)
        
        return {
            "available_tools": len(available_tools),
            "selector_version": "1.0.0",
            "langchain_enabled": LANGCHAIN_AVAILABLE and self.llm is not None,
            "prompt_config_loaded": bool(self.prompt_config),
            "llm_provider": self.prompt_config.get('llm_config', {}).get('provider', 'unknown') if self.prompt_config else 'none'
        }
