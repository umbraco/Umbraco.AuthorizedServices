import type { ManifestMenuItemTreeKind } from "@umbraco-cms/backoffice/extension-registry";

const menuItem: ManifestMenuItemTreeKind = {
  type: "menuItem",
  kind: "tree",
  alias: "AuthorizedServices.MenuItem.Service",
  name: "Authorized Services Service Menu Item",
  weight: 600,
  meta: {
    label: "Authorized Services",
    treeAlias: "AuthorizedServices.Tree.Services",
    menus: ['Umb.Menu.AdvancedSettings'],
  },
};

export const manifests = [menuItem];
