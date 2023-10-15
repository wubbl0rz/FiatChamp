using System.Text;
using Amazon;
using Amazon.CognitoIdentity;
using Amazon.Runtime;
using Flurl;
using Flurl.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace FiatChamp;

public interface IFiatClient
{
  Task LoginAndKeepSessionAlive();
  Task SendCommand(string vin, string command, string pin, string action);
  Task<Vehicle[]> Fetch();
}

public class FiatClientFake : IFiatClient
{
  public Task LoginAndKeepSessionAlive()
  {
    return Task.CompletedTask;
  }

  public Task SendCommand(string vin, string command, string pin, string action)
  {
    return Task.CompletedTask;
  }

  public Task<Vehicle[]> Fetch()
  {
    var vehicle = JsonConvert.DeserializeObject<Vehicle>("""
    {
      "RegStatus": "COMPLETED_STAGE_3",
      "Color": "BLUE",
      "Year": 2022,
      "TsoBodyCode": "",
      "NavEnabledHu": false,
      "Language": "",
      "CustomerRegStatus": "Y",
      "Radio": "",
      "ActivationSource": "DEALER",
      "Nickname": "KEKW",
      "Vin": "LDM1SN7DHD7DHSHJ6753D",
      "Company": "FCA",
      "Model": 332,
      "ModelDescription": "Neuer 500 3+1",
      "TcuType": 2,
      "Make": "FIAT",
      "BrandCode": "12",
      "SoldRegion": "EMEA"
    }
    """);
    
    vehicle.Details = JObject.Parse("""
    {
      "vehicleInfo": {
        "totalRangeADA": null,
        "odometer": {
          "odometer": {
            "value": "1234",
            "unit": "km"
          }
        },
        "daysToService": "null",
        "fuel": {
          "fuelAmountLevel": null,
          "isFuelLevelLow": false,
          "distanceToEmpty": {
            "value": "150",
            "unit": "km"
          },
          "fuelAmount": {
            "value": "null",
            "unit": "null"
          }
        },
        "oilLevel": {
          "oilLevel": null
        },
        "tyrePressure": [
          {
            "warning": false,
            "pressure": {
              "value": "null",
              "unit": "kPa"
            },
            "type": "FL",
            "status": "NORMAL"
          },
          {
            "warning": false,
            "pressure": {
              "value": "null",
              "unit": "kPa"
            },
            "type": "FR",
            "status": "NORMAL"
          },
          {
            "warning": false,
            "pressure": {
              "value": "null",
              "unit": "kPa"
            },
            "type": "RL",
            "status": "NORMAL"
          },
          {
            "warning": false,
            "pressure": {
              "value": "null",
              "unit": "kPa"
            },
            "type": "RR",
            "status": "NORMAL"
          }
        ],
        "batteryInfo": {
          "batteryStatus": "0",
          "batteryVoltage": {
            "value": "14.55",
            "unit": "volts"
          }
        },
        "tripsInfo": {
          "trips": [
            {
              "totalElectricDistance": {
                "value": "null",
                "unit": "km"
              },
              "name": "TripA",
              "totalDistance": {
                "value": "1013",
                "unit": "km"
              },
              "energyUsed": {
                "value": "null",
                "unit": "kmpl"
              },
              "averageEnergyUsed": {
                "value": "null",
                "unit": "kmpl"
              },
              "totalHybridDistance": {
                "value": "null",
                "unit": "km"
              }
            },
            {
              "totalElectricDistance": {
                "value": "null",
                "unit": "km"
              },
              "name": "TripB",
              "totalDistance": {
                "value": "14",
                "unit": "km"
              },
              "energyUsed": {
                "value": "null",
                "unit": "kmpl"
              },
              "averageEnergyUsed": {
                "value": "null",
                "unit": "kmpl"
              },
              "totalHybridDistance": {
                "value": "null",
                "unit": "km"
              }
            }
          ]
        },
        "batPwrUsageDisp": null,
        "distanceToService": {
          "distanceToService": {
            "value": "5127.0",
            "unit": "km"
          }
        },
        "wheelCount": 4,
        "hvacPwrUsageDisp": null,
        "mtrPwrUsageDisp": null,
        "tpmsvehicle": false,
        "hVBatSOH": null,
        "isTPMSVehicle": false,
        "timestamp": 1665779022952
      },
      "evInfo": {
        "chargeSchedules": [],
        "battery": {
          "stateOfCharge": 72,
          "chargingLevel": "LEVEL_2",
          "plugInStatus": true,
          "timeToFullyChargeL2": 205,
          "chargingStatus": "CHARGING",
          "totalRange": 172,
          "distanceToEmpty": {
            "value": 172,
            "unit": "km"
          }
        },
        "timestamp": 1665822611085,
        "schedules": [
          {
            "chargeToFull": false,
            "scheduleType": "NONE",
            "enableScheduleType": false,
            "scheduledDays": {
              "sunday": false,
              "saturday": false,
              "tuesday": false,
              "wednesday": false,
              "thursday": false,
              "friday": false,
              "monday": false
            },
            "startTime": "00:00",
            "endTime": "00:00",
            "cabinPriority": false,
            "repeatSchedule": true
          },
          {
            "chargeToFull": false,
            "scheduleType": "NONE",
            "enableScheduleType": false,
            "scheduledDays": {
              "sunday": false,
              "saturday": false,
              "tuesday": false,
              "wednesday": false,
              "thursday": false,
              "friday": false,
              "monday": false
            },
            "startTime": "00:00",
            "endTime": "00:00",
            "cabinPriority": false,
            "repeatSchedule": true
          },
          {
            "chargeToFull": false,
            "scheduleType": "NONE",
            "enableScheduleType": false,
            "scheduledDays": {
              "sunday": false,
              "saturday": false,
              "tuesday": false,
              "wednesday": false,
              "thursday": false,
              "friday": false,
              "monday": false
            },
            "startTime": "00:00",
            "endTime": "00:00",
            "cabinPriority": false,
            "repeatSchedule": true
          }
        ]
      },
      "timestamp": 1665822611085
    }
    """);
    
    vehicle.Location = JsonConvert.DeserializeObject<VehicleLocation>("""
    {
      "TimeStamp": 1665779022952,
      "Longitude": 4.1234365,
      "Latitude": 69.4765989,
      "Altitude": 40.346462111,
      "Bearing": 0,
      "IsLocationApprox": true
    }
    """);

    return Task.FromResult(new[] { vehicle });
  }
}

