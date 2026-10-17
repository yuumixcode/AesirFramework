@echo off
setlocal
cd /d "%~dp0\.."

set "GIT_BASH="
if exist "%ProgramFiles%\Git\bin\bash.exe" set "GIT_BASH=%ProgramFiles%\Git\bin\bash.exe"
if not defined GIT_BASH if exist "%ProgramFiles(x86)%\Git\bin\bash.exe" set "GIT_BASH=%ProgramFiles(x86)%\Git\bin\bash.exe"

if defined GIT_BASH goto run_git_bash

where bash >nul 2>&1
if errorlevel 1 goto no_bash
goto run_bash

:run_git_bash
"%GIT_BASH%" scripts/validate-meta-guids.sh
exit /b %ERRORLEVEL%

:run_bash
bash scripts/validate-meta-guids.sh
exit /b %ERRORLEVEL%

:no_bash
echo ERROR: bash not found. Install Git for Windows or add bash.exe to PATH.
exit /b 1
