#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Script para remover emojis de arquivos Python no backend
Solução para UnicodeEncodeError no Windows
"""

import re
import os
from pathlib import Path

# Mapeamento de emojis para texto
EMOJI_MAP = {
    '✅': 'OK:',
    '❌': 'ERRO:',
    '⚠️': 'AVISO:',
    '📋': 'INFO:',
    '🔧': 'CONFIG:',
    '💾': 'SAVE:',
    '📊': 'STATS:',
    '🎯': 'TARGET:',
    '🚀': 'START:',
    '⏰': 'TIME:',
    '👤': 'USER:',
    '🔍': 'SEARCH:',
    '📝': 'NOTE:',
    '💡': 'TIP:',
    '🔄': 'SYNC:',
    '📚': 'DOCS:',
    '🗑️': 'DELETE:',
    '📤': 'SEND:',
    '📥': 'RECEIVE:',
    '🔌': 'CONNECT:',
    '⚡': 'FAST:',
    '🎬': 'ACTION:',
    '🖱️': 'MOUSE:',
    '⌨️': 'KEYBOARD:',
    '💬': 'CHAT:',
    '🔵': 'BLUE:',
    '🔴': 'RED:',
    '🟢': 'GREEN:',
    '🟡': 'YELLOW:',
    '⏳': 'WAIT:',
    '🧹': 'CLEAN:',
    '📸': 'CAPTURE:',
    '👁️': 'VIEW:',
    '🆔': 'ID:',
    '📡': 'SIGNAL:',
}

def remove_emojis_from_file(file_path: Path):
    """Remove emojis de um arquivo Python"""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        original_content = content
        modified = False
        
        # Substituir emojis conhecidos
        for emoji, replacement in EMOJI_MAP.items():
            if emoji in content:
                content = content.replace(emoji, replacement)
                modified = True
        
        # Remover variação de emojis (FE0F)
        content = content.replace('\ufe0f', '')
        
        # Padrão para pegar qualquer emoji restante (U+1F000 até U+1FAFF)
        emoji_pattern = re.compile(
            "["
            "\U0001F600-\U0001F64F"  # emoticons
            "\U0001F300-\U0001F5FF"  # symbols & pictographs
            "\U0001F680-\U0001F6FF"  # transport & map symbols
            "\U0001F1E0-\U0001F1FF"  # flags
            "\U00002702-\U000027B0"  # dingbats
            "\U000024C2-\U0001F251"
            "\U0001F900-\U0001F9FF"  # supplemental symbols
            "\U0001FA00-\U0001FAFF"  # more symbols
            "]+", 
            flags=re.UNICODE
        )
        
        content = emoji_pattern.sub('', content)
        
        if content != original_content:
            with open(file_path, 'w', encoding='utf-8') as f:
                f.write(content)
            print(f"✓ Processado: {file_path}")
            return True
        else:
            print(f"  Sem mudanças: {file_path}")
            return False
            
    except Exception as e:
        print(f"✗ Erro em {file_path}: {e}")
        return False

def main():
    """Processa todos os arquivos Python no backend"""
    backend_dir = Path(__file__).parent
    
    # Arquivos a processar
    files_to_process = [
        backend_dir / "src" / "agentes" / "orb_agent" / "agente.py",
        backend_dir / "src" / "database" / "config_manager.py",
        backend_dir / "src" / "database" / "chat_memory.py",
        backend_dir / "test_config.py",
        backend_dir / "test_agent_integration.py",
        backend_dir / "tests" / "test_backend.py",
        backend_dir / "tests" / "test_backend_simple.py",
    ]
    
    processed = 0
    modified = 0
    
    print("Removendo emojis dos arquivos Python...")
    print("=" * 60)
    
    for file_path in files_to_process:
        if file_path.exists():
            processed += 1
            if remove_emojis_from_file(file_path):
                modified += 1
        else:
            print(f"  Arquivo não encontrado: {file_path}")
    
    print("=" * 60)
    print(f"Processados: {processed} arquivos")
    print(f"Modificados: {modified} arquivos")
    print("Concluído!")

if __name__ == "__main__":
    main()

