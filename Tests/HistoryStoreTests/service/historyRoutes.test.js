'use strict';

/**
 * Service-level tests for the HistoryStore Express API.
 * Uses supertest + mongodb-memory-server (in-process MongoDB).
 * Only the HistoryStore service is exercised — no Gateway, Auth, or Notification.
 */

jest.setTimeout(60_000);

const { MongoMemoryServer } = require('mongodb-memory-server');
const mongoose = require('mongoose');
const request = require('supertest');

const {
  INTERNAL_API_KEY,
  buildApp,
  cleanupTestEnv,
  setupTestEnv,
  signTestToken,
  validDevice,
  validSnapshotPayload,
} = require('./serviceTestHelpers');

let mongoServer;
let privateKeyPem;
let app;
let Snapshot;

beforeAll(async () => {
  mongoServer = await MongoMemoryServer.create();
  ({ privateKeyPem } = setupTestEnv(mongoServer.getUri()));
  await mongoose.connect(mongoServer.getUri());
  ({ Snapshot } = require('../../../Api/HistoryStore/history_store/src/db/snapshot.model'));
  app = buildApp();
}, 60_000);

afterAll(async () => {
  await mongoose.disconnect();
  if (mongoServer) {
    await mongoServer.stop();
  }
  cleanupTestEnv();
}, 30_000);

afterEach(async () => {
  await Snapshot.deleteMany({});
});

describe('HistoryStore service tests', () => {
  describe('POST /snapshots', () => {
    it('returns 401 when x-api-key is missing', async () => {
      const res = await request(app)
        .post('/snapshots')
        .send(validSnapshotPayload());

      expect(res.status).toBe(401);
      expect(res.body.error).toBe('Invalid API key');
    });

    it('returns 201 and persists a valid snapshot', async () => {
      const payload = validSnapshotPayload();

      const res = await request(app)
        .post('/snapshots')
        .set('x-api-key', INTERNAL_API_KEY)
        .send(payload);

      expect(res.status).toBe(201);
      expect(res.body.devices).toHaveLength(1);
      expect(res.body.devices[0].name).toBe('Kitchen');

      const stored = await Snapshot.find().lean();
      expect(stored).toHaveLength(1);
      expect(stored[0].devices[0].name).toBe('Kitchen');
    });
  });

  describe('GET /history/latest', () => {
    it('returns 401 without a valid JWT', async () => {
      const res = await request(app).get('/history/latest');

      expect(res.status).toBe(401);
      expect(res.body.error).toBe('Unauthorized');
    });

    it('returns the most recent snapshot ordered by capturedAt desc', async () => {
      await Snapshot.create({
        capturedAt: new Date('2024-01-01T00:00:00.000Z'),
        devices: [validDevice({ name: 'Older' })],
      });
      await Snapshot.create({
        capturedAt: new Date('2024-06-15T12:00:00.000Z'),
        devices: [validDevice({ name: 'Newest' })],
      });

      const token = signTestToken(privateKeyPem);
      const res = await request(app)
        .get('/history/latest')
        .set('Authorization', `Bearer ${token}`);

      expect(res.status).toBe(200);
      expect(res.body.devices[0].name).toBe('Newest');
    });
  });

  describe('GET /history', () => {
    it('returns paginated history for an authenticated request', async () => {
      await Snapshot.create({
        capturedAt: new Date('2024-06-10T10:00:00.000Z'),
        devices: [validDevice({ name: 'Kitchen' })],
      });
      await Snapshot.create({
        capturedAt: new Date('2024-06-11T10:00:00.000Z'),
        devices: [validDevice({ name: 'Bedroom' })],
      });

      const token = signTestToken(privateKeyPem);
      const res = await request(app)
        .get('/history')
        .query({ device: 'Kitchen', limit: 10, skip: 0 })
        .set('Authorization', `Bearer ${token}`);

      expect(res.status).toBe(200);
      expect(res.body.total).toBe(1);
      expect(res.body.limit).toBe(10);
      expect(res.body.skip).toBe(0);
      expect(res.body.snapshots).toHaveLength(1);
      expect(res.body.snapshots[0].devices[0].name).toBe('Kitchen');
    });
  });
});
