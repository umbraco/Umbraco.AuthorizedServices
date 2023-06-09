function authorizedServiceResource($q, $http) {

  const apiRoot = "backoffice/UmbracoAuthorizedServices/AuthorizedService/";

  return {

    getByAlias: function (alias: string) {
      return $http.get(apiRoot + "GetByAlias?alias=" + alias);
    },

    sendSampleRequest: function (alias: string, path: string) {
      return $http.get(apiRoot + "SendSampleRequest?alias=" + alias + "&path=" + path);
    },

    revokeAccess: function (alias: string) {
      return $http.post(apiRoot + "RevokeAccess", { alias: alias });
    }
  };
}

angular.module('umbraco.resources').factory('authorizedServiceResource', authorizedServiceResource);
