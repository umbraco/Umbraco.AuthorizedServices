import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import { LitElement, customElement, html, css, state } from "@umbraco-cms/backoffice/external/lit";

const elementName = "authorized-service-workspace";

@customElement(elementName)
export class AuthorizedServiceWorkspaceEditorElement extends UmbElementMixin(LitElement) {

  @state()
  serviceDisplay!: string;

  render() {
    return html`
      <umb-workspace-editor headline="Authorized Services: ${this.serviceDisplay}" alias="authorizedservice.workspace">
        <umb-body-layout header-transparent header-fit-height>
            <uui-box headline="Status">
            </uui-box>
            ${this.renderOAuth1TokenSection()}
            ${this.renderOAuth2TokenSection()}
            ${this.renderKeySection()}
            <uui-box headline="Settings">
            </uui-box>
          </umb-body-layout>
      </umb-workspace-editor>
    `;
  }

  renderOAuth1TokenSection() {
    return html`
       <uui-box headline="Provide OAuth1 Access Token and Token Secret">
        <uui-icon-registry-essential>
            <uui-card-content-node name="OAuth1 Access Token and Token Secret">
              <uui-icon slot="icon" name="add"></uui-icon>
              <p class="auth-srv">Enter service access token and token secret</p>
              <p>This service is configured indicating that an access token and a token secret can be generated via the service's developer portal. Once you have obtained them you can copy and paste them here to authorize the service.</p>
            </uui-card-content-node>
          </uui-icon-registry-essential>
       </uui-box>
    `;
  }

  renderOAuth2TokenSection() {
    return html`
      <uui-box headline="Provide Token">
        <uui-icon-registry-essential>
            <uui-card-content-node name="Access Token">
              <uui-icon slot="icon" name="add"></uui-icon>
              <p class="auth-srv">Enter service access token</p>
              <p>This service is configured indicating that a token can be generated via the service's developer portal. Once you have obtained one you can copy and paste it here to authorize the service.</p>
              <div>

              </div>
            </uui-card-content-node>
          </uui-icon-registry-essential>
      </uui-box>
    `;
  }

  renderKeySection() {
    return html`
      <uui-box headline="Provide API Key">
        <uui-icon-registry-essential>
            <uui-card-content-node name="API Key">
              <uui-icon slot="icon" name="add"></uui-icon>
              <p class="auth-srv">Enter service API key</p>
              <p>
                This service is configured indicating that an API key can be created via the service's developer portal.
                Once you have obtained one you can copy and paste it here to authorize the service.
              </p>
              <div>
              </div>
            </uui-card-content-node>
          </uui-icon-registry-essential>
      </uui-box>
    `;
  }

  static styles = [
    css`
      :host {
        display: block;
        width: 100%;
        height: 100%;
      }

      uui-box {
        margin-bottom: 20px;
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
