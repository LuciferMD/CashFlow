import { computeMetrics } from "./calculations";
import type { IotDevice, IotHistoryEntry } from "../model/types";

const STORAGE_KEY = "iot-dashboard-history";
const MAX_ENTRIES = 100;

function loadRaw(): IotHistoryEntry[] {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) return [];
    const parsed = JSON.parse(raw) as IotHistoryEntry[];
    return Array.isArray(parsed) ? parsed : [];
  } catch {
    return [];
  }
}

function saveRaw(entries: IotHistoryEntry[]): void {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(entries.slice(0, MAX_ENTRIES)));
}

export function loadHistory(): IotHistoryEntry[] {
  return loadRaw();
}

export function appendHistorySnapshot(devices: IotDevice[]): IotHistoryEntry[] {
  const entry: IotHistoryEntry = {
    id: crypto.randomUUID(),
    timestamp: new Date().toISOString(),
    devices,
    metrics: computeMetrics(devices),
  };

  const existing = loadRaw();
  const updated = [entry, ...existing].slice(0, MAX_ENTRIES);
  saveRaw(updated);
  return updated;
}
