import { render, screen } from "@testing-library/react";
import { IotSummaryCards, IotStatusStrip } from "../../widgets/iot-summary/ui/IotSummaryCards";
import type { IotMetrics } from "../../entities/iot/model/types";

function makeMetrics(overrides: Partial<IotMetrics> = {}): IotMetrics {
  return {
    totalEnergy: 0,
    avgCo2: null,
    avgPm25: null,
    avgHumidity: null,
    motionZones: 0,
    airQualityAlerts: 0,
    roomCount: 0,
    ...overrides,
  };
}

describe("IotSummaryCards", () => {
  it("displays totalEnergy with one decimal and kWh unit", () => {
    render(<IotSummaryCards metrics={makeMetrics({ totalEnergy: 12.3456, roomCount: 2 })} />);
    expect(screen.getByText(/12\.3 kWh/)).toBeInTheDocument();
  });

  it("shows 'No sensors' for CO₂ when avgCo2 is null", () => {
    // Provide humidity so only the CO₂ card shows "No sensors"
    render(<IotSummaryCards metrics={makeMetrics({ avgHumidity: 45 })} />);
    expect(screen.getByText("No sensors")).toBeInTheDocument();
  });

  it("displays the rounded humidity percentage when avgHumidity is set", () => {
    render(<IotSummaryCards metrics={makeMetrics({ avgHumidity: 47.9 })} />);
    expect(screen.getByText(/48%/)).toBeInTheDocument();
  });

  it("shows the air quality alert count when airQualityAlerts > 0", () => {
    render(<IotSummaryCards metrics={makeMetrics({ airQualityAlerts: 3 })} />);
    expect(screen.getByText(/3 rooms need attention/)).toBeInTheDocument();
  });

  it("shows the safe-limits message when airQualityAlerts is 0", () => {
    render(<IotSummaryCards metrics={makeMetrics({ airQualityAlerts: 0 })} />);
    expect(screen.getByText(/All rooms within safe limits/)).toBeInTheDocument();
  });

  it("shows motionZones count in the Live Activity card", () => {
    render(<IotSummaryCards metrics={makeMetrics({ motionZones: 5 })} />);
    expect(screen.getByText(/5 zones/)).toBeInTheDocument();
  });
});

describe("IotStatusStrip", () => {
  it("displays the number of rooms online", () => {
    render(<IotStatusStrip metrics={makeMetrics({ roomCount: 4 })} />);
    expect(screen.getByText(/4 rooms online/)).toBeInTheDocument();
  });

  it("displays the number of motion sensors", () => {
    render(<IotStatusStrip metrics={makeMetrics({ motionZones: 7 })} />);
    expect(screen.getByText(/7 motion sensors/)).toBeInTheDocument();
  });
});
