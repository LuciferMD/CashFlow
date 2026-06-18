import type {
  AirQualityLevel,
  DeviceType,
  HistorySortMode,
  IotDevice,
  IotHistoryEntry,
  IotMetrics,
  RoomSnapshot,
} from "../model/types";

function average(values: number[]): number | null {
  if (values.length === 0) return null;
  return values.reduce((sum, value) => sum + value, 0) / values.length;
}

function max(values: number[]): number {
  return values.length === 0 ? 0 : Math.max(...values);
}

export function groupDevicesByRoom(devices: IotDevice[]): RoomSnapshot[] {
  const rooms = new Map<string, RoomSnapshot>();

  for (const device of devices) {
    const room =
      rooms.get(device.name) ??
      ({
        name: device.name,
        energy: null,
        hasMotion: false,
        co2: null,
        pm25: null,
        humidity: null,
      } satisfies RoomSnapshot);

    if (device.type === "energy" && device.payload.energy != null) {
      room.energy = device.payload.energy;
    }
    if (device.type === "motion") {
      room.hasMotion = true;
    }
    if (device.type === "air_quality") {
      room.co2 = device.payload.co2;
      room.pm25 = device.payload.pm25;
      room.humidity = device.payload.humidity;
    }

    rooms.set(device.name, room);
  }

  return Array.from(rooms.values()).sort((a, b) => a.name.localeCompare(b.name));
}

export function computeMetrics(devices: IotDevice[]): IotMetrics {
  const rooms = groupDevicesByRoom(devices);
  const energyValues = devices
    .filter((d) => d.type === "energy" && d.payload.energy != null)
    .map((d) => d.payload.energy!);
  const co2Values = devices
    .filter((d) => d.type === "air_quality" && d.payload.co2 != null)
    .map((d) => d.payload.co2!);
  const pm25Values = devices
    .filter((d) => d.type === "air_quality" && d.payload.pm25 != null)
    .map((d) => d.payload.pm25!);
  const humidityValues = devices
    .filter((d) => d.type === "air_quality" && d.payload.humidity != null)
    .map((d) => d.payload.humidity!);

  const airQualityAlerts = rooms.filter((room) => {
    const co2Level = getCo2Level(room.co2);
    const pm25Level = getPm25Level(room.pm25);
    return co2Level === "poor" || co2Level === "moderate" || pm25Level === "poor";
  }).length;

  return {
    totalEnergy: energyValues.reduce((sum, value) => sum + value, 0),
    avgCo2: average(co2Values),
    avgPm25: average(pm25Values),
    avgHumidity: average(humidityValues),
    motionZones: devices.filter((d) => d.type === "motion").length,
    airQualityAlerts,
    roomCount: rooms.length,
  };
}

export function getCo2Level(co2: number | null): AirQualityLevel {
  if (co2 == null) return "unknown";
  if (co2 < 600) return "good";
  if (co2 <= 1000) return "moderate";
  return "poor";
}

export function getPm25Level(pm25: number | null): AirQualityLevel {
  if (pm25 == null) return "unknown";
  if (pm25 < 12) return "good";
  if (pm25 <= 35) return "moderate";
  return "poor";
}

export function getHumidityLevel(humidity: number | null): AirQualityLevel {
  if (humidity == null) return "unknown";
  if (humidity >= 30 && humidity <= 60) return "good";
  if (humidity >= 20 && humidity <= 70) return "moderate";
  return "poor";
}

export function getDeviceTypeLabel(type: DeviceType): string {
  switch (type) {
    case "energy":
      return "Energy";
    case "motion":
      return "Motion";
    case "air_quality":
      return "Air Quality";
  }
}

export function formatEnergy(value: number | null): string {
  if (value == null) return "—";
  return `${value.toFixed(1)} kWh`;
}

export function formatTimestamp(iso: string): string {
  return new Date(iso).toLocaleString(undefined, {
    month: "short",
    day: "numeric",
    hour: "2-digit",
    minute: "2-digit",
    second: "2-digit",
  });
}

export function sortHistoryEntries(
  entries: IotHistoryEntry[],
  mode: HistorySortMode,
): IotHistoryEntry[] {
  const sorted = [...entries];

  switch (mode) {
    case "date-asc":
      return sorted.sort(
        (a, b) => new Date(a.timestamp).getTime() - new Date(b.timestamp).getTime(),
      );
    case "energy-desc":
      return sorted.sort((a, b) => b.metrics.totalEnergy - a.metrics.totalEnergy);
    case "co2-desc":
      return sorted.sort(
        (a, b) => (b.metrics.avgCo2 ?? 0) - (a.metrics.avgCo2 ?? 0),
      );
    case "pm25-desc":
      return sorted.sort(
        (a, b) => (b.metrics.avgPm25 ?? 0) - (a.metrics.avgPm25 ?? 0),
      );
    case "date-desc":
    default:
      return sorted.sort(
        (a, b) => new Date(b.timestamp).getTime() - new Date(a.timestamp).getTime(),
      );
  }
}

export function getHistoryPeakCo2(entry: IotHistoryEntry): number {
  return max(
    entry.devices
      .filter((d) => d.type === "air_quality" && d.payload.co2 != null)
      .map((d) => d.payload.co2!),
  );
}

export function getHistoryPeakPm25(entry: IotHistoryEntry): number {
  return max(
    entry.devices
      .filter((d) => d.type === "air_quality" && d.payload.pm25 != null)
      .map((d) => d.payload.pm25!),
  );
}
