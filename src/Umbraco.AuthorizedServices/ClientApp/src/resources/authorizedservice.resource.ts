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
    },
    saveToken: function (alias: string, token: string) {
      return $http.post(apiRoot + "SaveToken", { alias: alias, token: token });
    },
    saveApiKey: function (alias: string, apiKey: string) {
      return $http.post(apiRoot + "SaveApiKey", { alias: alias, apiKey: apiKey });
    },
    generateToken: function (alias: string) {
      return $http.post(apiRoot + "GenerateToken", { alias: alias });
    }
  };
}

angular.module('umbraco.resources').factory('authorizedServiceResource', authorizedServiceResource);
