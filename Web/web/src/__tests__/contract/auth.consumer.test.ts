import path from "node:path";
import { readFileSync } from "node:fs";
import { PactV4, MatchersV3 } from "@pact-foundation/pact";
import { describe, expect, it } from "vitest";
import { register } from "../../app/pages/login/api/Register";
import { login } from "../../app/pages/login/api/Login";
import { PACTS_DIR } from "./pactPaths";

const { string, regex } = MatchersV3;

const constants = JSON.parse(
  readFileSync(path.join(PACTS_DIR, "contract-constants.json"), "utf8"),
) as {
  loginEmail: string;
  loginPassword: string;
  userName: string;
};

describe("Web -> Auth contract", () => {
  const provider = new PactV4({
    consumer: "Web",
    provider: "Auth",
    dir: PACTS_DIR,
  });

  it("register returns 200 and sets GuardPass cookie", async () => {
    await provider
      .addInteraction()
      .given("registration is available")
      .uponReceiving("a registration request")
      .withRequest("POST", "/auth/register", (builder) => {
        builder.headers({ "Content-Type": "application/json" });
        builder.jsonBody({
          userName: string(constants.userName),
          email: regex(/^[\w.-]+@example\.com$/, constants.loginEmail),
          password: string(constants.loginPassword),
        });
      })
      .willRespondWith(200, (builder) => {
        builder.headers({
          "Set-Cookie": regex(
            /^GuardPass=[^;]+; path=\/; samesite=strict; httponly$/,
            "GuardPass=signed-jwt; path=/; samesite=strict; httponly",
          ),
        });
      })
      .executeTest(async (mockServer) => {
        const ok = await register(
          constants.userName,
          constants.loginEmail,
          constants.loginPassword,
          mockServer.url,
        );
        expect(ok).toBe(true);
      });
  });

  it("login returns 200 and sets GuardPass cookie for valid credentials", async () => {
    await provider
      .addInteraction()
      .given("user exists with valid credentials")
      .uponReceiving("a successful login request")
      .withRequest("POST", "/auth/login", (builder) => {
        builder.headers({ "Content-Type": "application/json" });
        builder.jsonBody({
          email: string(constants.loginEmail),
          password: string(constants.loginPassword),
        });
      })
      .willRespondWith(200, (builder) => {
        builder.headers({
          "Set-Cookie": regex(
            /^GuardPass=[^;]+; path=\/; samesite=strict; httponly$/,
            "GuardPass=signed-jwt; path=/; samesite=strict; httponly",
          ),
        });
      })
      .executeTest(async (mockServer) => {
        const ok = await login(
          constants.loginEmail,
          constants.loginPassword,
          mockServer.url,
        );
        expect(ok).toBe(true);
      });
  });

  it("login returns 401 for invalid credentials", async () => {
    await provider
      .addInteraction()
      .given("credentials are invalid")
      .uponReceiving("a failed login request")
      .withRequest("POST", "/auth/login", (builder) => {
        builder.headers({ "Content-Type": "application/json" });
        builder.jsonBody({
          email: string("unknown@example.com"),
          password: string("wrong-password"),
        });
      })
      .willRespondWith(401)
      .executeTest(async (mockServer) => {
        const ok = await login(
          "unknown@example.com",
          "wrong-password",
          mockServer.url,
        );
        expect(ok).toBe(false);
      });
  });
});
