@echo off
cls

SET BUILD_DIR=build
SET TOOLS_DIR=%BUILD_DIR%\tools
SET NUGET_PATH=%TOOLS_DIR%\nuget.exe

IF NOT EXIST %TOOLS_DIR%\ (
  mkdir %TOOLS_DIR%
)

IF NOT EXIST %NUGET_PATH% (
  echo Downloading NuGet.exe ...
  powershell -Command "Invoke-WebRequest https://dist.nuget.org/win-x86-commandline/v4.3.0/nuget.exe -OutFile %NUGET_PATH%"
)

IF NOT EXIST "%TOOLS_DIR%\FAKE.Core\tools\Fake.exe" (
  %NUGET_PATH% install "FAKE.Core" -Version 4.63.2 -OutputDirectory %TOOLS_DIR% -ExcludeVersion 
)

echo Running FAKE Build...
%TOOLS_DIR%\FAKE.Core\tools\Fake.exe build.fsx %* -BuildDir=%BUILD_DIR%
