import type {
  ManifestRepository,
  ManifestTreeStore,
} from "@umbraco-cms/backoffice/extension-registry";
import {
  AUTHORIZED_SERVICE_ROOT_ENTITY_TYPE,
  AUTHORIZED_SERVICE_ENTITY_TYPE,
} from "@umbraco-authorizedservices/entities";
import { AuthorizedServicesTreeRepository } from "./tree.repository.js";
import { AuthorizedServicesTreeStore } from "./tree.store.js";
import { manifests as menuManifests } from "./menu-item/manifests.js";

export const AUTHORIZED_SERVICES_TREE_REPOSITORY_ALIAS =
  "AuthorizedServices.Repository.Services.Tree";
export const AUTHORIZED_SERVICES_TREE_STORE_ALIAS =
  "AuthorizedServices.Store.Services.Tree";
export const AUTHORIZED_SERVICES_TREE_ALIAS =
  "AuthorizedServices.Tree.Services";

const treeRepository: ManifestRepository = {
  type: "repository",
  alias: AUTHORIZED_SERVICES_TREE_REPOSITORY_ALIAS,
  name: "Authorized Services Tree Repository",
  api: AuthorizedServicesTreeRepository,
};

const treeStore: ManifestTreeStore = {
  type: "treeStore",
  alias: AUTHORIZED_SERVICES_TREE_STORE_ALIAS,
  name: "Authorized Services  Tree Store",
  api: AuthorizedServicesTreeStore,
};

const tree: UmbExtensionManifest = {
  type: "tree",
  kind: "default",
  alias: AUTHORIZED_SERVICES_TREE_ALIAS,
  name: "Authorized Services Tree",
  meta: {
    repositoryAlias: AUTHORIZED_SERVICES_TREE_REPOSITORY_ALIAS,
  },
};

const treeItem: UmbExtensionManifest = {
  type: "treeItem",
  kind: "default",
  alias: "AuthorizedServices.TreeItem.Service",
  name: "Authorized Services Tree Item",
  forEntityTypes: [
    AUTHORIZED_SERVICE_ROOT_ENTITY_TYPE,
    AUTHORIZED_SERVICE_ENTITY_TYPE,
  ],
};

export const manifests = [
  treeRepository,
  treeStore,
  tree,
  treeItem,
  ...menuManifests,
];
