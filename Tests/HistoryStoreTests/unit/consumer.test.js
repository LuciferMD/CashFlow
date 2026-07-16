'use strict';

// ─── Module mocks ─────────────────────────────────────────────────────────────

// kafkajs is in the *source* project's node_modules, not the test project's.
// { virtual: true } tells Jest to register a mock without resolving the real file.
jest.mock(
  'kafkajs',
  () => ({
    Kafka: jest.fn(),
  }),
  { virtual: true },
);

jest.mock('../../../Api/HistoryStore/history_store/src/config', () => ({
  config: {
    kafka: {
      enabled: false, // default; tests mutate this via the required object
      brokers: ['localhost:9092'],
      topicIotSnapshots: 'iot.snapshots',
      groupId: 'history-store',
    },
    jwt: { publicKey: '', issuer: '', audience: '' },
    internal: { apiKey: '' },
  },
}));

jest.mock(
  '../../../Api/HistoryStore/history_store/src/services/snapshotService',
  () => ({
    saveSnapshotIfNotExistsForDay: jest.fn(),
  }),
);

// ─── Module imports ──────────────────────────────────────────────────────────

const { Kafka } = require('kafkajs');
const { saveSnapshotIfNotExistsForDay } = require('../../../Api/HistoryStore/history_store/src/services/snapshotService');
const { config } = require('../../../Api/HistoryStore/history_store/src/config');
const { startKafkaConsumer } = require('../../../Api/HistoryStore/history_store/src/kafka/consumer');

// ─── Helpers ─────────────────────────────────────────────────────────────────

/**
 * Set up a fresh Kafka mock that returns a fully wired consumer mock.
 * Returns the consumer mock so individual tests can inspect calls on it.
 */
function setupKafkaMock() {
  const mockConsumer = {
    connect: jest.fn().mockResolvedValue(undefined),
    subscribe: jest.fn().mockResolvedValue(undefined),
    run: jest.fn().mockResolvedValue(undefined),
  };
  Kafka.mockImplementation(() => ({
    consumer: jest.fn().mockReturnValue(mockConsumer),
  }));
  return mockConsumer;
}

// ─── Tests ───────────────────────────────────────────────────────────────────

describe('startKafkaConsumer', () => {
  beforeEach(() => {
    jest.clearAllMocks();
    // Suppress console output in tests
    jest.spyOn(console, 'log').mockImplementation(() => {});
    jest.spyOn(console, 'warn').mockImplementation(() => {});
    jest.spyOn(console, 'error').mockImplementation(() => {});
    config.kafka.enabled = false; // safe default; tests override as needed
  });

  afterEach(() => {
    jest.restoreAllMocks();
  });

  it('returns null without creating a Kafka instance when kafka is disabled', async () => {
    config.kafka.enabled = false;
    const result = await startKafkaConsumer();
    expect(result).toBeNull();
    expect(Kafka).not.toHaveBeenCalled();
  });

  it('creates a Kafka client with the configured brokers when enabled', async () => {
    config.kafka.enabled = true;
    setupKafkaMock();
    await startKafkaConsumer();
    expect(Kafka).toHaveBeenCalledWith(
      expect.objectContaining({ brokers: config.kafka.brokers }),
    );
  });

  it('connects the consumer before subscribing', async () => {
    config.kafka.enabled = true;
    const mockConsumer = setupKafkaMock();
    await startKafkaConsumer();
    expect(mockConsumer.connect).toHaveBeenCalledTimes(1);
  });

  it('subscribes to the configured IoT snapshots topic', async () => {
    config.kafka.enabled = true;
    const mockConsumer = setupKafkaMock();
    await startKafkaConsumer();
    expect(mockConsumer.subscribe).toHaveBeenCalledWith(
      expect.objectContaining({ topic: config.kafka.topicIotSnapshots }),
    );
  });

  it('returns the consumer instance after starting', async () => {
    config.kafka.enabled = true;
    const mockConsumer = setupKafkaMock();
    const result = await startKafkaConsumer();
    expect(result).toBe(mockConsumer);
  });
});

// ─── eachMessage callback (handleSnapshotMessage logic) ──────────────────────

describe('eachMessage callback', () => {
  let eachMessageFn;
  let mockConsumer;

  beforeEach(async () => {
    jest.clearAllMocks();
    jest.spyOn(console, 'log').mockImplementation(() => {});
    jest.spyOn(console, 'warn').mockImplementation(() => {});
    jest.spyOn(console, 'error').mockImplementation(() => {});

    config.kafka.enabled = true;
    mockConsumer = setupKafkaMock();

    // Capture the eachMessage function passed to consumer.run()
    mockConsumer.run.mockImplementation(async ({ eachMessage }) => {
      eachMessageFn = eachMessage;
    });

    await startKafkaConsumer();
  });

  afterEach(() => {
    jest.restoreAllMocks();
  });

  function makeMessage(jsonPayload) {
    return {
      topic: 'iot.snapshots',
      partition: 0,
      message: { value: Buffer.from(JSON.stringify(jsonPayload)) },
    };
  }

  it('calls saveSnapshotIfNotExistsForDay with devices and capturedAt from the message', async () => {
    const payload = {
      devices: [{ type: 'energy', name: 'Kitchen', payload: { energy: 2.5 } }],
      capturedAt: '2024-06-15T10:00:00Z',
    };
    saveSnapshotIfNotExistsForDay.mockResolvedValue({
      saved: true,
      snapshot: { capturedAt: new Date('2024-06-15T10:00:00Z') },
    });

    await eachMessageFn(makeMessage(payload));

    expect(saveSnapshotIfNotExistsForDay).toHaveBeenCalledWith({
      devices: payload.devices,
      capturedAt: payload.capturedAt,
    });
  });

  it('logs a warning and skips processing when the message value is empty', async () => {
    await eachMessageFn({ topic: 'iot.snapshots', partition: 0, message: { value: null } });
    expect(saveSnapshotIfNotExistsForDay).not.toHaveBeenCalled();
    expect(console.warn).toHaveBeenCalled();
  });

  it('re-throws errors from saveSnapshotIfNotExistsForDay after logging', async () => {
    const error = new Error('DB connection lost');
    saveSnapshotIfNotExistsForDay.mockRejectedValue(error);
    const payload = {
      devices: [{ type: 'motion', name: 'Hall', payload: {} }],
      capturedAt: '2024-06-15T10:00:00Z',
    };

    await expect(eachMessageFn(makeMessage(payload))).rejects.toThrow('DB connection lost');
    expect(console.error).toHaveBeenCalled();
  });
});
