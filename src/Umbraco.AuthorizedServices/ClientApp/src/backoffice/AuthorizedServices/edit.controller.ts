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
      });
  };

  loadServiceDetails(serviceAlias);

}

angular.module("umbraco").controller("AuthorizedServiceEditController", AuthorizedServiceEditController);
