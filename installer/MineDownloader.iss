; Mine Downloader 安装包脚本 (Inno Setup 6)
; 用法: ISCC.exe installer\MineDownloader.iss   （先运行 build.ps1 -Configuration Release）

#define AppName "Mine Downloader"
#define AppExe "Mine Downloader.exe"
#define BinDir "..\Mine Downloader\bin"
#define AppVersion GetVersionNumbersString(BinDir + "\" + AppExe)

[Setup]
AppId={{6B1C6E0E-6E3B-4C55-9D7A-4D2B0F8A3C11}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher=Frank-Symbol
AppPublisherURL=https://github.com/Frank-Symbol/Mine-Downloader
AppSupportURL=https://github.com/Frank-Symbol/Mine-Downloader/issues
VersionInfoVersion={#AppVersion}
; 启动器会在程序目录写入配置/数据，因此默认装到用户目录，无需管理员权限
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
UninstallDisplayIcon={app}\{#AppExe}
SetupIconFile=..\Mine Downloader\Images\icon.ico
OutputDir=..\交付
OutputBaseFilename=Mine-Downloader-v{#AppVersion}-setup
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
ArchitecturesInstallIn64BitMode=x64compatible

[Languages]
Name: "chinesesimplified"; MessagesFile: "ChineseSimplified.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[Files]
Source: "{#BinDir}\{#AppExe}"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#BinDir}\{#AppExe}.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#BinDir}\PCLCS.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#BinDir}\MeloongCore.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#BinDir}\MeloongCore.Wpf.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#BinDir}\Microsoft.Win32.Registry.dll"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExe}"
Name: "{group}\{cm:UninstallProgram,{#AppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExe}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExe}"; Description: "{cm:LaunchProgram,{#AppName}}"; Flags: nowait postinstall skipifsilent
