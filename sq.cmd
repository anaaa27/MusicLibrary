@echo off
rem SharpQuest helper for Windows. Usage: sq test, sq update, sq upload, sq help
rem Works with a .NET 9 or .NET 10 SDK. The first run builds sq itself (10-20 s).
rem MANAGED FILE: `sq update` and CI replace it with the official copy.

rem sq is built from tools\sq\sq.csproj. A repository made in session 1 never received that file,
rem and a fresh clone lacks it if it was never committed. Put it back instead of failing.
if exist "%~dp0tools\sq\sq.csproj" goto run
mkdir "%~dp0tools\sq" 2>nul
> "%~dp0tools\sq\sq.csproj" echo ^<Project Sdk="Microsoft.NET.Sdk"^>
>>"%~dp0tools\sq\sq.csproj" echo   ^<PropertyGroup^>
>>"%~dp0tools\sq\sq.csproj" echo     ^<OutputType^>Exe^</OutputType^>
>>"%~dp0tools\sq\sq.csproj" echo     ^<AssemblyName^>sq^</AssemblyName^>
>>"%~dp0tools\sq\sq.csproj" echo     ^<RootNamespace^>SharpQuest.Helper^</RootNamespace^>
>>"%~dp0tools\sq\sq.csproj" echo     ^<EnableDefaultCompileItems^>false^</EnableDefaultCompileItems^>
>>"%~dp0tools\sq\sq.csproj" echo     ^<IsPackable^>false^</IsPackable^>
>>"%~dp0tools\sq\sq.csproj" echo   ^</PropertyGroup^>
>>"%~dp0tools\sq\sq.csproj" echo   ^<ItemGroup^>
>>"%~dp0tools\sq\sq.csproj" echo     ^<Compile Include="../../sq.cs" /^>
>>"%~dp0tools\sq\sq.csproj" echo   ^</ItemGroup^>
>>"%~dp0tools\sq\sq.csproj" echo ^</Project^>
echo sq: tools\sq\sq.csproj was missing, so sq put it back.
echo     Save it with your work: git add tools, then commit and push.

:run
rem The exit on the same line stops cmd from reading this file again after sq update replaces it.
dotnet run --project "%~dp0tools\sq" -- %* & exit /b
