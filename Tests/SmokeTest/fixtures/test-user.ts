export interface TestUser {
  name: string;
  email: string;
  password: string;
}

export function createTestUser(label = "smoke"): TestUser {
  const suffix = `${Date.now()}-${Math.random().toString(36).slice(2, 8)}`;

  return {
    name: `E2E ${label}`,
    email: `e2e-${label}-${suffix}@example.com`,
    password: "SecurePass1!",
  };
}
