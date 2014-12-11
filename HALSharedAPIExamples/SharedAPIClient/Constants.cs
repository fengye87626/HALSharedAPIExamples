namespace SharedAPIClient
{
    public static class Constants
    {
        public static readonly string UserNameConfigurationKey = "username";
        public static readonly string TestPasswordConfigurationKey = "testPassword";
        public static readonly string BaseSharedApiUrlConfigurationKey = "BaseSharedApiUrl";
        public static readonly string TestVehicleGuidConfigurationKey = "TestVehicleGuid";
        public static readonly string RootApiUrlConfigurationKey = "RootApiUrl";
        public static readonly string VehicleListRel = "vehicle-list";
        public static readonly string TokenAction = "token";
        public static readonly string TokenGrantTypeConfigurationKey = "TokenGrantType";
        public static readonly string TokenContextTypeConfigurationKey = "TokenContextType";
        public static readonly string TokenGrantTypeName = "grant_type";
        public static readonly string TokenUserName = "username";
        public static readonly string TokenPassword = "password";

        public static readonly string AuthClientIdConfigurationKey = "AuthClientId";
        public static readonly string AuthSecretPairConfigurationKey = "AuthSecretPair";

        public static readonly string DefaultContentTypeHeaderValue = "application/json";
        public static readonly string DefaultAcceptHeaderValue = "application/hal+json";
        public static readonly string ContentTypeHeaderName = "Content-Type";
        public static readonly string AcceptHeaderName = "Accept";
        public static readonly string AuthorizationHeaderName = "Authorization";
        public static readonly string AuthorizationType = "Bearer";
        public static readonly string AuthBasic = "Basic";

    }
}
