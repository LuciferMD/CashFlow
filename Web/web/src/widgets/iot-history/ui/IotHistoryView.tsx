import { useMemo, useState } from "react";
import { ArrowDownUp, ChevronDown, ChevronUp, Clock3 } from "lucide-react";
import { Badge } from "../../../app/components/ui/badge";
import { Button } from "../../../app/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "../../../app/components/ui/card";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "../../../app/components/ui/select";
import {
  formatTimestamp,
  getHistoryPeakCo2,
  getHistoryPeakPm25,
  groupDevicesByRoom,
  sortHistoryEntries,
  type HistorySortMode,
  type IotHistoryEntry,
} from "../../../entities/iot";

interface IotHistoryViewProps {
  history: IotHistoryEntry[];
}

function HistoryEntryCard({ entry }: { entry: IotHistoryEntry }) {
  const [expanded, setExpanded] = useState(false);
  const rooms = groupDevicesByRoom(entry.devices);
  const peakCo2 = getHistoryPeakCo2(entry);
  const peakPm25 = getHistoryPeakPm25(entry);

  return (
    <Card className="border-slate-200 bg-white shadow-sm">
      <CardHeader className="pb-3">
        <div className="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
          <div className="space-y-2">
            <CardTitle className="flex items-center gap-2 text-base text-slate-900">
              <Clock3 className="h-4 w-4 text-cyan-600" />
              {formatTimestamp(entry.timestamp)}
            </CardTitle>
            <div className="flex flex-wrap gap-2">
              <Badge variant="outline" className="border-amber-200 text-amber-800">
                {entry.metrics.totalEnergy.toFixed(1)} kWh total
              </Badge>
              {entry.metrics.avgCo2 != null && (
                <Badge variant="outline" className="border-cyan-200 text-cyan-800">
                  CO₂ avg {Math.round(entry.metrics.avgCo2)} ppm
                </Badge>
              )}
              {peakCo2 > 0 && (
                <Badge variant="outline" className="border-rose-200 text-rose-800">
                  CO₂ peak {peakCo2} ppm
                </Badge>
              )}
              {peakPm25 > 0 && (
                <Badge variant="outline" className="border-orange-200 text-orange-800">
                  PM2.5 peak {peakPm25} µg/m³
                </Badge>
              )}
            </div>
          </div>

          <Button
            variant="outline"
            className="border-slate-200 bg-white text-slate-700 hover:bg-slate-50"
            onClick={() => setExpanded((value) => !value)}
          >
            {expanded ? (
              <>
                Hide rooms
                <ChevronUp className="ml-2 h-4 w-4" />
              </>
            ) : (
              <>
                View {rooms.length} rooms
                <ChevronDown className="ml-2 h-4 w-4" />
              </>
            )}
          </Button>
        </div>
      </CardHeader>

      {expanded && (
        <CardContent className="border-t border-slate-100 pt-4">
          <div className="grid gap-3 md:grid-cols-2 xl:grid-cols-3">
            {rooms.map((room) => (
              <div
                key={`${entry.id}-${room.name}`}
                className="rounded-xl border border-slate-200 bg-slate-50 p-4"
              >
                <p className="mb-3 font-medium text-slate-900">{room.name}</p>
                <dl className="grid grid-cols-2 gap-2 text-sm">
                  <div>
                    <dt className="text-slate-500">Energy</dt>
                    <dd className="text-slate-800">
                      {room.energy != null ? `${room.energy.toFixed(1)} kWh` : "—"}
                    </dd>
                  </div>
                  <div>
                    <dt className="text-slate-500">Motion</dt>
                    <dd className="text-slate-800">{room.hasMotion ? "Active" : "Idle"}</dd>
                  </div>
                  <div>
                    <dt className="text-slate-500">CO₂</dt>
                    <dd className="text-slate-800">
                      {room.co2 != null ? `${room.co2} ppm` : "—"}
                    </dd>
                  </div>
                  <div>
                    <dt className="text-slate-500">PM2.5</dt>
                    <dd className="text-slate-800">
                      {room.pm25 != null ? `${room.pm25} µg/m³` : "—"}
                    </dd>
                  </div>
                  <div className="col-span-2">
                    <dt className="text-slate-500">Humidity</dt>
                    <dd className="text-slate-800">
                      {room.humidity != null ? `${room.humidity}%` : "—"}
                    </dd>
                  </div>
                </dl>
              </div>
            ))}
          </div>
        </CardContent>
      )}
    </Card>
  );
}

export function IotHistoryView({ history }: IotHistoryViewProps) {
  const [sortMode, setSortMode] = useState<HistorySortMode>("date-desc");

  const sortedHistory = useMemo(
    () => sortHistoryEntries(history, sortMode),
    [history, sortMode],
  );

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-4 rounded-2xl border border-slate-200 bg-white p-4 shadow-sm sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h3 className="text-lg font-medium text-slate-900">Reading history</h3>
          <p className="text-sm text-slate-500">
            Snapshots are captured on each refresh and stored locally in your browser.
          </p>
        </div>

        <div className="flex items-center gap-2">
          <ArrowDownUp className="h-4 w-4 text-slate-400" />
          <Select value={sortMode} onValueChange={(value) => setSortMode(value as HistorySortMode)}>
            <SelectTrigger className="w-[220px] border-slate-200 bg-white text-slate-700">
              <SelectValue placeholder="Sort by" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="date-desc">Newest first</SelectItem>
              <SelectItem value="date-asc">Oldest first</SelectItem>
              <SelectItem value="energy-desc">Highest energy</SelectItem>
              <SelectItem value="co2-desc">Highest CO₂</SelectItem>
              <SelectItem value="pm25-desc">Highest PM2.5</SelectItem>
            </SelectContent>
          </Select>
        </div>
      </div>

      {sortedHistory.length === 0 ? (
        <Card className="border-dashed border-slate-300 bg-white/60">
          <CardContent className="py-16 text-center text-slate-500">
            History will appear after the first successful sensor sync.
          </CardContent>
        </Card>
      ) : (
        <div className="space-y-4">
          {sortedHistory.map((entry) => (
            <HistoryEntryCard key={entry.id} entry={entry} />
          ))}
        </div>
      )}
    </div>
  );
}
