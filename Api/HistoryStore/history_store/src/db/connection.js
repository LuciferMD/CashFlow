const mongoose = require('mongoose');
const { config } = require('../config');

async function connectDb() {
  await mongoose.connect(config.mongodb.uri);
  console.log('[db] Connected to MongoDB');
}

module.exports = { connectDb };
