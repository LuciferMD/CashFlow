'use strict';

const path = require('node:path');
const {
  MessageConsumerPact,
  synchronousBodyHandler,
  MatchersV3,
} = require('@pact-foundation/pact');

const PACTS_DIR = path.resolve(__dirname, '../../pacts');

const { saveSnapshotIfNotExistsForDay } = require(
  '../../../Api/HistoryStore/history_store/src/services/snapshotService',
);

jest.mock('../../../Api/HistoryStore/history_store/src/db/snapshot.model', () => ({
  Snapshot: {
    exists: jest.fn(),
    create: jest.fn(),
  },
}));

const { Snapshot } = require('../../../Api/HistoryStore/history_store/src/db/snapshot.model');

const { integer, string, like, eachLike } = MatchersV3;

let pactInitialized = false;

function ensureFreshPactFile() {
  if (pactInitialized) {
    return;
  }

  pactInitialized = true;
  const pactFile = path.join(PACTS_DIR, 'HistoryStore-Gateway.json');
  if (require('node:fs').existsSync(pactFile)) {
    require('node:fs').unlinkSync(pactFile);
  }
}

async function handleKafkaSnapshot(message) {
  const payload = typeof message === 'string' ? JSON.parse(message) : message;
  await saveSnapshotIfNotExistsForDay(payload);
}

describe('HistoryStore -> Gateway contract', () => {
  const messagePact = new MessageConsumerPact({
    consumer: 'HistoryStore',
    provider: 'Gateway',
    dir: PACTS_DIR,
  });

  beforeAll(() => {
    ensureFreshPactFile();
  });

  beforeEach(() => {
    Snapshot.exists.mockReset();
    Snapshot.create.mockReset();
    Snapshot.exists.mockResolvedValue(null);
    Snapshot.create.mockResolvedValue({
      capturedAt: new Date('2024-06-15T10:00:00.000Z'),
      devices: [],
    });
  });

  it('expects an IoT snapshot with device readings', async () => {
    await messagePact
      .given('Gateway published a snapshot to iot.snapshots')
      .expectsToReceive('an IoT snapshot with device readings')
      .withContent({
        capturedAt: string('2024-06-15T10:00:00.000Z'),
        devices: eachLike({
          type: string('sensor'),
          name: string('Kitchen'),
          payload: like({
            co2: integer(400),
            pm25: integer(10),
            humidity: integer(60),
            energy: like(1.5),
          }),
        }),
      })
      .withMetadata({ contentType: 'application/json' })
      .verify(synchronousBodyHandler(handleKafkaSnapshot));
  });

  it('expects an IoT snapshot with nullable payload fields', async () => {
    await messagePact
      .given('Gateway published a snapshot with sparse device payload')
      .expectsToReceive('an IoT snapshot with nullable payload fields')
      .withContent({
        capturedAt: string('2024-06-15T10:00:00.000Z'),
        devices: eachLike({
          type: string('sensor'),
          name: string('Hall'),
          payload: like({
            humidity: integer(55),
          }),
        }),
      })
      .withMetadata({ contentType: 'application/json' })
      .verify(synchronousBodyHandler(handleKafkaSnapshot));
  });
});
