import {
  Activity,
  Droplets,
  Flame,
  MapPin,
  Wind,
  Zap,
} from "lucide-react";
import { Badge } from "../../../app/components/ui/badge";
import { Card, CardContent, CardHeader, CardTitle } from "../../../app/components/ui/card";
import {
  formatEnergy,
  getCo2Level,
  getHumidityLevel,
  getPm25Level,
  groupDevicesByRoom,
  type IotDevice,
  type RoomSnapshot,
} from "../../../entities/iot";

function levelClasses(level: ReturnType<typeof getCo2Level>): string {
  switch (level) {
    case "good":
      return "border-emerald-200 bg-emerald-50 text-emerald-800";
    case "moderate":
      return "border-amber-200 bg-amber-50 text-amber-800";
    case "poor":
      return "border-rose-200 bg-rose-50 text-rose-800";
    default:
      return "border-slate-200 bg-slate-50 text-slate-500";
  }
}

function RoomCard({ room }: { room: RoomSnapshot }) {
  const co2Level = getCo2Level(room.co2);
  const pm25Level = getPm25Level(room.pm25);
  const humidityLevel = getHumidityLevel(room.humidity);

  return (
    <Card className="overflow-hidden border-slate-200 bg-white shadow-sm">
      <CardHeader className="border-b border-slate-100 pb-4">
        <div className="flex items-center justify-between gap-3">
          <CardTitle className="flex items-center gap-2 text-lg text-slate-900">
            <MapPin className="h-4 w-4 text-cyan-600" />
            {room.name}
          </CardTitle>
          {room.hasMotion && (
            <Badge className="border-emerald-200 bg-emerald-50 text-emerald-700">
              <Activity className="mr-1 h-3 w-3" />
              Motion
            </Badge>
          )}
        </div>
      </CardHeader>
      <CardContent className="grid gap-3 pt-4 sm:grid-cols-2">
        <div className="rounded-xl border border-slate-200 bg-slate-50 p-4">
          <div className="mb-2 flex items-center gap-2 text-xs uppercase tracking-wider text-slate-500">
            <Zap className="h-3.5 w-3.5 text-amber-600" />
            Energy
          </div>
          <p className="text-xl font-semibold text-slate-900">{formatEnergy(room.energy)}</p>
        </div>

        <div className={`rounded-xl border p-4 ${levelClasses(co2Level)}`}>
          <div className="mb-2 flex items-center gap-2 text-xs uppercase tracking-wider opacity-80">
            <Wind className="h-3.5 w-3.5" />
            CO₂
          </div>
          <p className="text-xl font-semibold">
            {room.co2 != null ? `${room.co2} ppm` : "—"}
          </p>
        </div>

        <div className={`rounded-xl border p-4 ${levelClasses(pm25Level)}`}>
          <div className="mb-2 flex items-center gap-2 text-xs uppercase tracking-wider opacity-80">
            <Flame className="h-3.5 w-3.5" />
            PM2.5
          </div>
          <p className="text-xl font-semibold">
            {room.pm25 != null ? `${room.pm25} µg/m³` : "—"}
          </p>
        </div>

        <div className={`rounded-xl border p-4 ${levelClasses(humidityLevel)}`}>
          <div className="mb-2 flex items-center gap-2 text-xs uppercase tracking-wider opacity-80">
            <Droplets className="h-3.5 w-3.5" />
            Humidity
          </div>
          <p className="text-xl font-semibold">
            {room.humidity != null ? `${room.humidity}%` : "—"}
          </p>
        </div>
      </CardContent>
    </Card>
  );
}

interface IotCurrentViewProps {
  devices: IotDevice[];
}

export function IotCurrentView({ devices }: IotCurrentViewProps) {
  const rooms = groupDevicesByRoom(devices);

  if (rooms.length === 0) {
    return (
      <Card className="border-dashed border-slate-300 bg-white/60">
        <CardContent className="py-16 text-center text-slate-500">
          No sensor readings available yet.
        </CardContent>
      </Card>
    );
  }

  return (
    <div className="grid gap-4 lg:grid-cols-2">
      {rooms.map((room) => (
        <RoomCard key={room.name} room={room} />
      ))}
    </div>
  );
}
