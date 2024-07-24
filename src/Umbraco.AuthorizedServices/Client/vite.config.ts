import { defineConfig, loadEnv } from "vite";
import tsconfigPaths from "vite-tsconfig-paths";


export default defineConfig(({ command, mode }) =>
{
  const env = loadEnv(mode, process.cwd(), '');

  return {
    define: {
      __APP_OUTDIR: env.outDir,
    },
    build: {
      lib: {
        entry: "src/index.ts",
        formats: ["es"],
      },
      outDir: env.outDir ?? "../wwwroot/App_Plugins/UmbracoAuthorizedServices",
      emptyOutDir: true,
      sourcemap: true,
      rollupOptions: {
        external: [/^@umbraco-cms/],
        onwarn: () => { },
      },
    },
    plugins: [tsconfigPaths()]
  }
});
