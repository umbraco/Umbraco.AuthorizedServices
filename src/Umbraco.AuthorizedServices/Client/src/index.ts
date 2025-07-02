import type { UmbEntryPointOnInit } from "@umbraco-cms/backoffice/extension-api";
import { UMB_AUTH_CONTEXT } from "@umbraco-cms/backoffice/auth";
import { client } from "../generated/index.js";
import { umbHttpClient } from "@umbraco-cms/backoffice/http-client";
import { manifests as workspaceManifests } from "./workspace/manifests.js";
import { manifests as treeManifests } from "./tree/manifests.js";

export const onInit: UmbEntryPointOnInit = (_host, extensionRegistry) => {
  extensionRegistry.registerMany([...workspaceManifests, ...treeManifests]);

  _host.consumeContext(UMB_AUTH_CONTEXT, async (auth) => {
    if (!auth) return;

    client.setConfig(umbHttpClient.getConfig());
  });
};
