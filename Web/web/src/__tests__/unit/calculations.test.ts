import {
  computeMetrics,
  formatEnergy,
  formatTimestamp,
  getCo2Level,
  getDeviceTypeLabel,
  getHistoryPeakCo2,
  getHistoryPeakPm25,
  getHumidityLevel,
  getPm25Level,
  groupDevicesByRoom,
  sortHistoryEntries,
} from "../../entities/iot/lib/calculations";
import type { IotDevice, IotHistoryEntry } from "../../entities/iot/model/types";

// ─── helpers ───────────────────────────────────────────────────────────────

function makeDevice(
  type: IotDevice["type"],
  name: string,
  payload: Partial<IotDevice["payload"]> = {},
): IotDevice {
  return {
    type,
    name,
    payload: { co2: null, pm25: null, humidity: null, energy: null, ...payload },
  };
}

function makeHistoryEntry(
  overrides: Partial<IotHistoryEntry> = {},
): IotHistoryEntry {
  return {
    id: "test-id",
    timestamp: "2024-01-01T00:00:00.000Z",
    devices: [],
    metrics: {
      totalEnergy: 0,
      avgCo2: null,
      avgPm25: null,
      avgHumidity: null,
      motionZones: 0,
      airQualityAlerts: 0,
      roomCount: 0,
    },
    ...overrides,
  };
}

// ─── groupDevicesByRoom ─────────────────────────────────────────────────────

describe("groupDevicesByRoom", () => {
  it("populates room.energy from an energy device", () => {
    const devices = [makeDevice("energy", "Kitchen", { energy: 4.2 })];
    const rooms = groupDevicesByRoom(devices);
    expect(rooms).toHaveLength(1);
    expect(rooms[0].energy).toBeCloseTo(4.2);
  });

  it("populates co2, pm25, humidity from an air_quality device", () => {
    const devices = [makeDevice("air_quality", "Bedroom", { co2: 700, pm25: 10, humidity: 45 })];
    const [room] = groupDevicesByRoom(devices);
    expect(room.co2).toBe(700);
    expect(room.pm25).toBe(10);
    expect(room.humidity).toBe(45);
  });

  it("sets hasMotion=true for a motion device", () => {
    const devices = [makeDevice("motion", "Hall")];
    const [room] = groupDevicesByRoom(devices);
    expect(room.hasMotion).toBe(true);
  });

  it("merges multiple device types into the same room", () => {
    const devices = [
      makeDevice("energy", "Office", { energy: 2.0 }),
      makeDevice("motion", "Office"),
      makeDevice("air_quality", "Office", { co2: 500 }),
    ];
    const rooms = groupDevicesByRoom(devices);
    expect(rooms).toHaveLength(1);
    expect(rooms[0].energy).toBe(2.0);
    expect(rooms[0].hasMotion).toBe(true);
    expect(rooms[0].co2).toBe(500);
  });

  it("returns rooms sorted alphabetically by name", () => {
    const devices = [
      makeDevice("energy", "Zebra", { energy: 1 }),
      makeDevice("energy", "Alpha", { energy: 1 }),
      makeDevice("energy", "Middle", { energy: 1 }),
    ];
    const names = groupDevicesByRoom(devices).map((r) => r.name);
    expect(names).toEqual(["Alpha", "Middle", "Zebra"]);
  });
});

// ─── computeMetrics ─────────────────────────────────────────────────────────

describe("computeMetrics", () => {
  it("sums totalEnergy across all energy devices", () => {
    const devices = [
      makeDevice("energy", "Room A", { energy: 3.0 }),
      makeDevice("energy", "Room B", { energy: 7.5 }),
    ];
    expect(computeMetrics(devices).totalEnergy).toBeCloseTo(10.5);
  });

  it("returns null avgCo2 when there are no air_quality devices", () => {
    const devices = [makeDevice("energy", "Room A", { energy: 1 })];
    expect(computeMetrics(devices).avgCo2).toBeNull();
  });

  it("counts motionZones from motion-type devices", () => {
    const devices = [
      makeDevice("motion", "Hall"),
      makeDevice("motion", "Garden"),
      makeDevice("energy", "Kitchen", { energy: 1 }),
    ];
    expect(computeMetrics(devices).motionZones).toBe(2);
  });

  it("increments airQualityAlerts when CO₂ is poor (> 1000 ppm)", () => {
    const devices = [
      makeDevice("air_quality", "Lab", { co2: 1200, pm25: 5, humidity: 40 }),
    ];
    expect(computeMetrics(devices).airQualityAlerts).toBe(1);
  });

  it("does not alert when air quality is good", () => {
    const devices = [
      makeDevice("air_quality", "Office", { co2: 450, pm25: 5, humidity: 40 }),
    ];
    expect(computeMetrics(devices).airQualityAlerts).toBe(0);
  });
});

// ─── getCo2Level ────────────────────────────────────────────────────────────

describe("getCo2Level", () => {
  it('returns "unknown" for null', () => {
    expect(getCo2Level(null)).toBe("unknown");
  });

  it("returns correct level at each threshold", () => {
    expect(getCo2Level(400)).toBe("good");      // < 600
    expect(getCo2Level(600)).toBe("moderate");  // boundary (600 ≤ x ≤ 1000)
    expect(getCo2Level(1000)).toBe("moderate"); // boundary
    expect(getCo2Level(1001)).toBe("poor");     // > 1000
  });
});

