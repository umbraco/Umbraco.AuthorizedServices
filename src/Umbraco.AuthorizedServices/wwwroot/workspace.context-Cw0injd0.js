var R = (e) => {
  throw TypeError(e);
};
var C = (e, t, i) => t.has(e) || R("Cannot " + i);
var p = (e, t, i) => (C(e, t, "read from private field"), i ? i.call(e) : t.get(e)), x = (e, t, i) => t.has(e) ? R("Cannot add the same private member more than once") : t instanceof WeakSet ? t.add(e) : t.set(e, i), z = (e, t, i, s) => (C(e, t, "write to private field"), s ? s.call(e, i) : t.set(e, i), i);
import { UmbWorkspaceRouteManager as F } from "@umbraco-cms/backoffice/workspace";
import { UmbObjectState as G } from "@umbraco-cms/backoffice/observable-api";
import { tryExecute as d } from "@umbraco-cms/backoffice/resources";
import { UmbContextBase as J } from "@umbraco-cms/backoffice/class-api";
import { A as I, S as v, W as $ } from "./index-BJOu1Tl7.js";
import { UmbElementMixin as j } from "@umbraco-cms/backoffice/element-api";
import { LitElement as Q, when as l, html as r, css as ee, state as O, customElement as te } from "@umbraco-cms/backoffice/external/lit";
import { UMB_NOTIFICATION_CONTEXT as ie } from "@umbraco-cms/backoffice/notification";
import { UmbContextToken as se } from "@umbraco-cms/backoffice/context-api";
const ae = "AuthorizedServicesWorkspaceContext", N = new se(
  ae,
  void 0,
  (e) => {
    var t;
    return ((t = e.getEntityType) == null ? void 0 : t.call(e)) === I;
  }
);
var oe = Object.defineProperty, ne = Object.getOwnPropertyDescriptor, P = (e) => {
  throw TypeError(e);
}, g = (e, t, i, s) => {
  for (var u = s > 1 ? void 0 : s ? ne(t, i) : t, h = e.length - 1, E; h >= 0; h--)
    (E = e[h]) && (u = (s ? E(t, i, u) : E(u)) || u);
  return s && u && oe(t, i, u), u;
}, w = (e, t, i) => t.has(e) || P("Cannot " + i), o = (e, t, i) => (w(e, t, "read from private field"), i ? i.call(e) : t.get(e)), b = (e, t, i) => t.has(e) ? P("Cannot add the same private member more than once") : t instanceof WeakSet ? t.add(e) : t.set(e, i), M = (e, t, i, s) => (w(e, t, "write to private field"), t.set(e, i), i), n = (e, t, i) => (w(e, t, "access private method"), i), y, m, a, K, k, S, A, T, W, q, U, B, V, _, D, L, H, X, Z, Y;
const re = "authorized-service-workspace";
let f = class extends j(Q) {
  constructor() {
    super(), b(this, a), b(this, y), b(this, m), this._authenticationMethod = "None", this._sampleRequestResponse = void 0, b(
      this,
      k,
      () => this._authenticationMethod === "OAuth1"
      /* OAuth1 */
    ), b(
      this,
      S,
      () => this._authenticationMethod === "OAuth2AuthorizationCode"
      /* OAuth2AuthorizationCode */
    ), b(
      this,
      A,
      () => this._authenticationMethod === "OAuth2ClientCredentials"
      /* OAuth2ClientCredentials */
    ), b(
      this,
      T,
      () => this._authenticationMethod === "ApiKey"
      /* ApiKey */
    ), this.consumeContext(N, (e) => {
      M(this, y, e), n(this, a, K).call(this);
    }), this.consumeContext(ie, (e) => {
      M(this, m, e);
    });
  }
  render() {
    return r`
      ${l(
      this._service,
      () => r`
          <umb-workspace-editor headline="Authorized Services: ${this._service.displayName}" alias="AuthorizedServices.Workspace">
            <umb-body-layout header-transparent header-fit-height>
                ${n(this, a, L).call(this, this._service)}
                ${l(
        this._service.canManuallyProvideToken && o(this, k).call(this),
        () => n(this, a, H).call(this)
      )}
                ${l(
        this._service.canManuallyProvideToken && (o(this, S).call(this) || o(this, A).call(this)),
        () => n(this, a, X).call(this)
      )}
                ${l(
        this._service.canManuallyProvideApiKey && o(this, T).call(this),
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
y = /* @__PURE__ */ new WeakMap();
m = /* @__PURE__ */ new WeakMap();
a = /* @__PURE__ */ new WeakSet();
K = function() {
  o(this, y) && this.observe(o(this, y).data, (e) => {
    e && (this._service = e, this._authenticationMethod = e.authenticationMethod);
  });
};
k = /* @__PURE__ */ new WeakMap();
S = /* @__PURE__ */ new WeakMap();
A = /* @__PURE__ */ new WeakMap();
T = /* @__PURE__ */ new WeakMap();
W = async function() {
  var e;
  if (o(this, A).call(this)) {
    const { data: t } = await d(
      this,
      v.postServiceOauth2ClientCredentials({
        body: {
          alias: this._service.alias
        }
      })
    );
    t && ((e = o(this, m)) == null || e.peek("positive", {
      data: { message: `The '${this._service.displayName}' service has been authorized.` }
    }), n(this, a, _).call(this));
  } else if (o(this, k).call(this)) {
    const { data: t } = await d(
      this,
      v.postServiceOauth1RequestToken({
        body: {
          alias: this._service.alias
        }
      })
    );
    t && (location.href = t);
  } else
    location.href = this._service.authorizationUrl ?? "";
};
q = async function() {
  await d(
    this,
    v.postServiceRevoke({
      body: {
        alias: this._service.alias
      }
    })
  ), n(this, a, _).call(this);
};
U = async function() {
  var s, u, h;
  const e = (s = this.shadowRoot) == null ? void 0 : s.getElementById("inAccessToken"), t = (u = this.shadowRoot) == null ? void 0 : u.getElementById("inTokenSecret");
  if (!e || !t) return;
  const { error: i } = await d(
    this,
    v.postServiceOauth1({
      body: {
        alias: this._service.alias,
        token: e.value,
        tokenSecret: t.value
      }
    })
  );
  i || ((h = o(this, m)) == null || h.peek("positive", {
    data: { message: `The '${this._service.displayName}' service access token and token secret have been saved.` }
  }), e.value = "", n(this, a, _).call(this));
};
B = async function() {
  var i, s;
  const e = (i = this.shadowRoot) == null ? void 0 : i.getElementById("inAccessToken");
  if (!e) return;
  const { error: t } = await d(
    this,
    v.postServiceOauth2({
      body: {
        alias: this._service.alias,
        token: e.value
      }
    })
  );
  t || ((s = o(this, m)) == null || s.peek("positive", {
    data: { message: `The '${this._service.displayName}' service access token has been saved.` }
  }), e.value = "", n(this, a, _).call(this));
};
V = async function() {
  var i, s;
  const e = (i = this.shadowRoot) == null ? void 0 : i.getElementById("inApiKey");
  if (!e) return;
  const { error: t } = await d(
    this,
    v.postServiceApiKey({
      body: {
        alias: this._service.alias,
        apiKey: e.value
      }
    })
  );
  t || ((s = o(this, m)) == null || s.peek("positive", {
    data: { message: `The '${this._service.displayName}' service API key has been saved.` }
  }), e.value = "", n(this, a, _).call(this));
};
_ = function() {
  var e;
  (e = o(this, y)) == null || e.load(this._service.alias);
};
D = async function() {
  var i;
  this._sampleRequestResponse = void 0;
  const { data: e, error: t } = await d(
    this,
    v.getServiceByAliasSampleRequest({
      path: {
        alias: this._service.alias
      }
    })
  );
  !e || t ? (i = o(this, m)) == null || i.peek("danger", {
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
        ${l(
    e.isAuthorized,
    () => r`
            <div>
              ${l(
      e.sampleRequest,
      () => r`
                  <uui-button
                    label="Verify Sample Request"
                    look="outline"
                    @click=${n(this, a, D)}></uui-button>
                `
    )}
              ${l(
      o(this, k).call(this) || o(this, S).call(this) || o(this, A).call(this),
      () => r`
                  <uui-button
                    label="Revoke Access"
                    look="primary"
                    color="danger"
                    @click=${n(this, a, q)}></uui-button>
                `
    )}

              ${l(
      this._sampleRequestResponse,
      () => r`
                    <div id="sample-response">
                      <div>${this._sampleRequestResponse}</div>
                    </div>`
    )}
            </div>
          `
  )}

        ${l(
    !e.isAuthorized && !o(this, T).call(this),
    () => r`
            <div>
              <p>To authorize the service click to sign-in to the provider's portal, confirm the permission request and return to Umbraco.</p>
              <uui-button
                label="Authorize Service"
                look="primary"
                color="positive"
                @click=${n(this, a, W)}></uui-button>
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
                  @click=${n(this, a, U)}></uui-button>
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
                @click=${n(this, a, B)}></uui-button>
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
f.styles = [
  ee`
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
], f.prototype, "_service", 2);
g([
  O()
], f.prototype, "_authenticationMethod", 2);
g([
  O()
], f.prototype, "_sampleRequestResponse", 2);
f = g([
  te(re)
], f);
var c;
class ye extends J {
  constructor(i) {
    super(i, $);
    x(this, c);
    z(this, c, new G(void 0)), this.data = p(this, c).asObservable(), this.alias = p(this, c).asObservablePart((s) => s == null ? void 0 : s.alias), this.name = p(this, c).asObservablePart((s) => s == null ? void 0 : s.displayName), this.provideContext(N, this), this.workspaceAlias = $, this.routes = new F(i), this.routes.setRoutes([
      {
        path: "edit/:alias",
        component: f,
        setup: (s, u) => {
          const h = u.match.params.alias;
          this.load(h);
        }
      }
    ]);
  }
  async load(i) {
    const { data: s } = await d(
      this,
      v.getServiceByAlias({ path: { alias: i } })
    );
    s && p(this, c).update(s);
  }
  getData() {
    return p(this, c).getValue();
  }
  getEntityType() {
    return I;
  }
  getName() {
    var i;
    return (i = p(this, c).getValue()) == null ? void 0 : i.displayName;
  }
  destroy() {
    p(this, c).destroy(), super.destroy();
  }
}
c = new WeakMap();
export {
  ye as AuthorizedServiceWorkspaceContext,
  ye as api
};
//# sourceMappingURL=workspace.context-Cw0injd0.js.map
