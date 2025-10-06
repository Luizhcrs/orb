"""
Serviço Windows para ORB Backend
Implementação usando pywin32 para execução como serviço do Windows
"""

import sys
import os
import time
import logging
import threading
from typing import Optional
import asyncio
import signal

# Adiciona o diretório src ao path
sys.path.insert(0, os.path.join(os.path.dirname(__file__), '..'))

try:
    import win32serviceutil
    import win32service
    import win32event
    import servicemanager
    WIN32_AVAILABLE = True
except ImportError:
    WIN32_AVAILABLE = False
    print("pywin32 não está instalado. Execute: pip install pywin32")

# Importa app do FastAPI
from api.main import app
import uvicorn

class ORBService(win32serviceutil.ServiceFramework):
    """Serviço Windows para ORB Backend"""
    
    _svc_name_ = "ORBBackend"
    _svc_display_name_ = "ORB Backend API Service"
    _svc_description_ = "Serviço backend para o assistente ORB - Assistente de IA flutuante"
    
    def __init__(self, args):
        """Inicializa o serviço"""
        if not WIN32_AVAILABLE:
            raise ImportError("pywin32 não está disponível")
        
        win32serviceutil.ServiceFramework.__init__(self, args)
        
        # Configura evento para parar o serviço
        self.hWaitStop = win32event.CreateEvent(None, 0, 0, None)
        
        # Configura logging para Event Viewer
        self._setup_event_logging()
        
        # Servidor uvicorn
        self.server = None
        self.server_thread = None
        
        self.logger = logging.getLogger(__name__)
        self.logger.info("ORB Service inicializado")
    
    def _setup_event_logging(self):
        """Configura logging para Event Viewer do Windows"""
        try:
            # Handler para Event Log
            handler = logging.handlers.NTEventLogHandler("ORB Backend")
            handler.setLevel(logging.INFO)
            
            # Formato simples para Event Log
            formatter = logging.Formatter(
                '%(levelname)s - %(message)s'
            )
            handler.setFormatter(formatter)
            
            # Adiciona handler ao logger root
            logging.getLogger().addHandler(handler)
            
            self.logger.info("Logging para Event Viewer configurado")
            
        except Exception as e:
            print(f"Erro ao configurar Event Log: {str(e)}")
    
    def SvcStop(self):
        """Para o serviço"""
        self.logger.info("Parando ORB Service...")
        
        # Sinaliza para parar
        win32event.SetEvent(self.hWaitStop)
        
        # Para o servidor uvicorn
        if self.server:
            self.logger.info("Parando servidor uvicorn...")
            # uvicorn não tem método stop direto, então usamos threading.Event
            if hasattr(self, 'stop_event'):
                self.stop_event.set()
        
        self.logger.info("ORB Service parado")
    
    def SvcDoRun(self):
        """Executa o serviço"""
        try:
            self.logger.info("Iniciando ORB Service...")
            
            # Registra o serviço
            servicemanager.LogMsg(
                servicemanager.EVENTLOG_INFORMATION_TYPE,
                servicemanager.PYS_SERVICE_STARTED,
                (self._svc_name_, '')
            )
            
            # Inicia o servidor em thread separada
            self._start_server()
            
            # Aguarda evento de parada
            win32event.WaitForSingleObject(self.hWaitStop, win32event.INFINITE)
            
        except Exception as e:
            self.logger.error(f"Erro no serviço: {str(e)}")
            servicemanager.LogErrorMsg(f"Erro no ORB Service: {str(e)}")
    
    def _start_server(self):
        """Inicia o servidor uvicorn em thread separada"""
        def run_server():
            try:
                self.logger.info("Iniciando servidor uvicorn...")
                
                # Configura uvicorn
                config = uvicorn.Config(
                    app,
                    host="0.0.0.0",
                    port=8000,
                    log_level="info",
                    access_log=True
                )
                
                self.server = uvicorn.Server(config)
                
                # Inicia servidor
                asyncio.run(self.server.serve())
                
            except Exception as e:
                self.logger.error(f"Erro no servidor uvicorn: {str(e)}")
        
        # Inicia thread do servidor
        self.server_thread = threading.Thread(target=run_server, daemon=True)
        self.server_thread.start()
        
        self.logger.info("Servidor uvicorn iniciado em thread separada")

