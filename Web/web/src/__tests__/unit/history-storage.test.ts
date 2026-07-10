import { appendHistorySnapshot, loadHistory } from "../../entities/iot/lib/history-storage";
import type { IotDevice } from "../../entities/iot/model/types";

const STORAGE_KEY = "iot-dashboard-history";

function makeDevice(name: string, energy = 1.0): IotDevice {
  return {
    type: "energy",
    name,
    payload: { co2: null, pm25: null, humidity: null, energy },
  };
}

beforeEach(() => {
  localStorage.clear();
});

describe("loadHistory", () => {
  it("returns an empty array when localStorage is empty", () => {
    expect(loadHistory()).toEqual([]);
  });

  it("returns parsed entries from localStorage", () => {
    const entry = {
      id: "abc",
      timestamp: "2024-01-01T00:00:00.000Z",
      devices: [],
      metrics: { totalEnergy: 0, avgCo2: null, avgPm25: null, avgHumidity: null, motionZones: 0, airQualityAlerts: 0, roomCount: 0 },
    };
    localStorage.setItem(STORAGE_KEY, JSON.stringify([entry]));
    const result = loadHistory();
    expect(result).toHaveLength(1);
    expect(result[0].id).toBe("abc");
  });

  it("returns an empty array when localStorage contains malformed JSON", () => {
    localStorage.setItem(STORAGE_KEY, "not-valid-json{{{");
    expect(loadHistory()).toEqual([]);
  });

  it("returns an empty array when stored value is not an array", () => {
    localStorage.setItem(STORAGE_KEY, JSON.stringify({ oops: true }));
    expect(loadHistory()).toEqual([]);
  });
});

describe("appendHistorySnapshot", () => {
  it("adds a new entry and returns it as the first element", () => {
    const devices = [makeDevice("Kitchen", 3.5)];
    const result = appendHistorySnapshot(devices);
    expect(result).toHaveLength(1);
    expect(result[0].metrics.totalEnergy).toBeCloseTo(3.5);
  });

  it("prepends the new snapshot so the latest entry is first", () => {
    appendHistorySnapshot([makeDevice("Room A", 1)]);
    const result = appendHistorySnapshot([makeDevice("Room B", 2)]);
    expect(result[0].metrics.totalEnergy).toBeCloseTo(2);
    expect(result[1].metrics.totalEnergy).toBeCloseTo(1);
  });

  it("persists the updated list to localStorage", () => {
    appendHistorySnapshot([makeDevice("Office", 5)]);
    const saved = loadHistory();
    expect(saved).toHaveLength(1);
    expect(saved[0].metrics.totalEnergy).toBeCloseTo(5);
  });

  it("generates a unique id and an ISO timestamp for each entry", () => {
    const [entry] = appendHistorySnapshot([makeDevice("A", 1)]);
    expect(typeof entry.id).toBe("string");
    expect(entry.id.length).toBeGreaterThan(0);
    expect(new Date(entry.timestamp).getTime()).not.toBeNaN();
  });
});
