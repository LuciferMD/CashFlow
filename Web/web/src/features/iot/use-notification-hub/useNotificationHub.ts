import { useEffect, useRef } from "react";
import * as signalR from "@microsoft/signalr";
import { toast } from "sonner";
import { config } from "../../../config/api";
import type { IotDevice } from "../../../entities/iot";

interface HumidityAlertPayload {
  deviceName: string;
  deviceType: string;
  humidity: number;
  threshold: number;
  capturedAt: string;
}

interface IotSnapshotPayload {
  capturedAt: string;
  devices: IotDevice[];
}

interface UseNotificationHubOptions {
  onIotSnapshot?: (devices: IotDevice[]) => void;
}

/**
 * Connects to the SignalR notification hub and handles two server-push events:
 *
 *  - "IotSnapshot"   → calls onIotSnapshot with the latest device list so the
 *                       dashboard updates in real time without polling.
 *  - "HumidityAlert" → shows a warning toast whenever a device exceeds the
 *                       configured humidity threshold.
 */
export function useNotificationHub({
  onIotSnapshot,
}: UseNotificationHubOptions = {}) {
  // Keep callback ref stable so the effect doesn't re-run on every render.
  const onIotSnapshotRef = useRef(onIotSnapshot);
  onIotSnapshotRef.current = onIotSnapshot;

  useEffect(() => {
    // Flag lets us distinguish an intentional stop (cleanup/StrictMode) from a
    // genuine connection failure so we don't log a misleading error.
    let destroyed = false;

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(config.notificationHubUrl)
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    connection.on("IotSnapshot", (snapshot: IotSnapshotPayload) => {
      onIotSnapshotRef.current?.(snapshot.devices);
    });

    connection.on("HumidityAlert", (alert: HumidityAlertPayload) => {
      toast.warning(`High humidity — ${alert.deviceName}`, {
        description: `${alert.humidity}% detected (threshold: ${alert.threshold}%)`,
        duration: 8_000,
      });
    });

    connection.start().catch((err) => {
      if (!destroyed) {
        console.warn("[NotificationHub] failed to connect:", err);
      }
    });

    return () => {
      destroyed = true;
      connection.stop();
    };
  }, []);
}
