import type { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import {
  UmbTreeServerDataSourceBase,
  type UmbTreeAncestorsOfRequestArgs,
  type UmbTreeChildrenOfRequestArgs,
  type UmbTreeRootItemsRequestArgs,
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

// eslint-disable-next-line local-rules/no-direct-api-import
const getRootItems = (args: UmbTreeRootItemsRequestArgs) =>
  TreeService.children();

const getChildrenOf = (args: UmbTreeChildrenOfRequestArgs) => {
  if (args.parent.unique === null) {
    return getRootItems(args);
  }
  //eslint-disable-next-line local-rules/no-direct-api-import
  return TreeService.children({
    parentId: args.parent.unique!,
  });
};

const getAncestorsOf = (args: UmbTreeAncestorsOfRequestArgs) => {
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
