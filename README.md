### What is this?
An assignment to create a Blood Establishment Computer Software in an Intro to Biomedical Software course.

### Technologies used
[![Technologies used](https://skillicons.dev/icons?i=visualstudio,dotnet,cs)](https://skillicons.dev)

Created using Windows Presentation Foundation, with XAML for front-end and C# for the code-behind logic.

### Usage instructions
After downloading `SCE24-BioMedSW-Blood_Establishment-WPF.zip`, extract it anywhere and run `SCE24-BioMedSW-Blood_Establishment-WPF.exe`, the program is self contained so you don't need to install anything. In order for the program's data to persist on the machine, a file named `SCE24-BioMedSW-BECS-data.xml` is created in `%localappdata%` (`C:\Users\user\AppData\Local\SCE24-BioMedSW-BECS-data.xml`).

### Build instructions
After cloning the repository, open the solution/project with Visual Studio. Open the terminal and run `dotnet publish SCE24-BioMedSW-Blood_Establishment-WPF.csproj -c Release --self-contained -p:PublishSingleFile=true`. The build files should be in `<project directory>\bin\Release\net8.0-windows\win-x64\publish`, .pdb file is optional.

### Required packages For Build
`ClosedXML` - Installed using the NuGet Package Manager

### Screenshots
![image](https://github.com/user-attachments/assets/d4ca9c83-636d-4082-8ed2-078e4238aa99)




