const { Kafka } = require('kafkajs');
const { config } = require('../config');
const { saveSnapshotIfNotExistsForDay } = require('../services/snapshotService');

async function handleSnapshotMessage(rawValue) {
  const payload = JSON.parse(rawValue);
  const { devices, capturedAt } = payload ?? {};

  const result = await saveSnapshotIfNotExistsForDay({ devices, capturedAt });

  if (result.saved) {
    console.log(
      `[kafka] Saved snapshot for ${new Date(result.snapshot.capturedAt).toISOString().slice(0, 10)}`
    );
    return;
  }

  console.log(
    `[kafka] Skipped snapshot (${result.reason}) for ${new Date(capturedAt ?? Date.now()).toISOString().slice(0, 10)}`
  );
}

async function startKafkaConsumer() {
  if (!config.kafka.enabled) {
    console.log('[kafka] Consumer disabled (KAFKA_BROKERS not set)');
    return null;
  }

  const kafka = new Kafka({
    clientId: 'history-store',
    brokers: config.kafka.brokers,
  });

  const consumer = kafka.consumer({ groupId: config.kafka.groupId });

  await consumer.connect();
  await consumer.subscribe({
    topic: config.kafka.topicIotSnapshots,
    fromBeginning: false,
  });

  await consumer.run({
    eachMessage: async ({ topic, partition, message }) => {
      const rawValue = message.value?.toString();
      if (!rawValue) {
        console.warn(`[kafka] Empty message on ${topic} partition ${partition}`);
        return;
      }

      try {
        await handleSnapshotMessage(rawValue);
      } catch (err) {
        console.error('[kafka] Failed to process snapshot message:', err.message);
        throw err;
      }
    },
  });

  console.log(
    `[kafka] Consumer listening on topic "${config.kafka.topicIotSnapshots}"`
  );

  return consumer;
}

module.exports = { startKafkaConsumer };
