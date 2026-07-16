import path from "node:path";
import { fileURLToPath } from "node:url";

const testDir = path.dirname(fileURLToPath(import.meta.url));

export const PACTS_DIR = path.resolve(testDir, "../../../../../Tests/pacts");
