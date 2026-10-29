@echo off
setlocal
set "DOTNET_ROOT=%~dp0dotnet"
"%DOTNET_ROOT%\dotnet.exe" "%~dp0MiniBible.App.dll"
if errorlevel 1 pause