public enum FcaBrand
{
  Fiat,
  Ram,
  Jeep,
  Dodge,
  AlfaRomeo
}

public enum FcaRegion
{
  Europe,
  America
}

public class FiatClient : IFiatClient
{
  private readonly string _loginApiKey = "3_mOx_J2dRgjXYCdyhchv3b5lhi54eBcdCTX4BI8MORqmZCoQWhA0mV2PTlptLGUQI";
  private readonly string _apiKey = "2wGyL6PHec9o1UeLPYpoYa1SkEWqeBur9bLsi24i";
  private readonly string _loginUrl = "https://loginmyuconnect.fiat.com";
  private readonly string _tokenUrl = "https://authz.sdpr-01.fcagcv.com/v2/cognito/identity/token";
  private readonly string _apiUrl = "https://channels.sdpr-01.fcagcv.com";
  private readonly string _authApiKey = "JWRYW7IYhW9v0RqDghQSx4UcRYRILNmc8zAuh5ys"; // for pin
  private readonly string _authUrl = "https://mfa.fcl-01.fcagcv.com"; // for pin
  private readonly string _locale = "de_de"; // for pin
  private readonly RegionEndpoint _awsEndpoint = RegionEndpoint.EUWest1; 
  
  private readonly string _user;
  private readonly string _password;
  private readonly FcaBrand _brand;
  private readonly FcaRegion _region;
  private readonly CookieJar _cookieJar = new();

  private readonly IFlurlClient _defaultHttpClient;

  private (string userUid, ImmutableCredentials awsCredentials)? _loginInfo = null;

