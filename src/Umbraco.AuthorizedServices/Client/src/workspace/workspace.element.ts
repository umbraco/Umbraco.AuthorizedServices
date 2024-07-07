import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import { LitElement, customElement, html, css, state, when } from "@umbraco-cms/backoffice/external/lit";
import { ServiceService, type AuthorizedServiceDisplay } from "@umbraco-authorizedservices/generated";
import { tryExecute, tryExecuteAndNotify } from "@umbraco-cms/backoffice/resources";
import { UMB_NOTIFICATION_CONTEXT } from "@umbraco-cms/backoffice/notification";
import { AUTHORIZED_SERVICES_WORKSPACE_CONTEXT } from "./workspace.context-token.js";

const elementName = "authorized-service-workspace";

enum AuthenticationMethod {
  None = "None",
  OAuth1 = "OAuth1",
  OAuth2AuthorizationCode = "OAuth2AuthorizationCode",
  OAuth2ClientCredentials = "OAuth2ClientCredentials",
  ApiKey = "ApiKey"
}

@customElement(elementName)
export class AuthorizedServiceWorkspaceEditorElement extends UmbElementMixin(LitElement) {

  #workspaceContext?: typeof AUTHORIZED_SERVICES_WORKSPACE_CONTEXT.TYPE;
  #notificationContext?: typeof UMB_NOTIFICATION_CONTEXT.TYPE;

  @state()
  private _service?: AuthorizedServiceDisplay;

  @state()
  private _authenticationMethod = AuthenticationMethod.None;

  @state()
  private _sampleRequestResponse?: string = undefined;

  constructor() {
    super();

    this.consumeContext(AUTHORIZED_SERVICES_WORKSPACE_CONTEXT, (context) => {
      this.#workspaceContext = context;
			this.#observeService();
		});

    this.consumeContext(UMB_NOTIFICATION_CONTEXT, (instance) => {
      this.#notificationContext = instance;
    });
	}

