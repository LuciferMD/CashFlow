const jwt = require('jsonwebtoken');
const { config } = require('../config');

/**
 * Mirrors the Gateway's OnMessageReceived logic:
 *   1. Read token from GuardPass cookie
 *   2. Fall back to Authorization: Bearer <token> header
 *   3. Verify RS256 signature, issuer and audience
 */
function requireAuth(req, res, next) {
  let token = req.cookies?.GuardPass;

  if (!token) {
    const authHeader = req.headers['authorization'];
    if (authHeader?.startsWith('Bearer ')) {
      token = authHeader.slice('Bearer '.length);
    }
  }

  if (!token) {
    return res.status(401).json({ error: 'Unauthorized' });
  }

  try {
    jwt.verify(token, config.jwt.publicKey, {
      algorithms: ['RS256'],
      issuer: config.jwt.issuer || undefined,
      audience: config.jwt.audience || undefined,
    });
    next();
  } catch {
    res.status(401).json({ error: 'Invalid or expired token' });
  }
}

module.exports = { requireAuth };
