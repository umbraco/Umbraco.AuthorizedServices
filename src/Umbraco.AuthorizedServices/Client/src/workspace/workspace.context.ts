import { UmbWorkspaceRouteManager, type UmbWorkspaceContext } from "@umbraco-cms/backoffice/workspace";
import type { UmbControllerHostElement } from "@umbraco-cms/backoffice/controller-api";
import { UmbObjectState } from "@umbraco-cms/backoffice/observable-api";
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { UmbContextBase } from "@umbraco-cms/backoffice/class-api";
import { AUTHORIZED_SERVICE_ENTITY_TYPE } from "@umbraco-authorizedservices/entities";
import { type AuthorizedServiceDisplay, Service } from "@umbraco-authorizedservices/generated";
import { WORKSPACE_ALIAS } from "./manifests.js";
import { AuthorizedServiceWorkspaceEditorElement } from "./workspace.element.js";
import { AUTHORIZED_SERVICES_WORKSPACE_CONTEXT } from "./workspace.context-token.js";

export class AuthorizedServiceWorkspaceContext
  extends UmbContextBase
  implements UmbWorkspaceContext
{
  #data = new UmbObjectState<AuthorizedServiceDisplay | undefined>(undefined);

  readonly data = this.#data.asObservable();
  readonly alias = this.#data.asObservablePart((data) => data?.alias);
  readonly name = this.#data.asObservablePart((data) => data?.displayName);

  readonly workspaceAlias: string;
  readonly routes: UmbWorkspaceRouteManager;

  constructor(host: UmbControllerHostElement) {
    super(host, WORKSPACE_ALIAS);
    this.provideContext(AUTHORIZED_SERVICES_WORKSPACE_CONTEXT, this);

    this.workspaceAlias = WORKSPACE_ALIAS;
    this.routes = new UmbWorkspaceRouteManager(host);

    this.routes.setRoutes([
      {
        path: "edit/:alias",
        component: AuthorizedServiceWorkspaceEditorElement,
        setup: (_component, info) => {
          const alias = info.match.params.alias;
          this.load(alias);
        },
      },
    ]);
  }

  async load(alias: string) {
    
		const { data } = await tryExecute(
      this,
      Service.getServiceByAlias({ path: { alias } })
		);

    if (data) {
      this.#data.update(data);
    }
  }

  getData() {
    return this.#data.getValue();
  }

  getEntityType(): string {
    return AUTHORIZED_SERVICE_ENTITY_TYPE;
  }

  getName() {
    return this.#data.getValue()?.displayName;
  }

  destroy() {
    this.#data.destroy();
  }
}

export { AuthorizedServiceWorkspaceContext as api };
