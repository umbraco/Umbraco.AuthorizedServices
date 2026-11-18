import type { UmbEntryPointOnInit } from "@umbraco-cms/backoffice/extension-api";
import { UMB_AUTH_CONTEXT } from "@umbraco-cms/backoffice/auth";
import { client } from "../generated/index.js";
import { manifests as workspaceManifests } from "./workspace/manifests.js";
import { manifests as treeManifests } from "./tree/manifests.js";

export const onInit: UmbEntryPointOnInit = async (host, extensionRegistry) => {
  // Register first so the tree and workspace aren't held up waiting for the auth context.
  extensionRegistry.registerMany([...workspaceManifests, ...treeManifests]);

  // getContext resolves once the context is provided, as the docs recommend for a one-off configuration.
  const authContext = await host.getContext(UMB_AUTH_CONTEXT);
  if (!authContext) {
    console.warn("UMB_AUTH_CONTEXT is not available, so the Authorized Services API client is not configured");
    return;
  }

  authContext.configureClient(client);
};
