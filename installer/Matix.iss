;  Matix - Windows installer script (Inno Setup 6)
;  Produces: dist\matix-installer.exe
;
;  Don't run this file directly - run build-installer.bat in the folder above,
;  which publishes the app first and then compiles this script.

#define MyAppName        "Matix"
#define MyAppVersion     "1.0.0"
#define MyAppPublisher   "Matix the Math Club"
#define MyAppExeName     "Matix.exe"

[Setup]
AppId={{8F3A6C21-4B7E-4B2A-9E4D-6C1A2B7D9E01}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\Matix
DefaultGroupName=Matix
DisableProgramGroupPage=yes
OutputDir=..\dist
OutputBaseFilename=matix-installer
SetupIconFile=..\build\icon.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
UninstallDisplayName={#MyAppName}
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern

; Installs for the current user by default so Windows never asks for an
; admin password. Users can still switch to all-users in the first dialog.
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
ArchitecturesInstallIn64BitMode=x64compatible
ArchitecturesAllowed=x64compatible

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Shortcuts:"

[Files]
; Everything produced by 'dotnet publish', including the app\ folder that
; holds app.html, download.html and logo.svg.
Source: "..\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

; Optional: bundle the WebView2 runtime so Matix works even on a PC that
; doesn't already have it. Download the "Evergreen Bootstrapper" from
; https://developer.microsoft.com/microsoft-edge/webview2/ and save it in
; this installer folder as MicrosoftEdgeWebview2Setup.exe. If it isn't there
; these lines are skipped automatically and the installer still builds.
#if FileExists("MicrosoftEdgeWebview2Setup.exe")
Source: "MicrosoftEdgeWebview2Setup.exe"; DestDir: "{tmp}"; Flags: deleteafterinstall
#endif

[Icons]
Name: "{autoprograms}\Matix"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\Matix"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
#if FileExists("MicrosoftEdgeWebview2Setup.exe")
Filename: "{tmp}\MicrosoftEdgeWebview2Setup.exe"; Parameters: "/silent /install"; \
  StatusMsg: "Installing a component Matix needs..."; Check: NeedsWebView2; Flags: waituntilterminated
#endif
Filename: "{app}\{#MyAppExeName}"; Description: "Open Matix now"; \
  Flags: nowait postinstall skipifsilent

[Code]
{ True when the WebView2 runtime is missing. Windows 11 and most updated
  Windows 10 machines already have it installed. }
function NeedsWebView2: Boolean;
var
  Value: String;
  Key: String;
begin
  Key := 'SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate\Clients\' +
         '{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}';

  Result := True;

  if RegQueryStringValue(HKEY_LOCAL_MACHINE, Key, 'pv', Value) then
    if (Value <> '') and (Value <> '0.0.0.0') then
      Result := False;

  if Result then
    if RegQueryStringValue(HKEY_CURRENT_USER, Key, 'pv', Value) then
      if (Value <> '') and (Value <> '0.0.0.0') then
        Result := False;
end;
