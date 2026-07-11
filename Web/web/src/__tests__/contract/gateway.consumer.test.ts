import { PactV4, MatchersV3 } from "@pact-foundation/pact";
import { describe, expect, it } from "vitest";
import { fetchIot } from "../../entities/iot/api/fetchIot";
import { PACTS_DIR } from "./pactPaths";

const { string, integer, like, eachLike, regex } = MatchersV3;

const IOT_QUERY = `query GetIot { iot { devices { type name payload { co2 pm25 humidity energy } } } }`;

describe("Web -> Gateway contract", () => {
  const provider = new PactV4({
    consumer: "Web",
    provider: "Gateway",
    dir: PACTS_DIR,
  });

  it("returns IoT devices from the GraphQL query", async () => {
    await provider
      .addInteraction()
      .given("authenticated user requests IoT devices")
      .uponReceiving("a GraphQL request for IoT devices")
      .withRequest("POST", "/graphql", (builder) => {
        builder.headers({
          "Content-Type": "application/json",
          Authorization: string("Bearer contract-test-jwt"),
        });
        builder.jsonBody({
          query: string(IOT_QUERY),
        });
      })
      .willRespondWith(200, (builder) => {
        builder.headers({
          "Content-Type": regex(
            /^application\/(json|graphql-response\+json)(;.*)?$/,
            "application/json",
          ),
        });
        builder.jsonBody({
          data: {
            iot: {
              devices: eachLike({
                type: string("sensor"),
                name: string("Kitchen"),
                payload: like({
                  co2: integer(400),
                  pm25: integer(10),
                  humidity: integer(60),
                  energy: like(1.5),
                }),
              }),
            },
          },
        });
      })
      .executeTest(async (mockServer) => {
        const devices = await fetchIot({
          baseUrl: `${mockServer.url}/graphql`,
          token: "contract-test-jwt",
        });

        expect(devices).toHaveLength(1);
        expect(devices[0]?.name).toBe("Kitchen");
        expect(devices[0]?.type).toBe("sensor");
      });
  });
});
