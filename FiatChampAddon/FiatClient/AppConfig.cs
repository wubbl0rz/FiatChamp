using System.ComponentModel.DataAnnotations;

namespace FiatChamp;

public class AppConfig
{
  [Required(AllowEmptyStrings = false)] public string FiatUser { get; set; } = null!;
  [Required(AllowEmptyStrings = false)] public string FiatPw { get; set; } = null!;
  public string? FiatPin { get; set; }
  [Required(AllowEmptyStrings = false)] public string MqttServer { get; set; } = null!;
  [Range(1, 65536)] public int MqttPort { get; set; } = 1883;
  [Required(AllowEmptyStrings = false)] public string MqttUser { get; set; } = null!;
  [Required(AllowEmptyStrings = false)] public string MqttPw { get; set; } = null!;
  [Range(1, 1440)] public int RefreshInterval { get; set; } = 15;
  public bool AutoRefreshLocation { get; set; } = false;
  public bool AutoRefreshBattery { get; set; } = false;
  public bool UseCommands { get; set; } = false;
  public bool EnableDangerousCommands { get; set; } = false;
  public bool DevMode { get; set; } = false;

  public bool IsPinSet()
  {
    return !string.IsNullOrWhiteSpace(this.FiatPin);
  }
}