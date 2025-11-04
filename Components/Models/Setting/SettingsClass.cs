using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TaskManagerWeb.Components.Models.Setting;
public class SettingsInfo
{
    [JsonPropertyName("myNetData")]
    [Required]
    public NetData MyNetData { get; set; } = new();

    [JsonPropertyName("taskManagerNetData")]
    [Required]
    public NetData TaskManagerNetData { get; set; } = new();

    [JsonPropertyName("containerNetData")]
    [Required]
    public NetData ContainerNetData { get; set; } = new();

    [JsonPropertyName("container2NetData")]
    [Required]
    public NetData Container2NetData { get; set; } = new();

    [JsonPropertyName("scanNetData")]
    [Required]
    public NetData ScanNetData { get; set; } = new();

    [JsonPropertyName("rCSNetData")]
    [Required]
    public NetData RCSNetData { get; set; } = new();

    [JsonPropertyName("rCSData")]
    [Required]
    public RCSData RCSData { get; set; } = new();

    [JsonIgnore]
    [NotMapped]
    public bool IsBusy { get; set; } = false;

    [JsonIgnore]
    [NotMapped]
    public bool IsDefault { get; set; } = false;

    [JsonIgnore]
    [NotMapped]
    public bool IsChecking { get; set; } = false;
}

public class NetData
{
    [JsonPropertyName("isHttps")]
    [Required]
    public bool IsHttps { get; set; } = false;

    [JsonPropertyName("iP")]
    [Required]
    [RegularExpression(@"^(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\."
                     + @"(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\."
                     + @"(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\."
                     + @"(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$",
                     ErrorMessage = "Invalid IP address")]
    public string IP { get; set; } = string.Empty;

    [JsonPropertyName("port")]
    [Required]
    [RegularExpression(@"^([0-9][0-9][0-9][0-9]?)$",
                     ErrorMessage = "Invalid port number")]
    public string Port { get; set; } = string.Empty;
}

public class RCSData
{
    [JsonPropertyName("mapId")]
    [Range(1, int.MaxValue, ErrorMessage = "MapId must be greater than 0.")]
    public int MapId { get; set; } = 1;
}