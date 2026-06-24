const { config } = require('../config');

/**
 * Protects service-to-service endpoints (e.g. POST snapshots from Gateway).
 * Skipped when INTERNAL_API_KEY is not configured (local dev only).
 */
function requireInternalKey(req, res, next) {
  const expected = config.internal.apiKey;
  if (!expected) {
    return next();
  }

  const provided = req.headers['x-api-key'];
  if (provided !== expected) {
    return res.status(401).json({ error: 'Invalid API key' });
  }

  next();
}

module.exports = { requireInternalKey };
