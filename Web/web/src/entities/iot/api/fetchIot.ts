import { config } from "../../../config/api";
import type { IotDevice } from "../model/types";

const IOT_QUERY = `
  query GetIot {
    iot {
      devices {
        type
        name
        payload {
          co2
          pm25
          humidity
          energy
        }
      }
    }
  }
`;

interface GraphQlResponse {
  data?: {
    iot?: {
      devices?: IotDevice[];
    };
  };
  errors?: Array<{ message: string }>;
}

export async function fetchIot(options?: {
  baseUrl?: string;
  token?: string;
}): Promise<IotDevice[]> {
  const baseUrl = options?.baseUrl ?? config.gatewayApiUrl;
  const headers: Record<string, string> = {
    "Content-Type": "application/json",
  };

  if (options?.token) {
    headers.Authorization = `Bearer ${options.token}`;
  }

  const response = await fetch(baseUrl, {
    method: "POST",
    headers,
    credentials: "include",
    body: JSON.stringify({ query: IOT_QUERY }),
  });

  if (!response.ok) {
    throw new Error(`Failed to load IoT data (${response.status})`);
  }

  const payload = (await response.json()) as GraphQlResponse;

  if (payload.errors?.length) {
    throw new Error(payload.errors[0]?.message ?? "GraphQL request failed");
  }

  return payload.data?.iot?.devices ?? [];
}
