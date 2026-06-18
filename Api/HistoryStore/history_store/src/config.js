const path = require('path');
const fs = require('fs');

function findRepoRoot() {
  let dir = __dirname;
  while (true) {
    if (
      fs.existsSync(path.join(dir, 'docker-compose.yml')) ||
      fs.existsSync(path.join(dir, '.env'))
    ) {
      return dir;
    }
    const parent = path.dirname(dir);
    if (parent === dir) break;
    dir = parent;
  }
  return process.cwd();
}

const repoRoot = findRepoRoot();

// Load repo-root .env for local development (no-op if vars already set)
require('dotenv').config({ path: path.join(repoRoot, '.env') });

const rawKeyPath = process.env['JwtOptions__PublicKeyPath'] ?? 'keys/jwt-public.pem';
const resolvedKeyPath = path.isAbsolute(rawKeyPath)
  ? rawKeyPath
  : path.resolve(repoRoot, rawKeyPath);

const config = {
  jwt: {
    publicKey: fs.readFileSync(resolvedKeyPath, 'utf8'),
    issuer: process.env['JwtOptions__Issuer'] ?? 'cashflow-auth',
    audience: process.env['JwtOptions__Audience'] ?? 'cashflow-api',
  },
  api: {
    port: parseInt(process.env['PORT'] ?? '4000', 10),
  },
};

module.exports = { config };
