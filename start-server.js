const { spawn } = require('child_process');
const readline = require('readline');

const rl = readline.createInterface({
  input: process.stdin,
  output: process.stdout
});

rl.question('Enter file path from root (Default: app/app.html): ', (userInput) => {
  rl.close();
  
  const filePath = userInput.trim() || 'app/app.html';
  
  console.log(`\nStarting live-server on port 2525...`);

  // "spawn" streams output in real-time so your terminal never freezes
  // Keeping the root directory at "." allows access to assets like ../build/
  const server = spawn('npx', [
    'live-server',
    '.', // Forces the server root to be the main project folder
    '--host=localhost',
    '--port=2525',
    `--open=${filePath}` // Tells the browser exactly where to land
  ], { 
    shell: true, 
    stdio: 'inherit' 
  });

  server.on('error', (err) => {
    console.error('Failed to start server:', err);
  });
});
