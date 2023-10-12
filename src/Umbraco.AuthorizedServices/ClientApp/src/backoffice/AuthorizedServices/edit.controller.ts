import { AuthenticationMethod } from "./authorizedservice.constants";

function AuthorizedServiceEditController(this: any, $routeParams, $location, authorizedServiceResource, notificationsService) {

  const vm = this;
  const serviceAlias = $routeParams.id;

  function loadServiceDetails(serviceAlias) {
    authorizedServiceResource.getByAlias(serviceAlias)
      .then(function (response) {
        const serviceData = response.data;
        vm.displayName = serviceData.displayName;
        vm.headerName = "Authorized Services: " + vm.displayName;
        vm.isAuthorized = serviceData.isAuthorized;
        vm.canManuallyProvideToken = serviceData.canManuallyProvideToken;
        vm.canManuallyProvideApiKey = serviceData.canManuallyProvideApiKey;
        vm.authorizationUrl = serviceData.authorizationUrl;
        vm.sampleRequest = serviceData.sampleRequest;
        vm.sampleRequestResponse = null;
        vm.settings = serviceData.settings;

        vm.authenticationMethod = {
          isOAuth1: serviceData.authenticationMethod === AuthenticationMethod.OAuth1,
          isOAuth2AuthorizationCode: serviceData.authenticationMethod === AuthenticationMethod.OAuth2AuthorizationCode,
          isOAuth2ClientCredentials: serviceData.authenticationMethod === AuthenticationMethod.OAuth2ClientCredentials,
          isApiKey: serviceData.authenticationMethod === AuthenticationMethod.ApiKey
        };
      }, function (ex) {
        notificationsService.error("Authorized Services", ex.data.ExceptionMessage);
      });
  }

  vm.authorizeAccess = function () {
    if (vm.authenticationMethod.isOAuth2ClientCredentials) {
      authorizedServiceResource.generateToken(serviceAlias)
        .then(function () {
          notificationsService.success("Authorized Services", "The '" + vm.displayName + "' service has been authorized.");
          loadServiceDetails(serviceAlias);
        });
    } if (vm.authenticationMethod.isOAuth1) {
      authorizedServiceResource.generateRequestToken(serviceAlias)
        .then(function (response) {
          location.href = response.data.message;
        });
    }
    else {
      location.href = vm.authorizationUrl;
    }
  };

  vm.revokeAccess = function () {
    authorizedServiceResource.revokeAccess(serviceAlias)
      .then(function () {
        notificationsService.success("Authorized Services", "The '" + vm.displayName + "' service access has been revoked.");
        loadServiceDetails(serviceAlias);
      });
  };

  vm.sendSampleRequest = function () {
    vm.sampleRequestResponse = null;
    authorizedServiceResource.sendSampleRequest(serviceAlias)
      .then(function (response) {
        vm.sampleRequestResponse = "Request: " + vm.sampleRequest + "\r\nResponse: " + JSON.stringify(response.data, null, 2);
      })
      .catch(function (e) {
        notificationsService.error("Authorized Services", "The sample request did not complete: " + e.data.ExceptionMessage);
      });
  };

  vm.saveOAuth2AccessToken = function () {
    let inAccessToken = <HTMLInputElement>document.getElementById("inAccessToken");

    if (inAccessToken) {
      authorizedServiceResource.saveOAuth2Token(serviceAlias, inAccessToken.value)
        .then(function () {
          notificationsService.success("Authorized Services", "The '" + vm.displayName + "' service access token has been saved.");
          inAccessToken.value = "";
          loadServiceDetails(serviceAlias);
        });
    }
  }

  vm.saveOAuth1TokenDetails = function () {
    let inAccessToken = <HTMLInputElement>document.getElementById("inAccessToken");
    let inTokenSecret = <HTMLInputElement>document.getElementById("inTokenSecret");

    if (inAccessToken && inTokenSecret) {
      authorizedServiceResource.saveOAuth1Token(serviceAlias, inAccessToken.value, inTokenSecret.value)
        .then(function () {
          notificationsService.success("Authorized Services", "The '" + vm.displayName + "' service access token and token secret have been saved.");
          inAccessToken.value = "";
          inTokenSecret.value = "";
          loadServiceDetails(serviceAlias);
        });
    }
  }

  vm.saveApiKey = function () {
    let inApiKey = <HTMLInputElement>document.getElementById("inApiKey");

    if (inApiKey) {
      authorizedServiceResource.saveApiKey(serviceAlias, inApiKey.value)
        .then(function () {
          notificationsService.success("Authorized Services", "The '" + vm.displayName + "' service API key has been saved.");
          inApiKey.value = "";
          loadServiceDetails(serviceAlias);
        });
    }
  }

  loadServiceDetails(serviceAlias);

}

angular.module("umbraco").controller("AuthorizedServiceEditController", AuthorizedServiceEditController);
