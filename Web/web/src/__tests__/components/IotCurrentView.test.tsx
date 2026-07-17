import { render, screen } from "@testing-library/react";
import { IotCurrentView } from "../../widgets/iot-current/ui/IotCurrentView";
import type { IotDevice } from "../../entities/iot/model/types";

function makeDevice(
  type: IotDevice["type"],
  name: string,
  payload: Partial<IotDevice["payload"]> = {},
): IotDevice {
  return {
    type,
    name,
    payload: { co2: null, pm25: null, humidity: null, energy: null, ...payload },
  };
}

describe("IotCurrentView", () => {
  it("shows the empty-state message when there are no devices", () => {
    render(<IotCurrentView devices={[]} />);
    expect(screen.getByText(/No sensor readings available/)).toBeInTheDocument();
  });

  it("renders a card with the room name", () => {
    render(<IotCurrentView devices={[makeDevice("energy", "Living Room", { energy: 1 })]} />);
    expect(screen.getByText("Living Room")).toBeInTheDocument();
  });

  it("shows formatted energy value inside the card", () => {
    render(<IotCurrentView devices={[makeDevice("energy", "Kitchen", { energy: 4.5 })]} />);
    expect(screen.getByText("4.5 kWh")).toBeInTheDocument();
  });

  it('shows at least one "—" placeholder when energy is null', () => {
    render(<IotCurrentView devices={[makeDevice("energy", "Office", { energy: null })]} />);
    // Multiple sensor cells show "—" when their values are null
    expect(screen.getAllByText("—").length).toBeGreaterThan(0);
  });

  it("shows the CO₂ value in ppm", () => {
    render(
      <IotCurrentView
        devices={[makeDevice("air_quality", "Lab", { co2: 750, pm25: 5, humidity: 40 })]}
      />,
    );
    expect(screen.getByText("750 ppm")).toBeInTheDocument();
  });

  it("shows the Motion badge when the room has a motion device", () => {
    render(<IotCurrentView devices={[makeDevice("motion", "Hall")]} />);
    expect(screen.getByText("Motion")).toBeInTheDocument();
  });
});
