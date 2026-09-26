@echo off
cd /d "%~dp0"
py -3 export_localization.py %*
exit /b %errorlevel%
