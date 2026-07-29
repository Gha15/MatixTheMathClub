@echo off
REM ====================================================================
REM  Matix - one-click Windows installer builder
REM  Double-click this file. It does two things:
REM    1. Publishes the C# app into the 'publish' folder
REM    2. Compiles that into dist\matix-installer.exe
REM ====================================================================

cd /d "%~dp0"
title Building the Matix installer

echo.
echo ==========================================
echo   Matix installer builder
echo ==========================================
echo.

REM ---- 1. Check the .NET SDK is installed -----------------------------
where dotnet >nul 2>nul
if errorlevel 1 (
  echo [X] The .NET SDK was not found.
  echo.
  echo     Install the .NET 8 SDK ^(not just the runtime^) from:
  echo     https://dotnet.microsoft.com/download/dotnet/8.0
  echo.
  pause
  exit /b 1
)

REM ---- 2. Publish the app --------------------------------------------
echo [1/2] Building Matix... this can take a couple of minutes.
echo.

if exist "publish" rmdir /s /q "publish"

dotnet publish MatixMathClub.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false -o publish

if errorlevel 1 (
  echo.
  echo [X] The build failed. Scroll up to see the first red error.
  echo.
  pause
  exit /b 1
)

if not exist "publish\Matix.exe" (
  echo.
  echo [X] Build finished but Matix.exe is missing from the publish folder.
  echo.
  pause
  exit /b 1
)

REM ---- 3. Find Inno Setup ---------------------------------------------
set "ISCC="
if exist "%ProgramFiles(x86)%\Inno Setup 6\ISCC.exe" set "ISCC=%ProgramFiles(x86)%\Inno Setup 6\ISCC.exe"
if exist "%ProgramFiles%\Inno Setup 6\ISCC.exe"      set "ISCC=%ProgramFiles%\Inno Setup 6\ISCC.exe"

if not defined ISCC (
  echo.
  echo [X] Inno Setup 6 was not found.
  echo.
  echo     Download and install it ^(free^) from:
  echo     https://jrsoftware.org/isdl.php
  echo.
  echo     Then run this file again.
  echo.
  pause
  exit /b 1
)

REM ---- 4. Build the installer -----------------------------------------
echo.
echo [2/2] Packaging the installer...
echo.

"%ISCC%" "installer\Matix.iss"

if errorlevel 1 (
  echo.
  echo [X] Inno Setup reported a problem. Scroll up for details.
  echo.
  pause
  exit /b 1
)

echo.
echo ==========================================
echo   Done!
echo.
echo   Your installer is here:
echo   %cd%\dist\matix-installer.exe
echo ==========================================
echo.

if exist "dist" explorer "dist"
pause
