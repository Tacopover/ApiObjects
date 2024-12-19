using System;
using System.Threading.Tasks;
using Autodesk.SDKManager;
using Autodesk.Authentication.Model;
using System.Collections.Generic;
using Autodesk.Authentication;

public class Tokens
{
    public string InternalToken { get; set; }
    public string PublicToken { get; set; }
    public string RefreshToken { get; set; }
    public DateTime ExpiresAt { get; set; }
}

public partial class APS
{
    private readonly SDKManager _sdkManager;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _callbackUri;
    private readonly List<Scopes> InternalTokenScopes = new List<Scopes> { Scopes.DataRead, Scopes.ViewablesRead };
    private readonly List<Scopes> PublicTokenScopes = new List<Scopes> { Scopes.ViewablesRead };


    public APS(string clientId, string clientSecret, string callbackUri)
    {
        _sdkManager = SdkManagerBuilder.Create().Build();
        _clientId = clientId;
        _clientSecret = clientSecret;
        _callbackUri = callbackUri;
    }

    public string GetAuthorizationURL()
    {
        var authenticationClient = new AuthenticationClient(_sdkManager);
        return authenticationClient.Authorize(_clientId, ResponseType.Code, _callbackUri, InternalTokenScopes);
    }

    public async Task<Tokens> GenerateTokens(string code)
    {
        var authenticationClient = new AuthenticationClient(_sdkManager);
        var internalAuth = await authenticationClient.GetThreeLeggedTokenAsync(_clientId, _clientSecret, code, _callbackUri);
        var publicAuth = await authenticationClient.GetRefreshTokenAsync(_clientId, _clientSecret, internalAuth.RefreshToken, PublicTokenScopes);

        return new Tokens
        {
            PublicToken = publicAuth.AccessToken,
            InternalToken = internalAuth.AccessToken,
            RefreshToken = publicAuth._RefreshToken,
            ExpiresAt = DateTime.Now.ToUniversalTime().AddSeconds((double)internalAuth.ExpiresIn)
        };
    }

    public async Task<Tokens> RefreshTokens(Tokens tokens)
    {
        var authenticationClient = new AuthenticationClient(_sdkManager);
        var internalAuth = await authenticationClient.GetRefreshTokenAsync(_clientId, _clientSecret, tokens.RefreshToken, InternalTokenScopes);
        var publicAuth = await authenticationClient.GetRefreshTokenAsync(_clientId, _clientSecret, internalAuth._RefreshToken, PublicTokenScopes);
        return new Tokens
        {
            PublicToken = publicAuth.AccessToken,
            InternalToken = internalAuth.AccessToken,
            RefreshToken = publicAuth._RefreshToken,
            ExpiresAt = DateTime.Now.ToUniversalTime().AddSeconds((double)internalAuth.ExpiresIn)
        };
    }

    public async Task<UserInfo> GetUserProfile(Tokens tokens)
    {
        var authenticationClient = new AuthenticationClient(_sdkManager);
        UserInfo userInfo = await authenticationClient.GetUserInfoAsync(tokens.InternalToken);
        return userInfo;
    }
}