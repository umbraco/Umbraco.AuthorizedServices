import { UmbContextToken } from "@umbraco-cms/backoffice/context-api";
import type { UmbWorkspaceContext } from "@umbraco-cms/backoffice/workspace";
import { AUTHORIZED_SERVICE_ENTITY_TYPE } from "../entities.js";
import type { WorkspaceContext } from "./workspace.context.js";

export const AUTHORIZED_SERVICES_WORKSPACE_CONTEXT = new UmbContextToken<
  UmbWorkspaceContext,
  WorkspaceContext
>(
  "UmbWorkspaceContext",
  undefined,
  (context): context is WorkspaceContext =>
    context.getEntityType?.() === AUTHORIZED_SERVICE_ENTITY_TYPE,
);
