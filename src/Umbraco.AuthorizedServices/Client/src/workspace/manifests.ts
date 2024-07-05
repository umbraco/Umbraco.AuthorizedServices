import { ManifestWorkspace } from "@umbraco-cms/backoffice/extension-registry";
import { AUTHORIZED_SERVICE_ENTITY_TYPE } from "../entities.js";

export const WORKSPACE_ALIAS = "AuthorizedServices.Workspace";
const workspaceManifest: ManifestWorkspace = {
  type: "workspace",
  kind: 'routable',
  alias: WORKSPACE_ALIAS,
  name: "Authorized Service Workspace",
  api: () => import("./workspace.context.js"),
  meta: {
    entityType: AUTHORIZED_SERVICE_ENTITY_TYPE
  }
};

export const manifest = workspaceManifest;
