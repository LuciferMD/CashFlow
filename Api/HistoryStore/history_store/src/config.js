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

require('dotenv').config({ path: path.join(repoRoot, '.env') });

const rawKeyPath =
  process.env['JwtOptions__PublicKeyPath'] ??
  process.env['JWT_PUBLIC_KEY_PATH'] ??
  'keys/jwt-public.pem';
const resolvedKeyPath = path.isAbsolute(rawKeyPath)
  ? rawKeyPath
  : path.resolve(repoRoot, rawKeyPath);

const config = {
  jwt: {
    publicKey: fs.readFileSync(resolvedKeyPath, 'utf8'),
    issuer:
      process.env['JwtOptions__Issuer'] ??
      process.env['JWT_ISSUER'] ??
      'CashFlow.Auth',
    audience:
      process.env['JwtOptions__Audience'] ??
      process.env['JWT_AUDIENCE'] ??
      'CashFlow',
  },
  mongodb: {
    uri:
      process.env['MONGODB_URI'] ??
      'mongodb://localhost:27017/history_store',
  },
  internal: {
    apiKey: process.env['INTERNAL_API_KEY'] ?? '',
  },
  api: {
    port: parseInt(process.env['PORT'] ?? '4000', 10),
  },
  kafka: {
    enabled: Boolean(process.env['KAFKA_BROKERS']),
    brokers: (process.env['KAFKA_BROKERS'] ?? '')
      .split(',')
      .map((broker) => broker.trim())
      .filter(Boolean),
    topicIotSnapshots:
      process.env['KAFKA_TOPIC_IOT_SNAPSHOTS'] ?? 'iot.snapshots',
    groupId: process.env['KAFKA_GROUP_ID'] ?? 'history-store',
  },
};

module.exports = { config };
