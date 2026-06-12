export type DeviceType = "energy" | "motion" | "air_quality";

export interface IotPayload {
  co2: number | null;
  pm25: number | null;
  humidity: number | null;
  energy: number | null;
}

export interface IotDevice {
  type: DeviceType;
  name: string;
  payload: IotPayload;
}

export interface IotData {
  devices: IotDevice[];
}

export interface RoomSnapshot {
  name: string;
  energy: number | null;
  hasMotion: boolean;
  co2: number | null;
  pm25: number | null;
  humidity: number | null;
}

export interface IotMetrics {
  totalEnergy: number;
  avgCo2: number | null;
  avgPm25: number | null;
  avgHumidity: number | null;
  motionZones: number;
  airQualityAlerts: number;
  roomCount: number;
}

export interface IotHistoryEntry {
  id: string;
  timestamp: string;
  devices: IotDevice[];
  metrics: IotMetrics;
}

export type HistorySortMode =
  | "date-desc"
  | "date-asc"
  | "energy-desc"
  | "co2-desc"
  | "pm25-desc";

export type AirQualityLevel = "good" | "moderate" | "poor" | "unknown";
