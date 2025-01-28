using System.Text;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace CollabAPIMEP.APS
{
    public class APS_Service
    {
        #region Fields
        AuthenticationClient authenticationClient = null;
        string codeVerifier = null;
        ThreeLeggedToken ThreeLeggedToken = null;
        #endregion

        #region Properties
        public string ClientId { get; private set; }
        public string CallbackUrl { get; private set; }
        public Scopes[] Scopes { get; private set; }
        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="APS_Service"/> class.
        /// </summary>
        /// <param name="clientId">The client ID.</param>
        /// <param name="callbackUrl">The callback URL.</param>
        /// <param name="scopes">The scopes.</param>
        public APS_Service(string clientId, string callbackUrl, params Scopes[] scopes)
        {
            this.ClientId = clientId;
            this.CallbackUrl = callbackUrl;

            if (scopes.Length == 0) 
                scopes = new Scopes[] { Autodesk.Authentication.Model.Scopes.UserProfileRead };

            this.Scopes = scopes;

            SDKManager sdkManager = SdkManagerBuilder
              .Create()
              .Build();

            authenticationClient = new AuthenticationClient(sdkManager);
            ThreeLeggedToken = ThreeLeggedToken.Load();
        }

        #endregion

        #region Methods

        public string Authorize()
        {
            var codeChallenge = CreateCodeChallenge();
            var codeChallengeMethod = "S256";

            return authenticationClient.Authorize(ClientId, ResponseType.Code, CallbackUrl, Scopes.ToList(),
#if !DEBUG
                prompt:"login",
#endif
                codeChallenge: codeChallenge,
                codeChallengeMethod: codeChallengeMethod);
        }

        private string CreateCodeChallenge(string codeVerifier = null)
        {
            if (codeVerifier is null)
            {
                codeVerifier = Guid.NewGuid().ToString() + Guid.NewGuid().ToString() + Guid.NewGuid().ToString();
            }
            this.codeVerifier = codeVerifier;

            var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(codeVerifier);
            var hash = sha256.ComputeHash(bytes);
            return Base64UrlEncode(hash);
        }

        private string Base64UrlEncode(byte[] hash)
        {
            var base64 = Convert.ToBase64String(hash);
            var base64Url = base64.Replace('+', '-').Replace('/', '_').TrimEnd('=');
            return base64Url;
        }

        public async Task GetPKCEThreeLeggedTokenAsync(string code)
        {
            ThreeLeggedToken = await authenticationClient.GetThreeLeggedTokenAsync(
                ClientId,
                code,
                CallbackUrl,
                clientSecret: null,
                codeVerifier: codeVerifier);

            ThreeLeggedToken.Save();
        }

        public async Task RefreshToken()
        {
            if (ThreeLeggedToken is null) return;

            try
            {
                ThreeLeggedToken = await authenticationClient.RefreshTokenAsync(
                clientId: ClientId,
                clientSecret: null,
                refreshToken: ThreeLeggedToken.RefreshToken);

                ThreeLeggedToken.Save();
            }
            catch
            {
                ThreeLeggedToken = null;
                throw;
            }
        }

        public async Task EnsureTokenIsValid()
        {
            if (ThreeLeggedToken is null)
            {
                throw new Exception("Not logged in.");
            }

            var token = await authenticationClient.IntrospectTokenAsync(ThreeLeggedToken.AccessToken, ClientId, clientSecret: null);
            //Debug.WriteLine($"Token: {token.Active} {token.Exp}");
            if (token.Active == false)
            {
                await RefreshToken();
            }
        }

        public async Task<UserInfo> GetUserInfoAsync()
        {
            await EnsureTokenIsValid();
            return await authenticationClient.GetUserInfoAsync(ThreeLeggedToken.AccessToken);
        }

        public bool IsLoggedIn()
        {
            return ThreeLeggedToken is not null;
        }

        public async Task Logout()
        {
            if (ThreeLeggedToken is null) return;

            var token = ThreeLeggedToken.AccessToken;
            var refreshToken = ThreeLeggedToken.RefreshToken;
            ThreeLeggedToken = null;
            ThreeLeggedToken.Save();

            await authenticationClient.RevokeAsync(token, ClientId, clientSecret: null, TokenTypeHint.AccessToken);
            await authenticationClient.RevokeAsync(token, ClientId, clientSecret: null, TokenTypeHint.RefreshToken);
        }

        #endregion

    }
}