  public FiatClient(string user, string password, FcaBrand brand = FcaBrand.Fiat, FcaRegion region = FcaRegion.Europe)
  {
    _user = user;
    _password = password;
    _brand = brand;
    _region = region;

    if (_brand == FcaBrand.Ram)
    {
      _loginApiKey = "3_7YjzjoSb7dYtCP5-D6FhPsCciggJFvM14hNPvXN9OsIiV1ujDqa4fNltDJYnHawO";
      _apiKey = "OgNqp2eAv84oZvMrXPIzP8mR8a6d9bVm1aaH9LqU";
      _loginUrl = "https://login-us.ramtrucks.com";
      _tokenUrl = "https://authz.sdpr-02.fcagcv.com/v2/cognito/identity/token";
      _apiUrl = "https://channels.sdpr-02.fcagcv.com";
      _authApiKey = "JWRYW7IYhW9v0RqDghQSx4UcRYRILNmc8zAuh5ys"; // UNKNOWN
      _authUrl = "https://mfa.fcl-02.fcagcv.com"; // UNKNOWN
      _awsEndpoint = RegionEndpoint.USEast1;
      _locale = "en_us";
    }
    else if(_brand == FcaBrand.Dodge)
    {
      _loginApiKey = "3_etlYkCXNEhz4_KJVYDqnK1CqxQjvJStJMawBohJU2ch3kp30b0QCJtLCzxJ93N-M";
      _apiKey = "OgNqp2eAv84oZvMrXPIzP8mR8a6d9bVm1aaH9LqU";
      _loginUrl = "https://login-us.dodge.com";
      _tokenUrl = "https://authz.sdpr-02.fcagcv.com/v2/cognito/identity/token";
      _apiUrl = "https://channels.sdpr-02.fcagcv.com";
      _authApiKey = "JWRYW7IYhW9v0RqDghQSx4UcRYRILNmc8zAuh5ys"; // UNKNOWN
      _authUrl = "https://mfa.fcl-01.fcagcv.com"; // UNKNOWN
      _awsEndpoint = RegionEndpoint.USEast1;
      _locale = "en_us";
    }
    else if (_brand == FcaBrand.Fiat && _region == FcaRegion.America)
    {
      _loginApiKey = "3_etlYkCXNEhz4_KJVYDqnK1CqxQjvJStJMawBohJU2ch3kp30b0QCJtLCzxJ93N-M";
      _apiKey = "OgNqp2eAv84oZvMrXPIzP8mR8a6d9bVm1aaH9LqU";
      _loginUrl = "https://login-us.fiat.com";
      _tokenUrl = "https://authz.sdpr-02.fcagcv.com/v2/cognito/identity/token";
      _apiUrl = "https://channels.sdpr-02.fcagcv.com";
      _authApiKey = "JWRYW7IYhW9v0RqDghQSx4UcRYRILNmc8zAuh5ys"; // UNKNOWN
      _authUrl = "https://mfa.fcl-01.fcagcv.com"; // UNKNOWN
      _awsEndpoint = RegionEndpoint.USEast1;
      _locale = "en_us";
    }
    else if (_brand == FcaBrand.Jeep)
    {
      if (_region == FcaRegion.Europe)
      {
        _loginApiKey = "3_ZvJpoiZQ4jT5ACwouBG5D1seGEntHGhlL0JYlZNtj95yERzqpH4fFyIewVMmmK7j";
        _loginUrl = "https://login.jeep.com";
      }
      else
      {
        _loginApiKey = "3_5qxvrevRPG7--nEXe6huWdVvF5kV7bmmJcyLdaTJ8A45XUYpaR398QNeHkd7EB1X";
        _apiKey = "OgNqp2eAv84oZvMrXPIzP8mR8a6d9bVm1aaH9LqU";
        _loginUrl = "https://login-us.jeep.com";
        _tokenUrl = "https://authz.sdpr-02.fcagcv.com/v2/cognito/identity/token";
        _apiUrl = "https://channels.sdpr-02.fcagcv.com";
        _authApiKey = "fNQO6NjR1N6W0E5A6sTzR3YY4JGbuPv48Nj9aZci"; 
        _authUrl = "https://mfa.fcl-02.fcagcv.com"; 
        _awsEndpoint = RegionEndpoint.USEast1;
        _locale = "en_us";
      }
    }

    _defaultHttpClient = new FlurlClient().Configure(settings =>
    {
      settings.HttpClientFactory = new PollyHttpClientFactory();
    });
  }

  public async Task LoginAndKeepSessionAlive()
  {
    if (_loginInfo is not null)
      return;
    
    await this.Login();
    
    _ = Task.Run(async () =>
    {
      var timer = new PeriodicTimer(TimeSpan.FromMinutes(2));
      
      while (await timer.WaitForNextTickAsync())
      {
        try
        {
          Log.Information("REFRESH SESSION");
          await this.Login();
        }
        catch (Exception e)
        {
          
          Log.Error("ERROR WHILE REFRESH SESSION");
          Log.Debug("{0}", e);
        }
      }
    });
  }

