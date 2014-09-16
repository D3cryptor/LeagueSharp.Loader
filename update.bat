@echo off
ping 127.0.0.1 -n 2 
setlocal
for %%i in ("%~dp0..") do set "folder=%%~fi"
xcopy "%~dp0*" "%folder%\" /S /Y
start "%folder%\leaguesharp.loader.exe"