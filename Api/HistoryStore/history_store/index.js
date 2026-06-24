const express = require('express');
const cookieParser = require('cookie-parser');
const { config } = require('./src/config');
const { connectDb } = require('./src/db/connection');
const historyRouter = require('./src/routes/history');

async function main() {
  await connectDb();

  const app = express();

  app.use(express.json());
  app.use(cookieParser());

  app.get('/health', (req, res) => {
    res.json({ status: 'ok', time: new Date().toISOString() });
  });

  app.use(historyRouter);

  app.listen(config.api.port, () => {
    console.log(`[history-store] Listening on port ${config.api.port}`);
  });
}

main().catch((err) => {
  console.error('[fatal]', err);
  process.exit(1);
});