def install_service():
    """Instala o serviço Windows"""
    if not WIN32_AVAILABLE:
        print("pywin32 não está instalado. Execute: pip install pywin32")
        return False
    
    try:
        print("Instalando ORB Backend Service...")
        win32serviceutil.InstallService(
            ORBService._svc_reg_class_,
            ORBService._svc_name_,
            ORBService._svc_display_name_,
            description=ORBService._svc_description_
        )
        print(f"Serviço '{ORBService._svc_display_name_}' instalado com sucesso!")
        print(f"Você pode gerenciá-lo via Services.msc ou usar:")
        print(f"  net start {ORBService._svc_name_}")
        print(f"  net stop {ORBService._svc_name_}")
        return True
        
    except Exception as e:
        print(f"Erro ao instalar serviço: {str(e)}")
        return False

def uninstall_service():
    """Remove o serviço Windows"""
    if not WIN32_AVAILABLE:
        print("pywin32 não está instalado. Execute: pip install pywin32")
        return False
    
    try:
        print("Removendo ORB Backend Service...")
        win32serviceutil.RemoveService(ORBService._svc_name_)
        print(f"Serviço '{ORBService._svc_display_name_}' removido com sucesso!")
        return True
        
    except Exception as e:
        print(f"Erro ao remover serviço: {str(e)}")
        return False

def start_service():
    """Inicia o serviço Windows"""
    if not WIN32_AVAILABLE:
        print("pywin32 não está instalado. Execute: pip install pywin32")
        return False
    
    try:
        print(f"Iniciando serviço '{ORBService._svc_name_}'...")
        win32serviceutil.StartService(ORBService._svc_name_)
        print("Serviço iniciado com sucesso!")
        return True
        
    except Exception as e:
        print(f"Erro ao iniciar serviço: {str(e)}")
        return False

def stop_service():
    """Para o serviço Windows"""
    if not WIN32_AVAILABLE:
        print("pywin32 não está instalado. Execute: pip install pywin32")
        return False
    
    try:
        print(f"Parando serviço '{ORBService._svc_name_}'...")
        win32serviceutil.StopService(ORBService._svc_name_)
        print("Serviço parado com sucesso!")
        return True
        
    except Exception as e:
        print(f"Erro ao parar serviço: {str(e)}")
        return False

def get_service_status():
    """Obtém status do serviço"""
    if not WIN32_AVAILABLE:
        print("pywin32 não está instalado. Execute: pip install pywin32")
        return None
    
    try:
        status = win32serviceutil.QueryServiceStatus(ORBService._svc_name_)
        status_map = {
            win32service.SERVICE_STOPPED: "Parado",
            win32service.SERVICE_START_PENDING: "Iniciando",
            win32service.SERVICE_STOP_PENDING: "Parando",
            win32service.SERVICE_RUNNING: "Executando",
            win32service.SERVICE_CONTINUE_PENDING: "Continuando",
            win32service.SERVICE_PAUSE_PENDING: "Pausando",
            win32service.SERVICE_PAUSED: "Pausado"
        }
        
        status_text = status_map.get(status[1], "Desconhecido")
        print(f"Status do serviço '{ORBService._svc_name_}': {status_text}")
        return status_text
        
    except Exception as e:
        print(f"Erro ao obter status: {str(e)}")
        return None

if __name__ == "__main__":
    if not WIN32_AVAILABLE:
        print("pywin32 não está instalado. Execute: pip install pywin32")
        sys.exit(1)
    
    # Verifica argumentos da linha de comando
    if len(sys.argv) == 1:
        # Executa como serviço
        servicemanager.Initialize()
        servicemanager.PrepareToHostSingle(ORBService)
        servicemanager.StartServiceCtrlDispatcher()
    else:
        # Comandos de gerenciamento
        command = sys.argv[1].lower()
        
        if command == "install":
            install_service()
        elif command == "uninstall":
            uninstall_service()
        elif command == "start":
            start_service()
        elif command == "stop":
            stop_service()
        elif command == "status":
            get_service_status()
        elif command == "debug":
            # Modo debug - executa sem ser serviço
            print("Executando em modo debug...")
            service = ORBService([])
            try:
                service.SvcDoRun()
            except KeyboardInterrupt:
                print("Interrompido pelo usuário")
                service.SvcStop()
        else:
            print("Comandos disponíveis:")
            print("  install   - Instala o serviço")
            print("  uninstall - Remove o serviço")
            print("  start     - Inicia o serviço")
            print("  stop      - Para o serviço")
            print("  status    - Mostra status do serviço")
            print("  debug     - Executa em modo debug")
