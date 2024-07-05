import type { UmbWorkspaceContext } from "@umbraco-cms/backoffice/workspace";
import { UmbWorkspaceRouteManager } from "@umbraco-cms/backoffice/workspace";
import type { UmbControllerHostElement } from "@umbraco-cms/backoffice/controller-api";
import { UmbObjectState } from "@umbraco-cms/backoffice/observable-api";
import { AUTHORIZED_SERVICE_ENTITY_TYPE } from "../entities.js";
import { WORKSPACE_ALIAS } from "./manifests.js";
import { UmbContextBase } from "@umbraco-cms/backoffice/class-api";
import AuthorizedServiceWorkspaceEditorElement from "./workspace.element.js";
import { AuthorizedServiceDisplay, ServiceService } from "../../generated/index.js";
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

export class WorkspaceContext
  extends UmbContextBase<AuthorizedServiceDisplay>
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

		const { data } = await tryExecuteAndNotify(
			this,
			ServiceService.getByAlias({alias})
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

export const api = WorkspaceContext;
