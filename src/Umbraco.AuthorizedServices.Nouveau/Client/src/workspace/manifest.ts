import { ManifestWorkspace } from "@umbraco-cms/backoffice/extension-registry";

const authorizedServiceWorkspaceManifest: ManifestWorkspace = {
  type: "workspace",
  alias: "authorizedservice.workspace",
  name: "Authorized Service Workspace",
  js: () => import("./workspace.element.js"),
  meta: {
    entityType: "authorizedservice-workspace"
  }
};

export const manifest = authorizedServiceWorkspaceManifest;
