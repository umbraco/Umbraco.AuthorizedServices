import { defineConfig } from "vite";
import { outputPath } from "./config.outputPath.js";
import { baseConfig } from "./vite.config.js";

export default defineConfig({
  ...baseConfig,
  ...{ build: { ...baseConfig.build, ...{ outDir: outputPath } } },
});
