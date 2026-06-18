import { Cpu, LogOut, RefreshCw } from "lucide-react";
import { Button } from "../../../app/components/ui/button";
import { formatTimestamp } from "../../../entities/iot";

interface DashboardHeaderProps {
  onLogout: () => void;
  onRefresh: () => void;
  refreshing: boolean;
  lastUpdated: string | null;
}

export function DashboardHeader({
  onLogout,
  onRefresh,
  refreshing,
  lastUpdated,
}: DashboardHeaderProps) {
  return (
    <header className="sticky top-0 z-20 border-b border-slate-200 bg-white/85 backdrop-blur-xl">
      <div className="container mx-auto flex flex-col gap-4 px-4 py-4 lg:flex-row lg:items-center lg:justify-between">
        <div className="flex items-center gap-4">
          <div className="relative flex h-12 w-12 items-center justify-center rounded-2xl bg-gradient-to-br from-cyan-400 to-emerald-500 shadow-lg shadow-cyan-500/20">
            <Cpu className="h-6 w-6 text-white" />
            <span className="absolute -right-1 -top-1 h-3 w-3 rounded-full border-2 border-white bg-emerald-500" />
          </div>
          <div>
            <h1 className="text-2xl font-semibold text-slate-900">SensorHub</h1>
            <p className="text-sm text-slate-500">Smart home monitoring dashboard</p>
          </div>
        </div>

        <div className="flex flex-wrap items-center gap-3">
          {lastUpdated && (
            <span className="text-sm text-slate-500">
              Updated {formatTimestamp(lastUpdated)}
            </span>
          )}
          <Button
            variant="outline"
            className="border-slate-200 bg-white text-slate-700 hover:bg-slate-50"
            onClick={onRefresh}
            disabled={refreshing}
          >
            <RefreshCw className={`mr-2 h-4 w-4 ${refreshing ? "animate-spin" : ""}`} />
            Refresh
          </Button>
          <Button
            variant="outline"
            className="border-slate-200 bg-white text-slate-700 hover:bg-slate-50"
            onClick={onLogout}
          >
            <LogOut className="mr-2 h-4 w-4" />
            Logout
          </Button>
        </div>
      </div>
    </header>
  );
}
