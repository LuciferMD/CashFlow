const { Router } = require('express');
const { requireAuth } = require('../middleware/auth');

const router = Router();

const mockSnapshots = [
  {
    id: '1',
    capturedAt: new Date(Date.now() - 60_000).toISOString(),
    devices: [
      { type: 'air',    name: 'sensor-01', payload: { co2: 420, pm25: 12, humidity: 55, energy: null } },
      { type: 'energy', name: 'meter-01',  payload: { co2: null, pm25: null, humidity: null, energy: 3.14 } },
    ],
  },
  {
    id: '2',
    capturedAt: new Date(Date.now() - 30_000).toISOString(),
    devices: [
      { type: 'air',    name: 'sensor-01', payload: { co2: 435, pm25: 14, humidity: 53, energy: null } },
      { type: 'energy', name: 'meter-01',  payload: { co2: null, pm25: null, humidity: null, energy: 3.21 } },
    ],
  },
];

/**
 * GET /history/latest
 * Returns the most recent mock snapshot. Requires a valid GuardPass JWT.
 */
router.get('/history/latest', requireAuth, (req, res) => {
  const latest = mockSnapshots.at(-1);
  if (!latest) return res.status(404).json({ error: 'No snapshots found yet' });
  res.json(latest);
});

/**
 * GET /history
 * Returns all mock snapshots. Requires a valid GuardPass JWT.
 */
router.get('/history', requireAuth, (req, res) => {
  res.json({ total: mockSnapshots.length, snapshots: mockSnapshots });
});

module.exports = router;
