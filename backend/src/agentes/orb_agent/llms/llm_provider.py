"""
LLM Provider para o Agente ORB
Suporte para OpenAI e Anthropic
"""

import os
import logging
from typing import Dict, Any, Optional, List
from abc import ABC, abstractmethod

class BaseLLMProvider(ABC):
    """Classe base para provedores de LLM"""
    
    @abstractmethod
    async def generate_response(self, context: Dict[str, Any]) -> str:
        """Gera resposta baseada no contexto"""
        pass

class OpenAIProvider(BaseLLMProvider):
    """Provedor OpenAI"""
    
    def __init__(self, config: Dict[str, Any]):
        self.config = config
        self.logger = logging.getLogger(__name__)
        
        # Importa OpenAI
        try:
            import openai
            self.client = openai.AsyncOpenAI(
                api_key=os.getenv('OPENAI_API_KEY')
            )
            self.logger.info("OpenAI client inicializado")
        except ImportError:
            raise ImportError("OpenAI não está instalado. Execute: pip install openai")
        except Exception as e:
            self.logger.error(f"Erro ao inicializar OpenAI: {str(e)}")
            raise
    
    async def generate_response(self, context: Dict[str, Any]) -> str:
        """Gera resposta usando OpenAI"""
        try:
            # Prepara mensagens para o chat
            messages = [
                {"role": "system", "content": context.get('system_prompt', 'Você é um assistente útil.')},
            ]
            
            # Adiciona histórico da conversa
            conversation_history = context.get('conversation_history', '')
            if conversation_history:
                # Parse do histórico de conversa
                for line in conversation_history.strip().split('\n'):
                    if line.startswith('Usuário: '):
                        messages.append({"role": "user", "content": line[9:]})
                    elif line.startswith('Assistente: '):
                        messages.append({"role": "assistant", "content": line[12:]})
            
            # Prepara mensagem atual do usuário
            user_input = context.get('user_input', '')
            image_data = context.get('image_data')
            
            if image_data:
                # Se há imagem, prepara mensagem multimodal
                # Log para debug do formato da imagem
                self.logger.info(f"Processando imagem - Tamanho base64: {len(image_data)} caracteres")
                self.logger.info(f"Primeiros 50 caracteres: {image_data[:50]}...")
                
                user_message = {
                    "role": "user",
                    "content": [
                        {"type": "text", "text": user_input},
                        {
                            "type": "image_url",
                            "image_url": {
                                "url": f"data:image/jpeg;base64,{image_data}"
                            }
                        }
                    ]
                }
            else:
                # Mensagem apenas texto
                user_message = {"role": "user", "content": user_input}
            
            messages.append(user_message)
            
            # Chama a API
            response = await self.client.chat.completions.create(
                model=self.config.get('llm_model', 'gpt-4o-mini'),
                messages=messages,
                max_tokens=self.config.get('max_tokens', 1000),
                temperature=self.config.get('temperature', 0.7)
            )
            
            return response.choices[0].message.content
            
        except Exception as e:
            self.logger.error(f"Erro ao gerar resposta OpenAI: {str(e)}")
            return "Desculpe, ocorreu um erro ao processar sua mensagem."

class AnthropicProvider(BaseLLMProvider):
    """Provedor Anthropic"""
    
    def __init__(self, config: Dict[str, Any]):
        self.config = config
        self.logger = logging.getLogger(__name__)
        
        # Importa Anthropic
        try:
            import anthropic
            self.client = anthropic.AsyncAnthropic(
                api_key=os.getenv('ANTHROPIC_API_KEY')
            )
            self.logger.info("Anthropic client inicializado")
        except ImportError:
            raise ImportError("Anthropic não está instalado. Execute: pip install anthropic")
        except Exception as e:
            self.logger.error(f"Erro ao inicializar Anthropic: {str(e)}")
            raise
    
    async def generate_response(self, context: Dict[str, Any]) -> str:
        """Gera resposta usando Anthropic"""
        try:
            # Prepara contexto da conversa
            system_prompt = context.get('system_prompt', 'Você é um assistente útil.')
            conversation_history = context.get('conversation_history', '')
            user_input = context.get('user_input', '')
            image_data = context.get('image_data')
            
            # Prepara conteúdo da mensagem
            message_content = []
            
            # Adiciona texto da mensagem
            message_content.append({"type": "text", "text": user_input})
            
            # Adiciona imagem se disponível
            if image_data:
                message_content.append({
                    "type": "image",
                    "source": {
                        "type": "base64",
                        "media_type": "image/jpeg",
                        "data": image_data
                    }
                })
            
            # Constrói o prompt completo para contexto
            full_prompt = ""
            if conversation_history:
                full_prompt += f"Histórico da conversa:\n{conversation_history}\n\n"
            full_prompt += f"Usuário: {user_input}"
            
            # Chama a API
            response = await self.client.messages.create(
                model=self.config.get('llm_model', 'claude-3-haiku-20240307'),
                max_tokens=self.config.get('max_tokens', 1000),
                temperature=self.config.get('temperature', 0.7),
                system=system_prompt,
                messages=[
                    {"role": "user", "content": message_content}
                ]
            )
            
            return response.content[0].text
            
        except Exception as e:
            self.logger.error(f"Erro ao gerar resposta Anthropic: {str(e)}")
            return "Desculpe, ocorreu um erro ao processar sua mensagem."

