import { UMB_AUTH_CONTEXT as Q } from "@umbraco-cms/backoffice/auth";
import { UmbTreeServerDataSourceBase as X, UmbTreeRepositoryBase as ee } from "@umbraco-cms/backoffice/tree";
const te = {
  bodySerializer: (t) => JSON.stringify(t, (e, r) => typeof r == "bigint" ? r.toString() : r)
};
function re({
  onRequest: t,
  onSseError: e,
  onSseEvent: r,
  responseTransformer: s,
  responseValidator: o,
  sseDefaultRetryDelay: c,
  sseMaxRetryAttempts: i,
  sseMaxRetryDelay: a,
  sseSleepFn: n,
  url: b,
  ...l
}) {
  let u;
  const S = n ?? ((d) => new Promise((h) => setTimeout(h, d)));
  return { stream: async function* () {
    let d = c ?? 3e3, h = 0;
    const v = l.signal ?? new AbortController().signal;
    for (; !v.aborted; ) {
      h++;
      const O = l.headers instanceof Headers ? l.headers : new Headers(l.headers);
      u !== void 0 && O.set("Last-Event-ID", u);
      try {
        const z = {
          redirect: "follow",
          ...l,
          body: l.serializedBody,
          headers: O,
          signal: v
        };
        let A = new Request(b, z);
        t && (A = await t(b, z));
        const p = await (l.fetch ?? globalThis.fetch)(A);
        if (!p.ok) throw new Error(`SSE failed: ${p.status} ${p.statusText}`);
        if (!p.body) throw new Error("No body in SSE response");
        const g = p.body.pipeThrough(new TextDecoderStream()).getReader();
        let y = "";
        const q = () => {
          try {
            g.cancel();
          } catch {
          }
        };
        v.addEventListener("abort", q);
        try {
          for (; ; ) {
            const { done: Y, value: Z } = await g.read();
            if (Y) break;
            y += Z, y = y.replace(/\r\n?/g, `
`);
            const U = y.split(`

`);
            y = U.pop() ?? "";
            for (const G of U) {
              const K = G.split(`
`), C = [];
              let $;
              for (const w of K)
                if (w.startsWith("data:"))
                  C.push(w.replace(/^data:\s*/, ""));
                else if (w.startsWith("event:"))
                  $ = w.replace(/^event:\s*/, "");
                else if (w.startsWith("id:"))
                  u = w.replace(/^id:\s*/, "");
                else if (w.startsWith("retry:")) {
                  const _ = Number.parseInt(w.replace(/^retry:\s*/, ""), 10);
                  Number.isNaN(_) || (d = _);
                }
              let E, B = !1;
              if (C.length) {
                const w = C.join(`
`);
                try {
                  E = JSON.parse(w), B = !0;
                } catch {
                  E = w;
                }
              }
              B && (o && await o(E), s && (E = await s(E))), r == null || r({
                data: E,
                event: $,
                id: u,
                retry: d
              }), C.length && (yield E);
            }
          }
        } finally {
          v.removeEventListener("abort", q), g.releaseLock();
        }
        break;
      } catch (z) {
        if (e == null || e(z), i !== void 0 && h >= i)
          break;
        const A = Math.min(d * 2 ** (h - 1), a ?? 3e4);
        await S(A);
      }
    }
  }() };
}
const se = (t) => {
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
}, ae = (t) => {
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
}, ie = (t) => {
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
}, H = ({
  allowReserved: t,
  explode: e,
  name: r,
  style: s,
  value: o
}) => {
  if (!e) {
    const a = (t ? o : o.map((n) => encodeURIComponent(n))).join(ae(s));
    switch (s) {
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
  const c = se(s), i = o.map((a) => s === "label" || s === "simple" ? t ? a : encodeURIComponent(a) : I({
    allowReserved: t,
    name: r,
    value: a
  })).join(c);
  return s === "label" || s === "matrix" ? c + i : i;
}, I = ({
  allowReserved: t,
  name: e,
  value: r
}) => {
  if (r == null)
    return "";
  if (typeof r == "object")
    throw new Error(
      "Deeply-nested arrays/objects aren’t supported. Provide your own `querySerializer()` to handle these."
    );
  return `${e}=${t ? r : encodeURIComponent(r)}`;
}, W = ({
  allowReserved: t,
  explode: e,
  name: r,
  style: s,
  value: o,
  valueOnly: c
}) => {
  if (o instanceof Date)
    return c ? o.toISOString() : `${r}=${o.toISOString()}`;
  if (s !== "deepObject" && !e) {
    let n = [];
    Object.entries(o).forEach(([l, u]) => {
      n = [...n, l, t ? u : encodeURIComponent(u)];
    });
    const b = n.join(",");
    switch (s) {
      case "form":
        return `${r}=${b}`;
      case "label":
        return `.${b}`;
      case "matrix":
        return `;${r}=${b}`;
      default:
        return b;
    }
  }
  const i = ie(s), a = Object.entries(o).map(
    ([n, b]) => I({
      allowReserved: t,
      name: s === "deepObject" ? `${r}[${n}]` : n,
      value: b
    })
  ).join(i);
  return s === "label" || s === "matrix" ? i + a : a;
}, ne = /\{[^{}]+\}/g, oe = ({ path: t, url: e }) => {
  let r = e;
  const s = e.match(ne);
  if (s)
    for (const o of s) {
      let c = !1, i = o.substring(1, o.length - 1), a = "simple";
      i.endsWith("*") && (c = !0, i = i.substring(0, i.length - 1)), i.startsWith(".") ? (i = i.substring(1), a = "label") : i.startsWith(";") && (i = i.substring(1), a = "matrix");
      const n = t[i];
      if (n == null)
        continue;
      if (Array.isArray(n)) {
        r = r.replace(o, H({ explode: c, name: i, style: a, value: n }));
        continue;
      }
      if (typeof n == "object") {
        r = r.replace(
          o,
          W({
            explode: c,
            name: i,
            style: a,
            value: n,
            valueOnly: !0
          })
        );
        continue;
      }
      if (a === "matrix") {
        r = r.replace(
          o,
          `;${I({
            name: i,
            value: n
          })}`
        );
        continue;
      }
      const b = encodeURIComponent(
        a === "label" ? `.${n}` : n
      );
      r = r.replace(o, b);
    }
  return r;
}, ce = ({
  baseUrl: t,
  path: e,
  query: r,
  querySerializer: s,
  url: o
}) => {
  const c = o.startsWith("/") ? o : `/${o}`;
  let i = (t ?? "") + c;
  e && (i = oe({ path: e, url: i }));
  let a = r ? s(r) : "";
  return a.startsWith("?") && (a = a.substring(1)), a && (i += `?${a}`), i;
};
function N(t) {
  const e = t.body !== void 0;
  if (e && t.bodySerializer)
    return "serializedBody" in t ? t.serializedBody !== void 0 && t.serializedBody !== "" ? t.serializedBody : null : t.body !== "" ? t.body : null;
  if (e)
    return t.body;
}
const ue = async (t, e) => {
  const r = typeof e == "function" ? await e(t) : e;
  if (r)
    return t.scheme === "bearer" ? `Bearer ${r}` : t.scheme === "basic" ? `Basic ${btoa(r)}` : r;
}, V = ({
  parameters: t = {},
  ...e
} = {}) => (s) => {
  const o = [];
  if (s && typeof s == "object")
    for (const c in s) {
      const i = s[c];
      if (i == null)
        continue;
      const a = t[c] || e;
      if (Array.isArray(i)) {
        const n = H({
          allowReserved: a.allowReserved,
          explode: !0,
          name: c,
          style: "form",
          value: i,
          ...a.array
        });
        n && o.push(n);
      } else if (typeof i == "object") {
        const n = W({
          allowReserved: a.allowReserved,
          explode: !0,
          name: c,
          style: "deepObject",
          value: i,
          ...a.object
        });
        n && o.push(n);
      } else {
        const n = I({
          allowReserved: a.allowReserved,
          name: c,
          value: i
        });
        n && o.push(n);
      }
    }
  return o.join("&");
}, le = (t) => {
  var r;
  if (!t)
    return "stream";
  const e = (r = t.split(";")[0]) == null ? void 0 : r.trim();
  if (e) {
    if (e.startsWith("application/json") || e.endsWith("+json"))
      return "json";
    if (e === "multipart/form-data")
      return "formData";
    if (["application/", "audio/", "image/", "video/"].some((s) => e.startsWith(s)))
      return "blob";
    if (e.startsWith("text/"))
      return "text";
  }
}, de = (t, e) => {
  var r, s;
  return e ? !!(t.headers.has(e) || (r = t.query) != null && r[e] || (s = t.headers.get("Cookie")) != null && s.includes(`${e}=`)) : !1;
};
async function fe(t) {
  for (const e of t.security ?? []) {
    if (de(t, e.name))
      continue;
    const r = await ue(e, t.auth);
    if (!r)
      continue;
    const s = e.name ?? "Authorization";
    switch (e.in) {
      case "query":
        t.query || (t.query = {}), t.query[s] = r;
        break;
      case "cookie":
        t.headers.append("Cookie", `${s}=${r}`);
        break;
      case "header":
      default:
        t.headers.set(s, r);
        break;
    }
  }
}
const D = (t) => ce({
  baseUrl: t.baseUrl,
  path: t.path,
  query: t.query,
  querySerializer: typeof t.querySerializer == "function" ? t.querySerializer : V(t.querySerializer),
  url: t.url
}), P = (t, e) => {
  var s;
  const r = { ...t, ...e };
  return (s = r.baseUrl) != null && s.endsWith("/") && (r.baseUrl = r.baseUrl.substring(0, r.baseUrl.length - 1)), r.headers = L(t.headers, e.headers), r;
}, he = (t) => {
  const e = [];
  return t.forEach((r, s) => {
    e.push([s, r]);
  }), e;
}, L = (...t) => {
  const e = new Headers();
  for (const r of t) {
    if (!r)
      continue;
    const s = r instanceof Headers ? he(r) : Object.entries(r);
    for (const [o, c] of s)
      if (c === null)
        e.delete(o);
      else if (Array.isArray(c))
        for (const i of c)
          e.append(o, i);
      else c !== void 0 && e.set(
        o,
        typeof c == "object" ? JSON.stringify(c) : c
      );
  }
  return e;
};
class R {
  constructor() {
    this.fns = [];
  }
  clear() {
    this.fns = [];
  }
  eject(e) {
    const r = this.getInterceptorIndex(e);
    this.fns[r] && (this.fns[r] = null);
  }
  exists(e) {
    const r = this.getInterceptorIndex(e);
    return !!this.fns[r];
  }
  getInterceptorIndex(e) {
    return typeof e == "number" ? this.fns[e] ? e : -1 : this.fns.indexOf(e);
  }
  update(e, r) {
    const s = this.getInterceptorIndex(e);
    return this.fns[s] ? (this.fns[s] = r, e) : !1;
  }
  use(e) {
    return this.fns.push(e), this.fns.length - 1;
  }
}
const pe = () => ({
  error: new R(),
  request: new R(),
  response: new R()
}), ye = V({
  allowReserved: !1,
  array: {
    explode: !0,
    style: "form"
  },
  object: {
    explode: !0,
    style: "deepObject"
  }
}), me = {
  "Content-Type": "application/json"
}, M = (t = {}) => ({
  ...te,
  headers: me,
  parseAs: "auto",
  querySerializer: ye,
  ...t
}), be = (t = {}) => {
  let e = P(M(), t);
  const r = () => ({ ...e }), s = (l) => (e = P(e, l), r()), o = pe(), c = async (l) => {
    const u = {
      ...e,
      ...l,
      fetch: l.fetch ?? e.fetch ?? globalThis.fetch,
      headers: L(e.headers, l.headers),
      serializedBody: void 0
    };
    u.security && await fe(u), u.requestValidator && await u.requestValidator(u), u.body !== void 0 && u.bodySerializer && (u.serializedBody = u.bodySerializer(u.body)), (u.body === void 0 || u.serializedBody === "") && u.headers.delete("Content-Type");
    const S = u, m = D(S);
    return { opts: S, url: m };
  }, i = async (l) => {
    const u = l.throwOnError ?? e.throwOnError, S = l.responseStyle ?? e.responseStyle;
    let m, f;
    try {
      const { opts: d, url: h } = await c(l), v = {
        redirect: "follow",
        ...d,
        body: N(d)
      };
      m = new Request(h, v);
      for (const p of o.request.fns)
        p && (m = await p(m, d));
      const O = d.fetch;
      f = await O(m);
      for (const p of o.response.fns)
        p && (f = await p(f, m, d));
      const z = {
        request: m,
        response: f
      };
      if (f.ok) {
        const p = (d.parseAs === "auto" ? le(f.headers.get("Content-Type")) : d.parseAs) ?? "json";
        if (f.status === 204 || f.headers.get("Content-Length") === "0") {
          let y;
          switch (p) {
            case "arrayBuffer":
            case "blob":
            case "text":
              y = await f[p]();
              break;
            case "formData":
              y = new FormData();
              break;
            case "stream":
              y = f.body;
              break;
            case "json":
            default:
              y = {};
              break;
          }
          return d.responseStyle === "data" ? y : {
            data: y,
            ...z
          };
        }
        let g;
        switch (p) {
          case "arrayBuffer":
          case "blob":
          case "formData":
          case "text":
            g = await f[p]();
            break;
          case "json": {
            const y = await f.text();
            g = y ? JSON.parse(y) : {};
            break;
          }
          case "stream":
            return d.responseStyle === "data" ? f.body : {
              data: f.body,
              ...z
            };
        }
        return p === "json" && (d.responseValidator && await d.responseValidator(g), d.responseTransformer && (g = await d.responseTransformer(g))), d.responseStyle === "data" ? g : {
          data: g,
          ...z
        };
      }
      const A = await f.text();
      let j;
      try {
        j = JSON.parse(A);
      } catch {
      }
      throw j ?? A;
    } catch (d) {
      let h = d;
      for (const v of o.error.fns)
        v && (h = await v(h, f, m, l));
      if (h = h || {}, u)
        throw h;
      return S === "data" ? void 0 : {
        error: h,
        request: m,
        response: f
      };
    }
  }, a = (l) => (u) => i({ ...u, method: l }), n = (l) => async (u) => {
    const { opts: S, url: m } = await c(u);
    return re({
      ...S,
      body: S.body,
      method: l,
      onRequest: async (f, d) => {
        let h = new Request(f, d);
        for (const v of o.request.fns)
          v && (h = await v(h, S));
        return h;
      },
      serializedBody: N(S),
      url: m
    });
  };
  return {
    buildUrl: (l) => D({ ...e, ...l }),
    connect: a("CONNECT"),
    delete: a("DELETE"),
    get: a("GET"),
    getConfig: r,
    head: a("HEAD"),
    interceptors: o,
    options: a("OPTIONS"),
    patch: a("PATCH"),
    post: a("POST"),
    put: a("PUT"),
    request: i,
    setConfig: s,
    sse: {
      connect: n("CONNECT"),
      delete: n("DELETE"),
      get: n("GET"),
      head: n("HEAD"),
      options: n("OPTIONS"),
      patch: n("PATCH"),
      post: n("POST"),
      put: n("PUT"),
      trace: n("TRACE")
    },
    trace: a("TRACE")
  };
}, T = be(M({ baseUrl: "http://localhost:3010/", throwOnError: !0 }));
class Ue {
  static postService(e) {
    return (e.client ?? T).post({
      security: [{ scheme: "bearer", type: "http" }],
      url: "/umbraco/authorized-services/management/api/v1/service",
      ...e,
      headers: {
        "Content-Type": "application/json",
        ...e.headers
      }
    });
  }
  static getServiceByAlias(e) {
    return (e.client ?? T).get({
      security: [{ scheme: "bearer", type: "http" }],
      url: "/umbraco/authorized-services/management/api/v1/service/{alias}",
      ...e
    });
  }
  static getServiceByAliasSampleRequest(e) {
    return (e.client ?? T).get({
      security: [{ scheme: "bearer", type: "http" }],
      url: "/umbraco/authorized-services/management/api/v1/service/{alias}/sample-request",
      ...e
    });
  }
  static postServiceApiKey(e) {
    return (e.client ?? T).post({
      security: [{ scheme: "bearer", type: "http" }],
      url: "/umbraco/authorized-services/management/api/v1/service/api-key",
      ...e,
      headers: {
        "Content-Type": "application/json",
        ...e.headers
      }
    });
  }
  static postServiceOauth1(e) {
    return (e.client ?? T).post({
      security: [{ scheme: "bearer", type: "http" }],
      url: "/umbraco/authorized-services/management/api/v1/service/oauth1",
      ...e,
      headers: {
        "Content-Type": "application/json",
        ...e.headers
      }
    });
  }
  static postServiceOauth1RequestToken(e) {
    return (e.client ?? T).post({
      security: [{ scheme: "bearer", type: "http" }],
      url: "/umbraco/authorized-services/management/api/v1/service/oauth1/request-token",
      ...e,
      headers: {
        "Content-Type": "application/json",
        ...e.headers
      }
    });
  }
  static postServiceOauth2(e) {
    return (e.client ?? T).post({
      security: [{ scheme: "bearer", type: "http" }],
      url: "/umbraco/authorized-services/management/api/v1/service/oauth2",
      ...e,
      headers: {
        "Content-Type": "application/json",
        ...e.headers
      }
    });
  }
  static postServiceOauth2ClientCredentials(e) {
    return (e.client ?? T).post({
      security: [{ scheme: "bearer", type: "http" }],
      url: "/umbraco/authorized-services/management/api/v1/service/oauth2/client-credentials",
      ...e,
      headers: {
        "Content-Type": "application/json",
        ...e.headers
      }
    });
  }
  static postServiceRevoke(e) {
    return (e.client ?? T).post({
      security: [{ scheme: "bearer", type: "http" }],
      url: "/umbraco/authorized-services/management/api/v1/service/revoke",
      ...e,
      headers: {
        "Content-Type": "application/json",
        ...e.headers
      }
    });
  }
}
class ve {
  static getTree(e) {
    return ((e == null ? void 0 : e.client) ?? T).get({
      security: [{ scheme: "bearer", type: "http" }],
      url: "/umbraco/authorized-services/management/api/v1/tree",
      ...e
    });
  }
}
const k = "authorized-service", x = "authorized-service-root", Se = "AuthorizedServices.Workspace", ge = [
  {
    type: "workspace",
    kind: "routable",
    alias: Se,
    name: "Authorized Service Workspace",
    api: () => import("./workspace.context-Cw0injd0.js"),
    meta: {
      entityType: k
    }
  }
];
class we extends X {
  constructor(e) {
    super(e, {
      getRootItems: F,
      getChildrenOf: Te,
      getAncestorsOf: ze,
      mapper: Ae
    });
  }
}
const F = async (t) => {
  const { data: e } = await ve.getTree();
  return e ? { data: { ...e, total: Number(e.total), totalBefore: 0, totalAfter: 0 } } : { error: void 0 };
}, Te = (t) => {
  if (t.parent.unique === null)
    return F();
  throw new Error("Not supported for the authorized services tree");
}, ze = (t) => {
  throw new Error("Not supported for the authorized services tree");
}, Ae = (t) => ({ ...t, parent: {
  unique: null,
  entityType: x
}, entityType: k, isFolder: !1, hasChildren: !1 });
class Ee extends ee {
  constructor(e) {
    super(e, we);
  }
  async requestTreeRoot() {
    return { data: {
      unique: null,
      entityType: x,
      name: "Authorized Services",
      hasChildren: !0,
      isFolder: !0
    } };
  }
}
const Oe = [
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
], J = "AuthorizedServices.Repository.Services.Tree", Ce = "AuthorizedServices.Tree.Services", Ie = {
  type: "repository",
  alias: J,
  name: "Authorized Services Tree Repository",
  api: Ee
}, je = [
  {
    type: "tree",
    kind: "default",
    alias: Ce,
    name: "Authorized Services Tree",
    meta: {
      repositoryAlias: J
    }
  }
], Re = [
  {
    type: "treeItem",
    kind: "default",
    alias: "AuthorizedServices.TreeItem.Service",
    name: "Authorized Services Tree Item",
    forEntityTypes: [
      x,
      k
    ]
  }
], ke = [
  Ie,
  ...je,
  ...Re,
  ...Oe
], $e = (t, e) => {
  e.registerMany([...ge, ...ke]), t.consumeContext(Q, async (r) => {
    if (!r) return;
    const s = r.getOpenApiConfiguration();
    T.setConfig({
      baseUrl: (s == null ? void 0 : s.base) ?? "",
      auth: (s == null ? void 0 : s.token) ?? void 0,
      credentials: (s == null ? void 0 : s.credentials) ?? "same-origin"
    });
  });
};
export {
  k as A,
  Ue as S,
  Se as W,
  $e as o
};
//# sourceMappingURL=index-BJOu1Tl7.js.map
