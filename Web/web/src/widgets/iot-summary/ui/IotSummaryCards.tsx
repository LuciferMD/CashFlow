import {
  Activity,
  AlertTriangle,
  Droplets,
  Gauge,
  Radio,
  Wind,
  Zap,
} from "lucide-react";
import { Card, CardContent } from "../../../app/components/ui/card";
import type { IotMetrics } from "../../../entities/iot";

interface IotSummaryCardsProps {
  metrics: IotMetrics;
}

function MetricCard({
  label,
  value,
  hint,
  icon: Icon,
  accent,
}: {
  label: string;
  value: string;
  hint: string;
  icon: typeof Zap;
  accent: string;
}) {
  return (
    <Card className="border-slate-200 bg-white/90 shadow-sm shadow-slate-200/50">
      <CardContent className="p-5">
        <div className="flex items-start justify-between gap-3">
          <div className="space-y-2">
            <p className="text-xs uppercase tracking-[0.2em] text-slate-500">{label}</p>
            <p className="text-2xl font-semibold text-slate-900">{value}</p>
            <p className="text-sm text-slate-500">{hint}</p>
          </div>
          <div className={`rounded-2xl p-3 ${accent}`}>
            <Icon className="h-5 w-5" />
          </div>
        </div>
      </CardContent>
    </Card>
  );
}

export function IotSummaryCards({ metrics }: IotSummaryCardsProps) {
  return (
    <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
      <MetricCard
        label="Total Energy"
        value={`${metrics.totalEnergy.toFixed(1)} kWh`}
        hint={`Across ${metrics.roomCount} monitored rooms`}
        icon={Zap}
        accent="bg-amber-100 text-amber-700"
      />
      <MetricCard
        label="Air Quality"
        value={
          metrics.avgCo2 != null ? `${Math.round(metrics.avgCo2)} ppm CO₂` : "No sensors"
        }
        hint={
          metrics.avgPm25 != null
            ? `PM2.5 avg ${Math.round(metrics.avgPm25)} µg/m³`
            : "Waiting for readings"
        }
        icon={Wind}
        accent="bg-cyan-100 text-cyan-700"
      />
      <MetricCard
        label="Humidity"
        value={
          metrics.avgHumidity != null ? `${Math.round(metrics.avgHumidity)}%` : "No sensors"
        }
        hint="Comfort range 30–60%"
        icon={Droplets}
        accent="bg-blue-100 text-blue-700"
      />
      <MetricCard
        label="Live Activity"
        value={`${metrics.motionZones} zones`}
        hint={
          metrics.airQualityAlerts > 0
            ? `${metrics.airQualityAlerts} rooms need attention`
            : "All rooms within safe limits"
        }
        icon={metrics.airQualityAlerts > 0 ? AlertTriangle : Activity}
        accent={
          metrics.airQualityAlerts > 0
            ? "bg-rose-100 text-rose-700"
            : "bg-emerald-100 text-emerald-700"
        }
      />
    </div>
  );
}

export function IotStatusStrip({ metrics }: IotSummaryCardsProps) {
  return (
    <div className="flex flex-wrap items-center gap-3 text-sm text-slate-600">
      <span className="inline-flex items-center gap-2 rounded-full border border-slate-200 bg-white px-3 py-1 shadow-sm">
        <Radio className="h-3.5 w-3.5 text-emerald-600" />
        {metrics.roomCount} rooms online
      </span>
      <span className="inline-flex items-center gap-2 rounded-full border border-slate-200 bg-white px-3 py-1 shadow-sm">
        <Gauge className="h-3.5 w-3.5 text-cyan-600" />
        {metrics.motionZones} motion sensors
      </span>
    </div>
  );
}
