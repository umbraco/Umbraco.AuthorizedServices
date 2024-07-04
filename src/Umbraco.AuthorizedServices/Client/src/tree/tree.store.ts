import { UmbContextToken } from "@umbraco-cms/backoffice/context-api";
import type { UmbControllerHostElement } from "@umbraco-cms/backoffice/controller-api";
import { UmbUniqueTreeStore } from "@umbraco-cms/backoffice/tree";

export class AuthorizedServicesTreeStore extends UmbUniqueTreeStore {
  constructor(host: UmbControllerHostElement) {
    super(host, AUTHORIZED_SERVICES_TREE_STORE_CONTEXT.toString());
  }
}

export const AUTHORIZED_SERVICES_TREE_STORE_CONTEXT =
  new UmbContextToken<AuthorizedServicesTreeStore>(
    "AuthorizedServicesTreeStore"
  );
