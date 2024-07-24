import { defineConfig } from "vite";
import { outputPath } from "./config.outputPath.js";
import { baseConfig } from './vite.config.js';

console.log(outputPath)
export default defineConfig(() => {
  const config = {
    ...baseConfig
  };
  config.build.outDir = outputPath;

  return config;
});
