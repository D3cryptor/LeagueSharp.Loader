#dim Version[4]
#expr ParseVersion("..\bin\Release\LeagueSharp.Loader.exe", Version[0], Version[1], Version[2], Version[3])
#define MyAppVersion Str(Version[0]) + "." + Str(Version[1]) + "." + Str(Version[2])
#define MyAppName "LeagueSharp"
#define MyAppExeName "LeagueSharp.Loader.exe"

[Setup]
AppName={#MyAppName}
AppVersion={#MyAppVersion}
Compression=lzma2
SolidCompression=yes
CreateAppDir=no
DisableReadyPage=yes
DisableStartupPrompt=yes
DisableFinishedPage=yes
OutputDir=Output\
OutputBaseFilename=LeagueSharp-update
PrivilegesRequired=admin

[Files]
;Loader
Source: "..\bin\Release\*.exe"; Excludes: *.vshost.exe; DestDir: {src}; Flags: ignoreversion
Source: "..\bin\Release\*.config"; Excludes: *.vshost.exe.config;  DestDir: {src}; Flags: ignoreversion
Source: "..\bin\Release\*.dll"; DestDir: "{src}\bin\"; Flags: ignoreversion

;System
Source: "..\bin\Release\System\LeagueSharp.AppDomainManager.dll"; DestDir: "{src}\System\"; Flags: ignoreversion
Source: "..\bin\Release\System\LeagueSharp.Bootstrap.dll"; DestDir: "{src}\System\"; Flags: ignoreversion
Source: "..\bin\Release\System\Leaguesharp.Core.dll"; DestDir: "{src}\System\"; Flags: ignoreversion
Source: "..\bin\Release\System\LeagueSharp.dll"; DestDir: "{src}\System\"; Flags: ignoreversion
Source: "..\bin\Release\System\LeagueSharp.xml"; DestDir: "{src}\System\"; Flags: ignoreversion
Source: "..\bin\Release\System\SharpDX.Direct3D9.dll"; DestDir: "{src}\System\"; Flags: ignoreversion
Source: "..\bin\Release\System\SharpDX.dll"; DestDir: "{src}\System\"; Flags: ignoreversion
Source: "..\bin\Release\System\SharpDX.Toolkit.dll"; DestDir: "{src}\System\"; Flags: ignoreversion
Source: "..\bin\Release\System\SharpDX.Toolkit.Graphics.dll"; DestDir: "{src}\System\"; Flags: ignoreversion
Source: "..\bin\Release\System\SharpDX.Toolkit.Graphics.xml"; DestDir: "{src}\System\"; Flags: ignoreversion
Source: "..\bin\Release\System\SharpDX.Toolkit.xml"; DestDir: "{src}\System\"; Flags: ignoreversion

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
Filename: {src}\{#MyAppExeName}; Flags: shellexec nowait; 

[Code]
function InitializeSetup(): Boolean;
begin
	initwinversion();
	msi31('3.1');
	dotnetfx45(0);
	vcredist2013();
	Result := true;
end;