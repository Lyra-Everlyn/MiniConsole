[Setup]
AppId={{8F9A2B3C-4D5E-6F7A-8B9C-0D1E2F3A4B5C}
AppName=MiniConsole
AppVersion=1.0.0
AppPublisher=Lyra Everlyn
AppPublisherURL=https://github.com/Lyra-Everlyn/MiniConsole
AppSupportURL=https://github.com/Lyra-Everlyn/MiniConsole/issues
AppUpdatesURL=https://github.com/Lyra-Everlyn/MiniConsole/releases
DefaultDirName={autopf}\MiniConsole
DefaultGroupName=MiniConsole

SetupIconFile=MiniConsole\icon.ico
UninstallDisplayIcon={app}\MiniConsole.exe
OutputDir=Installer
OutputBaseFilename=MiniConsole_Setup_v1.0.0
Compression=lzma2/max
SolidCompression=yes
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[Files]
Source: "MiniConsole\Publish\MiniConsole.exe"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\MiniConsole"; Filename: "{app}\MiniConsole.exe"; WorkingDir: "{app}"
Name: "{autodesktop}\MiniConsole"; Filename: "{app}\MiniConsole.exe"; WorkingDir: "{app}"; Tasks: desktopicon

[Run]
Filename: "{cmd}"; Parameters: "/c echo Installed > ""{app}\.installed"""; Flags: runhidden
Filename: "{app}\MiniConsole.exe"; Description: "{cm:LaunchProgram,MiniConsole}"; WorkingDir: "{app}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: files; Name: "{app}\appsettings.json"
Type: files; Name: "{app}\.installed"
Type: files; Name: "{app}\MiniConsole.exe.old"
Type: files; Name: "{app}\MiniConsole.exe.new"