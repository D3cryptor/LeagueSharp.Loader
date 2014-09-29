#dim Version[4]
#expr ParseVersion("..\bin\Release\LeagueSharp.Loader.exe", Version[0], Version[1], Version[2], Version[3])
#define MyAppVersion Str(Version[0]) + "." + Str(Version[1]) + "." + Str(Version[2])
#define MyAppName "LeagueSharp"
#define MyAppExeName "LeagueSharp.Loader.exe"

[Setup]
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppId={#MyAppName}
DefaultDirName="{src}\LeagueSharp"
Compression=lzma2
DisableDirPage=true
SolidCompression=yes
DisableReadyPage=yes
DisableStartupPrompt=yes
DisableFinishedPage=yes
Uninstallable=no
OutputDir=Output\
OutputBaseFilename=LeagueSharp-update
PrivilegesRequired=admin

[Files]
;Loader
Source: "..\bin\Release\*.exe"; Excludes: *.vshost.exe; DestDir: {app}; Flags: ignoreversion
Source: "..\bin\Release\*.config"; Excludes: *.vshost.exe.config;  DestDir: {app}; Flags: ignoreversion
Source: "..\bin\Release\*.dll"; DestDir: "{app}\bin\"; Flags: ignoreversion

;System
Source: "..\bin\Release\System\LeagueSharp.AppDomainManager.dll"; DestDir: "{app}\System\"; Flags: ignoreversion
Source: "..\bin\Release\System\LeagueSharp.Bootstrap.dll"; DestDir: "{app}\System\"; Flags: ignoreversion
Source: "..\bin\Release\System\Leaguesharp.Core.dll"; DestDir: "{app}\System\"; Flags: ignoreversion
Source: "..\bin\Release\System\LeagueSharp.dll"; DestDir: "{app}\System\"; Flags: ignoreversion
Source: "..\bin\Release\System\LeagueSharp.xml"; DestDir: "{app}\System\"; Flags: ignoreversion
Source: "..\bin\Release\System\SharpDX.Direct3D9.dll"; DestDir: "{app}\System\"; Flags: ignoreversion
Source: "..\bin\Release\System\SharpDX.dll"; DestDir: "{app}\System\"; Flags: ignoreversion
Source: "..\bin\Release\System\SharpDX.Toolkit.dll"; DestDir: "{app}\System\"; Flags: ignoreversion
Source: "..\bin\Release\System\SharpDX.Toolkit.Graphics.dll"; DestDir: "{app}\System\"; Flags: ignoreversion
Source: "..\bin\Release\System\SharpDX.Toolkit.Graphics.xml"; DestDir: "{app}\System\"; Flags: ignoreversion
Source: "..\bin\Release\System\SharpDX.Toolkit.xml"; DestDir: "{app}\System\"; Flags: ignoreversion
Source: "..\bin\Release\System\clipper_library.dll"; DestDir: "{app}\System\"; Flags: ignoreversion

[Languages]
Name: "en"; MessagesFile: "compiler:Default.isl"
Name: "de"; MessagesFile: "compiler:Languages\German.isl"

#include "Scripts\products.iss"
#include "Scripts\products\stringversion.iss"
#include "Scripts\products\winversion.iss"
#include "Scripts\products\fileversion.iss"
#include "Scripts\products\dotnetfxversion.iss"
#include "Scripts\products\msi31.iss"
#include "Scripts\products\dotnetfx45.iss"
#include "Scripts\products\vcredist2013.iss"

[Run]
Filename: {app}\{#MyAppExeName}; Flags: shellexec nowait; 

[Code]
function InitializeSetup(): Boolean;
begin
	initwinversion();
	msi31('3.1');
	dotnetfx45(1);
	vcredist2013();
	Result := true;
end;
