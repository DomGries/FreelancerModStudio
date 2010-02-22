#include "scripts\products.iss"

#include "scripts\products\winversion.iss"
#include "scripts\products\fileversion.iss"

#include "scripts\products\ngen.iss"
#include "scripts\products\msi31.iss"
#include "scripts\products\dotnetfx35sp1.iss"

[Setup]
AppName=FMS System Watcher
AppVersion=1.0
AppVerName=FMS System Watcher
AppCopyright=Copyright © stfx 2010
VersionInfoVersion=1.0
VersionInfoCompany=stfx
AppPublisher=stfx
AppPublisherURL=http://code.google.com/p/freelancermodstudio
AppUpdatesURL=http://code.google.com/p/freelancermodstudio/downloads/list
OutputBaseFilename=FMS-System_Watcher-1.0
DefaultGroupName=FMS System Watcher
DefaultDirName={pf}\FMS System Watcher
Uninstallable=yes
CreateUninstallRegKey=yes
UpdateUninstallLogAppName=yes
CreateAppDir=yes
OutputDir=bin
SourceDir=.
AllowNoIcons=yes
WizardImageFile=src\FreelancerModManager.bmp
WizardSmallImageFile=src\FreelancerModManager_small.bmp
LanguageDetectionMethod=uilanguage
SolidCompression=yes
Compression=lzma/max

MinVersion=0,5.01
PrivilegesRequired=admin
ArchitecturesAllowed=x86 x64
ArchitecturesInstallIn64BitMode=x64 ia64

[Languages]
Name: "en"; MessagesFile: "compiler:Default.isl"
Name: "de"; MessagesFile: "compiler:Languages\German.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[Files]
Source: "..\bin\Release\FMS - System Watcher.exe"; DestDir: "{app}"
Source: "..\bin\Release\HelixEngine.dll"; DestDir: "{app}"
Source: "..\bin\Release\Template.xml"; DestDir: "{app}"

[Icons]
Name: "{group}\FMS System Watcher"; Filename: "{app}\FMS - System Watcher.exe"
Name: "{group}\{cm:UninstallProgram,FMS System Watcher}"; Filename: "{uninstallexe}"
Name: "{commondesktop}\FMS System Watcher"; Filename: "{app}\FMS - System Watcher.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\FMS - System Watcher.exe"; Description: "{cm:LaunchProgram,FMS System Watcher}"; Flags: nowait postinstall skipifsilent

[Code]
function InitializeSetup(): Boolean;
begin
	//init windows version
	initwinversion();
    
	msi31('3.1');
	
	//install .netfx 3.5 sp1
	dotnetfx35sp1();
	
	Result := true;
end;


