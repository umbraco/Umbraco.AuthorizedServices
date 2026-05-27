import type { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import {
  UmbTreeServerDataSourceBase,
  type UmbTreeAncestorsOfRequestArgs,
  type UmbTreeChildrenOfRequestArgs,
  type UmbTreeRootItemsRequestArgs,
} from "@umbraco-cms/backoffice/tree";
import {
  type AuthorizedServiceTreeItemResponseModel,
  Tree,
} from "@umbraco-authorizedservices/generated";
import {
  AUTHORIZED_SERVICE_ENTITY_TYPE,
  AUTHORIZED_SERVICE_ROOT_ENTITY_TYPE,
} from "@umbraco-authorizedservices/entities";
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

// eslint-disable-next-line local-rules/no-direct-api-import, @typescript-eslint/no-unused-vars
const getRootItems = async (_args: UmbTreeRootItemsRequestArgs) => {
  const { data } = await Tree.getTree();
  if (!data) return { error: undefined };
  // hey-api widens the backend `long Total` to `number | string`; coerce back to `number`
  // to satisfy UmbTargetPagedModel.
  return { data: { ...data, total: Number(data.total), totalBefore: 0, totalAfter: 0 } };
};

const getChildrenOf = (args: UmbTreeChildrenOfRequestArgs) => {
  if (args.parent.unique === null) {
    return getRootItems(args as UmbTreeRootItemsRequestArgs);
  }
  throw new Error("Not supported for the authorized services tree");
};

// eslint-disable-next-line @typescript-eslint/no-unused-vars
const getAncestorsOf = (_args: UmbTreeAncestorsOfRequestArgs) => {
  throw new Error("Not supported for the authorized services tree");
};

const mapper = (
  item: AuthorizedServiceTreeItemResponseModel
): AuthorizedServicesTreeItemModel => {
  return { ...item, ...{
    parent: {
      unique: null,
      entityType: AUTHORIZED_SERVICE_ROOT_ENTITY_TYPE,
    },
    entityType: AUTHORIZED_SERVICE_ENTITY_TYPE,
    isFolder: false,
    hasChildren: false,
  }};
};
