; Inno Setup script for Teron Addon Manager.
;
; Normally you don't need to run this directly: publishing the win-x64 or win-x86 profile
; (via "dotnet publish -p:PublishProfile=win-x64" or Visual Studio's Publish dialog) builds
; this automatically as a post-publish MSBuild step - see the BuildInnoSetupInstaller target
; in TeronAddonManager.csproj. The lines below are only needed to run it manually:
;   dotnet publish -p:PublishProfile=win-x64 -c Release
;   dotnet publish -p:PublishProfile=win-x86 -c Release
;   ISCC TeronAddonManager.iss              (defaults to x64)
;   ISCC /DArch=x86 TeronAddonManager.iss    (x86 build)
;
; Output goes to bin\InstallerPackage\TeronAddonManagerSetup-<arch>.exe

#ifndef Arch
  #define Arch "x64"
#endif

#ifndef MyAppVersion
  #define MyAppVersion "2.6.2"
#endif

#define MyAppName "Teron Addon Manager"
#define MyAppPublisher "Teronverse"
#define MyAppExeName "TeronAddonManager.exe"
#define MyPublishDir "..\bin\Publish\TeronAddonManager_Win_" + Arch

[Setup]
AppId={{F4D7F0AD-9049-4B38-86F2-A3F796CA6121}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\TeronAddonManager
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
LicenseFile=..\LICENSE.txt
SetupIconFile=..\Resources\app.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
OutputDir=..\bin\InstallerPackage
OutputBaseFilename=TeronAddonManagerSetup-{#Arch}
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed={#(Arch == "x64" ? "x64compatible" : "x86compatible")}
ArchitecturesInstallIn64BitMode={#(Arch == "x64" ? "x64compatible" : "")}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop shortcut"; GroupDescription: "Additional shortcuts:"

[Files]
Source: "{#MyPublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Uninstall {#MyAppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch {#MyAppName}"; Flags: nowait postinstall skipifsilent