// ─── getPm25Level ───────────────────────────────────────────────────────────

describe("getPm25Level", () => {
  it("returns correct level at each threshold", () => {
    expect(getPm25Level(null)).toBe("unknown");
    expect(getPm25Level(5)).toBe("good");      // < 12
    expect(getPm25Level(12)).toBe("moderate"); // boundary
    expect(getPm25Level(35)).toBe("moderate"); // boundary
    expect(getPm25Level(36)).toBe("poor");     // > 35
  });
});

// ─── getHumidityLevel ───────────────────────────────────────────────────────

describe("getHumidityLevel", () => {
  it('returns "good" inside the 30–60 comfort range', () => {
    expect(getHumidityLevel(45)).toBe("good");
    expect(getHumidityLevel(30)).toBe("good");
    expect(getHumidityLevel(60)).toBe("good");
  });

  it('returns "moderate" in the 20–70 range outside 30–60', () => {
    expect(getHumidityLevel(25)).toBe("moderate");
    expect(getHumidityLevel(65)).toBe("moderate");
  });

  it('returns "poor" outside the 20–70 range', () => {
    expect(getHumidityLevel(10)).toBe("poor");
    expect(getHumidityLevel(85)).toBe("poor");
  });
});

// ─── getDeviceTypeLabel ──────────────────────────────────────────────────────

describe("getDeviceTypeLabel", () => {
  it("maps each device type to its human-readable label", () => {
    expect(getDeviceTypeLabel("energy")).toBe("Energy");
    expect(getDeviceTypeLabel("motion")).toBe("Motion");
    expect(getDeviceTypeLabel("air_quality")).toBe("Air Quality");
  });
});

// ─── formatEnergy ───────────────────────────────────────────────────────────

describe("formatEnergy", () => {
  it('returns "—" for null', () => {
    expect(formatEnergy(null)).toBe("—");
  });

  it("formats with one decimal place and kWh suffix", () => {
    expect(formatEnergy(3.14159)).toBe("3.1 kWh");
    expect(formatEnergy(0)).toBe("0.0 kWh");
  });
});

// ─── sortHistoryEntries ──────────────────────────────────────────────────────

describe("sortHistoryEntries", () => {
  const entries: IotHistoryEntry[] = [
    makeHistoryEntry({
      id: "a",
      timestamp: "2024-01-01T00:00:00.000Z",
      metrics: { totalEnergy: 5, avgCo2: null, avgPm25: null, avgHumidity: null, motionZones: 0, airQualityAlerts: 0, roomCount: 1 },
    }),
    makeHistoryEntry({
      id: "b",
      timestamp: "2024-06-15T00:00:00.000Z",
      metrics: { totalEnergy: 20, avgCo2: null, avgPm25: null, avgHumidity: null, motionZones: 0, airQualityAlerts: 0, roomCount: 1 },
    }),
    makeHistoryEntry({
      id: "c",
      timestamp: "2024-03-10T00:00:00.000Z",
      metrics: { totalEnergy: 10, avgCo2: null, avgPm25: null, avgHumidity: null, motionZones: 0, airQualityAlerts: 0, roomCount: 1 },
    }),
  ];

  it("sorts date-desc (newest first) by default", () => {
    const result = sortHistoryEntries(entries, "date-desc");
    expect(result.map((e) => e.id)).toEqual(["b", "c", "a"]);
  });

  it("sorts date-asc (oldest first)", () => {
    const result = sortHistoryEntries(entries, "date-asc");
    expect(result.map((e) => e.id)).toEqual(["a", "c", "b"]);
  });

  it("sorts energy-desc (highest energy first)", () => {
    const result = sortHistoryEntries(entries, "energy-desc");
    expect(result.map((e) => e.id)).toEqual(["b", "c", "a"]);
  });

  it("does not mutate the original array", () => {
    const original = [...entries];
    sortHistoryEntries(entries, "date-asc");
    expect(entries).toEqual(original);
  });
});

// ─── getHistoryPeakCo2 / getHistoryPeakPm25 ─────────────────────────────────

describe("getHistoryPeakCo2", () => {
  it("returns the highest CO₂ value from devices in the entry", () => {
    const entry = makeHistoryEntry({
      devices: [
        makeDevice("air_quality", "A", { co2: 500 }),
        makeDevice("air_quality", "B", { co2: 900 }),
        makeDevice("air_quality", "C", { co2: 700 }),
      ],
    });
    expect(getHistoryPeakCo2(entry)).toBe(900);
  });

  it("returns 0 when no air_quality devices are present", () => {
    const entry = makeHistoryEntry({ devices: [makeDevice("energy", "A", { energy: 1 })] });
    expect(getHistoryPeakCo2(entry)).toBe(0);
  });
});

describe("getHistoryPeakPm25", () => {
  it("returns the highest PM2.5 value from devices in the entry", () => {
    const entry = makeHistoryEntry({
      devices: [
        makeDevice("air_quality", "A", { pm25: 8 }),
        makeDevice("air_quality", "B", { pm25: 40 }),
      ],
    });
    expect(getHistoryPeakPm25(entry)).toBe(40);
  });
});
