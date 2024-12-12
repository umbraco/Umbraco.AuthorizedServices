
export const manifests: Array<UmbExtensionManifest> = [
  {
  type: "menuItem",
  kind: "tree",
  alias: "AuthorizedServices.MenuItem.Service",
  name: "Authorized Services Service Menu Item",
  weight: 1,
  meta: {
    label: "Authorized Services",
    treeAlias: "AuthorizedServices.Tree.Services",
    menus: ['Umb.Menu.AdvancedSettings'],
  },
  }
];
