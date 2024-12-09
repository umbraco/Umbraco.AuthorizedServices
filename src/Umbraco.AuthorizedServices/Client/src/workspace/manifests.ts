import { AUTHORIZED_SERVICE_ENTITY_TYPE } from "@umbraco-authorizedservices/entities";

export const WORKSPACE_ALIAS = "AuthorizedServices.Workspace";

const workspaceManifest: UmbExtensionManifest = {
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
