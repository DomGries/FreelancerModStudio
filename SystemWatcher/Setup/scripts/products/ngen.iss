[CustomMessages]
en.ngen_install=Creating native .NET assembly image...
de.ngen_install=Erstelle systemeigenes .NET Anwendungs-Image...

en.ngen_uninstall=Deleting native .NET assembly image...
de.ngen_uninstall=Entferne systemeigenes .NET Anwendungs-Image...

[Run]
Filename: "{dotnet2032}\ngen.exe"; Parameters: "install {app}\FreelancerModStudio.exe /silent"; StatusMsg: "{cm:ngen_install}"; Flags: skipifdoesntexist; Check: not PreviousInstall and not Is64BitInstallMode

Filename: "{dotnet2064}\ngen.exe"; Parameters: "install {app}\FreelancerModStudio.exe /silent"; StatusMsg: "{cm:ngen_install}"; Flags: skipifdoesntexist; Check: not PreviousInstall and Is64BitInstallMode

Filename: "{dotnet2032}\ngen.exe"; Parameters: "update {app}\FreelancerModStudio.exe /silent"; StatusMsg: "{cm:ngen_install}"; Flags: skipifdoesntexist; Check: PreviousInstall and not Is64BitInstallMode

Filename: "{dotnet2064}\ngen.exe"; Parameters: "update {app}\FreelancerModStudio.exe /silent"; StatusMsg: "{cm:ngen_install}"; Flags: skipifdoesntexist; Check: PreviousInstall and Is64BitInstallMode

[UninstallRun]
Filename: "{dotnet2032}\ngen.exe"; Parameters: "uninstall {app}\FreelancerModStudio.exe /silent"; StatusMsg: "{cm:ngen_uninstall}"; Flags: skipifdoesntexist ; Check: not Is64BitInstallMode

Filename: "{dotnet2064}\ngen.exe"; Parameters: "uninstall {app}\FreelancerModStudio.exe /silent"; StatusMsg: "{cm:ngen_uninstall}"; Flags: skipifdoesntexist ; Check: Is64BitInstallMode

[Code]
function PreviousInstall: Boolean;
begin
	Result := WizardForm.PrevAppDir <> '';
end;