	#observeService() {
		if (!this.#workspaceContext) return;
		this.observe(this.#workspaceContext.data, (data) => {
      if (!data) return;

      this._service = data;
      this._authenticationMethod = data.authenticationMethod as AuthenticationMethod;
    });
	}

  #isOAuth1 = () => this._authenticationMethod === AuthenticationMethod.OAuth1;
  #isOAuth2AuthorizationCode = () => this._authenticationMethod === AuthenticationMethod.OAuth2AuthorizationCode;
  #isOAuth2ClientCredentials = () => this._authenticationMethod === AuthenticationMethod.OAuth2ClientCredentials;
  #isApiKey = () => this._authenticationMethod === AuthenticationMethod.ApiKey;

  async #authorizeAccess() {
    if (this.#isOAuth2ClientCredentials()) {
      const { data } = await tryExecuteAndNotify(
        this,
        ServiceService.generateOauth2ClientCredentialsToken({requestBody: {
          alias: this._service!.alias
        }})
      );

      if (data) {
        this.#notificationContext?.peek("positive", {
          data: { message: `The '${this._service!.displayName}' service has been authorized.` },
        });

        this.#refreshServiceDetails();
      }
    } else if (this.#isOAuth1()) {
      const { data } = await tryExecuteAndNotify(
        this,
        ServiceService.generateOauth1RequestToken({requestBody: {
          alias: this._service!.alias
        }})
      );

      if (data) {
        location.href = data;
      }
    } else {
      location.href = this._service!.authorizationUrl ?? "";
    }
  }

  async #revokeAccess() {
		await tryExecuteAndNotify(
			this,
			ServiceService.revokeAccess({requestBody: {
        alias: this._service!.alias
      }})
		);

    this.#refreshServiceDetails();
  }

  async #saveOAuth1AccessToken() {
    const accessTokenInputElement = <HTMLInputElement>this.shadowRoot?.getElementById("inAccessToken");
    const tokenSecretInputElement = <HTMLInputElement>this.shadowRoot?.getElementById("inTokenSecret");
    if (!accessTokenInputElement || !tokenSecretInputElement) return;

		const { error } = await tryExecuteAndNotify(
			this,
			ServiceService.saveOauth1Token({requestBody: {
        alias: this._service!.alias,
        token: accessTokenInputElement.value,
        tokenSecret: tokenSecretInputElement.value
      }})
		);

    if (!error) {
      this.#notificationContext?.peek("positive", {
        data: { message: `The '${this._service!.displayName}' service access token and token secret have been saved.` },
      });
      accessTokenInputElement.value = "";
      this.#refreshServiceDetails();
    }
  }

  async #saveOAuth2AccessToken() {
    const accessTokenInputElement = <HTMLInputElement>this.shadowRoot?.getElementById("inAccessToken");
    if (!accessTokenInputElement) return;

		const { error } = await tryExecuteAndNotify(
			this,
			ServiceService.saveOauth2Token({requestBody: {
        alias: this._service!.alias,
        token: accessTokenInputElement.value
      }})
		);

    if (!error) {
      this.#notificationContext?.peek("positive", {
        data: { message: `The '${this._service!.displayName}' service access token has been saved.` },
      });
      accessTokenInputElement.value = "";
      this.#refreshServiceDetails();
    }
  }

  async #saveApiKey() {
    const apiKeyInputElement = <HTMLInputElement>this.shadowRoot?.getElementById("inApiKey");
    if (!apiKeyInputElement) return;

		const { error } = await tryExecuteAndNotify(
			this,
			ServiceService.saveApiKey({requestBody: {
        alias: this._service!.alias,
        apiKey: apiKeyInputElement.value
      }})
		);

    if (!error) {
      this.#notificationContext?.peek("positive", {
        data: { message: `The '${this._service!.displayName}' service API key has been saved.` },
      });
      apiKeyInputElement.value = "";
      this.#refreshServiceDetails();
    }
  }

  #refreshServiceDetails() {
    // Reload the service data to get the updated status.
    this.#workspaceContext?.load(this._service!.alias);
  }

  async #sendSampleRequest() {
    this._sampleRequestResponse = undefined;

		const { data, error } = await tryExecute(
			ServiceService.sendSampleRequest({
        alias: this._service!.alias
      })
		);

    if (!data || error) {
      this.#notificationContext?.peek("danger", {
        data: { message: "The sample request did not complete: " + error?.message },
      });
    }
    else {
      this._sampleRequestResponse = "Request: " + this._service!.sampleRequest + "\r\nResponse: " + JSON.stringify(data, null, 2);
    }
  }

  render() {
    return html`
      ${when(this._service,
        () => html`
          <umb-workspace-editor headline="Authorized Services: ${this._service!.displayName}" alias="AuthorizedServices.Workspace">
            <umb-body-layout header-transparent header-fit-height>
                ${this.#renderStatusSection(this._service!)}
                ${when(this._service!.canManuallyProvideToken && this.#isOAuth1(),
                  () => this.#renderOAuth1TokenSection()
                )}
                ${when(this._service!.canManuallyProvideToken && (this.#isOAuth2AuthorizationCode() || this.#isOAuth2ClientCredentials()),
                  () => this.#renderOAuth2TokenSection()
                )}
                ${when(this._service!.canManuallyProvideApiKey && this.#isApiKey(),
                  () => this.#renderKeySection()
                )}
                ${this.#renderSettingsSection(this._service!)}
              </umb-body-layout>
          </umb-workspace-editor>
        `)}
    `;
  }

  #renderStatusSection(service: AuthorizedServiceDisplay) {
    return html`
      <uui-box headline="Status">
        <uui-tag size="s" slot="header-actions" color="${service.isAuthorized ? "positive" : "danger"}">
          ${service.isAuthorized ? "Authorized" : "Not Authorized"}
        </uui-tag>        
        <p><b>${service.displayName}</b> has been configured as an authorized service.</p>
        ${when(service.isAuthorized,
          () => html`
            <div>
              ${when(service.sampleRequest,
                () => html`
                  <uui-button
                    label="Verify Sample Request"
                    look="outline"
                    @click=${this.#sendSampleRequest}></uui-button>
                `)}
              ${when(this.#isOAuth1() || this.#isOAuth2AuthorizationCode() || this.#isOAuth2ClientCredentials(),
                () => html`
                  <uui-button
                    label="Revoke Access"
                    look="primary"
                    color="danger"
                    @click=${this.#revokeAccess}></uui-button>
                `)}

              ${when(this._sampleRequestResponse,
                () => html`
                    <div id="sample-response">
                      <div>${this._sampleRequestResponse}</div>
                    </div>`
                )}
            </div>
          `)}

        ${when(!service.isAuthorized && !this.#isApiKey(),
          () => html`
            <div>
              <p>To authorize the service click to sign-in to the provider's portal, confirm the permission request and return to Umbraco.</p>
              <uui-button
                label="Authorize Service"
                look="primary"
                color="positive"
                @click=${this.#authorizeAccess}></uui-button>
            </div>
          `)}

      </uui-box>
      `;
  }

  #renderOAuth1TokenSection() {
    return html`
       <uui-box headline="Provide OAuth1 Access Token and Token Secret">
          <p>Enter service access token and token secret</p>
          <p>This service is configured indicating that an access token and a token secret can be generated via the service's developer portal. Once you have obtained them you can copy and paste them here to authorize the service.</p>
          <div>
            <uui-form>
              <uui-form-layout-item class="form-item">
                <uui-label for="inAccessToken" slot="label">Access Token</uui-label>
                <uui-input id="inAccessToken" name="access_token" type="text" label="Access Token"></uui-input>
              </uui-form-layout-item>
              <uui-form-layout-item class="form-item">
                <uui-label for="inTokenSecret" slot="label">Token Secret</uui-label>
                <uui-input id="inTokenSecret" name="token_secret" type="text" label="Token Secret"></uui-input>
              </uui-form-layout-item>
              <div>
                <uui-button
                  label="Save"
                  look="primary"
                  color="positive"
                  @click=${this.#saveOAuth1AccessToken}></uui-button>
              </div>
            </uui-form>
          </div>
       </uui-box>
    `;
  }

  #renderOAuth2TokenSection() {
    return html`
      <uui-box headline="Provide Token">
        <p>Enter service access token</p>
        <p>This service is configured indicating that a token can be generated via the service's developer portal. Once you have obtained one you can copy and paste it here to authorize the service.</p>
        <div>
          <uui-form>
            <uui-form-layout-item class="form-item">
              <uui-label for="inAccessToken" slot="label">Access Token</uui-label>
              <uui-input id="inAccessToken" name="access_token" type="text" label="Access Token"></uui-input>
            </uui-form-layout-item>
            <div>
              <uui-button
                label="Save"
                look="primary"
                color="positive"
                @click=${this.#saveOAuth2AccessToken}></uui-button>
            </div>
          </uui-form>
        </div>
      </uui-box>
    `;
  }

  #renderKeySection() {
    return html`
      <uui-box headline="Provide API Key">
        <p>Enter service API key</p>
        <p>
          This service is configured indicating that an API key can be created via the service's developer portal.
          Once you have obtained one you can copy and paste it here to authorize the service.
        </p>
        <div>
          <uui-form>
            <uui-form-layout-item class="form-item">
              <uui-label for="inApiKey" slot="label">Api Key</uui-label>
              <uui-input id="inApiKey" name="api_key" type="text" label="Api Key"></uui-input>
            </uui-form-layout-item>
            <div>
              <uui-button
                label="Save"
                look="primary"
                color="positive"
                @click=${this.#saveApiKey}></uui-button>
            </div>
          </uui-form>
        </div>
      </uui-box>
    `;
  }

  #renderSettingsSection(service: AuthorizedServiceDisplay) {
    return html`
      <uui-box headline="Settings">
        <uui-table>
          <uui-table-head>
            <uui-table-head-cell>Key</uui-table-head-cell>
            <uui-table-head-cell>Value</uui-table-head-cell>
          </uui-table-head>
          ${Object.entries(service.settings).map(([key, value]) =>
            html`<uui-table-row>
              <uui-table-cell>${key}</uui-table-cell>
              <uui-table-cell>${value}</uui-table-cell>
            </uui-table-row>`)}
        </uui-table>
      </uui-box>
    `;
  }

  static styles = [
    css`
      uui-box + uui-box {
        margin-top: var(--uui-size-8);
      }

      #sample-response {
        background: var(--uui-color-disabled);
        border-radius: var(--uui-border-radius);
        border: 2px solid var(--uui-color-default);
        margin-top: var(--uui-size-8);
      }

      #sample-response div {
        font-family: Consolas, Menlo, Monaco, Lucida Console, Liberation Mono, DejaVu Sans Mono, Bitstream Vera Sans Mono, Courier New, monospace, serif;
        font-size: 12px;
        min-height: 48px;
        max-height: 512px;
        overflow: auto;
        white-space: pre;
        padding: var(--uui-size-3) var(--uui-size-7);
      }

      .form-item uui-input {
          width: 40%;
      }
    `
  ];

}

export default AuthorizedServiceWorkspaceEditorElement;

declare global {
  interface HTMLElementTagNameMap {
    [elementName]: AuthorizedServiceWorkspaceEditorElement
  }
}
