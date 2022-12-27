public class FiatUconnect
{
  public static readonly FiatCommand DEEPREFRESH = new() { Action = "ev", Message = "DEEPREFRESH" };
  public static readonly FiatCommand VF = new() { Action = "location", Message = "VF" };
  public static readonly FiatCommand HBLF = new() { Message = "HBLF" };
  public static readonly FiatCommand CNOW = new() { Action = "ev/chargenow", Message = "CNOW" };
  public static readonly FiatCommand ROPRECOND = new() { Message = "ROPRECOND" };
  public static readonly FiatCommand ROPRECOND_OFF = new() { Message = "ROPRECOND" };
  public static readonly FiatCommand ROTRUNKUNLOCK = new() { Message = "ROTRUNKUNLOCK" };
  public static readonly FiatCommand ROTRUNKLOCK = new() { Message = "ROTRUNKLOCK" };
  public static readonly FiatCommand RDU = new() { Message = "RDU" };
  public static readonly FiatCommand RDL = new() { Message = "RDL" };
  public required string Message { get; init; }
  public string Action { get; init; } = "remote";
}
