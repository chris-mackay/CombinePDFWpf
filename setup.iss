[Setup]
AppId={{BB021374-4336-4E98-9002-4953BF28238F}
AppName=CombinePDFWpf
AppCopyright=Copyright © 2021 Christopher Mackay
AppVersion=1.0.0
VersionInfoVersion=1.0.0
AppVerName=CombinePDFWpf
AppPublisher=Christopher Mackay
AppPublisherURL=https://github.com/chris-mackay/CombinePDFWpf/
AppSupportURL=https://github.com/chris-mackay/CombinePDFWpf/
AppUpdatesURL=https://github.com/chris-mackay/CombinePDFWpf/releases/
DefaultDirName={pf}\Christopher Mackay\CombinePDFWpf
DefaultGroupName=CombinePDFWpf
OutputDir=C:\Programming\CombinePDFWpf
OutputBaseFilename=CombinePDFWpf-Setup
Compression=lzma
SolidCompression=yes
UninstallDisplayIcon={app}\CombinePDFWpf.exe
DisableDirPage=yes
LicenseFile=C:\Programming\CombinePDFWpf\LICENSE
PrivilegesRequired=admin

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}";

[Files]
Source: "C:\Programming\CombinePDFWpf\CombinePDFWpf\bin\x64\Release\CombinePDFWpf.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Programming\CombinePDFWpf\CombinePDFWpf\bin\x64\Release\Microsoft.WindowsAPICodePack.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Programming\CombinePDFWpf\CombinePDFWpf\bin\x64\Release\Microsoft.WindowsAPICodePack.Shell.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Programming\CombinePDFWpf\CombinePDFWpf\bin\x64\Release\PdfSharp.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Programming\CombinePDFWpf\icons\combine_pdf.ico"; DestDir: "{app}\Icons";

[Icons]
Name: "{group}\CombinePDFWpf"; Filename: "{app}\CombinePDFWpf.exe"
Name: "{commondesktop}\CombinePDFWpf"; Filename: "{app}\CombinePDFWpf.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\CombinePDFWpf.exe"; Description: "{cm:LaunchProgram,CombinePDFWpf}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: filesandordirs; Name: "{localappdata}\Chris Mackay"