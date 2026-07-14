using Azure.Core;
using Azure.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateSlimBuilder(args);
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});
var app = builder.Build();

app.MapGet("/api/status", () => Results.Ok(new StatusResponse(Status: "ok", Timestamp: DateTime.UtcNow.ToString("o"))));

app.MapGet("/metadata/identity/oauth2/token", async (HttpRequest request) =>
{
    var resource = request.Query["resource"].Single()!;
    var credential = new AzureCliCredential();
    var tokenReply = await credential.GetTokenAsync(new TokenRequestContext([resource]));
    var jwt = new JwtSecurityTokenHandler().ReadJwtToken(tokenReply.Token);
    return Results.Ok(new TokenResponse(
        AccessToken: tokenReply.Token,
        RefreshToken: string.Empty,
        ExpiresIn: ((int)(jwt.ValidTo - jwt.ValidFrom).TotalSeconds).ToString(),
        ExpiresOn: jwt.Claims.Single(e => e.Type == "exp").Value,
        NotBefore: jwt.Claims.Single(e => e.Type == "nbf").Value,
        Resource: resource,
        TokenType: "Bearer"
    ));
});

app.Run();

record StatusResponse(
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("timestamp")] string Timestamp);

record TokenResponse(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("refresh_token")] string RefreshToken,
    [property: JsonPropertyName("expires_in")] string ExpiresIn,
    [property: JsonPropertyName("expires_on")] string ExpiresOn,
    [property: JsonPropertyName("not_before")] string NotBefore,
    [property: JsonPropertyName("resource")] string Resource,
    [property: JsonPropertyName("token_type")] string TokenType);

[JsonSerializable(typeof(StatusResponse))]
[JsonSerializable(typeof(TokenResponse))]
partial class AppJsonSerializerContext : JsonSerializerContext;
