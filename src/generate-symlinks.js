const path = require("path");
const fs = require("fs");

const dirs = fs.readdirSync(__dirname).filter(function (file) {
	const entry = path.join(__dirname, file);
	return !file.startsWith('.') && fs.statSync(entry).isDirectory();
});

const sourceDirs = dirs.map((dir) => path.join(__dirname, dir));
const targetDirs = dirs.map((dir) => path.join(__dirname, "..", "..", dir));

for (let i = 0; i < sourceDirs.length; i++) {
  const sourceDir = sourceDirs[i];
  const targetDir = targetDirs[i];

  if (!fs.existsSync(targetDir)) {
    fs.symlinkSync(sourceDir, targetDir, "dir");
  }
}