"""
Pipeline principal do Agente ORB
Pipeline: recebe sessão/input → verificação de contexto → verifica tools → salva contexto → responde
"""

import os
import logging
import yaml
from typing import Dict, List, Any, Optional
from datetime import datetime
from dotenv import load_dotenv
from pathlib import Path
import sys

# Carrega variáveis de ambiente do arquivo .env
load_dotenv()

# Adicionar database ao path
database_path = Path(__file__).parent.parent.parent / "database"
if str(database_path) not in sys.path:
    sys.path.insert(0, str(database_path))

# Importações do database
try:
    from database.config_manager import ConfigManager
    from database.chat_memory import ChatMemoryManager
    DATABASE_AVAILABLE = True
except ImportError:
    DATABASE_AVAILABLE = False
    print("AVISO:  Database modules não disponíveis, usando apenas .env")

# Importações dos módulos do agente
from .tools.tool_selector import ToolSelector
from .llms.llm_provider import LLMProvider
from .utils.logging_config import get_utf8_logger

class AgenteORB:
    """Classe principal do Agente ORB - Pipeline: input → contexto → tools → salva → responde"""
    
    def __init__(self, config_path: Optional[str] = None, session_id: Optional[str] = None):
        """
        Inicializa o Agente ORB com Lazy Loading
        
        Args:
            config_path: Caminho para arquivo de configuração (opcional)
            session_id: ID da sessão para histórico de chat (opcional)
        """
        self.logger = self._setup_logging()
        self.logger.info("Inicializando Agente ORB com Lazy Loading...")
        
        # Inicializar database managers (com fallback)
        if DATABASE_AVAILABLE:
            try:
                self.config_manager = ConfigManager()
                self.chat_memory = ChatMemoryManager()
                self.logger.info("OK: Database managers inicializados")
            except Exception as e:
                self.logger.warning(f"AVISO: Erro ao inicializar database: {e}, usando .env")
                self.config_manager = None
                self.chat_memory = None
        else:
            self.config_manager = None
            self.chat_memory = None
        
        # Carrega apenas configurações essenciais
        self.config = self._load_config(config_path)
        
        # Componentes serão inicializados sob demanda (Lazy Loading)
        self._llm_provider = None
        self._tool_selector = None
        self._tools = None
        self._system_prompt = None
        self._generation_params = None
        
        # Flag para controlar inicialização
        self._initialized = False
        
        # Sessão atual de chat
        self.session_id = session_id
        
        # Histórico de conversação (em memória para compatibilidade)
        self.conversation_history = {}
        
        self.logger.info("Agente ORB configurado - componentes serão carregados sob demanda")
    
    def _ensure_initialized(self):
        """Garante que todos os componentes estejam inicializados (Lazy Loading)"""
        if not self._initialized:
            # Inicializa componentes na ordem correta (sem logs verbosos)
            self._llm_provider = self._init_llm_provider()
            self._tool_selector = self._init_tool_selector()
            self._tools = self._init_tools()
            
            # Carrega prompt do sistema
            self._system_prompt, self._generation_params = self._load_system_prompt()
            
            self._initialized = True
    
    @property
    def llm_provider(self):
        """Lazy loading para llm_provider"""
        if self._llm_provider is None:
            self._ensure_initialized()
        return self._llm_provider
    
    @property
    def tool_selector(self):
        """Lazy loading para tool_selector"""
        if self._tool_selector is None:
            self._ensure_initialized()
        return self._tool_selector
    
    @property
    def tools(self):
        """Lazy loading para tools"""
        if self._tools is None:
            self._ensure_initialized()
        return self._tools
    
    @property
    def system_prompt(self):
        """Lazy loading para system_prompt"""
        if self._system_prompt is None:
            self._ensure_initialized()
        return self._system_prompt
    
    @property
    def generation_params(self):
        """Lazy loading para generation_params"""
        if self._generation_params is None:
            self._ensure_initialized()
        return self._generation_params
    
    def is_initialized(self) -> bool:
        """Verifica se o agente está inicializado"""
        return self._initialized
    
    def _setup_logging(self) -> logging.Logger:
        """Configura sistema de logging com suporte a UTF-8"""
        try:
            # Usa configuração UTF-8 para Windows
            return get_utf8_logger("agente_orb")
        except Exception as e:
            # Fallback para configuração básica
            logging.basicConfig(
                level=logging.INFO,
                format='%(asctime)s - %(name)s - %(levelname)s - %(message)s',
                handlers=[logging.StreamHandler()]
            )
            return logging.getLogger(__name__)
    
    def _load_config(self, config_path: Optional[str]) -> Dict[str, Any]:
        """Carrega configurações do agente (database prioritário, .env apenas se database não disponível)"""
        # Tentar carregar do database primeiro
        if self.config_manager:
            try:
                llm_config = self.config_manager.get_llm_config()
                
                # Verificar se API key está configurada
                if not llm_config.get('api_key'):
                    self.logger.error("API key não configurada no banco de dados!")
                    raise ValueError(
                        "Configure a API key na tela de configuração (CommandOrControl+Shift+O). "
                        "Não é permitido usar .env quando o banco de dados está disponível."
                    )
                
                self.logger.info(f"Configuração do LLM carregada do database: {llm_config.get('provider')}/{llm_config.get('model')}")
                return {
                    'llm_provider': llm_config.get('provider', 'openai'),
                    'model': llm_config.get('model', 'gpt-4o-mini'),
                    'environment': os.getenv('ENVIRONMENT', 'development'),
                    'max_tokens': int(os.getenv('MAX_TOKENS', 1000)),
                    'temperature': float(os.getenv('TEMPERATURE', 0.7)),
                    'api_key': llm_config.get('api_key')  # API key do database
                }
            except ValueError:
                # Re-raise ValueError para propagar erro de configuração
                raise
            except Exception as e:
                self.logger.warning(f"Erro ao carregar config do database: {e}")
                self.logger.info("Database indisponível, usando .env como fallback")
        
        # Fallback para .env APENAS se database não estiver disponível
        self.logger.warning("Database não disponível, usando configurações do .env")
        return {
            'llm_provider': os.getenv('LLM_PROVIDER', 'openai'),
            'environment': os.getenv('ENVIRONMENT', 'development'),
            'model': os.getenv('DEFAULT_MODEL', 'gpt-3.5-turbo'),
            'max_tokens': int(os.getenv('MAX_TOKENS', 1000)),
            'temperature': float(os.getenv('TEMPERATURE', 0.7))
        }
    
    def _init_llm_provider(self) -> LLMProvider:
        """Inicializa provedor de LLM"""
        try:
            # Carrega configuração do prompt para usar modelo correto
            prompt_config = self._load_prompt_config()
            llm_config = prompt_config.get('llm_config', {})
            
            self.logger.info(f"Prompt config carregado: {llm_config}")
            
            # Combina configuração do prompt com configuração base
            llm_config_combined = {
                **self.config,
                'llm_provider': llm_config.get('provider', 'openai'),
                'llm_model': llm_config.get('model', 'gpt-4o-mini')
            }
            
            self.logger.info(f"Configuração final do LLM: {llm_config_combined}")
            
            provider = LLMProvider(llm_config_combined)
            self.logger.info(f"LLM Provider inicializado: {llm_config.get('provider', 'openai')}/{llm_config.get('model', 'gpt-4o-mini')}")
            return provider
        except Exception as e:
            self.logger.error(f"Erro ao inicializar LLM Provider: {str(e)}")
            raise
    
    def _init_tool_selector(self) -> ToolSelector:
        """Inicializa seletor de ferramentas"""
        try:
            # Cria um registry simples com as ferramentas disponíveis
            class SimpleTool:
                def __init__(self, name, description=""):
                    self.name = name
                    self.description = description
                    self.trigger_keywords = []
            
            class SimpleToolRegistry:
                def __init__(self, tools):
                    self.tools = tools
                
                def list_tools(self, enabled_only=True):
                    # Retorna objetos SimpleTool em vez de strings
                    return [SimpleTool(name, f"Ferramenta {name}") for name in self.tools.keys()]
                
                def get_tool_metadata(self, tool_name):
                    return {"description": f"Ferramenta {tool_name}"}
            
            # Inicializa com registry vazio por enquanto (será atualizado após tools)
            selector = ToolSelector(SimpleToolRegistry({}))
            self.logger.info("Tool Selector inicializado")
            return selector
        except Exception as e:
            self.logger.error(f"Erro ao inicializar Tool Selector: {str(e)}")
            raise
    
    def _init_tools(self) -> Dict[str, Any]:
        """Inicializa ferramentas do agente"""
        tools = {}
        
        # Por enquanto, não temos ferramentas específicas como RAG
        # Mas mantemos a estrutura para futuras implementações
        self.logger.info("Nenhuma ferramenta específica configurada - modo conversacional simples")
        
        return tools
    
    def _load_prompt_config(self, prompt_type: str = 'system_prompt') -> Dict[str, Any]:
        """Carrega configuração do prompt do arquivo YAML"""
        try:
            current_dir = os.path.dirname(os.path.abspath(__file__))
            prompt_file = os.path.join(current_dir, 'prompts', f'{prompt_type}.yaml')
            
            if os.path.exists(prompt_file):
                with open(prompt_file, 'r', encoding='utf-8') as file:
                    return yaml.safe_load(file)
            else:
                self.logger.warning(f"Arquivo de prompt não encontrado: {prompt_file}")
                return {}
        except Exception as e:
            self.logger.error(f"Erro ao carregar configuração do prompt: {str(e)}")
            return {}
    
    def _load_system_prompt(self, prompt_type: str = 'system_prompt') -> tuple[str, Dict[str, Any]]:
        """Carrega prompt do sistema e parâmetros de geração"""
        try:
            # Carrega configuração do prompt - caminho absoluto
            import os
            current_dir = os.path.dirname(os.path.abspath(__file__))
            prompt_file = os.path.join(current_dir, 'prompts', f'{prompt_type}.yaml')
            
            if os.path.exists(prompt_file):
                prompt_config = load_prompt_config(prompt_file)
                
                # Seleciona template baseado no contexto
                template = prompt_config.get('prompt_templates', {}).get('default', '')
                
                # Extrai parâmetros de geração
                generation_params = prompt_config.get('generation_params', {})
                
                # O template será formatado posteriormente com os dados corretos
                prompt_text = template
                
                return prompt_text, generation_params
            else:
                # Fallback se arquivo não existir
                default_prompt = "Você é o Agente ORB, um assistente de IA flutuante útil e amigável. Responda sempre em português brasileiro de forma concisa e clara."
                default_params = {'temperature': 0.7, 'max_tokens': 1000}
                return default_prompt, default_params
            
        except Exception as e:
            self.logger.warning(f"Erro ao carregar prompt do sistema: {str(e)}")
            default_prompt = "Você é o Agente ORB, um assistente de IA flutuante útil e amigável. Responda sempre em português brasileiro de forma concisa e clara."
            default_params = {'temperature': 0.7, 'max_tokens': 1000}
            return default_prompt, default_params
    
    def _validate_session_id(self, session_id: str) -> str:
        """
        Valida e normaliza session_id
        Aceita qualquer string não vazia, gera UUID apenas se vazio
        """
        import uuid
        
        # Se session_id é vazio ou None, gera um novo UUID
        if not session_id or session_id.strip() == "":
            new_session_id = str(uuid.uuid4())
            self.logger.info(f"Session ID vazio, gerando novo UUID: {new_session_id}")
            return new_session_id
        
        # Aceita qualquer session_id não vazio (para testes e uso real)
        return session_id.strip()
    
    async def process_message(self, message: str, session_id: str, image_data: Optional[str] = None) -> Dict[str, Any]:
        """
        Pipeline principal: recebe sessão/input → verificação de contexto → verifica tools → salva contexto → responde
        
        Args:
            message: Mensagem do usuário
            session_id: ID da sessão
            image_data: Dados de imagem (opcional)
            
        Returns:
            Resposta do agente
        """
        try:
            # Valida e corrige session_id se necessário
            session_id = self._validate_session_id(session_id)
            self.logger.info(f"Iniciando pipeline para sessão {session_id}")
            
            # Verifica contexto de conversação
            conversation_context = await self._verify_context_async(session_id, message)
            
            # Verifica se precisa usar alguma ferramenta
            tool_result = await self._check_tools_needed_async(message, conversation_context)
            
            # Gera resposta usando LLM
            response = await self._generate_response(message, conversation_context, tool_result, image_data)
            
            # Salva contexto da conversação (incluindo image_data se houver)
            self._save_context(session_id, message, response, tool_result, image_data)
            
            self.logger.info("Pipeline concluída com sucesso")
            return response
            
        except Exception as e:
            self.logger.error(f"Erro na pipeline: {str(e)}")
            return {
                'content': 'Desculpe, ocorreu um erro ao processar sua mensagem. Tente novamente.',
                'error': str(e),
                'timestamp': datetime.now().isoformat(),
                'pipeline_step': 'error'
            }
    
    async def _verify_context_async(self, session_id: str, message: str) -> Dict[str, Any]:
        """Verificação de contexto assíncrona com suporte a banco de dados"""
        conversation_history = []
        
        # Tentar carregar do banco primeiro
        if self.chat_memory:
            try:
                db_messages = self.chat_memory.get_messages(session_id, limit=20)
                # Converter ChatMessage para dict
                conversation_history = [
                    {
                        'role': msg.role,
                        'content': msg.content,
                        'timestamp': msg.created_at,
                        'additional_kwargs': msg.additional_kwargs
                    }
                    for msg in db_messages
                ]
                self.logger.info(f"Historico carregado do banco: {len(conversation_history)} mensagens")
            except Exception as e:
                self.logger.warning(f"Erro ao carregar historico do banco: {e}")
        
        # Fallback para memória RAM se banco não disponível
        if not conversation_history:
            conversation_history = self.conversation_history.get(session_id, [])
            if conversation_history:
                self.logger.info(f"Usando historico da memoria RAM: {len(conversation_history)} mensagens")
        
        context_analysis = {
            'session_id': session_id,
            'message': message,
            'conversation_history': conversation_history,
            'message_length': len(message),
            'is_question': message.strip().endswith('?'),
            'has_keywords': self._extract_keywords(message),
            'context_type': self._classify_context_type(message, conversation_history),
            'timestamp': datetime.now().isoformat()
        }
        
        # Contexto verificado silenciosamente
        return context_analysis
    
    async def _check_tools_needed_async(self, message: str, context: Dict[str, Any]) -> Dict[str, Any]:
        """Verificação de tools necessárias assíncrona"""
        try:
            # Por enquanto, não temos ferramentas específicas
            # Mas mantemos a estrutura para futuras implementações
            return {
                'tool_used': None,
                'tool_result': None,
                'decision': {'tool': 'none', 'reasoning': 'Nenhuma ferramenta específica disponível'},
                'needs_tool': False
            }
        except Exception as e:
            self.logger.error(f"Erro na verificação de tools: {str(e)}")
            return {
                'tool_used': None,
                'tool_result': None,
                'error': str(e),
                'needs_tool': False
            }
    
    async def _generate_response(self, message: str, context: Dict[str, Any], tool_result: Dict[str, Any], image_data: Optional[str] = None) -> Dict[str, Any]:
        """
        ETAPA 3: Gera resposta usando LLM
        """
        try:
            # Prepara contexto para o LLM
            llm_context = self._prepare_llm_context(message, context, tool_result, image_data)
            
            # Gera resposta usando LLM Provider
            response_content = await self.llm_provider.generate_response(llm_context)
            
            # Formata resposta no padrão esperado
            response = {
                'content': response_content,
                'pipeline_step': 'response_generated',
                'tool_used': tool_result.get('tool_used'),
                'reasoning': tool_result.get('decision', {}).get('reasoning'),
                'model_used': self.config.get('llm_model', 'gpt-4o-mini'),
                'provider': self.config.get('llm_provider', 'openai'),
                'context_verified': True,
                'timestamp': datetime.now().isoformat()
            }
            
            # Resposta gerada silenciosamente
            return response
            
        except Exception as e:
            self.logger.error(f"Erro na geração de resposta: {str(e)}")
            return {
                'content': 'Desculpe, ocorreu um erro ao gerar a resposta.',
                'error': str(e),
                'pipeline_step': 'response_error',
                'timestamp': datetime.now().isoformat()
            }
    
    def _save_context(self, session_id: str, message: str, response: Dict[str, Any], tool_result: Dict[str, Any], image_data: Optional[str] = None) -> bool:
        """
        ETAPA 4: Salva contexto
        Armazena conversa no banco de dados E no histórico local (fallback)
        """
        try:
            # Salvar no banco de dados (se disponível)
            if self.chat_memory:
                try:
                    # Criar sessão se não existir
                    session_created = self.chat_memory.create_session(session_id)
                    self.logger.info(f"Sessao criada/verificada: {session_id}")
                    
                    # Verificar se é a primeira mensagem da sessão
                    session_info = self.chat_memory.get_session_info(session_id)
                    if session_info and session_info.get('message_count', 0) == 0:
                        # Primeira mensagem - usar como título (limitar a 50 caracteres)
                        title = message[:50] + '...' if len(message) > 50 else message
                        self.chat_memory.update_session_title(session_id, title)
                        self.logger.info(f"Titulo da sessao atualizado: {title}")
                    
                    # Salvar mensagem do usuário
                    user_saved = self.chat_memory.add_user_message(session_id, message, image_data)
                    self.logger.info(f"Mensagem do usuario salva: {user_saved}")
                    
                    # Salvar resposta do assistente
                    assistant_saved = self.chat_memory.add_assistant_message(
                        session_id, 
                        response.get('content', '')
                    )
                    self.logger.info(f"Resposta do assistente salva: {assistant_saved}")
                    
                    self.logger.info(f"Mensagens salvas no banco para sessao {session_id}")
                except Exception as e:
                    self.logger.error(f"Erro ao salvar no banco: {e}", exc_info=True)
                    import traceback
                    traceback.print_exc()
            else:
                self.logger.warning("ChatMemory nao disponivel, usando apenas memoria")
            
            # Fallback: Salvar em memória também (compatibilidade)
            # Recupera histórico atual
            if session_id not in self.conversation_history:
                self.conversation_history[session_id] = []
            
            conversation_history = self.conversation_history[session_id]
            
            # Adiciona mensagem do usuário
            user_message = {
                'role': 'user',
                'content': message,
                'timestamp': datetime.now().isoformat()
            }
            if image_data:
                user_message['image_data'] = image_data
            conversation_history.append(user_message)
            
            # Adiciona resposta do assistente
            conversation_history.append({
                'role': 'assistant',
                'content': response.get('content', ''),
                'timestamp': datetime.now().isoformat(),
                'tool_used': tool_result.get('tool_used'),
                'pipeline_step': response.get('pipeline_step')
            })
            
            # Mantém apenas as últimas 20 mensagens para não exceder limites
            if len(conversation_history) > 20:
                self.conversation_history[session_id] = conversation_history[-20:]
            
            # Contexto salvo silenciosamente
            return True
            
        except Exception as e:
            self.logger.error(f"Erro ao salvar contexto: {str(e)}")
            return False
    
    def _extract_keywords(self, message: str) -> List[str]:
        """Extrai palavras-chave da mensagem"""
        keywords = []
        message_lower = message.lower()
        
        # Palavras-chave básicas para análise
        if 'ajuda' in message_lower or 'help' in message_lower:
            keywords.append('help_request')
        if 'obrigado' in message_lower or 'obrigada' in message_lower:
            keywords.append('thanks')
        if '?' in message:
            keywords.append('question')
        
        return keywords
    
    def _classify_context_type(self, message: str, history: List[Dict]) -> str:
        """Classifica o tipo de contexto da conversa"""
        if not history:
            return 'new_conversation'
        
        if len(history) < 3:
            return 'early_conversation'
        
        return 'ongoing_conversation'
    
    def _prepare_llm_context(self, message: str, context: Dict[str, Any], tool_result: Dict[str, Any], image_data: Optional[str] = None) -> Dict[str, Any]:
        """Prepara contexto para o LLM Provider"""
        # Prepara histórico da conversa (enviar como array de dicts)
        conversation_history = context.get('conversation_history', [])
        
        # Pegar apenas as últimas 10 mensagens para não exceder limites
        if conversation_history:
            conversation_history = conversation_history[-10:]
        
        return {
            'user_input': message,
            'conversation_history': conversation_history,  # Enviar array diretamente
            'system_prompt': self.system_prompt,
            'image_data': image_data,
            'context_analysis': f"Tipo: {context.get('context_type', 'unknown')}, Palavras-chave: {context.get('has_keywords', [])}"
        }
    
    def cleanup(self):
        """Limpa recursos do agente"""
        try:
            self.logger.info("Limpeza concluída")
        except Exception as e:
            self.logger.error(f"Erro na limpeza: {str(e)}")
    
    def get_status(self) -> Dict[str, Any]:
        """Retorna status completo do agente"""
        return {
            'name': 'Agente ORB',
            'version': '1.0.0',
            'status': 'active',
            'initialized': self._initialized,
            'llm_provider': self.config.get('llm_provider', 'openai'),
            'model': self.config.get('model', 'gpt-3.5-turbo'),
            'tools_available': list(self.tools.keys()) if self._tools else [],
            'active_sessions': len(self.conversation_history),
            'timestamp': datetime.now().isoformat(),
            'components': {
                "llm_provider": self._llm_provider is not None,
                "tool_selector": self._tool_selector is not None,
                "tools": self._tools is not None,
                "system_prompt": self._system_prompt is not None
            }
        }

# Função para carregar configuração do prompt
def load_prompt_config(config_path: str) -> Dict[str, Any]:
    """Carrega configuração do prompt de arquivo YAML"""
    with open(config_path, 'r', encoding='utf-8') as file:
        return yaml.safe_load(file)

# Exemplo de uso
if __name__ == "__main__":
    agente = AgenteORB()
    response = agente.process_message("Olá! Como você pode me ajudar?", "test_session_001")
    print(f"Resposta: {response['content']}")
    agente.cleanup()
