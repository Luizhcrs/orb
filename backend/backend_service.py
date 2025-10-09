"""
ORB Backend - Executável para rodar como serviço Windows
"""

import sys
import os
import logging
from pathlib import Path

# Adicionar src ao path
sys.path.insert(0, str(Path(__file__).parent / "src"))

# Configurar logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s',
    handlers=[
        logging.FileHandler('orb-backend.log'),
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
            # Executável PyInstaller - usar diretório do executável
            base_dir = Path(sys.executable).parent
        else:
            # Modo desenvolvimento - usar raiz do projeto
            base_dir = Path(__file__).parent
        
        # Criar diretório de dados se não existir
        data_dir = base_dir / "data"
        data_dir.mkdir(exist_ok=True)
        
        # Configurar variáveis de ambiente para o banco de dados
        db_path = data_dir / "orb.db"
        os.environ['DATABASE_PATH'] = str(db_path)
        
        # Configurar ENCRYPTION_KEY se não existir
        if not os.getenv('ENCRYPTION_KEY'):
            from cryptography.fernet import Fernet
            encryption_key = Fernet.generate_key().decode()
            os.environ['ENCRYPTION_KEY'] = encryption_key
            logger.warning(f" Nova ENCRYPTION_KEY gerada: {encryption_key}")
            logger.warning(" Esta chave será perdida ao reiniciar o serviço!")
            logger.warning(" Configure ENCRYPTION_KEY como variável de ambiente do sistema!")
        
        logger.info(f" Banco de dados: {db_path}")
        
        # Importar e iniciar o servidor
        import uvicorn
        from src.api.main import app
        
        # Configurações
        host = os.getenv("HOST", "127.0.0.1")
        port = int(os.getenv("PORT", "8000"))
        
        logger.info(f" Servidor iniciando em {host}:{port}")
        
        # Iniciar servidor
        uvicorn.run(
            app,
            host=host,
            port=port,
            log_level="info",
            access_log=True,
            reload=False  # Sem reload em produção
        )
        
    except Exception as e:
        logger.error(f" Erro fatal: {e}", exc_info=True)
        sys.exit(1)

if __name__ == "__main__":
    main()

