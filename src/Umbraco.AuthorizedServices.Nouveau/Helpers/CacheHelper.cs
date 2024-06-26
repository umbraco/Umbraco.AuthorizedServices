namespace Umbraco.AuthorizedServices.Helpers
{
    internal static class CacheHelper
    {
        private const string AuthorizationPayloadKeyFormat = "Umbraco_AuthorizedServices_Payload_{0}";

        private const string AuthorizationApiKeyFormat = "Umbraco_AuthorizedServices_ApiKey_{0}";

        private const string AuthorizationTokenKeyFormat = "Umbraco_AuthorizedServices_Token_{0}";

        private const string AuthorizationTokenSecretFormat = "Umbraco_AuthorizedServices_TokenSecret_{0}";

        public static string GetPayloadKey(string serviceAlias) => GetCacheKey(AuthorizationPayloadKeyFormat, serviceAlias);

        public static string GetApiKeyCacheKey(string serviceAlias) => GetCacheKey(AuthorizationApiKeyFormat, serviceAlias);

        public static string GetTokenCacheKey(string serviceAlias) => GetCacheKey(AuthorizationTokenKeyFormat, serviceAlias);

        public static string GetTokenSecretCacheKey(string serviceAlias) => GetCacheKey(AuthorizationTokenSecretFormat, serviceAlias);

        private static string GetCacheKey(string format, string serviceAlias) => string.Format(format, serviceAlias);
    }
}
