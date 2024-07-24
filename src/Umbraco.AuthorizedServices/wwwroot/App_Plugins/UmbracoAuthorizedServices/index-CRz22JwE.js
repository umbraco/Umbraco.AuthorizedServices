import { UMB_AUTH_CONTEXT as I } from "@umbraco-cms/backoffice/auth";
import { UmbTreeServerDataSourceBase as w, UmbUniqueTreeStore as q, UmbTreeRepositoryBase as j } from "@umbraco-cms/backoffice/tree";
import { UmbContextToken as z } from "@umbraco-cms/backoffice/context-api";
class E extends Error {
  constructor(e, r, s) {
    super(s), this.name = "ApiError", this.url = r.url, this.status = r.status, this.statusText = r.statusText, this.body = r.body, this.request = e;
  }
}
class N extends Error {
  constructor(e) {
    super(e), this.name = "CancelError";
  }
  get isCancelled() {
    return !0;
  }
}
class D {
  constructor(e) {
    this._isResolved = !1, this._isRejected = !1, this._isCancelled = !1, this.cancelHandlers = [], this.promise = new Promise((r, s) => {
      this._resolve = r, this._reject = s;
      const i = (a) => {
        this._isResolved || this._isRejected || this._isCancelled || (this._isResolved = !0, this._resolve && this._resolve(a));
      }, n = (a) => {
        this._isResolved || this._isRejected || this._isCancelled || (this._isRejected = !0, this._reject && this._reject(a));
      }, o = (a) => {
        this._isResolved || this._isRejected || this._isCancelled || this.cancelHandlers.push(a);
      };
      return Object.defineProperty(o, "isResolved", {
        get: () => this._isResolved
      }), Object.defineProperty(o, "isRejected", {
        get: () => this._isRejected
      }), Object.defineProperty(o, "isCancelled", {
        get: () => this._isCancelled
      }), e(i, n, o);
    });
  }
  get [Symbol.toStringTag]() {
    return "Cancellable Promise";
  }
  then(e, r) {
    return this.promise.then(e, r);
  }
  catch(e) {
    return this.promise.catch(e);
  }
  finally(e) {
    return this.promise.finally(e);
  }
  cancel() {
    if (!(this._isResolved || this._isRejected || this._isCancelled)) {
      if (this._isCancelled = !0, this.cancelHandlers.length)
        try {
          for (const e of this.cancelHandlers)
            e();
        } catch (e) {
          console.warn("Cancellation threw an error", e);
          return;
        }
      this.cancelHandlers.length = 0, this._reject && this._reject(new N("Request aborted"));
    }
  }
  get isCancelled() {
    return this._isCancelled;
  }
}
class v {
  constructor() {
    this._fns = [];
  }
  eject(e) {
    const r = this._fns.indexOf(e);
    r !== -1 && (this._fns = [...this._fns.slice(0, r), ...this._fns.slice(r + 1)]);
  }
  use(e) {
    this._fns = [...this._fns, e];
  }
}
const c = {
  BASE: "",
  CREDENTIALS: "include",
  ENCODE_PATH: void 0,
  HEADERS: void 0,
  PASSWORD: void 0,
  TOKEN: void 0,
  USERNAME: void 0,
  VERSION: "Latest",
  WITH_CREDENTIALS: !1,
  interceptors: {
    request: new v(),
    response: new v()
  }
}, h = (t) => typeof t == "string", m = (t) => h(t) && t !== "", y = (t) => t instanceof Blob, R = (t) => t instanceof FormData, U = (t) => {
  try {
    return btoa(t);
  } catch {
    return Buffer.from(t).toString("base64");
  }
}, H = (t) => {
  const e = [], r = (i, n) => {
    e.push(`${encodeURIComponent(i)}=${encodeURIComponent(String(n))}`);
  }, s = (i, n) => {
    n != null && (n instanceof Date ? r(i, n.toISOString()) : Array.isArray(n) ? n.forEach((o) => s(i, o)) : typeof n == "object" ? Object.entries(n).forEach(([o, a]) => s(`${i}[${o}]`, a)) : r(i, n));
  };
  return Object.entries(t).forEach(([i, n]) => s(i, n)), e.length ? `?${e.join("&")}` : "";
}, P = (t, e) => {
  const r = encodeURI, s = e.url.replace("{api-version}", t.VERSION).replace(/{(.*?)}/g, (n, o) => {
    var a;
    return (a = e.path) != null && a.hasOwnProperty(o) ? r(String(e.path[o])) : n;
  }), i = t.BASE + s;
  return e.query ? i + H(e.query) : i;
}, k = (t) => {
  if (t.formData) {
    const e = new FormData(), r = (s, i) => {
      h(i) || y(i) ? e.append(s, i) : e.append(s, JSON.stringify(i));
    };
    return Object.entries(t.formData).filter(([, s]) => s != null).forEach(([s, i]) => {
      Array.isArray(i) ? i.forEach((n) => r(s, n)) : r(s, i);
    }), e;
  }
}, p = async (t, e) => typeof e == "function" ? e(t) : e, B = async (t, e) => {
  const [r, s, i, n] = await Promise.all([
    p(e, t.TOKEN),
    p(e, t.USERNAME),
    p(e, t.PASSWORD),
    p(e, t.HEADERS)
  ]), o = Object.entries({
    Accept: "application/json",
    ...n,
    ...e.headers
  }).filter(([, a]) => a != null).reduce((a, [l, d]) => ({
    ...a,
    [l]: String(d)
  }), {});
  if (m(r) && (o.Authorization = `Bearer ${r}`), m(s) && m(i)) {
    const a = U(`${s}:${i}`);
    o.Authorization = `Basic ${a}`;
  }
  return e.body !== void 0 && (e.mediaType ? o["Content-Type"] = e.mediaType : y(e.body) ? o["Content-Type"] = e.body.type || "application/octet-stream" : h(e.body) ? o["Content-Type"] = "text/plain" : R(e.body) || (o["Content-Type"] = "application/json")), new Headers(o);
}, x = (t) => {
  var e, r;
  if (t.body !== void 0)
    return (e = t.mediaType) != null && e.includes("application/json") || (r = t.mediaType) != null && r.includes("+json") ? JSON.stringify(t.body) : h(t.body) || y(t.body) || R(t.body) ? t.body : JSON.stringify(t.body);
}, L = async (t, e, r, s, i, n, o) => {
  const a = new AbortController();
  let l = {
    headers: n,
    body: s ?? i,
    method: e.method,
    signal: a.signal
  };
  t.WITH_CREDENTIALS && (l.credentials = t.CREDENTIALS);
  for (const d of t.interceptors.request._fns)
    l = await d(l);
  return o(() => a.abort()), await fetch(r, l);
}, F = (t, e) => {
  if (e) {
    const r = t.headers.get(e);
    if (h(r))
      return r;
  }
}, $ = async (t) => {
  if (t.status !== 204)
    try {
      const e = t.headers.get("Content-Type");
      if (e) {
        const r = ["application/octet-stream", "application/pdf", "application/zip", "audio/", "image/", "video/"];
        if (e.includes("application/json") || e.includes("+json"))
          return await t.json();
        if (r.some((s) => e.includes(s)))
          return await t.blob();
        if (e.includes("multipart/form-data"))
          return await t.formData();
        if (e.includes("text/"))
          return await t.text();
      }
    } catch (e) {
      console.error(e);
    }
}, M = (t, e) => {
  const s = {
    400: "Bad Request",
    401: "Unauthorized",
    402: "Payment Required",
    403: "Forbidden",
    404: "Not Found",
    405: "Method Not Allowed",
    406: "Not Acceptable",
    407: "Proxy Authentication Required",
    408: "Request Timeout",
    409: "Conflict",
    410: "Gone",
    411: "Length Required",
    412: "Precondition Failed",
    413: "Payload Too Large",
    414: "URI Too Long",
    415: "Unsupported Media Type",
    416: "Range Not Satisfiable",
    417: "Expectation Failed",
    418: "Im a teapot",
    421: "Misdirected Request",
    422: "Unprocessable Content",
    423: "Locked",
    424: "Failed Dependency",
    425: "Too Early",
    426: "Upgrade Required",
    428: "Precondition Required",
    429: "Too Many Requests",
    431: "Request Header Fields Too Large",
    451: "Unavailable For Legal Reasons",
    500: "Internal Server Error",
    501: "Not Implemented",
    502: "Bad Gateway",
    503: "Service Unavailable",
    504: "Gateway Timeout",
    505: "HTTP Version Not Supported",
    506: "Variant Also Negotiates",
    507: "Insufficient Storage",
    508: "Loop Detected",
    510: "Not Extended",
    511: "Network Authentication Required",
    ...t.errors
  }[e.status];
  if (s)
    throw new E(t, e, s);
  if (!e.ok) {
    const i = e.status ?? "unknown", n = e.statusText ?? "unknown", o = (() => {
      try {
        return JSON.stringify(e.body, null, 2);
      } catch {
        return;
      }
    })();
    throw new E(
      t,
      e,
      `Generic Error: status: ${i}; status text: ${n}; body: ${o}`
    );
  }
}, u = (t, e) => new D(async (r, s, i) => {
  try {
    const n = P(t, e), o = k(e), a = x(e), l = await B(t, e);
    if (!i.isCancelled) {
      let d = await L(t, e, n, a, o, l, i);
      for (const C of t.interceptors.response._fns)
        d = await C(d);
      const g = await $(d), O = F(d, e.responseHeader), S = {
        url: n,
        ok: d.ok,
        status: d.status,
        statusText: d.statusText,
        body: O ?? g
      };
      M(e, S), r(S.body);
    }
  } catch (n) {
    s(n);
  }
});
class pe {
  /**
   * @param data The data for the request.
   * @param data.requestBody
   * @returns string OK
   * @throws ApiError
   */
  static generateOauth1RequestToken(e = {}) {
    return u(c, {
      method: "POST",
      url: "/umbraco/authorized-services/management/api/v1/service",
      body: e.requestBody,
      mediaType: "application/json"
    });
  }
  /**
   * @param data The data for the request.
   * @param data.alias
   * @returns unknown OK
   * @throws ApiError
   */
  static getByAlias(e) {
    return u(c, {
      method: "GET",
      url: "/umbraco/authorized-services/management/api/v1/service/{alias}",
      path: {
        alias: e.alias
      }
    });
  }
  /**
   * @param data The data for the request.
   * @param data.alias
   * @returns string OK
   * @throws ApiError
   */
  static sendSampleRequest(e) {
    return u(c, {
      method: "GET",
      url: "/umbraco/authorized-services/management/api/v1/service/{alias}/sample-request",
      path: {
        alias: e.alias
      },
      errors: {
        500: "Internal Server Error"
      }
    });
  }
  /**
   * @param data The data for the request.
   * @param data.requestBody
   * @returns string OK
   * @throws ApiError
   */
  static saveApiKey(e = {}) {
    return u(c, {
      method: "POST",
      url: "/umbraco/authorized-services/management/api/v1/service/api-key",
      body: e.requestBody,
      mediaType: "application/json",
      responseHeader: "Umb-Notifications"
    });
  }
  /**
   * @param data The data for the request.
   * @param data.requestBody
   * @returns string OK
   * @throws ApiError
   */
  static saveOauth1Token(e = {}) {
    return u(c, {
      method: "POST",
      url: "/umbraco/authorized-services/management/api/v1/service/oauth1",
      body: e.requestBody,
      mediaType: "application/json",
      responseHeader: "Umb-Notifications"
    });
  }
  /**
   * @param data The data for the request.
   * @param data.requestBody
   * @returns string OK
   * @throws ApiError
   */
  static generateOauth1RequestToken1(e = {}) {
    return u(c, {
      method: "POST",
      url: "/umbraco/authorized-services/management/api/v1/service/oauth1/request-token",
      body: e.requestBody,
      mediaType: "application/json"
    });
  }
  /**
   * @param data The data for the request.
   * @param data.requestBody
   * @returns string OK
   * @throws ApiError
   */
  static saveOauth2Token(e = {}) {
    return u(c, {
      method: "POST",
      url: "/umbraco/authorized-services/management/api/v1/service/oauth2",
      body: e.requestBody,
      mediaType: "application/json",
      responseHeader: "Umb-Notifications"
    });
  }
  /**
   * @param data The data for the request.
   * @param data.requestBody
   * @returns unknown OK
   * @throws ApiError
   */
  static generateOauth2ClientCredentialsToken(e = {}) {
    return u(c, {
      method: "POST",
      url: "/umbraco/authorized-services/management/api/v1/service/oauth2/client-credentials",
      body: e.requestBody,
      mediaType: "application/json"
    });
  }
  /**
   * @param data The data for the request.
   * @param data.requestBody
   * @returns string OK
   * @throws ApiError
   */
  static revokeAccess(e = {}) {
    return u(c, {
      method: "POST",
      url: "/umbraco/authorized-services/management/api/v1/service/revoke",
      body: e.requestBody,
      mediaType: "application/json",
      responseHeader: "Umb-Notifications"
    });
  }
}
class V {
  /**
   * @param data The data for the request.
   * @param data.skip
   * @param data.take
   * @returns unknown OK
   * @throws ApiError
   */
  static root(e = {}) {
    return u(c, {
      method: "GET",
      url: "/umbraco/authorized-services/management/api/v1/tree",
      query: {
        skip: e.skip,
        take: e.take
      }
    });
  }
}
const f = "authorized-service", T = "authorized-service-root", W = "AuthorizedServices.Workspace", G = {
  type: "workspace",
  kind: "routable",
  alias: W,
  name: "Authorized Service Workspace",
  api: () => import("./workspace.context-Ce55gKLZ.js"),
  meta: {
    entityType: f
  }
}, Z = G;
class K extends w {
  constructor(e) {
    super(e, {
      getRootItems: b,
      getChildrenOf: Y,
      getAncestorsOf: J,
      mapper: X
    });
  }
}
const b = () => V.root(), Y = (t) => {
  if (t.parent.unique === null)
    return b();
  throw new Error("Not supported for the authorized services tree");
}, J = () => {
  throw new Error("Not supported for the authorized services tree");
}, X = (t) => ({ ...t, parent: {
  unique: null,
  entityType: T
}, entityType: f, isFolder: !1, hasChildren: !1 });
class Q extends q {
  constructor(e) {
    super(e, A.toString());
  }
}
const A = new z(
  "AuthorizedServicesTreeStore"
);
class ee extends j {
  constructor(e) {
    super(
      e,
      K,
      A
    );
  }
  async requestTreeRoot() {
    return { data: {
      unique: null,
      entityType: T,
      name: "Authorized Services",
      hasChildren: !0,
      isFolder: !0
    } };
  }
}
const te = {
  type: "menuItem",
  kind: "tree",
  alias: "AuthorizedServices.MenuItem.Service",
  name: "Authorized Services Service Menu Item",
  weight: 1,
  meta: {
    label: "Authorized Services",
    treeAlias: "AuthorizedServices.Tree.Services",
    menus: ["Umb.Menu.AdvancedSettings"]
  }
}, re = [te], _ = "AuthorizedServices.Repository.Services.Tree", se = "AuthorizedServices.Store.Services.Tree", ie = "AuthorizedServices.Tree.Services", ne = {
  type: "repository",
  alias: _,
  name: "Authorized Services Tree Repository",
  api: ee
}, oe = {
  type: "treeStore",
  alias: se,
  name: "Authorized Services  Tree Store",
  api: Q
}, ae = {
  type: "tree",
  kind: "default",
  alias: ie,
  name: "Authorized Services Tree",
  meta: {
    repositoryAlias: _
  }
}, ce = {
  type: "treeItem",
  kind: "default",
  alias: "AuthorizedServices.TreeItem.Service",
  name: "Authorized Services Tree Item",
  forEntityTypes: [
    T,
    f
  ]
}, de = [
  ne,
  oe,
  ae,
  ce,
  ...re
], me = (t, e) => {
  e.registerMany([Z, ...de]), t.consumeContext(I, async (r) => {
    if (!r) return;
    const s = r.getOpenApiConfiguration();
    c.BASE = s.base, c.TOKEN = s.token, c.WITH_CREDENTIALS = s.withCredentials, c.CREDENTIALS = s.credentials;
  });
};
export {
  f as A,
  pe as S,
  W,
  me as o
};
//# sourceMappingURL=index-CRz22JwE.js.map
