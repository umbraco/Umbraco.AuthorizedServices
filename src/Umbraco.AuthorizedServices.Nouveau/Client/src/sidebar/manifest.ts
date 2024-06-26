import {
  ManifestSectionSidebarApp,
  ManifestMenu,
  ManifestMenuItem
} from "@umbraco-cms/backoffice/extension-registry";

const authorizedServicesSidebarAppManifest: ManifestSectionSidebarApp = {
  type: "sectionSidebarApp",
  kind: "menu",
  alias: 'authorizedservices.sidebar.app',
  name: 'AuthorizedServices Sidebar App',
  meta: {
    label: "Third Party",
    menu: "authorizedservices.menu"

  },
  conditions: [
    {
      alias: "Umb.Condition.SectionAlias",
      match: "Umb.Section.Settings"
    }
  ]
};

const authorizedServicesMenuManifest: ManifestMenu = {
  type: "menu",
  alias: "authorizedservices.menu",
  name: "AuthorizedServices Menu",
  meta: {
    label: "Authorized Services"
  }
};

const authorizedServicesMenuItemManifest: ManifestMenuItem = {
  type: "menuItem",
  alias: "authorizedservices.menu.item",
  name: "AuthorizedServices Menu Item",
  meta: {
    label: "Authorized Services",
    icon: "icon-folder",
    entityType: "authorizedservice-workspace",
    menus: [ "authorizedservices.menu"]
  }
}

const nestedMenuItems: ManifestMenuItem[] = [
  {
    type: "menuItem",
    alias: "menu.item.amazon",
    name: "Menu Item Amazon",
    meta: {
      menus: [ "authorizedservices.menu" ],
      icon: "icon-settings",
      label: "Amazon",
      entityType: "authorizedservice-workspace",
    }
  },
  {
    type: "menuItem",
    alias: "menu.item.hubspot",
    name: "Menu Item Hubspot",
    meta: {
      menus: [ "authorizedservices.menu" ],
      icon: "icon-settings",
      label: "HubSpot",
      entityType: "authorizedservice-workspace",
    }
  }
];

export const manifests = [
  authorizedServicesSidebarAppManifest,
  authorizedServicesMenuManifest,
  authorizedServicesMenuItemManifest,
  ...nestedMenuItems
];
