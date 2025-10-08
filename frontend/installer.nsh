; ORB Agent - Script de instalação customizado NSIS
; Instala o backend como serviço Windows automaticamente

!macro customInstall
  ; Criar diretório de logs
  CreateDirectory "$INSTDIR\resources\backend\logs"
  
  DetailPrint "ORB Backend incluído. Para rodar em background, execute install_service.bat como administrador."
!macroend

!macro customUnInstall
  DetailPrint "Removendo arquivos do ORB Backend..."
!macroend

