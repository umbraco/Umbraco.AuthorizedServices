import { defineConfig } from "vite";
import tsconfigPaths from "vite-tsconfig-paths";

export const baseConfig = {
  build: {
    lib: {
      entry: "src/index.ts",
      formats: ["es"],
    },
    outDir: "../wwwroot",
    emptyOutDir: true,
    sourcemap: true,
    rollupOptions: {
      external: [/^@umbraco-cms/],
      onwarn: () => { },
    },
  },
  plugins: [tsconfigPaths()]
}

export default defineConfig({
  ...baseConfig
});
