"""
Script para criar executável standalone do backend usando PyInstaller
"""

import PyInstaller.__main__
import os
import sys

# Caminho base
BASE_DIR = os.path.dirname(os.path.abspath(__file__))

# Argumentos do PyInstaller
args = [
    'backend_service.py',  # Script principal (novo entry point)
    '--name=orb-backend',  # Nome do executável
    '--onefile',  # Um único arquivo
    '--noconfirm',  # Sobrescrever sem perguntar
    '--clean',  # Limpar cache
    '--log-level=WARN',  # Menos verboso
    # NOTA: Não usar --noconsole para serviços Windows funcionarem corretamente
    
    # Adicionar todos os módulos do projeto
    f'--add-data={os.path.join(BASE_DIR, "src")};src',
    
    # Paths de importação
    f'--paths={BASE_DIR}',
    f'--paths={os.path.join(BASE_DIR, "src")}',
    
    # Hidden imports (módulos que PyInstaller pode não detectar)
    '--hidden-import=uvicorn',
    '--hidden-import=uvicorn.logging',
    '--hidden-import=uvicorn.loops',
    '--hidden-import=uvicorn.loops.auto',
    '--hidden-import=uvicorn.protocols',
    '--hidden-import=uvicorn.protocols.http',
    '--hidden-import=uvicorn.protocols.http.auto',
    '--hidden-import=uvicorn.protocols.websockets',
    '--hidden-import=uvicorn.protocols.websockets.auto',
    '--hidden-import=uvicorn.lifespan',
    '--hidden-import=uvicorn.lifespan.on',
    '--hidden-import=fastapi',
    '--hidden-import=pydantic',
    '--hidden-import=anthropic',
    '--hidden-import=openai',
    '--hidden-import=cryptography',
    '--hidden-import=cryptography.fernet',
    
    # Coletar submodules
    '--collect-submodules=uvicorn',
    '--collect-submodules=fastapi',
    '--collect-submodules=pydantic',
    
    # Diretório de saída
    '--distpath=dist',
    '--workpath=build',
    '--specpath=.',
]

if __name__ == '__main__':
    print(" Criando executável standalone do backend...")
    print(f" Diretório base: {BASE_DIR}")
    
    PyInstaller.__main__.run(args)
    
    print(" Build concluído!")
    backend_exe = os.path.join(BASE_DIR, 'dist', 'orb-backend.exe' if sys.platform == 'win32' else 'orb-backend')
    print(f" Executável: {backend_exe}")
    
    # Compilar wrapper do serviço C# e copiar backend para o local correto
    print("\n Compilando wrapper do serviço C#...")
    import subprocess
    try:
        result = subprocess.run(
            ["dotnet", "publish", "ServiceWrapper.csproj", "-c", "Release", "-r", "win-x64", "--self-contained"],
            cwd=BASE_DIR,
            capture_output=True,
            text=True
        )
        if result.returncode == 0:
            print(" Wrapper compilado com sucesso!")
            
            # Copiar backend para a pasta do wrapper
            import shutil
            publish_dir = os.path.join(BASE_DIR, "bin", "Release", "net9.0", "win-x64", "publish")
            dest_dir = os.path.join(publish_dir, "dist")
            os.makedirs(dest_dir, exist_ok=True)
            
            dest_file = os.path.join(dest_dir, "orb-backend.exe")
            shutil.copy2(backend_exe, dest_file)
            print(f" Backend copiado para: {dest_file}")
            print(f" Wrapper: {os.path.join(publish_dir, 'OrbBackendService.exe')}")
        else:
            print(f" Erro ao compilar wrapper: {result.stderr}")
    except Exception as e:
        print(f" Aviso: Não foi possível compilar o wrapper: {e}")

