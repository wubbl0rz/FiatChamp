using Amazon;
using Amazon.CognitoIdentity;
using Amazon.Runtime;
using AwsSignatureVersion4;
using FiatChamp;
using Flurl;
using Flurl.Http;
using Flurl.Http.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class FiatClient
{
  private readonly string _loginApiKey = "3_mOx_J2dRgjXYCdyhchv3b5lhi54eBcdCTX4BI8MORqmZCoQWhA0mV2PTlptLGUQI";
  private readonly string _apiKey = "2wGyL6PHec9o1UeLPYpoYa1SkEWqeBur9bLsi24i";
  private readonly string _loginUrl = "https://loginmyuconnect.fiat.com";
  private readonly string _tokenUrl = "https://authz.sdpr-01.fcagcv.com/v2/cognito/identity/token";
  private readonly string _apiUrl = "https://channels.sdpr-01.fcagcv.com";

  private readonly string _user;
  private readonly string _password;
  private readonly CookieJar _cookieJar = new();

  private ImmutableCredentials? _credentials;
  private string? _authUID;
  private readonly IFlurlClient _defaultHttpClient;

  public FiatClient(string user, string password)
  {
    _user = user;
    _password = password;

    _defaultHttpClient = new FlurlClient().Configure(settings =>
    {
      settings.HttpClientFactory = new PollyHttpClientFactory();
    });
  }

  public async Task Login()
  {
    var loginResponse = await _loginUrl
      .WithClient(_defaultHttpClient)
      .AppendPathSegment("accounts.webSdkBootstrap")
      .SetQueryParam("apiKey", _loginApiKey)
      .WithCookies(_cookieJar)
      .GetJsonAsync<FiatLoginResponse>();

    loginResponse.Dump();

    loginResponse.ThrowOnError("Login failed.");

    var authResponse = await _loginUrl
      .WithClient(_defaultHttpClient)
      .AppendPathSegment("accounts.login")
      .WithCookies(_cookieJar)
      .PostUrlEncodedAsync(
        WithFiatDefaultParameter(new()
        {
          { "loginID", _user },
          { "password", _password },
          { "sessionExpiration", TimeSpan.FromMinutes(1).TotalSeconds },
          { "include", "profile,data,emails,subscriptions,preferences" },
        }))
      .ReceiveJson<FiatAuthResponse>();

    authResponse.Dump();

    authResponse.ThrowOnError("Authentication failed.");

    _authUID = authResponse.UID;

    var jwtResponse = await _loginUrl
      .WithClient(_defaultHttpClient)
      .AppendPathSegment("accounts.getJWT")
      .SetQueryParams(
        WithFiatDefaultParameter(new()
        {
          { "fields", "profile.firstName,profile.lastName,profile.email,country,locale,data.disclaimerCodeGSDP" },
          { "login_token", authResponse.SessionInfo.LoginToken }
        }))
      .WithCookies(_cookieJar)
      .GetJsonAsync<FiatJwtResponse>();

    jwtResponse.Dump();

    jwtResponse.ThrowOnError("Authentication failed.");

    var identityResponse = await _tokenUrl
      .WithClient(_defaultHttpClient)
      .WithHeader("content-type", "application/json")
      .WithHeaders(WithAwsDefaultParameter())
      .PostJsonAsync(new
      {
        gigya_token = jwtResponse.IdToken,
      })
      .ReceiveJson<FcaIdentityResponse>();

    identityResponse.ThrowOnError("Identity failed.");

    identityResponse.Dump();

    var client = new AmazonCognitoIdentityClient(new AnonymousAWSCredentials(), RegionEndpoint.EUWest1);

    var res = await client.GetCredentialsForIdentityAsync(identityResponse.IdentityId,
      new Dictionary<string, string>()
      {
        { "cognito-identity.amazonaws.com", identityResponse.Token }
      });

    _credentials = new ImmutableCredentials(res.Credentials.AccessKeyId,
      res.Credentials.SecretKey,
      res.Credentials.SessionToken);
  }

  private Dictionary<string, object> WithAwsDefaultParameter(Dictionary<string, object>? parameters = null)
  {
    var dict = new Dictionary<string, object>()
    {
      { "x-clientapp-version", "1.0" },
      { "clientrequestid", Guid.NewGuid().ToString("N")[..16] },
      { "x-api-key", _apiKey },
      { "x-originator-type", "web" },
    };

    foreach (var parameter in parameters ?? new())
      dict.Add(parameter.Key, parameter.Value);

    return dict;
  }

  private Dictionary<string, object> WithFiatDefaultParameter(Dictionary<string, object>? parameters = null)
  {
    var dict = new Dictionary<string, object>()
    {
      { "targetEnv", "jssdk" },
      { "loginMode", "standard" },
      { "sdk", "js_latest" },
      { "authMode", "cookie" },
      { "sdkBuild", "12234" },
      { "format", "json" },
      { "APIKey", _loginApiKey },
    };

    foreach (var parameter in parameters ?? new())
      dict.Add(parameter.Key, parameter.Value);

    return dict;
  }

  public async Task Set(string vin, string action)
  {
  }

  public async Task<Vehicle[]> Fetch()
  {
    ArgumentNullException.ThrowIfNull(_credentials);
    ArgumentNullException.ThrowIfNull(_authUID);

    var vehicleResponse = await _apiUrl
      .WithClient(_defaultHttpClient)
      .AppendPathSegments("v4", "accounts", _authUID, "vehicles")
      .SetQueryParam("stage", "ALL")
      .WithHeaders(WithAwsDefaultParameter())
      .AwsSign(_credentials)
      .GetJsonAsync<VehicleResponse>();

    vehicleResponse.Dump();

    foreach (var vehicle in vehicleResponse.Vehicles)
    {
      var vehicleDetails = await _apiUrl
        .WithClient(_defaultHttpClient)
        .AppendPathSegments("v2", "accounts", _authUID, "vehicles", vehicleResponse.Vehicles.First().Vin, "status")
        .WithHeaders(WithAwsDefaultParameter())
        .AwsSign(_credentials)
        .GetJsonAsync<JObject>();

      vehicleDetails.Dump();

      vehicle.Details = vehicleDetails;

      var vehicleLocation = await _apiUrl
        .WithClient(_defaultHttpClient)
        .AppendPathSegments("v1", "accounts", _authUID, "vehicles", vehicle.Vin, "location", "lastknown")
        .WithHeaders(WithAwsDefaultParameter())
        .AwsSign(_credentials)
        .GetJsonAsync<VehicleLocation>();

      vehicle.Location = vehicleLocation;

      vehicleLocation.Dump();
    }

    return vehicleResponse.Vehicles;
  }
}