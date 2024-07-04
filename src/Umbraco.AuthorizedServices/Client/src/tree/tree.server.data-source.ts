import type { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import {
  UmbTreeServerDataSourceBase,
  type UmbTreeChildrenOfRequestArgs,
} from "@umbraco-cms/backoffice/tree";
import {
  type AuthorizedServiceTreeItemResponseModel,
  TreeService,
} from "../../generated/index.js";
import {
  AUTHORIZED_SERVICE_ENTITY_TYPE,
  AUTHORIZED_SERVICE_ROOT_ENTITY_TYPE,
} from "../entities.js";
import type { AuthorizedServicesTreeItemModel } from "./types.js";

export class AuthorizedServicesTreeServerDataSource extends UmbTreeServerDataSourceBase<
  any,
  AuthorizedServicesTreeItemModel
> {
  constructor(host: UmbControllerHost) {
    super(host, {
      getRootItems,
      getChildrenOf,
      getAncestorsOf,
      mapper,
    });
  }
}

const getRootItems = () =>
  TreeService.root();

const getChildrenOf = (args: UmbTreeChildrenOfRequestArgs) => {
  if (args.parent.unique === null) {
    return getRootItems();
  }
  throw new Error("Not supported for the authorized services tree");
};

const getAncestorsOf = () => {
  throw new Error("Not supported for the authorized services tree");
};

const mapper = (
  item: AuthorizedServiceTreeItemResponseModel
): AuthorizedServicesTreeItemModel => {
  return {
    unique: item.unique,
    parent: {
      unique: null,
      entityType: AUTHORIZED_SERVICE_ROOT_ENTITY_TYPE,
    },
    name: item.name,
    entityType: AUTHORIZED_SERVICE_ENTITY_TYPE,
    isFolder: false,
    hasChildren: false,
  };
};
