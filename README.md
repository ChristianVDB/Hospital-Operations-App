# Hospital-Operations-App

## Visual Studio (Windows) — Run & Debug (F5)

Steps to open, run, and debug the project in Visual Studio:

1. Install Visual Studio 2022 or later with the ".NET desktop development" workload (for desktop apps) or the ".NET SDK" components for console/web projects.
2. Install .NET 10 SDK: https://dotnet.microsoft.com/download/dotnet/10.0
3. From Visual Studio:
   - File → Open → Project/Solution and select Hospital-Operations-App.sln
   - Select the startup project `Hospital-Operations-App` in Solution Explorer (right-click → "Set as Startup Project" if needed)
   - Choose the run profile "Hospital-Operations-App" from the debug target dropdown (Properties/launchSettings.json provides this profile)
   - Press F5 to start debugging, or Ctrl+F5 to run without the debugger.
4. If you need environment-specific settings, edit Properties/launchSettings.json or use Visual Studio's Debug > open launchProfiles UI.

Notes

- This project targets .NET 10 (net10.0). Ensure Visual Studio uses the .NET 10 SDK in Tools → Options → .NET Core.
- If the app is a desktop application (WPF/WinForms), run on Windows; Visual Studio on Windows is required.
- CI (GitHub Actions) builds and publishes artifacts on the main branch; see .github/workflows/dotnet.yml.

