const { Snapshot } = require('../db/snapshot.model');

function isValidDevice(device) {
  return (
    device &&
    typeof device.type === 'string' &&
    typeof device.name === 'string' &&
    device.payload &&
    typeof device.payload === 'object'
  );
}

function getUtcDayBounds(date) {
  const capturedAt = new Date(date);
  const start = new Date(
    Date.UTC(
      capturedAt.getUTCFullYear(),
      capturedAt.getUTCMonth(),
      capturedAt.getUTCDate()
    )
  );
  const end = new Date(
    Date.UTC(
      capturedAt.getUTCFullYear(),
      capturedAt.getUTCMonth(),
      capturedAt.getUTCDate() + 1
    )
  );

  return { start, end };
}

async function hasSnapshotForDay(capturedAt) {
  const { start, end } = getUtcDayBounds(capturedAt);

  const existing = await Snapshot.exists({
    capturedAt: { $gte: start, $lt: end },
  });

  return existing !== null;
}

/**
 * Saves a snapshot only when no snapshot exists yet for the same UTC calendar day.
 */
async function saveSnapshotIfNotExistsForDay({ devices, capturedAt }) {
  if (!Array.isArray(devices) || devices.length === 0) {
    throw new Error('devices must be a non-empty array');
  }

  if (!devices.every(isValidDevice)) {
    throw new Error('Each device must have type, name, and payload');
  }

  const resolvedCapturedAt = capturedAt ? new Date(capturedAt) : new Date();

  if (Number.isNaN(resolvedCapturedAt.getTime())) {
    throw new Error('capturedAt must be a valid date');
  }

  if (await hasSnapshotForDay(resolvedCapturedAt)) {
    return { saved: false, reason: 'snapshot-already-exists-for-day' };
  }

  const snapshot = await Snapshot.create({
    capturedAt: resolvedCapturedAt,
    devices,
  });

  return { saved: true, snapshot };
}

module.exports = {
  isValidDevice,
  saveSnapshotIfNotExistsForDay,
};
