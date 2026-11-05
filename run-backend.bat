@echo off
set "PATH=%LOCALAPPDATA%\Microsoft\dotnet;%PATH%"
cd /d "%~dp0src\backend\DndJornada.Api"
dotnet run

