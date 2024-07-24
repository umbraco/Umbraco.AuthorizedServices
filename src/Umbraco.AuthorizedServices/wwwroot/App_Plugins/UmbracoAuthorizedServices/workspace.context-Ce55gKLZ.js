var R = (e) => {
  throw TypeError(e);
};
var C = (e, t, i) => t.has(e) || R("Cannot " + i);
var p = (e, t, i) => (C(e, t, "read from private field"), i ? i.call(e) : t.get(e)), x = (e, t, i) => t.has(e) ? R("Cannot add the same private member more than once") : t instanceof WeakSet ? t.add(e) : t.set(e, i), z = (e, t, i, s) => (C(e, t, "write to private field"), s ? s.call(e, i) : t.set(e, i), i);
import { UmbWorkspaceRouteManager as F } from "@umbraco-cms/backoffice/workspace";
import { UmbObjectState as G } from "@umbraco-cms/backoffice/observable-api";
import { tryExecuteAndNotify as f, tryExecute as J } from "@umbraco-cms/backoffice/resources";
import { UmbContextBase as j } from "@umbraco-cms/backoffice/class-api";
import { A as I, S as d, W as $ } from "./index-CRz22JwE.js";
import { UmbElementMixin as Q } from "@umbraco-cms/backoffice/element-api";
import { LitElement as ee, html as r, when as c, css as te, state as O, customElement as ie } from "@umbraco-cms/backoffice/external/lit";
import { UMB_NOTIFICATION_CONTEXT as se } from "@umbraco-cms/backoffice/notification";
import { UmbContextToken as ae } from "@umbraco-cms/backoffice/context-api";
const oe = "AuthorizedServicesWorkspaceContext", q = new ae(
  oe,
  void 0,
  (e) => {
    var t;
    return ((t = e.getEntityType) == null ? void 0 : t.call(e)) === I;
  }
);
var ne = Object.defineProperty, re = Object.getOwnPropertyDescriptor, N = (e) => {
  throw TypeError(e);
}, g = (e, t, i, s) => {
  for (var u = s > 1 ? void 0 : s ? re(t, i) : t, h = e.length - 1, E; h >= 0; h--)
    (E = e[h]) && (u = (s ? E(t, i, u) : E(u)) || u);
  return s && u && ne(t, i, u), u;
}, w = (e, t, i) => t.has(e) || N("Cannot " + i), o = (e, t, i) => (w(e, t, "read from private field"), i ? i.call(e) : t.get(e)), m = (e, t, i) => t.has(e) ? N("Cannot add the same private member more than once") : t instanceof WeakSet ? t.add(e) : t.set(e, i), M = (e, t, i, s) => (w(e, t, "write to private field"), t.set(e, i), i), n = (e, t, i) => (w(e, t, "access private method"), i), b, v, a, P, k, T, A, S, K, W, B, U, V, _, D, L, H, X, Z, Y;
const ue = "authorized-service-workspace";
let y = class extends Q(ee) {
  constructor() {
    super(), m(this, a), m(this, b), m(this, v), this._authenticationMethod = "None", this._sampleRequestResponse = void 0, m(
      this,
      k,
      () => this._authenticationMethod === "OAuth1"
      /* OAuth1 */
    ), m(
      this,
      T,
      () => this._authenticationMethod === "OAuth2AuthorizationCode"
      /* OAuth2AuthorizationCode */
    ), m(
      this,
      A,
      () => this._authenticationMethod === "OAuth2ClientCredentials"
      /* OAuth2ClientCredentials */
    ), m(
      this,
      S,
      () => this._authenticationMethod === "ApiKey"
      /* ApiKey */
    ), this.consumeContext(q, (e) => {
      M(this, b, e), n(this, a, P).call(this);
    }), this.consumeContext(se, (e) => {
      M(this, v, e);
    });
  }
  render() {
    return r`
      ${c(
      this._service,
      () => r`
          <umb-workspace-editor headline="Authorized Services: ${this._service.displayName}" alias="AuthorizedServices.Workspace">
            <umb-body-layout header-transparent header-fit-height>
                ${n(this, a, L).call(this, this._service)}
                ${c(
        this._service.canManuallyProvideToken && o(this, k).call(this),
        () => n(this, a, H).call(this)
      )}
                ${c(
        this._service.canManuallyProvideToken && (o(this, T).call(this) || o(this, A).call(this)),
        () => n(this, a, X).call(this)
      )}
                ${c(
        this._service.canManuallyProvideApiKey && o(this, S).call(this),
        () => n(this, a, Z).call(this)
      )}
                ${n(this, a, Y).call(this, this._service)}
              </umb-body-layout>
          </umb-workspace-editor>
        `
    )}
    `;
  }
};
b = /* @__PURE__ */ new WeakMap();
v = /* @__PURE__ */ new WeakMap();
a = /* @__PURE__ */ new WeakSet();
P = function() {
  o(this, b) && this.observe(o(this, b).data, (e) => {
    e && (this._service = e, this._authenticationMethod = e.authenticationMethod);
  });
};
k = /* @__PURE__ */ new WeakMap();
T = /* @__PURE__ */ new WeakMap();
A = /* @__PURE__ */ new WeakMap();
S = /* @__PURE__ */ new WeakMap();
K = async function() {
  var e;
  if (o(this, A).call(this)) {
    const { data: t } = await f(
      this,
      d.generateOauth2ClientCredentialsToken({ requestBody: {
        alias: this._service.alias
      } })
    );
    t && ((e = o(this, v)) == null || e.peek("positive", {
      data: { message: `The '${this._service.displayName}' service has been authorized.` }
    }), n(this, a, _).call(this));
  } else if (o(this, k).call(this)) {
    const { data: t } = await f(
      this,
      d.generateOauth1RequestToken({ requestBody: {
        alias: this._service.alias
      } })
    );
    t && (location.href = t);
  } else
    location.href = this._service.authorizationUrl ?? "";
};
W = async function() {
  await f(
    this,
    d.revokeAccess({ requestBody: {
      alias: this._service.alias
    } })
  ), n(this, a, _).call(this);
};
B = async function() {
  var s, u, h;
  const e = (s = this.shadowRoot) == null ? void 0 : s.getElementById("inAccessToken"), t = (u = this.shadowRoot) == null ? void 0 : u.getElementById("inTokenSecret");
  if (!e || !t) return;
  const { error: i } = await f(
    this,
    d.saveOauth1Token({ requestBody: {
      alias: this._service.alias,
      token: e.value,
      tokenSecret: t.value
    } })
  );
  i || ((h = o(this, v)) == null || h.peek("positive", {
    data: { message: `The '${this._service.displayName}' service access token and token secret have been saved.` }
  }), e.value = "", n(this, a, _).call(this));
};
U = async function() {
  var i, s;
  const e = (i = this.shadowRoot) == null ? void 0 : i.getElementById("inAccessToken");
  if (!e) return;
  const { error: t } = await f(
    this,
    d.saveOauth2Token({ requestBody: {
      alias: this._service.alias,
      token: e.value
    } })
  );
  t || ((s = o(this, v)) == null || s.peek("positive", {
    data: { message: `The '${this._service.displayName}' service access token has been saved.` }
  }), e.value = "", n(this, a, _).call(this));
};
V = async function() {
  var i, s;
  const e = (i = this.shadowRoot) == null ? void 0 : i.getElementById("inApiKey");
  if (!e) return;
  const { error: t } = await f(
    this,
    d.saveApiKey({ requestBody: {
      alias: this._service.alias,
      apiKey: e.value
    } })
  );
  t || ((s = o(this, v)) == null || s.peek("positive", {
    data: { message: `The '${this._service.displayName}' service API key has been saved.` }
  }), e.value = "", n(this, a, _).call(this));
};
_ = function() {
  var e;
  (e = o(this, b)) == null || e.load(this._service.alias);
};
D = async function() {
  var i;
  this._sampleRequestResponse = void 0;
  const { data: e, error: t } = await J(
    d.sendSampleRequest({
      alias: this._service.alias
    })
  );
  !e || t ? (i = o(this, v)) == null || i.peek("danger", {
    data: { message: "The sample request did not complete: " + (t == null ? void 0 : t.message) }
  }) : this._sampleRequestResponse = "Request: " + this._service.sampleRequest + `\r
Response: ` + JSON.stringify(e, null, 2);
};
L = function(e) {
  return r`
      <uui-box headline="Status">
        <uui-tag size="s" slot="header-actions" color="${e.isAuthorized ? "positive" : "danger"}">
          ${e.isAuthorized ? "Authorized" : "Not Authorized"}
        </uui-tag>        
        <p><b>${e.displayName}</b> has been configured as an authorized service.</p>
        ${c(
    e.isAuthorized,
    () => r`
            <div>
              ${c(
      e.sampleRequest,
      () => r`
                  <uui-button
                    label="Verify Sample Request"
                    look="outline"
                    @click=${n(this, a, D)}></uui-button>
                `
    )}
              ${c(
      o(this, k).call(this) || o(this, T).call(this) || o(this, A).call(this),
      () => r`
                  <uui-button
                    label="Revoke Access"
                    look="primary"
                    color="danger"
                    @click=${n(this, a, W)}></uui-button>
                `
    )}

              ${c(
      this._sampleRequestResponse,
      () => r`
                    <div id="sample-response">
                      <div>${this._sampleRequestResponse}</div>
                    </div>`
    )}
            </div>
          `
  )}

        ${c(
    !e.isAuthorized && !o(this, S).call(this),
    () => r`
            <div>
              <p>To authorize the service click to sign-in to the provider's portal, confirm the permission request and return to Umbraco.</p>
              <uui-button
                label="Authorize Service"
                look="primary"
                color="positive"
                @click=${n(this, a, K)}></uui-button>
            </div>
          `
  )}

      </uui-box>
      `;
};
H = function() {
  return r`
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
                  @click=${n(this, a, B)}></uui-button>
              </div>
            </uui-form>
          </div>
       </uui-box>
    `;
};
X = function() {
  return r`
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
                @click=${n(this, a, U)}></uui-button>
            </div>
          </uui-form>
        </div>
      </uui-box>
    `;
};
Z = function() {
  return r`
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
                @click=${n(this, a, V)}></uui-button>
            </div>
          </uui-form>
        </div>
      </uui-box>
    `;
};
Y = function(e) {
  return r`
      <uui-box headline="Settings">
        <uui-table>
          <uui-table-head>
            <uui-table-head-cell>Key</uui-table-head-cell>
            <uui-table-head-cell>Value</uui-table-head-cell>
          </uui-table-head>
          ${Object.entries(e.settings).map(([t, i]) => r`<uui-table-row>
              <uui-table-cell>${t}</uui-table-cell>
              <uui-table-cell>${i}</uui-table-cell>
            </uui-table-row>`)}
        </uui-table>
      </uui-box>
    `;
};
y.styles = [
  te`
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
g([
  O()
], y.prototype, "_service", 2);
g([
  O()
], y.prototype, "_authenticationMethod", 2);
g([
  O()
], y.prototype, "_sampleRequestResponse", 2);
y = g([
  ie(ue)
], y);
var l;
class _e extends j {
  constructor(i) {
    super(i, $);
    x(this, l);
    z(this, l, new G(void 0)), this.data = p(this, l).asObservable(), this.alias = p(this, l).asObservablePart((s) => s == null ? void 0 : s.alias), this.name = p(this, l).asObservablePart((s) => s == null ? void 0 : s.displayName), this.provideContext(q, this), this.workspaceAlias = $, this.routes = new F(i), this.routes.setRoutes([
      {
        path: "edit/:alias",
        component: y,
        setup: (s, u) => {
          const h = u.match.params.alias;
          this.load(h);
        }
      }
    ]);
  }
  async load(i) {
    const { data: s } = await f(
      this,
      d.getByAlias({ alias: i })
    );
    s && p(this, l).update(s);
  }
  getData() {
    return p(this, l).getValue();
  }
  getEntityType() {
    return I;
  }
  getName() {
    var i;
    return (i = p(this, l).getValue()) == null ? void 0 : i.displayName;
  }
  destroy() {
    p(this, l).destroy();
  }
}
l = new WeakMap();
export {
  _e as AuthorizedServiceWorkspaceContext,
  _e as api
};
//# sourceMappingURL=workspace.context-Ce55gKLZ.js.map
