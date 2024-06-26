import type { UmbEntryPointOnInit } from "@umbraco-cms/backoffice/extension-api";

import { manifests as sidebarAppManifests } from "./sidebar/manifest.js";
import { manifest as workspaceManifest  } from "./workspace/manifest.js";


export const onInit: UmbEntryPointOnInit = (_host, extensionRegistry) => {

  extensionRegistry.registerMany([...sidebarAppManifests, workspaceManifest]);

}
