# Building matix-installer.exe and the macOS installer

This folder is a complete Electron project. It wraps `app/app.html` in a real
desktop app window and builds installers for Windows and macOS.

---

## One-time setup (both platforms)

1. Install **Node.js LTS** from https://nodejs.org (this gives you `npm`).
2. Open a terminal **in this folder** and run:

   ```
   npm install
   ```

3. Try the app before building an installer:

   ```
   npm start
   ```

---

## Windows -> matix-installer.exe

Run this **on a Windows PC**:

```
npm run dist:win
```

Output: `dist/matix-installer.exe`

It is a normal wizard installer: the user picks a folder, and it creates a
Desktop shortcut and a Start Menu entry with the Matix logo. Unlike the WPF
version, **WebView2 is not required** — Electron bundles its own browser engine.

---

## macOS -> matix-installer.dmg

Run this **on a Mac** (Apple will not let you build a Mac app on Windows):

```
npm run dist:mac
```

Output: `dist/matix-installer.dmg`

It opens a window where the user drags **Matix** into **Applications**. The build
is *universal*, so it runs on both Apple Silicon (M1/M2/M3) and older Intel Macs.

### Important note about macOS warnings

Because the app is not signed with a paid Apple Developer account ($99/year),
the first launch shows *"Matix cannot be opened because it is from an
unidentified developer."*

Tell your club members to **right-click the app -> Open -> Open**. They only have
to do this once. To remove the warning permanently you would need to sign and
notarize the app with an Apple Developer account.

Windows may also show a blue "Windows protected your PC" SmartScreen box; click
**More info -> Run anyway**. That disappears once the installer builds a
reputation or if you buy a code-signing certificate.

---

## Can I build both on one machine?

- On a **Mac** you can build the Mac version, and the Windows version too if you
  install Wine.
- On **Windows** you can only build the Windows version.
- The easy free option is **GitHub Actions**, which can build both at once on
  Microsoft's servers.

---

## Updating the app later

When you change the site, just replace `app/app.html` with the new file and run
the build command again. Bump `"version"` in `package.json` each time so people
can tell the releases apart.

---

## What each file does

| File | Purpose |
|---|---|
| `main.js` | Creates the app window (frameless, so Matix draws its own purple bar) |
| `preload.js` | Safe bridge that lets the page minimize/maximize/close the window |
| `package.json` | Dependencies + all installer settings |
| `app/app.html` | The actual Matix site |
| `build/icon.ico` | Windows icon (made from logo.svg) |
| `build/icon.png` | macOS/Linux icon (made from logo.svg) |
