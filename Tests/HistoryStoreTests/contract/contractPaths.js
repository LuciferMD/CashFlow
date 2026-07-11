'use strict';

const path = require('node:path');

const PACTS_DIR = path.resolve(__dirname, '../../pacts');

module.exports = {
  PACTS_DIR,
  historyStoreGatewayPact: path.join(PACTS_DIR, 'HistoryStore-Gateway.json'),
};
