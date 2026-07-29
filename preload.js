// Matix — secure bridge between the page and the window.
// Exposes only these four things, nothing else.
const { contextBridge, ipcRenderer } = require('electron');

contextBridge.exposeInMainWorld('matixDesktop', {
  minimize: () => ipcRenderer.send('matix:minimize'),
  maximize: () => ipcRenderer.send('matix:maximize'),
  close:    () => ipcRenderer.send('matix:close'),
  onMaximizeChange: (cb) =>
    ipcRenderer.on('matix:maximized', (_e, isMax) => cb(isMax)),
  platform: process.platform
});
