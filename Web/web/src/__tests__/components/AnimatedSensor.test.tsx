import { render } from "@testing-library/react";

// motion/react uses browser-animation APIs that jsdom cannot run; replace with
// a lightweight pass-through so the component renders as plain HTML elements.
vi.mock("motion/react", () => ({
  motion: new Proxy({} as Record<string, string>, {
    get(_target, tag: string) {
      return tag; // e.g. motion.span → "span" — React renders a real <span>
    },
  }),
}));

import { AnimatedSensor } from "../../app/components/AnimatedSensor";

describe("AnimatedSensor", () => {
  it("renders without crashing", () => {
    const { container } = render(<AnimatedSensor isAnimating={false} />);
    expect(container.firstChild).not.toBeNull();
  });

  it("renders exactly three animated ring elements", () => {
    render(<AnimatedSensor isAnimating={false} />);
    // The three motion.span rings get an absolute-positioned class
    const rings = document.querySelectorAll(".absolute.rounded-full");
    expect(rings.length).toBeGreaterThanOrEqual(3);
  });

  it("renders the Radio icon wrapper that signals a live sensor", () => {
    render(<AnimatedSensor isAnimating={false} />);
    // The icon container has a gradient background utility class
    const iconWrapper = document.querySelector(".from-cyan-400");
    expect(iconWrapper).not.toBeNull();
  });

  it("renders the Wifi status icon in the bottom-right corner", () => {
    render(<AnimatedSensor isAnimating={false} />);
    const wifiIcon = document.querySelector(".bottom-1.right-1");
    expect(wifiIcon).not.toBeNull();
  });
});
