using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public abstract class BaseResponse
{
  public abstract bool CheckForError();

  public abstract void ThrowOnError(string message);
}

public class FiatResponse
{
  public string CallId { get; set; }
  public long ErrorCode { get; set; }
  public string ErrorDetails { get; set; }
  public string ErrorMessage { get; set; }
  public long ApiVersion { get; set; }
  public long StatusCode { get; set; }
  public string StatusReason { get; set; }
  public DateTimeOffset Time { get; set; }

  public bool CheckForError()
  {
    return StatusCode != 200;
  }

  public void ThrowOnError(string message)
  {
    if (CheckForError())
    {
      throw new Exception(message + $" {this.ErrorCode} {this.StatusReason} {this.ErrorMessage}");
    }
  }
}

public class FiatLoginResponse : FiatResponse
{
}

public class FiatAuthResponse : FiatResponse
{
  public string UID { get; set; }
  public FiatSessionInfo SessionInfo { get; set; }
}

public class FiatJwtResponse : FiatResponse
{
  [JsonProperty("id_token")] public string IdToken { get; set; }
}

public class FcaIdentityResponse : BaseResponse
{
  public string IdentityId { get; set; }
  public string Token { get; set; }

  public override bool CheckForError()
  {
    return string.IsNullOrEmpty(Token) || string.IsNullOrEmpty(IdentityId);
  }

  public override void ThrowOnError(string message)
  {
    if (CheckForError())
    {
      throw new Exception(message);
    }
  }
}

public class FiatSessionInfo
{
  [JsonProperty("login_token")] public string LoginToken { get; set; }
}

public partial class AwsCognitoIdentityResponse
{
  public AwsCognitoIdentityCredentials Credentials { get; set; }
  public string IdentityId { get; set; }
}

public class AwsCognitoIdentityCredentials
{
  public string AccessKeyId { get; set; }
  public long Expiration { get; set; }
  public string SecretKey { get; set; }
  public string SessionToken { get; set; }
}

public class VehicleResponse
{
  public string Userid { get; set; }
  public long Version { get; set; }
  public Vehicle[] Vehicles { get; set; }
}

public class Vehicle
{
  public string RegStatus { get; set; }
  public string Color { get; set; }
  public long Year { get; set; }
  public string TsoBodyCode { get; set; }
  public bool NavEnabledHu { get; set; }
  public string Language { get; set; }
  public string CustomerRegStatus { get; set; }
  public string Radio { get; set; }
  public string ActivationSource { get; set; }
  public string? Nickname { get; set; }
  public string Vin { get; set; }
  public string Company { get; set; }
  public string Model { get; set; }
  public string ModelDescription { get; set; }
  public long TcuType { get; set; }
  public string Make { get; set; }
  public string BrandCode { get; set; }
  public string SoldRegion { get; set; }
  [JsonIgnore] public JObject Details { get; set; }
  [JsonIgnore] public VehicleLocation Location { get; set; }
}

public class VehicleLocation
{
  public long TimeStamp { get; set; }
  public double Longitude { get; set; }
  public double Latitude { get; set; }
  public double? Altitude { get; set; }
  public object? Bearing { get; set; }
  public bool? IsLocationApprox { get; set; }
}

public class Battery
{
  public long StateOfCharge { get; set; }
  public string ChargingLevel { get; set; }
  public bool PlugInStatus { get; set; }
  public long TimeToFullyChargeL3 { get; set; }
  public long TimeToFullyChargeL2 { get; set; }
  public string ChargingStatus { get; set; }
  public long TotalRange { get; set; }
  public DistanceToEmpty DistanceToEmpty { get; set; }
}

public class DistanceToEmpty
{
  public long Value { get; set; }
  public string Unit { get; set; }
}

public class FcaPinAuthResponse
{
  public long Expiry { get; set; }
  public string Token { get; set; }
}

public class FcaCommandResponse
{
  public string Command { get; set; }
  public Guid CorrelationId { get; set; }
  public string ResponseStatus { get; set; }
  public long StatusTimestamp { get; set; }
  public long AsyncRespTimeout { get; set; }
}