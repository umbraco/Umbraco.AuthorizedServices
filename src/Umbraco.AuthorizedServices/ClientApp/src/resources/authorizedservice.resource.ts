function authorizedServiceResource($q, $http) {

  const apiRoot = "backoffice/UmbracoAuthorizedServices/AuthorizedService/";

  return {

    getByAlias: function (alias: string) {
      return $http.get(apiRoot + "GetByAlias?alias=" + alias);
    },
    sendSampleRequest: function (alias: string) {
      return $http.get(apiRoot + "SendSampleRequest?alias=" + alias);
    },
    revokeAccess: function (alias: string) {
      return $http.post(apiRoot + "RevokeAccess", { alias: alias });
    },
    saveOAuth2Token: function (alias: string, token: string) {
      return $http.post(apiRoot + "SaveOAuth2Token", { alias: alias, token: token });
    },
    saveOAuth1Token: function (alias: string, token: string, tokenSecret: string) {
      return $http.post(apiRoot + "SaveOAuth1Token", { alias: alias, token: token, tokenSecret: tokenSecret });
    },
    saveApiKey: function (alias: string, apiKey: string) {
      return $http.post(apiRoot + "SaveApiKey", { alias: alias, apiKey: apiKey });
    },
    generateOAuth2ClientCredentialsToken: function (alias: string) {
      return $http.post(apiRoot + "GenerateOAuth2ClientCredentialsToken", { alias: alias });
    },
    generateOAuth1RequestToken: function (alias: string) {
      return $http.post(apiRoot + "GenerateOAuth1RequestToken", { alias: alias});
    }
  };
}

angular.module('umbraco.resources').factory('authorizedServiceResource', authorizedServiceResource);
