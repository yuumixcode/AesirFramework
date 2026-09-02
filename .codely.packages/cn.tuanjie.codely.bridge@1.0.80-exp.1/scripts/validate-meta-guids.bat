@echo off
setlocal
cd /d "%~dp0\.."

set "GIT_BASH="
if exist "%ProgramFiles%\Git\bin\bash.exe" set "GIT_BASH=%ProgramFiles%\Git\bin\bash.exe"
if not defined GIT_BASH if exist "%ProgramFiles(x86)%\Git\bin\bash.exe" set "GIT_BASH=%ProgramFiles(x86)%\Git\bin\bash.exe"

if defined GIT_BASH (
  "%GIT_BASH%" scripts/validate-meta-guids.sh
  exit /b %ERRORLEVEL%
)

where bash >nul 2>&1
if %ERRORLEVEL% equ 0 (
  bash scripts/validate-meta-guids.sh
  exit /b %ERRORLEVEL%
)

echo ERROR: bash not found. Install Git for Windows or add bash.exe to PATH.
exit /b 1
