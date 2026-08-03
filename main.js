// Matix — Electron main process
// Creates a frameless window so the app can draw its own purple title bar.
const { app, BrowserWindow, ipcMain, shell, dialog } = require('electron');
const path = require('path');
const fs = require('fs');

let win = null;

function createWindow() {
  win = new BrowserWindow({
    width: 1280,
    height: 840,
    minWidth: 900,
    minHeight: 600,
    show: false,
    frame: false,              // we draw our own title bar in app.html
    backgroundColor: '#5b46c9',
    title: 'Matix — the Math Club',
    icon: path.join(__dirname, 'build', process.platform === 'win32' ? 'icon.ico' : 'icon.png'),
    webPreferences: {
      preload: path.join(__dirname, 'preload.js'),
      contextIsolation: true,
      nodeIntegration: false
    }
  });

  const page = path.join(__dirname, 'app', 'app.html');

  if (!fs.existsSync(page)) {
    dialog.showErrorBox('Matix could not start',
      'The app file is missing:\n\n' + page + '\n\nReinstall Matix to fix this.');
    app.quit();
    return;
  }

  win.loadFile(page);

  // Only show once painted — avoids an ugly white flash on launch
  win.once('ready-to-show', () => win.show());

  // Tell the page when the window is maximized so the button icon can change
  const sendMax = () => win.webContents.send('matix:maximized', win.isMaximized());
  win.on('maximize', sendMax);
  win.on('unmaximize', sendMax);

  // Open external links (and game popups) in the real browser, not a blank app window
  win.webContents.setWindowOpenHandler(({ url }) => {
    if (/^https?:/i.test(url)) shell.openExternal(url);
    return { action: 'deny' };
  });

  // Allow camera/microphone so Math Chat calling works without an extra native prompt
  win.webContents.session.setPermissionRequestHandler((webContents, permission, callback) => {
    callback(permission === 'media');
  });

  // Friendly message instead of a silent blank window
  win.webContents.on('render-process-gone', () => {
    dialog.showErrorBox('Matix stopped responding',
      'The app ran into a problem and needs to restart.');
  });

  win.on('closed', () => { win = null; });
}

// Window control buttons in the custom title bar
ipcMain.on('matix:minimize', () => win && win.minimize());
ipcMain.on('matix:maximize', () => {
  if (!win) return;
  win.isMaximized() ? win.unmaximize() : win.maximize();
});
ipcMain.on('matix:close', () => win && win.close());

// Single instance — focus the existing window instead of opening a second one
if (!app.requestSingleInstanceLock()) {
  app.quit();
} else {
  app.on('second-instance', () => {
    if (win) { if (win.isMinimized()) win.restore(); win.focus(); }
  });

  app.whenReady().then(createWindow);

  app.on('activate', () => {
    if (BrowserWindow.getAllWindows().length === 0) createWindow();
  });
}

app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') app.quit();
});
