using EmploAdImport.Log;
using IdentityModel.Client;

namespace EmploAdImport.EmploApi
{
    public class AuthorizationManager
    {
        private readonly ILogger _logger;
        static TokenClient _companyTokenClient;

        public AuthorizationManager(ILogger logger)
        {
            _logger = logger;
            var tokenendpoint = ApiConfiguration.TokenEndpoint;
            _companyTokenClient = new TokenClient(tokenendpoint, "ResourceOwnerClient", "6D359719-149A-4011-91D4-01CBA687DBBF");
        }

        public TokenResponse RequestToken(string login, string password)
        {
            _logger.WriteLine("Sending authorization request to " + ApiConfiguration.TokenEndpoint);
            return _companyTokenClient.RequestResourceOwnerPasswordAsync(login, password,
                "read write offline_access").Result;
        }

        public TokenResponse RefreshToken(string refreshToken)
        {
            _logger.WriteLine("Sending refresh token request to " + ApiConfiguration.TokenEndpoint);
            return _companyTokenClient.RequestRefreshTokenAsync(refreshToken).Result;
        }
    }
}
