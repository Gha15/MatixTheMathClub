# Making matix-installer.exe (the C# / WPF version)

This builds a real Windows installer for the C# app. Members double-click
`matix-installer.exe`, click through a normal setup wizard, and get Matix in
their Start menu with a desktop shortcut.

## You only need to do this once

Install these two free things on your Windows PC:

1. **.NET 8 SDK** - https://dotnet.microsoft.com/download/dotnet/8.0
   Make sure it's the **SDK**, not just the "Runtime".
2. **Inno Setup 6** - https://jrsoftware.org/isdl.php
   Click through with all the default options.

## Then, every time you want a new installer

Double-click **`build-installer.bat`**.

That's it. It builds the app, packages it, and opens the folder containing:

```
dist\matix-installer.exe
```

The first run takes a few minutes. Later runs are faster. If something is
missing, the script stops and tells you exactly what to install.

## What members get

- A normal setup wizard with the Matix logo
- Start menu entry + optional desktop shortcut
- Proper entry in "Add or remove programs" so it uninstalls cleanly
- **No admin password needed** - it installs just for that user
- **No need to install .NET** - the runtime is built into the app

That last point is why the installer is around 60-70 MB. It's worth it:
otherwise every member hits a "you need to install .NET" error and gives up.

## The SmartScreen warning

The first person to run it will see a blue box:

> Windows protected your PC

Click **More info** then **Run anyway**. This is normal and does not mean
anything is wrong. Windows shows it for every program that isn't signed with
a paid code-signing certificate (roughly $200-400 a year). The warning fades
away on its own as more people install it.

Tell your members about this up front, or they'll assume Matix is a virus.

## Optional: PCs without WebView2

Matix draws its interface using WebView2. Windows 11 and almost all updated
Windows 10 PCs already have it, so you can usually ignore this.

To be safe on older machines, download the **Evergreen Bootstrapper** from
https://developer.microsoft.com/microsoft-edge/webview2/ and save it as:

```
installer\MicrosoftEdgeWebview2Setup.exe
```

The build script picks it up automatically and installs it only on PCs that
need it. If you skip this, nothing breaks - the installer just builds without it.

## Updating the app later

The app itself is `app\app.html`. Edit it, run `build-installer.bat` again,
and share the new `matix-installer.exe`. Bump `MyAppVersion` at the top of
`installer\Matix.iss` so it's clear which version people have.

Installing a newer version over an older one works and keeps everyone's
sign-in state.

## The Electron files

`main.js`, `preload.js` and `package.json` are from the other approach we
tried. They're not used by the C# build and are ignored by the project. You
can delete them if you want a tidier folder.
