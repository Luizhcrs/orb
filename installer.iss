; Script do Inno Setup para Orb Agent
; Cria instalador único com Frontend WPF + Backend Python
#define MyAppName "Orb Agent"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Luiz Cavalcanti"
#define MyAppURL "https://github.com/Luizhcrs/orb"
#define MyAppExeName "Orb.exe"

[Setup]
; ID único (gerar novo GUID se forkar o projeto)
AppId={{F8E7D6C5-B4A3-9281-7065-4F3E2D1C0B9A}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\Orb Agent
DefaultGroupName=Orb Agent
AllowNoIcons=yes
; LicenseFile=LICENSE
OutputDir=release
OutputBaseFilename=OrbAgent-Setup-{#MyAppVersion}
SetupIconFile=frontend\Assets\ico.ico
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
DisableProgramGroupPage=yes
UninstallDisplayIcon={app}\{#MyAppExeName}

[Languages]
Name: "brazilianportuguese"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"

[Tasks]
Name: "desktopicon"; Description: "Criar atalho na Área de Trabalho"; GroupDescription: "Atalhos:"; Flags: checkedonce

[Files]
; Frontend WPF (todos os arquivos publicados)
Source: "frontend\bin\Release\net9.0-windows\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

  ; Backend: Executável Python standalone
  Source: "backend\dist\orb-backend.exe"; DestDir: "{app}\backend\dist"; Flags: ignoreversion

; Banco de dados será criado automaticamente no primeiro uso

; Assets (ícones, SVGs, etc.)
Source: "frontend\Assets\*"; DestDir: "{app}\Assets"; Flags: ignoreversion recursesubdirs createallsubdirs

; Documentação
Source: "README.md"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
; Menu Iniciar
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Comment: "Assistente Inteligente de IA"
Name: "{group}\Configurações do Orb"; Filename: "{app}\{#MyAppExeName}"; Parameters: "--config"; Comment: "Abrir configurações"
Name: "{group}\Desinstalar {#MyAppName}"; Filename: "{uninstallexe}"

; Desktop (opcional)
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon; Comment: "Assistente Inteligente de IA"

[Run]
; Iniciar aplicação após instalação
Filename: "{app}\{#MyAppExeName}"; Description: "Iniciar {#MyAppName}"; Flags: postinstall nowait skipifsilent

[Code]
// Funções Pascal para customização

function InitializeSetup(): Boolean;
begin
  Result := True;
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    // Após instalação, criar diretório de logs se não existir
    CreateDir(ExpandConstant('{app}\backend\logs'));
  end;
end;

function InitializeUninstall(): Boolean;
begin
  Result := True;
  
  // Confirmar desinstalação
  if MsgBox('Deseja realmente desinstalar o Orb Agent?' + #13#10 + #13#10 +
            'NOTA: Suas configurações e histórico de conversas serão preservados.' + #13#10 +
            'Para removê-los completamente, delete a pasta manualmente após a desinstalação.', 
            mbConfirmation, MB_YESNO or MB_DEFBUTTON2) = IDNO then
  begin
    Result := False;
  end;
end;

