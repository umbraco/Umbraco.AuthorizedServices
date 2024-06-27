import type {
  UmbTreeItemModel,
  UmbTreeRootModel,
} from "@umbraco-cms/backoffice/tree";
import type {
  AuthorizedServiceEntityType,
  AuthorizedServiceRootEntityType,
} from "../entities.js";

export interface AuthorizedServicesTreeItemModel extends UmbTreeItemModel {
  entityType: AuthorizedServiceEntityType;
}

export interface AuthorizedServicesTreeRootModel extends UmbTreeRootModel {
  entityType: AuthorizedServiceRootEntityType;
}
