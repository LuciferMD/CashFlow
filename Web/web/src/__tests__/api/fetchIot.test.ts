import { vi } from "vitest";
import { fetchIot } from "../../entities/iot/api/fetchIot";

const mockFetch = vi.fn();

beforeEach(() => {
  vi.stubGlobal("fetch", mockFetch);
  mockFetch.mockReset();
});

afterEach(() => {
  vi.unstubAllGlobals();
});

function mockOkResponse(body: unknown) {
  mockFetch.mockResolvedValue({
    ok: true,
    json: async () => body,
  });
}

describe("fetchIot", () => {
  it("returns devices from a successful GraphQL response", async () => {
    const devices = [
      { type: "energy", name: "Kitchen", payload: { co2: null, pm25: null, humidity: null, energy: 3.5 } },
    ];
    mockOkResponse({ data: { iot: { devices } } });

    const result = await fetchIot();
    expect(result).toHaveLength(1);
    expect(result[0].name).toBe("Kitchen");
  });

  it("returns an empty array when the devices field is missing", async () => {
    mockOkResponse({ data: { iot: {} } });
    const result = await fetchIot();
    expect(result).toEqual([]);
  });

  it("throws when the HTTP response is not OK (e.g. 401)", async () => {
    mockFetch.mockResolvedValue({ ok: false, status: 401 });
    await expect(fetchIot()).rejects.toThrow("401");
  });

  it("throws the first GraphQL error message", async () => {
    mockOkResponse({ errors: [{ message: "Unauthorized access" }] });
    await expect(fetchIot()).rejects.toThrow("Unauthorized access");
  });

  it("sends a POST request with Content-Type application/json", async () => {
    mockOkResponse({ data: { iot: { devices: [] } } });
    await fetchIot();

    const [, options] = mockFetch.mock.calls[0] as [string, RequestInit];
    expect(options.method).toBe("POST");
    expect((options.headers as Record<string, string>)["Content-Type"]).toBe("application/json");
  });
});
