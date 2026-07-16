'use strict';

// ─── Module mocks ─────────────────────────────────────────────────────────────

// jsonwebtoken lives in the source project's node_modules, not the test
// project's — use { virtual: true } so Jest doesn't try to resolve it here.
jest.mock('jsonwebtoken', () => ({ verify: jest.fn() }), { virtual: true });

// Prevent config.js from calling fs.readFileSync at require-time.
jest.mock('../../../Api/HistoryStore/history_store/src/config', () => ({
  config: {
    jwt: {
      publicKey: 'test-public-key',
      issuer: 'TestIssuer',
      audience: 'TestAudience',
    },
  },
}));

// ─── Module imports ──────────────────────────────────────────────────────────

const jwt = require('jsonwebtoken');
const { requireAuth } = require('../../../Api/HistoryStore/history_store/src/middleware/auth');

// ─── Helpers ─────────────────────────────────────────────────────────────────

function makeReq(overrides = {}) {
  return { cookies: {}, headers: {}, ...overrides };
}

function makeRes() {
  const res = {};
  res.status = jest.fn().mockReturnValue(res);
  res.json = jest.fn().mockReturnValue(res);
  return res;
}

// ─── Tests ───────────────────────────────────────────────────────────────────

describe('requireAuth', () => {
  beforeEach(() => {
    jwt.verify.mockReset();
  });

  it('returns 401 when neither cookie nor Authorization header is present', () => {
    const res = makeRes();
    requireAuth(makeReq(), res, jest.fn());
    expect(res.status).toHaveBeenCalledWith(401);
    expect(res.json).toHaveBeenCalledWith({ error: 'Unauthorized' });
  });

  it('reads the token from the GuardPass cookie', () => {
    jwt.verify.mockReturnValue({ sub: 'user1' });
    const next = jest.fn();
    requireAuth(makeReq({ cookies: { GuardPass: 'cookie-token' } }), makeRes(), next);
    expect(jwt.verify).toHaveBeenCalledWith(
      'cookie-token',
      'test-public-key',
      expect.any(Object),
    );
    expect(next).toHaveBeenCalled();
  });

  it('falls back to the Authorization Bearer header when no cookie exists', () => {
    jwt.verify.mockReturnValue({ sub: 'user2' });
    const next = jest.fn();
    requireAuth(
      makeReq({ headers: { authorization: 'Bearer bearer-token' } }),
      makeRes(),
      next,
    );
    expect(jwt.verify).toHaveBeenCalledWith(
      'bearer-token',
      'test-public-key',
      expect.any(Object),
    );
    expect(next).toHaveBeenCalled();
  });

  it('prefers the cookie token over the Authorization header', () => {
    jwt.verify.mockReturnValue({ sub: 'user3' });
    const next = jest.fn();
    requireAuth(
      makeReq({
        cookies: { GuardPass: 'cookie-token' },
        headers: { authorization: 'Bearer header-token' },
      }),
      makeRes(),
      next,
    );
    expect(jwt.verify).toHaveBeenCalledWith('cookie-token', 'test-public-key', expect.any(Object));
  });

  it('returns 401 when jwt.verify throws (invalid or expired token)', () => {
    jwt.verify.mockImplementation(() => {
      throw new Error('jwt signature verification failed');
    });
    const res = makeRes();
    const next = jest.fn();
    requireAuth(makeReq({ cookies: { GuardPass: 'bad-token' } }), res, next);
    expect(res.status).toHaveBeenCalledWith(401);
    expect(res.json).toHaveBeenCalledWith({ error: 'Invalid or expired token' });
    expect(next).not.toHaveBeenCalled();
  });

  it('passes RS256, issuer and audience from config to jwt.verify', () => {
    jwt.verify.mockReturnValue({});
    requireAuth(makeReq({ cookies: { GuardPass: 'token' } }), makeRes(), jest.fn());
    const options = jwt.verify.mock.calls[0][2];
    expect(options.algorithms).toContain('RS256');
    expect(options.issuer).toBe('TestIssuer');
    expect(options.audience).toBe('TestAudience');
  });

  it('returns 401 when Authorization header is not a Bearer token', () => {
    const res = makeRes();
    requireAuth(
      makeReq({ headers: { authorization: 'Basic dXNlcjpwYXNz' } }),
      res,
      jest.fn(),
    );
    expect(res.status).toHaveBeenCalledWith(401);
    expect(jwt.verify).not.toHaveBeenCalled();
  });

  it('calls next() exactly once on a valid token', () => {
    jwt.verify.mockReturnValue({ sub: 'ok' });
    const next = jest.fn();
    requireAuth(makeReq({ cookies: { GuardPass: 'valid' } }), makeRes(), next);
    expect(next).toHaveBeenCalledTimes(1);
  });
});
