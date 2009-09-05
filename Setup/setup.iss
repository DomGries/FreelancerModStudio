#include "scripts\products.iss"

#include "scripts\products\winversion.iss"
#include "scripts\products\fileversion.iss"

#include "scripts\products\ngen.iss"
//#include "scripts\products\iis.iss"

#include "scripts\products\kb835732.iss"
//#include "scripts\products\kb886903.iss"
//#include "scripts\products\kb928366.iss"

#include "scripts\products\msi20.iss"
#include "scripts\products\msi31.iss"
#include "scripts\products\ie6.iss"

//#include "scripts\products\dotnetfx11.iss"
//#include "scripts\products\dotnetfx11lp.iss"
//#include "scripts\products\dotnetfx11sp1.iss"

#include "scripts\products\dotnetfx20.iss"
#include "scripts\products\dotnetfx20lp.iss"
#include "scripts\products\dotnetfx20sp1.iss"
#include "scripts\products\dotnetfx20sp1lp.iss"
#include "scripts\products\dotnetfx20sp2.iss"
#include "scripts\products\dotnetfx20sp2lp.iss"

//#include "scripts\products\dotnetfx35.iss"
//#include "scripts\products\dotnetfx35lp.iss"
//#include "scripts\products\dotnetfx35sp1.iss"
//#include "scripts\products\dotnetfx35sp1lp.iss"

//#include "scripts\products\mdac28.iss"
//#include "scripts\products\jet4sp8.iss"
//#include "scripts\products\sql2005express.iss"

[CustomMessages]
win2000sp3_title=Windows 2000 Service Pack 3
winxpsp2_title=Windows XP Service Pack 2


[Setup]
AppName=Freelancer Mod Studio
AppVersion=0.8
AppVerName=Freelancer Mod Studio 0.8
AppCopyright=Copyright © stfx 2009
VersionInfoVersion=0.8
VersionInfoCompany=stfx
AppPublisher=stfx
AppPublisherURL=http://code.google.com/p/freelancermodstudio
AppSupportURL=http://groups.google.com/group/freelancer-mod-studio/topics
AppUpdatesURL=http://code.google.com/p/freelancermodstudio/downloads/list
OutputBaseFilename=FreelancerModStudio-0.8
DefaultGroupName=Freelancer Mod Studio
DefaultDirName={pf}\Freelancer Mod Studio
UninstallDisplayIcon={app}\FreelancerModStudio.exe
UninstallDisplayName=Freelancer Mod Studio
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

MinVersion=4.1,5.0
PrivilegesRequired=admin
ArchitecturesAllowed=x86 x64
ArchitecturesInstallIn64BitMode=x64 ia64

[Languages]
Name: "en"; MessagesFile: "compiler:Default.isl"
Name: "de"; MessagesFile: "compiler:Languages\German.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "..\FreelancerModStudio\bin\Release\FreelancerModStudio.exe"; DestDir: "{app}"
Source: "..\FreelancerModStudio\bin\Release\FreelancerModStudio.XmlSerializers.dll"; DestDir: "{app}"
Source: "..\FreelancerModStudio\bin\Release\ObjectListView.dll"; DestDir: "{app}"
Source: "..\FreelancerModStudio\bin\Release\WeifenLuo.WinFormsUI.Docking.dll"; DestDir: "{app}"
Source: "..\FreelancerModStudio\bin\Release\de\FreelancerModStudio.resources.dll"; DestDir: "{app}\de"
Source: "..\FreelancerModStudio\bin\Release\Template.xml"; DestDir: "{app}"
Source: "src\Settings_en.xml"; DestName: "FreelancerModStudio.xml"; DestDir: "{app}"; Languages: en
Source: "src\Settings_de.xml"; DestName: "FreelancerModStudio.xml"; DestDir: "{app}"; Languages: de

[Icons]
Name: "{group}\Freelancer Mod Studio"; Filename: "{app}\FreelancerModStudio"
Name: "{group}\{cm:UninstallProgram,Freelancer Mod Studio}"; Filename: "{uninstallexe}"
Name: "{commondesktop}\Freelancer Mod Studio"; Filename: "{app}\FreelancerModStudio.exe"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\FreelancerModStudio"; Filename: "{app}\FreelancerModStudio.exe"; Tasks: quicklaunchicon

[Run]
Filename: "{app}\FreelancerModStudio.exe"; Description: "{cm:LaunchProgram,FreelancerModStudio}"; Flags: nowait postinstall skipifsilent

[Code]
function InitializeSetup(): Boolean;
begin
	//init windows version
	initwinversion();
	
	//check if dotnetfx20 can be installed on this OS
	if not minwinspversion(5, 0, 3) then begin
		MsgBox(FmtMessage(CustomMessage('depinstall_missing'), [CustomMessage('win2000sp3_title')]), mbError, MB_OK);
		exit;
	end;
	if not minwinspversion(5, 1, 2) then begin
		MsgBox(FmtMessage(CustomMessage('depinstall_missing'), [CustomMessage('winxpsp2_title')]), mbError, MB_OK);
		exit;
	end;
    
	//if (not iis()) then exit;
	
	msi20('2.0');
	msi31('3.0');
	ie6('5.0.2919');
	
	//dotnetfx11();
	//dotnetfx11lp();
	//dotnetfx11sp1();
	//kb886903(); //better use windows update
	//kb928366(); //better use windows update
	
	//install .netfx 2.0 sp2 if possible; if not sp1 if possible; if not .netfx 2.0
	if minwinversion(5, 1) then begin
		dotnetfx20sp2();
		dotnetfx20sp2lp();
	end else begin
		if minwinversion(5, 0) and minwinspversion(5, 0, 4) then begin
			kb835732();
			dotnetfx20sp1();
			dotnetfx20sp1lp();
		end else begin
			dotnetfx20();
			dotnetfx20lp();
		end;
	end;
	
	//dotnetfx35();
	//dotnetfx35lp();
	//dotnetfx35sp1();
	//dotnetfx35sp1lp();
	
	//mdac28('2.7');
	//jet4sp8('4.0.8015');
	//sql2005express();
	
	Result := true;
end;


