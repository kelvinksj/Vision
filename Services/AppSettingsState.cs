public class AppSettingsState
{
    // Camera
    public string? CameraId { get; set; }
    public string? CameraLabel { get; set; }

    // Media Library
    public string? DefaultFolder { get; set; }
    public string DefaultMediaType { get; set; } = "photo";

    public bool HasCamera => !string.IsNullOrEmpty(CameraId);
    public bool HasLibraryDefaults => !string.IsNullOrEmpty(DefaultFolder);
}
