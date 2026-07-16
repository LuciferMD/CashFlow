'use strict';

const fs = require('fs');
const os = require('os');
const path = require('path');
const { generateKeyPairSync } = require('crypto');
const express = require('express');
const cookieParser = require('cookie-parser');
const jwt = require('jsonwebtoken');

const HISTORY_ROUTES = path.resolve(
  __dirname,
  '../../../Api/HistoryStore/history_store/src/routes/history',
);

const INTERNAL_API_KEY = 'test-internal-key';
const JWT_ISSUER = 'CashFlow.Auth';
const JWT_AUDIENCE = 'CashFlow';

let keyDirectory;

function setupTestEnv(mongoUri) {
  const { publicKey, privateKey } = generateKeyPairSync('rsa', { modulusLength: 2048 });

  keyDirectory = fs.mkdtempSync(path.join(os.tmpdir(), 'history-store-test-keys-'));
  const publicKeyPath = path.join(keyDirectory, 'jwt-public.pem');
  const privateKeyPem = privateKey.export({ type: 'pkcs8', format: 'pem' });

  fs.writeFileSync(publicKeyPath, publicKey.export({ type: 'spki', format: 'pem' }));

  process.env.MONGODB_URI = mongoUri;
  process.env.INTERNAL_API_KEY = INTERNAL_API_KEY;
  process.env.JWT_PUBLIC_KEY_PATH = publicKeyPath;
  process.env.JwtOptions__PublicKeyPath = publicKeyPath;
  process.env.JWT_ISSUER = JWT_ISSUER;
  process.env.JwtOptions__Issuer = JWT_ISSUER;
  process.env.JWT_AUDIENCE = JWT_AUDIENCE;
  process.env.JwtOptions__Audience = JWT_AUDIENCE;
  delete process.env.KAFKA_BROKERS;

  clearModuleCache();

  return { privateKeyPem, publicKeyPath };
}

function clearModuleCache() {
  Object.keys(require.cache).forEach((key) => {
    if (key.includes('Api\\HistoryStore') || key.includes('Api/HistoryStore')) {
      delete require.cache[key];
    }
  });
}

function cleanupTestEnv() {
  if (keyDirectory && fs.existsSync(keyDirectory)) {
    fs.rmSync(keyDirectory, { recursive: true, force: true });
    keyDirectory = undefined;
  }
}

function buildApp() {
  const app = express();
  app.use(express.json());
  app.use(cookieParser());
  app.use(require(HISTORY_ROUTES));
  return app;
}

function signTestToken(privateKeyPem, overrides = {}) {
  return jwt.sign(
    { userId: 'history-test-user', ...overrides },
    privateKeyPem,
    {
      algorithm: 'RS256',
      issuer: JWT_ISSUER,
      audience: JWT_AUDIENCE,
      expiresIn: '1h',
    },
  );
}

function validDevice(overrides = {}) {
  return {
    type: 'sensor',
    name: 'Kitchen',
    payload: { co2: 400, pm25: 10, humidity: 60, energy: 1.5 },
    ...overrides,
  };
}

function validSnapshotPayload(overrides = {}) {
  return {
    capturedAt: '2024-06-15T10:00:00.000Z',
    devices: [validDevice()],
    ...overrides,
  };
}

module.exports = {
  INTERNAL_API_KEY,
  buildApp,
  cleanupTestEnv,
  setupTestEnv,
  signTestToken,
  validDevice,
  validSnapshotPayload,
};
