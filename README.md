### What is this?
An assignment to create a Blood Establishment Computer Software in an Intro to Biomedical Software course.

### Technologies used
[![Technologies used](https://skillicons.dev/icons?i=visualstudio,dotnet,cs)](https://skillicons.dev)

Created using Windows Presentation Foundation, with XAML for front-end and C# for the code-behind logic.

### Usage instructions
After downloading `SCE24-BioMedSW-Blood_Establishment-WPF.zip`, extract it anywhere and run `SCE24-BioMedSW-Blood_Establishment-WPF.exe`, the program is self contained so you don't need to install anything. In order for the program's data to persist on the machine, a file named `donations.xml` is created in `%localappdata%` (`C:\Users\user\AppData\Local\donations.xml`).

### Build instructions
After cloning the repository, open the solution/project with Visual Studio. Open the terminal and run `dotnet publish -c Release --self-contained -p:PublishSingleFile=true`. The build files should be in `<project directory>\bin\Release\net8.0-windows\win-x64\publish`, .pdb file is optional.

### Screenshots
![image](https://github.com/Danny-Talor/SCE24-BioMedSW-Blood_Establishment-WPF/assets/93152770/2d17d20b-50de-4666-a4cf-48dedb7f9377)
![image](https://github.com/Danny-Talor/SCE24-BioMedSW-Blood_Establishment-WPF/assets/93152770/ed6e189f-6f1b-403c-a7cf-9511173c97f4)
![image](https://github.com/Danny-Talor/SCE24-BioMedSW-Blood_Establishment-WPF/assets/93152770/252e8e6d-21d3-4904-a325-621b6890f9a8)
