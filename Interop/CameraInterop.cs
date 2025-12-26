using System.IO;
using Microsoft.JSInterop;

namespace CameraWebApp.Interop
{
    public static class CameraInterop
    {
        [JSInvokable("SaveFile")]
        public static void SaveFile(byte[] data, string folderPath, string fileName)
        {
            try
            {
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                var fullPath = Path.Combine(folderPath, fileName);
                File.WriteAllBytes(fullPath, data);

                Console.WriteLine($"✅ File saved: {fullPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error saving file: {ex.Message}");
            }
        }
    }
}
