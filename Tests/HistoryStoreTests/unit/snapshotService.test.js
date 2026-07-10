'use strict';

// ─── Module mocks (hoisted before any requires) ──────────────────────────────

// Prevent config.js from reading the PEM key file at require-time.
// snapshotService doesn't use config directly, but this guards transitive deps.
jest.mock(
  '../../../Api/HistoryStore/history_store/src/db/snapshot.model',
  () => ({
    Snapshot: {
      exists: jest.fn(),
      create: jest.fn(),
    },
  }),
);

// ─── Module imports ──────────────────────────────────────────────────────────

const {
  isValidDevice,
  saveSnapshotIfNotExistsForDay,
} = require('../../../Api/HistoryStore/history_store/src/services/snapshotService');

const { Snapshot } = require('../../../Api/HistoryStore/history_store/src/db/snapshot.model');

// ─── Test fixtures ───────────────────────────────────────────────────────────

const validDevice = {
  type: 'energy',
  name: 'Kitchen',
  payload: { energy: 3.5, co2: null, pm25: null, humidity: null },
};

// ─── isValidDevice ────────────────────────────────────────────────────────────

describe('isValidDevice', () => {
  it('returns truthy for a fully valid device', () => {
    expect(isValidDevice(validDevice)).toBeTruthy();
  });

  it('returns falsy for null', () => {
    expect(isValidDevice(null)).toBeFalsy();
  });

  it('returns falsy when type is missing', () => {
    expect(isValidDevice({ name: 'A', payload: {} })).toBeFalsy();
  });

  it('returns falsy when type is not a string', () => {
    expect(isValidDevice({ type: 42, name: 'A', payload: {} })).toBeFalsy();
  });

  it('returns falsy when name is missing', () => {
    expect(isValidDevice({ type: 'energy', payload: {} })).toBeFalsy();
  });

  it('returns falsy when payload is absent', () => {
    expect(isValidDevice({ type: 'energy', name: 'Kitchen' })).toBeFalsy();
  });

  it('returns falsy when payload is not an object', () => {
    expect(isValidDevice({ type: 'energy', name: 'Kitchen', payload: 'bad' })).toBeFalsy();
  });
});

// ─── saveSnapshotIfNotExistsForDay ───────────────────────────────────────────

describe('saveSnapshotIfNotExistsForDay', () => {
  beforeEach(() => {
    Snapshot.exists.mockReset();
    Snapshot.create.mockReset();
  });

  it('throws when devices is an empty array', async () => {
    await expect(
      saveSnapshotIfNotExistsForDay({ devices: [], capturedAt: '2024-01-01' }),
    ).rejects.toThrow('devices must be a non-empty array');
  });

  it('throws when devices is not an array', async () => {
    await expect(
      saveSnapshotIfNotExistsForDay({ devices: null, capturedAt: '2024-01-01' }),
    ).rejects.toThrow('devices must be a non-empty array');
  });

  it('throws when any device fails validation', async () => {
    await expect(
      saveSnapshotIfNotExistsForDay({ devices: [{ bad: true }], capturedAt: '2024-01-01' }),
    ).rejects.toThrow('Each device must have type, name, and payload');
  });

  it('throws when capturedAt is not a valid date string', async () => {
    await expect(
      saveSnapshotIfNotExistsForDay({ devices: [validDevice], capturedAt: 'not-a-date' }),
    ).rejects.toThrow('capturedAt must be a valid date');
  });

  it('returns { saved: false } when a snapshot already exists for that UTC day', async () => {
    Snapshot.exists.mockResolvedValue({ _id: 'existing-id' });

    const result = await saveSnapshotIfNotExistsForDay({
      devices: [validDevice],
      capturedAt: '2024-06-15T10:00:00Z',
    });

    expect(result.saved).toBe(false);
    expect(result.reason).toBe('snapshot-already-exists-for-day');
    expect(Snapshot.create).not.toHaveBeenCalled();
  });

  it('creates and returns the snapshot when no snapshot exists for the day', async () => {
    Snapshot.exists.mockResolvedValue(null);
    const fakeSnapshot = {
      _id: 'new-id',
      capturedAt: new Date('2024-06-15T10:00:00Z'),
      devices: [validDevice],
    };
    Snapshot.create.mockResolvedValue(fakeSnapshot);

    const result = await saveSnapshotIfNotExistsForDay({
      devices: [validDevice],
      capturedAt: '2024-06-15T10:00:00Z',
    });

    expect(result.saved).toBe(true);
    expect(result.snapshot).toBe(fakeSnapshot);
    expect(Snapshot.create).toHaveBeenCalledTimes(1);
  });

  it('uses the current time when capturedAt is omitted', async () => {
    Snapshot.exists.mockResolvedValue(null);
    Snapshot.create.mockResolvedValue({ _id: 'x', capturedAt: new Date(), devices: [validDevice] });

    const result = await saveSnapshotIfNotExistsForDay({ devices: [validDevice] });

    expect(result.saved).toBe(true);
    expect(Snapshot.create).toHaveBeenCalledTimes(1);
  });

  it('queries Snapshot.exists with correct UTC midnight boundaries', async () => {
    Snapshot.exists.mockResolvedValue(null);
    Snapshot.create.mockResolvedValue({ _id: 'x', capturedAt: new Date(), devices: [validDevice] });

    await saveSnapshotIfNotExistsForDay({
      devices: [validDevice],
      capturedAt: '2024-06-15T18:30:00Z',
    });

    const query = Snapshot.exists.mock.calls[0][0];
    expect(query.capturedAt.$gte).toEqual(new Date('2024-06-15T00:00:00.000Z'));
    expect(query.capturedAt.$lt).toEqual(new Date('2024-06-16T00:00:00.000Z'));
  });

  it('passes devices and the resolved capturedAt to Snapshot.create', async () => {
    Snapshot.exists.mockResolvedValue(null);
    Snapshot.create.mockResolvedValue({ _id: 'y', capturedAt: new Date(), devices: [validDevice] });

    const capturedAt = '2024-03-10T08:00:00Z';
    await saveSnapshotIfNotExistsForDay({ devices: [validDevice], capturedAt });

    const [createArg] = Snapshot.create.mock.calls[0];
    expect(createArg.devices).toEqual([validDevice]);
    expect(createArg.capturedAt).toEqual(new Date(capturedAt));
  });
});
