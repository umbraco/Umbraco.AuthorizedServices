import { UmbContextToken } from "@umbraco-cms/backoffice/context-api";
import type { UmbWorkspaceContext } from "@umbraco-cms/backoffice/workspace";
import { AUTHORIZED_SERVICE_ENTITY_TYPE } from "../entities.js";
import type { AuthorizedServiceWorkspaceContext } from "./workspace.context.js";

export const AUTHORIZED_SERVICES_WORKSPACE_CONTEXT_ALIAS = "AuthorizedServicesWorkspaceContext";

export const AUTHORIZED_SERVICES_WORKSPACE_CONTEXT = new UmbContextToken<
  UmbWorkspaceContext,
  AuthorizedServiceWorkspaceContext
>(
  AUTHORIZED_SERVICES_WORKSPACE_CONTEXT_ALIAS,
  undefined,
  (context): context is AuthorizedServiceWorkspaceContext =>
    context.getEntityType?.() === AUTHORIZED_SERVICE_ENTITY_TYPE,
);
