[CustomMessages]
en.ngen_install=Creating .NET assembly image...
de.ngen_install=Erstelle .NET Anwendungs-Image...

en.ngen_uninstall=Deleting .NET assembly image...
de.ngen_uninstall=Entferne .NET Anwendungs-Image...

en.ngen_uninstall_prev=Deleting outdated .NET assembly image...
de.ngen_uninstall_prev=Entferne altes .NET Anwendungs-Image...

en.ngen_description=Install .NET assembly image (better startup time but bigger installation size)
de.ngen_description=.NET Anwendungs-Image installieren (schnellerer Programmstart aber größerer Speicherplatzbedarf)

[Tasks]
Name: "ngen"; Description: "{cm:ngen_description}"

[Run]
Filename: "{dotnet2032}\ngen.exe"; Parameters: "uninstall ""{app}\FreelancerModStudio.exe"""; StatusMsg: "{cm:ngen_uninstall_prev}"; Flags: skipifdoesntexist; Check: PreviousInstall and not Is64BitInstallMode
Filename: "{dotnet2064}\ngen.exe"; Parameters: "uninstall ""{app}\FreelancerModStudio.exe"""; StatusMsg: "{cm:ngen_uninstall_prev}"; Flags: skipifdoesntexist; Check: PreviousInstall and Is64BitInstallMode

Filename: "{dotnet2032}\ngen.exe"; Parameters: "install ""{app}\FreelancerModStudio.exe"""; StatusMsg: "{cm:ngen_install}"; Flags: skipifdoesntexist; Tasks: ngen; Check: not Is64BitInstallMode
Filename: "{dotnet2064}\ngen.exe"; Parameters: "install ""{app}\FreelancerModStudio.exe"""; StatusMsg: "{cm:ngen_install}"; Flags: skipifdoesntexist; Tasks: ngen; Check: Is64BitInstallMode

[UninstallRun]
Filename: "{dotnet2032}\ngen.exe"; Parameters: "uninstall ""{app}\FreelancerModStudio.exe"""; StatusMsg: "{cm:ngen_uninstall}"; Flags: skipifdoesntexist; Tasks: ngen; Check: not Is64BitInstallMode
Filename: "{dotnet2064}\ngen.exe"; Parameters: "uninstall ""{app}\FreelancerModStudio.exe"""; StatusMsg: "{cm:ngen_uninstall}"; Flags: skipifdoesntexist; Tasks: ngen; Check: Is64BitInstallMode

[Code]
function PreviousInstall: Boolean;
begin
	Result := WizardForm.PrevAppDir <> '';
end;