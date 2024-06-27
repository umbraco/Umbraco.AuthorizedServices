import type { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import type { UmbApi } from "@umbraco-cms/backoffice/extension-api";
import { UmbTreeRepositoryBase } from "@umbraco-cms/backoffice/tree";
import { AUTHORIZED_SERVICE_ROOT_ENTITY_TYPE } from "../entities.js";
import type {
  AuthorizedServicesTreeItemModel,
  AuthorizedServicesTreeRootModel,
} from "./types.js";
import { AuthorizedServicesTreeServerDataSource } from "./tree.server.data-source.js";
import { AUTHORIZED_SERVICES_TREE_STORE_CONTEXT } from "./tree.store.js";

export class AuthorizedServicesTreeRepository
  extends UmbTreeRepositoryBase<
    AuthorizedServicesTreeItemModel,
    AuthorizedServicesTreeRootModel
  >
  implements UmbApi
{
  constructor(host: UmbControllerHost) {
    super(
      host,
      AuthorizedServicesTreeServerDataSource,
      AUTHORIZED_SERVICES_TREE_STORE_CONTEXT
    );
  }

  async requestTreeRoot() {
    const data: AuthorizedServicesTreeRootModel = {
      unique: null,
      entityType: AUTHORIZED_SERVICE_ROOT_ENTITY_TYPE,
      name: "Authorized Services",
      hasChildren: true,
      isFolder: true,
    };

    return { data };
  }
}
