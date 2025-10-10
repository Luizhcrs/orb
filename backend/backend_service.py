"""
ORB Backend - Executável para rodar como serviço Windows
"""

import sys
import os
import logging
from pathlib import Path

# Adicionar src ao path
sys.path.insert(0, str(Path(__file__).parent / "src"))

# Determinar diretório para logs (usar AppData se instalado em Program Files)
if getattr(sys, 'frozen', False):
    # Executável PyInstaller
    base_dir = Path(sys.executable).parent
    # Se estiver em Program Files, usar AppData
    if "Program Files" in str(base_dir):
        log_dir = Path(os.getenv('APPDATA')) / 'OrbAgent' / 'logs'
    else:
        log_dir = base_dir
else:
    # Modo desenvolvimento
    log_dir = Path(__file__).parent

# Criar diretório de logs se não existir
log_dir.mkdir(parents=True, exist_ok=True)
log_file = log_dir / 'orb-backend.log'

# Configurar logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s',
    handlers=[
        logging.FileHandler(log_file),
        logging.StreamHandler(sys.stdout)
    ]
)

logger = logging.getLogger(__name__)

def main():
    """Entry point para o serviço"""
    try:
        logger.info(" Iniciando ORB Backend Service...")
        
        # Configurar diretório de dados
        if getattr(sys, 'frozen', False):
            # Executável PyInstaller
            base_dir = Path(sys.executable).parent
            # Se estiver em Program Files, usar AppData
            if "Program Files" in str(base_dir):
                data_dir = Path(os.getenv('APPDATA')) / 'OrbAgent' / 'data'
            else:
                data_dir = base_dir / "data"
        else:
            # Modo desenvolvimento - usar raiz do projeto
            base_dir = Path(__file__).parent
            data_dir = base_dir / "data"
        
        # Criar diretório de dados se não existir
        data_dir.mkdir(parents=True, exist_ok=True)
        
        # Configurar variáveis de ambiente para o banco de dados
        db_path = data_dir / "orb.db"
        os.environ['DATABASE_PATH'] = str(db_path)
        
        # Configurar ENCRYPTION_KEY - salvar de forma persistente
        encryption_key_file = data_dir / ".encryption_key"
        
        if encryption_key_file.exists():
            # Carregar chave existente
            with open(encryption_key_file, 'r') as f:
                encryption_key = f.read().strip()
            logger.info(" ENCRYPTION_KEY carregada do arquivo")
        elif os.getenv('ENCRYPTION_KEY'):
            # Usar chave da variável de ambiente e salvá-la
            encryption_key = os.getenv('ENCRYPTION_KEY')
            with open(encryption_key_file, 'w') as f:
                f.write(encryption_key)
            logger.info(" ENCRYPTION_KEY salva da variável de ambiente")
        else:
            # Gerar nova chave e salvá-la
            from cryptography.fernet import Fernet
            encryption_key = Fernet.generate_key().decode()
            with open(encryption_key_file, 'w') as f:
                f.write(encryption_key)
            logger.warning(" Nova ENCRYPTION_KEY gerada e salva")
        
        os.environ['ENCRYPTION_KEY'] = encryption_key
        
        logger.info(f" Banco de dados: {db_path}")
        
        # Importar e iniciar o servidor
        import uvicorn
        
        # Importar app de acordo com o modo de execução
        if getattr(sys, 'frozen', False):
            # Modo PyInstaller - importar diretamente
            from api.main import app
        else:
            # Modo desenvolvimento
            from src.api.main import app
        
        # Configurações
        host = os.getenv("HOST", "127.0.0.1")
        port = int(os.getenv("PORT", "8000"))
        
        logger.info(f" Servidor iniciando em {host}:{port}")
        
        # Configurar logging para não usar console quando executável
        log_config = uvicorn.config.LOGGING_CONFIG.copy()
        log_config["formatters"]["default"]["fmt"] = "%(asctime)s - %(name)s - %(levelname)s - %(message)s"
        log_config["formatters"]["access"]["fmt"] = "%(asctime)s - %(levelprefix)s %(client_addr)s - \"%(request_line)s\" %(status_code)s"
        
        # Remover uso de sys.stdout/stderr se não existirem
        if sys.stdout is None or sys.stderr is None:
            log_config["handlers"]["default"]["stream"] = "ext://sys.__stdout__"
            log_config["handlers"]["access"]["stream"] = "ext://sys.__stdout__"
        
        # Iniciar servidor
        uvicorn.run(
            app,
            host=host,
            port=port,
            log_level="info",
            access_log=True,
            reload=False,  # Sem reload em produção
            log_config=log_config
        )
        
    except Exception as e:
        logger.error(f" Erro fatal: {e}", exc_info=True)
        sys.exit(1)

if __name__ == "__main__":
    main()

