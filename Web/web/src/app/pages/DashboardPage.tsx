import { useState } from "react";
import { useNavigate } from "react-router";
import { Alert, AlertDescription } from "../components/ui/alert";
import { Skeleton } from "../components/ui/skeleton";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "../components/ui/tabs";
import { computeMetrics } from "../../entities/iot";
import { useIotDashboard } from "../../features/iot/use-iot-dashboard";
import { DashboardHeader } from "../../widgets/dashboard-header";
import { IotCurrentView } from "../../widgets/iot-current";
import { IotHistoryView } from "../../widgets/iot-history";
import { IotStatusStrip, IotSummaryCards } from "../../widgets/iot-summary";

export function DashboardPage() {
  const navigate = useNavigate();
  const [selectedTab, setSelectedTab] = useState("current");
  const { devices, history, loading, refreshing, error, lastUpdated, refresh } =
    useIotDashboard();

  const metrics = computeMetrics(devices);

  const handleLogout = () => {
    navigate("/");
  };

  return (
    <div className="min-h-screen bg-[radial-gradient(circle_at_top,_rgba(34,211,238,0.12),_transparent_40%),linear-gradient(to_bottom,_#f8fafc,_#ecfeff)]">
      <DashboardHeader
        onLogout={handleLogout}
        onRefresh={refresh}
        refreshing={refreshing}
        lastUpdated={lastUpdated}
      />

      <main className="container mx-auto px-4 py-8">
        <div className="mb-8 space-y-4">
          <div>
            <p className="text-sm uppercase tracking-[0.25em] text-cyan-700/80">
              Live monitoring
            </p>
            <h2 className="mt-2 text-3xl font-semibold text-slate-900">
              Home sensor overview
            </h2>
          </div>
          <IotStatusStrip metrics={metrics} />
        </div>

        {error && (
          <Alert className="mb-6 border-rose-200 bg-rose-50 text-rose-700">
            <AlertDescription>{error}</AlertDescription>
          </Alert>
        )}

        {loading ? (
          <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
            {Array.from({ length: 4 }).map((_, index) => (
              <Skeleton key={index} className="h-32 rounded-2xl bg-slate-200/80" />
            ))}
          </div>
        ) : (
          <div className="mb-8">
            <IotSummaryCards metrics={metrics} />
          </div>
        )}

        <Tabs value={selectedTab} onValueChange={setSelectedTab} className="space-y-6">
          <TabsList className="grid h-auto w-full max-w-md grid-cols-2 border border-slate-200 bg-white/80 p-1 shadow-sm">
            <TabsTrigger
              value="current"
              className="data-[state=active]:bg-cyan-500 data-[state=active]:text-slate-950"
            >
              Current data
            </TabsTrigger>
            <TabsTrigger
              value="history"
              className="data-[state=active]:bg-cyan-500 data-[state=active]:text-slate-950"
            >
              History
            </TabsTrigger>
          </TabsList>

          <TabsContent value="current" className="space-y-6">
            {loading ? (
              <div className="grid gap-4 lg:grid-cols-2">
                {Array.from({ length: 4 }).map((_, index) => (
                  <Skeleton key={index} className="h-64 rounded-2xl bg-slate-200/80" />
                ))}
              </div>
            ) : (
              <IotCurrentView devices={devices} />
            )}
          </TabsContent>

          <TabsContent value="history" className="space-y-6">
            <IotHistoryView history={history} />
          </TabsContent>
        </Tabs>
      </main>
    </div>
  );
}
