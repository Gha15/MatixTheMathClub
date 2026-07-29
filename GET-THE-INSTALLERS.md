# How to get matix-installer.exe and matix-installer.dmg

There are two ways. Pick ONE.

---

## Way 1: GitHub builds both for you (recommended)

Best option, especially for the Mac version - **you don't need to own a Mac**.
GitHub lends you a real Windows PC and a real Mac for free.

1. Push this folder to a GitHub repo (you already have git set up here).
2. On GitHub, open the **Actions** tab.
3. Click **Build installers** on the left, then the **Run workflow** button.
4. Wait about 10 minutes.
5. Open the finished run and scroll to the bottom. Under **Artifacts**
   you'll find:
   - `matix-installer-windows` -> the .exe
   - `matix-installer-mac` -> the .dmg

Download them and share with your members. Done.

### Tip: automatic releases

If you push a tag like `v1.0.0`, the workflow also creates a **GitHub Release**
with both files attached and gives you permanent download links - exactly the
links you paste into `DOWNLOADS` in `app.html`.

```bash
git tag v1.0.0
git push origin v1.0.0
```

---

## Way 2: Build the .exe yourself on your own PC

Only gives you the Windows version.

1. Install the **.NET 8 SDK** and **Inno Setup 6** (links in INSTALLER-README.md)
2. Double-click **build-installer.bat**
3. Find your file at `dist\matix-installer.exe`

---

## Why can't the .dmg be built on Windows?

Apple only allows Mac apps to be packaged on macOS. There is no legal way
around it. That's exactly why Way 1 exists - GitHub's Mac does it for you.

Also worth knowing: the Mac version is the **Electron** build, not the C# one.
C# WPF only runs on Windows, so the Mac app uses `main.js` + `app/app.html`.
Both versions load the identical `app/app.html`, so they look and behave the
same.

---

## Both apps are unsigned

- **Windows** shows "Windows protected your PC" -> More info -> Run anyway
- **Mac** shows "Matix cannot be opened" -> right-click the app -> Open

This is normal for apps without a paid certificate ($200-400/year for Windows,
$99/year for Apple). Warn your members first so they don't think it's a virus.
