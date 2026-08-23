; Chrome Profile Launcher - Legacy manual installer (production releases use Velopack)

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
SetupIconFile={#MyAssetsDir}\setup-icon.ico
Compression=lzma
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "japanese"; MessagesFile: "compiler:Languages\Japanese.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "startuppath"; Description: "スタートアップに登録する"; GroupDescription: "{cm:AdditionalIcons}"; Flags: checkedonce

[Files]
Source: "{#MyBuildOutputDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; Assets include
Source: "{#MyAssetsDir}\app.ico"; DestDir: "{app}\Assets"; Flags: ignoreversion
Source: "{#MyAssetsDir}\chrome-profile-launcher.png"; DestDir: "{app}\Assets"; Flags: ignoreversion

[Icons]
Name: "{userprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\Assets\app.ico"
Name: "{userdesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\Assets\app.ico"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
// 将来的に .NET 10.0 ランタイムのチェックを追加可能
procedure CurStepChanged(CurStep: TSetupStep);
begin
  if (CurStep = ssPostInstall) and IsTaskSelected('startuppath') then
  begin
    RegWriteStringValue(HKEY_CURRENT_USER, 'Software\Microsoft\Windows\CurrentVersion\Run', 'ChromeProfileLauncher', ExpandConstant('{app}\{#MyAppExeName}'));
  end;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usPostUninstall then
  begin
    RegDeleteValue(HKEY_CURRENT_USER, 'Software\Microsoft\Windows\CurrentVersion\Run', 'ChromeProfileLauncher');
  end;
end;
