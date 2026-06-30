const { Router } = require('express');
const { Snapshot } = require('../db/snapshot.model');
const { requireAuth } = require('../middleware/auth');
const { requireInternalKey } = require('../middleware/internalAuth');
const { isValidDevice } = require('../services/snapshotService');

const router = Router();

/**
 * POST /snapshots
 * Saves an IoT snapshot from another service (Gateway, message broker worker, etc.).
 * Protected by X-Api-Key when INTERNAL_API_KEY is set.
 */
router.post('/snapshots', requireInternalKey, async (req, res) => {
  try {
    const { devices, capturedAt } = req.body ?? {};

    if (!Array.isArray(devices) || devices.length === 0) {
      return res.status(400).json({ error: 'devices must be a non-empty array' });
    }

    if (!devices.every(isValidDevice)) {
      return res.status(400).json({
        error: 'Each device must have type, name, and payload',
      });
    }

    const snapshot = await Snapshot.create({
      capturedAt: capturedAt ? new Date(capturedAt) : new Date(),
      devices,
    });

    res.status(201).json(snapshot);
  } catch {
    res.status(500).json({ error: 'Internal server error' });
  }
});

/**
 * GET /history/latest
 * Returns the most recent snapshot. Requires a valid GuardPass JWT.
 */
router.get('/history/latest', requireAuth, async (req, res) => {
  try {
    const snapshot = await Snapshot.findOne().sort({ capturedAt: -1 }).lean();
    if (!snapshot) {
      return res.status(404).json({ error: 'No snapshots found yet' });
    }
    res.json(snapshot);
  } catch {
    res.status(500).json({ error: 'Internal server error' });
  }
});

/**
 * GET /history
 * Query params: from, to, device, limit (default 100, max 1000), skip (default 0)
 * Requires a valid GuardPass JWT.
 */
router.get('/history', requireAuth, async (req, res) => {
  try {
    const {
      from,
      to,
      device,
      limit = '100',
      skip = '0',
    } = req.query;

    const filter = {};

    if (from || to) {
      filter.capturedAt = {};
      if (from) filter.capturedAt.$gte = new Date(from);
      if (to) filter.capturedAt.$lte = new Date(to);
    }

    if (device) {
      filter['devices.name'] = device;
    }

    const parsedLimit = Math.min(parseInt(limit, 10) || 100, 1000);
    const parsedSkip = parseInt(skip, 10) || 0;

    const [snapshots, total] = await Promise.all([
      Snapshot.find(filter)
        .sort({ capturedAt: -1 })
        .skip(parsedSkip)
        .limit(parsedLimit)
        .lean(),
      Snapshot.countDocuments(filter),
    ]);

    res.json({ total, limit: parsedLimit, skip: parsedSkip, snapshots });
  } catch {
    res.status(500).json({ error: 'Internal server error' });
  }
});

module.exports = router;