  private async Task Login()
  {
    var loginResponse = await _loginUrl
      .WithClient(_defaultHttpClient)
      .AppendPathSegment("accounts.webSdkBootstrap")
      .SetQueryParam("apiKey", _loginApiKey)
      .WithCookies(_cookieJar)
      .GetJsonAsync<FiatLoginResponse>();

    Log.Debug("{0}", loginResponse.Dump());

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
          { "sessionExpiration", TimeSpan.FromMinutes(5).TotalSeconds },
          { "include", "profile,data,emails,subscriptions,preferences" },
        }))
      .ReceiveJson<FiatAuthResponse>();

    Log.Debug("{0}", authResponse.Dump());

    authResponse.ThrowOnError("Authentication failed.");

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

    Log.Debug("{0}", jwtResponse.Dump());

    jwtResponse.ThrowOnError("Authentication failed.");

    var identityResponse = await _tokenUrl
      .WithClient(_defaultHttpClient)
      .WithHeader("content-type", "application/json")
      .WithHeaders(WithAwsDefaultParameter(_apiKey))
      .PostJsonAsync(new
      {
        gigya_token = jwtResponse.IdToken,
      })
      .ReceiveJson<FcaIdentityResponse>();

    Log.Debug("{0}", identityResponse.Dump());
    
    identityResponse.ThrowOnError("Identity failed.");

    var client = new AmazonCognitoIdentityClient(new AnonymousAWSCredentials(), _awsEndpoint);

    var res = await client.GetCredentialsForIdentityAsync(identityResponse.IdentityId,
      new Dictionary<string, string>()
      {
        { "cognito-identity.amazonaws.com", identityResponse.Token }
      });

    _loginInfo = (authResponse.UID, new ImmutableCredentials(res.Credentials.AccessKeyId,
      res.Credentials.SecretKey,
      res.Credentials.SessionToken));
  }

  private Dictionary<string, object> WithAwsDefaultParameter(string apiKey, Dictionary<string, object>? parameters = null)
  {
    var dict = new Dictionary<string, object>()
    {
      { "x-clientapp-name", "CWP" },
      { "x-clientapp-version", "1.0" },
      { "clientrequestid", Guid.NewGuid().ToString("N")[..16] },
      { "x-api-key", apiKey },
      { "locale", _locale },
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
  
  public async Task SendCommand(string vin, string command, string pin, string action)
  {
    ArgumentNullException.ThrowIfNull(_loginInfo);
    
    var (userUid, awsCredentials) = _loginInfo.Value;

    var data = new
    {
      pin = Convert.ToBase64String(Encoding.UTF8.GetBytes(pin))
    };

    var pinAuthResponse = await _authUrl
      .AppendPathSegments("v1", "accounts", userUid, "ignite", "pin", "authenticate")
      .WithHeaders(WithAwsDefaultParameter(_authApiKey))
      .AwsSign(awsCredentials, _awsEndpoint, data)
      .PostJsonAsync(data)
      .ReceiveJson<FcaPinAuthResponse>();

    Log.Debug("{0}", pinAuthResponse.Dump());

    var json = new
    {
      command, 
      pinAuth = pinAuthResponse.Token
    };

    var commandResponse = await _apiUrl
      .AppendPathSegments("v1", "accounts", userUid, "vehicles", vin, action)
      .WithHeaders(WithAwsDefaultParameter(_apiKey))
      .AwsSign(awsCredentials, _awsEndpoint, json)
      .PostJsonAsync(json)
      .ReceiveJson<FcaCommandResponse>();

    Log.Debug("{0}", commandResponse.Dump());
  }

  public async Task<Vehicle[]> Fetch()
  {
    ArgumentNullException.ThrowIfNull(_loginInfo);

    var (userUid, awsCredentials) = _loginInfo.Value;

    var vehicleResponse = await _apiUrl
      .WithClient(_defaultHttpClient)
      .AppendPathSegments("v4", "accounts", userUid, "vehicles")
      .SetQueryParam("stage", "ALL")
      .WithHeaders(WithAwsDefaultParameter(_apiKey))
      .AwsSign(awsCredentials, _awsEndpoint)
      .GetJsonAsync<VehicleResponse>();

    Log.Debug("{0}", vehicleResponse.Dump());

    foreach (var vehicle in vehicleResponse.Vehicles)
    {
      var vehicleDetails = await _apiUrl
        .WithClient(_defaultHttpClient)
        .AppendPathSegments("v2", "accounts", userUid, "vehicles", vehicle.Vin, "status")
        .WithHeaders(WithAwsDefaultParameter(_apiKey))
        .AwsSign(awsCredentials, _awsEndpoint)
        .GetJsonAsync<JObject>();
      
      Log.Debug("{0}", vehicleDetails.Dump());

      vehicle.Details = vehicleDetails;

      var vehicleLocation = await _apiUrl
        .WithClient(_defaultHttpClient)
        .AppendPathSegments("v1", "accounts", userUid, "vehicles", vehicle.Vin, "location", "lastknown")
        .WithHeaders(WithAwsDefaultParameter(_apiKey))
        .AwsSign(awsCredentials, _awsEndpoint)
        .GetJsonAsync<VehicleLocation>();

      vehicle.Location = vehicleLocation;

      Log.Debug("{0}", vehicleLocation.Dump());
    }

    return vehicleResponse.Vehicles;
  }
}

