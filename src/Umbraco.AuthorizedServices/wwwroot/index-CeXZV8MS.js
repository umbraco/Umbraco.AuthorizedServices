var U = Object.defineProperty;
var $ = (t, e, r) => e in t ? U(t, e, { enumerable: !0, configurable: !0, writable: !0, value: r }) : t[e] = r;
var E = (t, e, r) => $(t, typeof e != "symbol" ? e + "" : e, r);
import { UMB_AUTH_CONTEXT as k } from "@umbraco-cms/backoffice/auth";
import { umbHttpClient as D } from "@umbraco-cms/backoffice/http-client";
import { UmbTreeServerDataSourceBase as W, UmbUniqueTreeStore as N, UmbTreeRepositoryBase as H } from "@umbraco-cms/backoffice/tree";
import { UmbContextToken as P } from "@umbraco-cms/backoffice/context-api";
var V = async (t, e) => {
  let r = typeof e == "function" ? await e(t) : e;
  if (r) return t.scheme === "bearer" ? `Bearer ${r}` : t.scheme === "basic" ? `Basic ${btoa(r)}` : r;
}, B = { bodySerializer: (t) => JSON.stringify(t, (e, r) => typeof r == "bigint" ? r.toString() : r) }, M = (t) => {
  switch (t) {
    case "label":
      return ".";
    case "matrix":
      return ";";
    case "simple":
      return ",";
    default:
      return "&";
  }
}, L = (t) => {
  switch (t) {
    case "form":
      return ",";
    case "pipeDelimited":
      return "|";
    case "spaceDelimited":
      return "%20";
    default:
      return ",";
  }
}, Z = (t) => {
  switch (t) {
    case "label":
      return ".";
    case "matrix":
      return ";";
    case "simple":
      return ",";
    default:
      return "&";
  }
}, z = ({ allowReserved: t, explode: e, name: r, style: i, value: n }) => {
  if (!e) {
    let a = (t ? n : n.map((u) => encodeURIComponent(u))).join(L(i));
    switch (i) {
      case "label":
        return `.${a}`;
      case "matrix":
        return `;${r}=${a}`;
      case "simple":
        return a;
      default:
        return `${r}=${a}`;
    }
  }
  let l = M(i), s = n.map((a) => i === "label" || i === "simple" ? t ? a : encodeURIComponent(a) : b({ allowReserved: t, name: r, value: a })).join(l);
  return i === "label" || i === "matrix" ? l + s : s;
}, b = ({ allowReserved: t, name: e, value: r }) => {
  if (r == null) return "";
  if (typeof r == "object") throw new Error("Deeply-nested arrays/objects aren’t supported. Provide your own `querySerializer()` to handle these.");
  return `${e}=${t ? r : encodeURIComponent(r)}`;
}, I = ({ allowReserved: t, explode: e, name: r, style: i, value: n, valueOnly: l }) => {
  if (n instanceof Date) return l ? n.toISOString() : `${r}=${n.toISOString()}`;
  if (i !== "deepObject" && !e) {
    let u = [];
    Object.entries(n).forEach(([m, p]) => {
      u = [...u, m, t ? p : encodeURIComponent(p)];
    });
    let d = u.join(",");
    switch (i) {
      case "form":
        return `${r}=${d}`;
      case "label":
        return `.${d}`;
      case "matrix":
        return `;${r}=${d}`;
      default:
        return d;
    }
  }
  let s = Z(i), a = Object.entries(n).map(([u, d]) => b({ allowReserved: t, name: i === "deepObject" ? `${r}[${u}]` : u, value: d })).join(s);
  return i === "label" || i === "matrix" ? s + a : a;
}, Y = /\{[^{}]+\}/g, J = ({ path: t, url: e }) => {
  let r = e, i = e.match(Y);
  if (i) for (let n of i) {
    let l = !1, s = n.substring(1, n.length - 1), a = "simple";
    s.endsWith("*") && (l = !0, s = s.substring(0, s.length - 1)), s.startsWith(".") ? (s = s.substring(1), a = "label") : s.startsWith(";") && (s = s.substring(1), a = "matrix");
    let u = t[s];
    if (u == null) continue;
    if (Array.isArray(u)) {
      r = r.replace(n, z({ explode: l, name: s, style: a, value: u }));
      continue;
    }
    if (typeof u == "object") {
      r = r.replace(n, I({ explode: l, name: s, style: a, value: u, valueOnly: !0 }));
      continue;
    }
    if (a === "matrix") {
      r = r.replace(n, `;${b({ name: s, value: u })}`);
      continue;
    }
    let d = encodeURIComponent(a === "label" ? `.${u}` : u);
    r = r.replace(n, d);
  }
  return r;
}, C = ({ allowReserved: t, array: e, object: r } = {}) => (i) => {
  let n = [];
  if (i && typeof i == "object") for (let l in i) {
    let s = i[l];
    if (s != null) if (Array.isArray(s)) {
      let a = z({ allowReserved: t, explode: !0, name: l, style: "form", value: s, ...e });
      a && n.push(a);
    } else if (typeof s == "object") {
      let a = I({ allowReserved: t, explode: !0, name: l, style: "deepObject", value: s, ...r });
      a && n.push(a);
    } else {
      let a = b({ allowReserved: t, name: l, value: s });
      a && n.push(a);
    }
  }
  return n.join("&");
}, F = (t) => {
  var r;
  if (!t) return "stream";
  let e = (r = t.split(";")[0]) == null ? void 0 : r.trim();
  if (e) {
    if (e.startsWith("application/json") || e.endsWith("+json")) return "json";
    if (e === "multipart/form-data") return "formData";
    if (["application/", "audio/", "image/", "video/"].some((i) => e.startsWith(i))) return "blob";
    if (e.startsWith("text/")) return "text";
  }
}, G = async ({ security: t, ...e }) => {
  for (let r of t) {
    let i = await V(r, e.auth);
    if (!i) continue;
    let n = r.name ?? "Authorization";
    switch (r.in) {
      case "query":
        e.query || (e.query = {}), e.query[n] = i;
        break;
      case "cookie":
        e.headers.append("Cookie", `${n}=${i}`);
        break;
      case "header":
      default:
        e.headers.set(n, i);
        break;
    }
    return;
  }
}, R = (t) => K({ baseUrl: t.baseUrl, path: t.path, query: t.query, querySerializer: typeof t.querySerializer == "function" ? t.querySerializer : C(t.querySerializer), url: t.url }), K = ({ baseUrl: t, path: e, query: r, querySerializer: i, url: n }) => {
  let l = n.startsWith("/") ? n : `/${n}`, s = (t ?? "") + l;
  e && (s = J({ path: e, url: s }));
  let a = r ? i(r) : "";
  return a.startsWith("?") && (a = a.substring(1)), a && (s += `?${a}`), s;
}, w = (t, e) => {
  var i;
  let r = { ...t, ...e };
  return (i = r.baseUrl) != null && i.endsWith("/") && (r.baseUrl = r.baseUrl.substring(0, r.baseUrl.length - 1)), r.headers = O(t.headers, e.headers), r;
}, O = (...t) => {
  let e = new Headers();
  for (let r of t) {
    if (!r || typeof r != "object") continue;
    let i = r instanceof Headers ? r.entries() : Object.entries(r);
    for (let [n, l] of i) if (l === null) e.delete(n);
    else if (Array.isArray(l)) for (let s of l) e.append(n, s);
    else l !== void 0 && e.set(n, typeof l == "object" ? JSON.stringify(l) : l);
  }
  return e;
}, T = class {
  constructor() {
    E(this, "_fns");
    this._fns = [];
  }
  clear() {
    this._fns = [];
  }
  getInterceptorIndex(t) {
    return typeof t == "number" ? this._fns[t] ? t : -1 : this._fns.indexOf(t);
  }
  exists(t) {
    let e = this.getInterceptorIndex(t);
    return !!this._fns[e];
  }
  eject(t) {
    let e = this.getInterceptorIndex(t);
    this._fns[e] && (this._fns[e] = null);
  }
  update(t, e) {
    let r = this.getInterceptorIndex(t);
    return this._fns[r] ? (this._fns[r] = e, t) : !1;
  }
  use(t) {
    return this._fns = [...this._fns, t], this._fns.length - 1;
  }
}, X = () => ({ error: new T(), request: new T(), response: new T() }), Q = C({ allowReserved: !1, array: { explode: !0, style: "form" }, object: { explode: !0, style: "deepObject" } }), ee = { "Content-Type": "application/json" }, _ = (t = {}) => ({ ...B, headers: ee, parseAs: "auto", querySerializer: Q, ...t }), te = (t = {}) => {
  let e = w(_(), t), r = () => ({ ...e }), i = (s) => (e = w(e, s), r()), n = X(), l = async (s) => {
    let a = { ...e, ...s, fetch: s.fetch ?? e.fetch ?? globalThis.fetch, headers: O(e.headers, s.headers) };
    a.security && await G({ ...a, security: a.security }), a.body && a.bodySerializer && (a.body = a.bodySerializer(a.body)), (a.body === void 0 || a.body === "") && a.headers.delete("Content-Type");
    let u = R(a), d = { redirect: "follow", ...a }, m = new Request(u, d);
    for (let o of n.request._fns) o && (m = await o(m, a));
    let p = a.fetch, c = await p(m);
    for (let o of n.response._fns) o && (c = await o(c, m, a));
    let v = { request: m, response: c };
    if (c.ok) {
      if (c.status === 204 || c.headers.get("Content-Length") === "0") return a.responseStyle === "data" ? {} : { data: {}, ...v };
      let o = (a.parseAs === "auto" ? F(c.headers.get("Content-Type")) : a.parseAs) ?? "json";
      if (o === "stream") return a.responseStyle === "data" ? c.body : { data: c.body, ...v };
      let y = await c[o]();
      return o === "json" && (a.responseValidator && await a.responseValidator(y), a.responseTransformer && (y = await a.responseTransformer(y))), a.responseStyle === "data" ? y : { data: y, ...v };
    }
    let S = await c.text();
    try {
      S = JSON.parse(S);
    } catch {
    }
    let f = S;
    for (let o of n.error._fns) o && (f = await o(S, c, m, a));
    if (f = f || {}, a.throwOnError) throw f;
    return a.responseStyle === "data" ? void 0 : { error: f, ...v };
  };
  return { buildUrl: R, connect: (s) => l({ ...s, method: "CONNECT" }), delete: (s) => l({ ...s, method: "DELETE" }), get: (s) => l({ ...s, method: "GET" }), getConfig: r, head: (s) => l({ ...s, method: "HEAD" }), interceptors: n, options: (s) => l({ ...s, method: "OPTIONS" }), patch: (s) => l({ ...s, method: "PATCH" }), post: (s) => l({ ...s, method: "POST" }), put: (s) => l({ ...s, method: "PUT" }), request: l, setConfig: i, trace: (s) => l({ ...s, method: "TRACE" }) };
};
const h = te(_({
  baseUrl: "http://localhost:3010",
  throwOnError: !0
}));
class Re {
  static postService(e) {
    return ((e == null ? void 0 : e.client) ?? h).post({
      security: [
        {
          scheme: "bearer",
          type: "http"
        }
      ],
      url: "/umbraco/authorized-services/management/api/v1/service",
      ...e,
      headers: {
        "Content-Type": "application/json",
        ...e == null ? void 0 : e.headers
      }
    });
  }
  static getServiceByAlias(e) {
    return (e.client ?? h).get({
      security: [
        {
          scheme: "bearer",
          type: "http"
        }
      ],
      url: "/umbraco/authorized-services/management/api/v1/service/{alias}",
      ...e
    });
  }
  static getServiceByAliasSampleRequest(e) {
    return (e.client ?? h).get({
      security: [
        {
          scheme: "bearer",
          type: "http"
        }
      ],
      url: "/umbraco/authorized-services/management/api/v1/service/{alias}/sample-request",
      ...e
    });
  }
  static postServiceApiKey(e) {
    return ((e == null ? void 0 : e.client) ?? h).post({
      security: [
        {
          scheme: "bearer",
          type: "http"
        }
      ],
      url: "/umbraco/authorized-services/management/api/v1/service/api-key",
      ...e,
      headers: {
        "Content-Type": "application/json",
        ...e == null ? void 0 : e.headers
      }
    });
  }
  static postServiceOauth1(e) {
    return ((e == null ? void 0 : e.client) ?? h).post({
      security: [
        {
          scheme: "bearer",
          type: "http"
        }
      ],
      url: "/umbraco/authorized-services/management/api/v1/service/oauth1",
      ...e,
      headers: {
        "Content-Type": "application/json",
        ...e == null ? void 0 : e.headers
      }
    });
  }
  static postServiceOauth1RequestToken(e) {
    return ((e == null ? void 0 : e.client) ?? h).post({
      security: [
        {
          scheme: "bearer",
          type: "http"
        }
      ],
      url: "/umbraco/authorized-services/management/api/v1/service/oauth1/request-token",
      ...e,
      headers: {
        "Content-Type": "application/json",
        ...e == null ? void 0 : e.headers
      }
    });
  }
  static postServiceOauth2(e) {
    return ((e == null ? void 0 : e.client) ?? h).post({
      security: [
        {
          scheme: "bearer",
          type: "http"
        }
      ],
      url: "/umbraco/authorized-services/management/api/v1/service/oauth2",
      ...e,
      headers: {
        "Content-Type": "application/json",
        ...e == null ? void 0 : e.headers
      }
    });
  }
  static postServiceOauth2ClientCredentials(e) {
    return ((e == null ? void 0 : e.client) ?? h).post({
      security: [
        {
          scheme: "bearer",
          type: "http"
        }
      ],
      url: "/umbraco/authorized-services/management/api/v1/service/oauth2/client-credentials",
      ...e,
      headers: {
        "Content-Type": "application/json",
        ...e == null ? void 0 : e.headers
      }
    });
  }
  static postServiceRevoke(e) {
    return ((e == null ? void 0 : e.client) ?? h).post({
      security: [
        {
          scheme: "bearer",
          type: "http"
        }
      ],
      url: "/umbraco/authorized-services/management/api/v1/service/revoke",
      ...e,
      headers: {
        "Content-Type": "application/json",
        ...e == null ? void 0 : e.headers
      }
    });
  }
}
class re {
  static getTree(e) {
    return ((e == null ? void 0 : e.client) ?? h).get({
      security: [
        {
          scheme: "bearer",
          type: "http"
        }
      ],
      url: "/umbraco/authorized-services/management/api/v1/tree",
      ...e
    });
  }
}
const g = "authorized-service", A = "authorized-service-root", ae = "AuthorizedServices.Workspace", se = [
  {
    type: "workspace",
    kind: "routable",
    alias: ae,
    name: "Authorized Service Workspace",
    api: () => import("./workspace.context-VBXKB-E-.js"),
    meta: {
      entityType: g
    }
  }
];
class ie extends W {
  constructor(e) {
    super(e, {
      getRootItems: j,
      getChildrenOf: ne,
      getAncestorsOf: le,
      mapper: ue
    });
  }
}
const j = () => re.getTree(), ne = (t) => {
  if (t.parent.unique === null)
    return j();
  throw new Error("Not supported for the authorized services tree");
}, le = () => {
  throw new Error("Not supported for the authorized services tree");
}, ue = (t) => ({ ...t, parent: {
  unique: null,
  entityType: A
}, entityType: g, isFolder: !1, hasChildren: !1 });
class ce extends N {
  constructor(e) {
    super(e, x.toString());
  }
}
const x = new P(
  "AuthorizedServicesTreeStore"
);
class oe extends H {
  constructor(e) {
    super(
      e,
      ie,
      x
    );
  }
  async requestTreeRoot() {
    return { data: {
      unique: null,
      entityType: A,
      name: "Authorized Services",
      hasChildren: !0,
      isFolder: !0
    } };
  }
}
const he = [
  {
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
  }
], q = "AuthorizedServices.Repository.Services.Tree", de = "AuthorizedServices.Store.Services.Tree", me = "AuthorizedServices.Tree.Services", fe = {
  type: "repository",
  alias: q,
  name: "Authorized Services Tree Repository",
  api: oe
}, ye = {
  type: "treeStore",
  alias: de,
  name: "Authorized Services  Tree Store",
  api: ce
}, pe = [
  {
    type: "tree",
    kind: "default",
    alias: me,
    name: "Authorized Services Tree",
    meta: {
      repositoryAlias: q
    }
  }
], ve = [
  {
    type: "treeItem",
    kind: "default",
    alias: "AuthorizedServices.TreeItem.Service",
    name: "Authorized Services Tree Item",
    forEntityTypes: [
      A,
      g
    ]
  }
], Se = [
  fe,
  ye,
  ...pe,
  ...ve,
  ...he
], we = (t, e) => {
  e.registerMany([...se, ...Se]), t.consumeContext(k, async (r) => {
    r && h.setConfig(D.getConfig());
  });
};
export {
  g as A,
  Re as S,
  ae as W,
  we as o
};
//# sourceMappingURL=index-CeXZV8MS.js.map
