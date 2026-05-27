import {
  AUTHORIZED_SERVICE_ROOT_ENTITY_TYPE,
  AUTHORIZED_SERVICE_ENTITY_TYPE,
} from "@umbraco-authorizedservices/entities";
import { AuthorizedServicesTreeRepository } from "./tree.repository.js";
import { manifests as menuManifests } from "./menu-item/manifests.js";

export const AUTHORIZED_SERVICES_TREE_REPOSITORY_ALIAS =
  "AuthorizedServices.Repository.Services.Tree";
export const AUTHORIZED_SERVICES_TREE_ALIAS =
  "AuthorizedServices.Tree.Services";

const treeRepository: UmbExtensionManifest = {
  type: "repository",
  alias: AUTHORIZED_SERVICES_TREE_REPOSITORY_ALIAS,
  name: "Authorized Services Tree Repository",
  api: AuthorizedServicesTreeRepository,
};

const treeManifests: Array<UmbExtensionManifest> = [
  {
    type: "tree",
    kind: "default",
    alias: AUTHORIZED_SERVICES_TREE_ALIAS,
    name: "Authorized Services Tree",
    meta: {
      repositoryAlias: AUTHORIZED_SERVICES_TREE_REPOSITORY_ALIAS,
    },
  }
];

const treeItemManifests: Array<UmbExtensionManifest> = [
  {
    type: "treeItem",
    kind: "default",
    alias: "AuthorizedServices.TreeItem.Service",
    name: "Authorized Services Tree Item",
    forEntityTypes: [
      AUTHORIZED_SERVICE_ROOT_ENTITY_TYPE,
      AUTHORIZED_SERVICE_ENTITY_TYPE,
    ],
  }
];

export const manifests = [
  treeRepository,
  ...treeManifests,
  ...treeItemManifests,
  ...menuManifests,
];
