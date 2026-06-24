const { Schema, model } = require('mongoose');

const PayloadSchema = new Schema(
  {
    co2: { type: Number, default: null },
    pm25: { type: Number, default: null },
    humidity: { type: Number, default: null },
    energy: { type: Number, default: null },
  },
  { _id: false }
);

const DeviceSchema = new Schema(
  {
    type: { type: String, required: true },
    name: { type: String, required: true },
    payload: { type: PayloadSchema, required: true },
  },
  { _id: false }
);

const SnapshotSchema = new Schema({
  capturedAt: { type: Date, required: true, index: true },
  devices: { type: [DeviceSchema], required: true },
});

const Snapshot = model('Snapshot', SnapshotSchema);

module.exports = { Snapshot };
