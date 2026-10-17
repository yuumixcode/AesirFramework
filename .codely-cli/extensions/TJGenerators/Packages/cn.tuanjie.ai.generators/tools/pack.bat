@echo off
cd /d "%~dp0\.."
node tools\build-bundle.mjs --archive
