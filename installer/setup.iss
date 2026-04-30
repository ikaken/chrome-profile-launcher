; Chrome Profile Launcher - Inno Setup Script

#define MyAppName "Chrome Profile Launcher"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "ikaken"
#define MyAppURL "https://github.com/ikaken/chrome-profile-launcher"
#define MyAppExeName "ChromeProfileLauncher.exe"
#define MyAssetsDir "..\Assets"
#define MyBuildOutputDir "..\bin\Release\net10.0-windows\win-x64\publish"

[Setup]
; AppId は各環境で一意である必要があります
AppId={{D8C868B7-3F7B-46D9-8E92-E93C623C6287}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={userpf}\{#MyAppName}
DisableProgramGroupPage=yes
; 管理者権限を要求しない（ユーザー単位のインストール）
PrivilegesRequired=lowest
UninstallDisplayIcon={app}\Assets\app.ico
OutputBaseFilename=ChromeProfileLauncherSetup
SetupIconFile={#MyAssetsDir}\app.ico
Compression=lzma
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "japanese"; MessagesFile: "compiler:Languages\Japanese.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#MyBuildOutputDir}\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#MyBuildOutputDir}\ChromeProfileLauncher.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#MyBuildOutputDir}\ChromeProfileLauncher.deps.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#MyBuildOutputDir}\ChromeProfileLauncher.runtimeconfig.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#MyBuildOutputDir}\System.Management.dll"; DestDir: "{app}"; Flags: ignoreversion
; Assets include
Source: "{#MyAssetsDir}\app.ico"; DestDir: "{app}\Assets"; Flags: ignoreversion
Source: "{#MyAssetsDir}\chrome-profile-launcher.png"; DestDir: "{app}\Assets"; Flags: ignoreversion

[Icons]
Name: "{userprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\Assets\app.ico"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\Assets\app.ico"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
// 将来的に .NET 10.0 ランタイムのチェックを追加可能
