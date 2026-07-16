'use strict';

// ─── Module mocks ─────────────────────────────────────────────────────────────

jest.mock('../../../Api/HistoryStore/history_store/src/config', () => ({
  config: {
    internal: { apiKey: '' },
    jwt: { publicKey: 'key', issuer: '', audience: '' },
  },
}));

// ─── Module imports ──────────────────────────────────────────────────────────

// Require the mocked config so we can mutate it between tests
const { config } = require('../../../Api/HistoryStore/history_store/src/config');
const { requireInternalKey } = require('../../../Api/HistoryStore/history_store/src/middleware/internalAuth');

// ─── Helpers ─────────────────────────────────────────────────────────────────

function makeReq(headers = {}) {
  return { headers };
}

function makeRes() {
  const res = {};
  res.status = jest.fn().mockReturnValue(res);
  res.json = jest.fn().mockReturnValue(res);
  return res;
}

// ─── Tests ───────────────────────────────────────────────────────────────────

describe('requireInternalKey', () => {
  beforeEach(() => {
    // Reset to "no API key configured" before every test
    config.internal.apiKey = '';
  });

  it('calls next() immediately when INTERNAL_API_KEY is not configured', () => {
    // apiKey = '' → middleware is a no-op (local dev mode)
    const next = jest.fn();
    requireInternalKey(makeReq(), makeRes(), next);
    expect(next).toHaveBeenCalledTimes(1);
  });

  it('calls next() when the correct x-api-key header is provided', () => {
    config.internal.apiKey = 'super-secret';
    const next = jest.fn();
    requireInternalKey(makeReq({ 'x-api-key': 'super-secret' }), makeRes(), next);
    expect(next).toHaveBeenCalledTimes(1);
  });

  it('returns 401 when the x-api-key header is wrong', () => {
    config.internal.apiKey = 'super-secret';
    const res = makeRes();
    const next = jest.fn();
    requireInternalKey(makeReq({ 'x-api-key': 'wrong-key' }), res, next);
    expect(res.status).toHaveBeenCalledWith(401);
    expect(next).not.toHaveBeenCalled();
  });

  it('returns 401 when the x-api-key header is missing entirely', () => {
    config.internal.apiKey = 'super-secret';
    const res = makeRes();
    const next = jest.fn();
    requireInternalKey(makeReq({}), res, next);
    expect(res.status).toHaveBeenCalledWith(401);
    expect(next).not.toHaveBeenCalled();
  });

  it('includes "Invalid API key" in the error response body', () => {
    config.internal.apiKey = 'secret';
    const res = makeRes();
    requireInternalKey(makeReq({ 'x-api-key': 'bad' }), res, jest.fn());
    expect(res.json).toHaveBeenCalledWith(expect.objectContaining({ error: 'Invalid API key' }));
  });
});
