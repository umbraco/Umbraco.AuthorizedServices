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
        vm.authenticationMethod = serviceData.authenticationMethod;
        vm.canManuallyProvideToken = serviceData.canManuallyProvideToken;
        vm.canManuallyProvideApiKey = serviceData.canManuallyProvideApiKey;
        vm.authorizationUrl = serviceData.authorizationUrl;
        vm.sampleRequest = serviceData.sampleRequest;
        vm.sampleRequestResponse = null;
        vm.settings = serviceData.settings;
        vm.isOAuthBasedAuthenticationMethod = serviceData.authenticationMethod !== AuthenticationMethod.ApiKey;
      });
  }

  vm.authorizeAccess = function () {
    if (vm.authenticationMethod.toString() === AuthenticationMethod.OAuth2ClientCredentials.toString()) {
      authorizedServiceResource.generateToken(serviceAlias)
        .then(function () {
          notificationsService.success("Authorized Services", "The '" + vm.displayName + "' service has been authorized.");
          loadServiceDetails(serviceAlias);
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
    authorizedServiceResource.sendSampleRequest(serviceAlias, vm.sampleRequest)
      .then(function (response) {
        vm.sampleRequestResponse = "Request: " + vm.sampleRequest + "\r\nResponse: " + JSON.stringify(response.data, null, 2);
      })
      .catch(function (e) {
        notificationsService.error("Authorized Services", "The sample request did not complete: " + e.data.ExceptionMessage);
      });
  };

  vm.saveAccessToken = function () {
    let inAccessToken = <HTMLInputElement>document.getElementById("inAccessToken");

    if (inAccessToken) {
      authorizedServiceResource.saveToken(serviceAlias, inAccessToken.value)
        .then(function () {
          notificationsService.success("Authorized Services", "The '" + vm.displayName + "' service access token has been saved.");
          inAccessToken.value = "";
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
