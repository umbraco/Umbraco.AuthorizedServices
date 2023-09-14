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
        vm.authorizationUrl = serviceData.authorizationUrl;
        vm.sampleRequest = serviceData.sampleRequest;
        vm.sampleRequestResponse = null;
        vm.settings = serviceData.settings;
      });
  }

  vm.authorizeAccess = function () {
    location.href = vm.authorizationUrl;
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

  loadServiceDetails(serviceAlias);

}

angular.module("umbraco").controller("AuthorizedServiceEditController", AuthorizedServiceEditController);
