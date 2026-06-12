import { useCallback, useEffect, useRef, useState } from "react";
import {
  appendHistorySnapshot,
  fetchIot,
  loadHistory,
  type IotDevice,
  type IotHistoryEntry,
} from "../../../entities/iot";

const REFRESH_INTERVAL_MS = 30_000;

export function useIotDashboard() {
  const [devices, setDevices] = useState<IotDevice[]>([]);
  const [history, setHistory] = useState<IotHistoryEntry[]>(() => loadHistory());
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [lastUpdated, setLastUpdated] = useState<string | null>(null);
  const hasLoadedRef = useRef(false);

  const refresh = useCallback(async (isManual = false) => {
    if (isManual) {
      setRefreshing(true);
    } else if (!hasLoadedRef.current) {
      setLoading(true);
    }

    setError(null);

    try {
      const nextDevices = await fetchIot();
      setDevices(nextDevices);
      setLastUpdated(new Date().toISOString());
      setHistory(appendHistorySnapshot(nextDevices));
      hasLoadedRef.current = true;
    } catch (err) {
      setError(err instanceof Error ? err.message : "Unable to load sensor data");
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  }, []);

  useEffect(() => {
    void refresh();
    const timer = window.setInterval(() => void refresh(true), REFRESH_INTERVAL_MS);
    return () => window.clearInterval(timer);
  }, [refresh]);

  return {
    devices,
    history,
    loading,
    refreshing,
    error,
    lastUpdated,
    refresh: () => refresh(true),
  };
}