class LLMProvider:
    """Gerenciador principal de provedores LLM"""
    
    def __init__(self, config: Dict[str, Any]):
        self.config = config
        self.logger = logging.getLogger(__name__)
        self.provider = self._initialize_provider()
    
    def _initialize_provider(self) -> BaseLLMProvider:
        """Inicializa o provedor LLM baseado na configuração"""
        provider_type = self.config.get('llm_provider', 'openai')
        
        if provider_type == 'openai':
            openai_key = os.getenv('OPENAI_API_KEY')
            if not openai_key:
                self.logger.warning("OPENAI_API_KEY não encontrada. Usando modo demonstração.")
                return DemoProvider(self.config)
            return OpenAIProvider(self.config)
        
        elif provider_type == 'anthropic':
            anthropic_key = os.getenv('ANTHROPIC_API_KEY')
            if not anthropic_key:
                self.logger.warning("ANTHROPIC_API_KEY não encontrada. Usando modo demonstração.")
                return DemoProvider(self.config)
            return AnthropicProvider(self.config)
        
        else:
            self.logger.warning(f"Provedor '{provider_type}' não suportado. Usando modo demonstração.")
            return DemoProvider(self.config)
    
    async def generate_response(self, context: Dict[str, Any]) -> str:
        """Gera resposta usando o provedor configurado"""
        return await self.provider.generate_response(context)

class DemoProvider(BaseLLMProvider):
    """Provedor de demonstração quando não há API keys"""
    
    def __init__(self, config: Dict[str, Any]):
        self.config = config
        self.logger = logging.getLogger(__name__)
        self.responses = [
            "Olá! Eu sou o Agente ORB, seu assistente de IA flutuante. Como posso ajudá-lo hoje?",
            "Interessante! Para usar o assistente AI real, configure uma chave de API no arquivo .env.",
            "Entendi! Atualmente estou no modo demonstração. Configure OPENAI_API_KEY ou ANTHROPIC_API_KEY para ativar o assistente AI.",
            "Mensagem recebida! Estou pronto para ajudar assim que você configurar as credenciais de API!",
            "Para respostas inteligentes, adicione suas chaves de API no arquivo .env.",
            "Você pode configurar OpenAI ou Anthropic para ter acesso completo ao assistente AI.",
            "Estou funcionando no modo demonstração. Configure suas API keys para funcionalidade completa."
        ]
    
    async def generate_response(self, context: Dict[str, Any]) -> str:
        """Gera resposta de demonstração"""
        import random
        
        user_input = context.get('user_input', '').lower()
        image_data = context.get('image_data')
        
        # Verifica se há imagem
        if image_data:
            return "📸 Imagem recebida! No modo demonstração, não posso analisar imagens. Configure suas chaves de API para análise completa de imagens com OpenAI GPT-4V ou Claude 3."
        
        # Respostas específicas baseadas no input
        if any(word in user_input for word in ['olá', 'oi', 'hello', 'hi']):
            return "Olá! Eu sou o Agente ORB, seu assistente de IA flutuante. Como posso ajudá-lo hoje?"
        
        elif any(word in user_input for word in ['ajuda', 'help', 'como']):
            return "Posso ajudá-lo com várias tarefas! Configure suas chaves de API (OpenAI ou Anthropic) no arquivo .env para funcionalidade completa, incluindo análise de imagens."
        
        elif any(word in user_input for word in ['configurar', 'config', 'api', 'chave']):
            return "Para configurar: 1) Copie env.example para .env, 2) Adicione suas chaves de API (OPENAI_API_KEY ou ANTHROPIC_API_KEY), 3) Reinicie o servidor."
        
        else:
            # Resposta aleatória
            return random.choice(self.responses)
