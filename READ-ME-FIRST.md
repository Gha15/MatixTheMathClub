# What was broken, and what I fixed

When you merged the WPF project and the Electron project into one folder,
the WPF side kept pointing at an `assets\` folder that no longer exists.
That is why Visual Studio failed / you got a loading error.

The web app files now live in **one shared `app/` folder**, and BOTH the
Windows (WPF) build and the Electron build read from it. Edit `app/app.html`
once and both versions update.

## Changes made

| File | Change |
|---|---|
| `MatixMathClub.csproj` | `assets\app.html` -> `app\app.html`, added `app\download.html`, app icon now `build\icon.ico`, and `dist/` + `node_modules/` are excluded so MSBuild ignores the Electron output |
| `MainWindow.xaml` | Window icon + splash image now use `build/icon.ico` |
| `MainWindow.xaml.cs` | Looks for `app.html` in the `app` folder |
| `app/app.html` | Added the **Download app** section to the sidebar menu |

## Folder layout

```
MatixMathClub/
  app/            <- shared web app (app.html, download.html, logo.svg)
  build/          <- icon.ico, icon.png
  main.js, preload.js, package.json   <- Electron (installers)
  *.xaml, *.cs, *.csproj              <- WPF (Visual Studio)
```

## Run the Windows app (Visual Studio)

1. Open `MatixMathClub.slnx`.
2. Press **F5**. NuGet restores WebView2 automatically.

## Build the installers (Electron)

```bash
npm install
npm run dist:win   # -> dist/matix-installer.exe
npm run dist:mac   # -> dist/matix-installer.dmg   (must run ON a Mac)
```

## IMPORTANT - making the download buttons actually work

Right now the download buttons say **"Coming soon"** and download nothing.
That is on purpose: the installer files do not exist on the internet yet.

To turn them on:

1. Build the installers with the commands above.
2. Upload `matix-installer.exe` and `matix-installer.dmg` to a **GitHub Release**
   (free, no size limit problems - the files are 80-150 MB each, too big for
   most normal web hosting).
3. Copy the two download links, then paste them in **two places**:

   - `app/app.html` - find `var DOWNLOADS=` (in the download section)
   - `app/download.html` - find `DOWNLOADS` at the top of the `<script>`

   ```js
   var DOWNLOADS={
     version:'1.0.0',
     win:'https://github.com/YOURNAME/matix/releases/download/v1.0.0/matix-installer.exe',
     mac:'https://github.com/YOURNAME/matix/releases/download/v1.0.0/matix-installer.dmg'
   };
   ```

As soon as those links are filled in, the buttons turn into real download
buttons and the "Not published yet" notice disappears by itself.
