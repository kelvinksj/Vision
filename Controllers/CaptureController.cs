using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using Microsoft.Extensions.FileProviders;

namespace CameraWebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CaptureController : ControllerBase
    {
        private readonly string PicturesRoot = @"C:\Users\tehyy\CameraWebApp\Picture";
        private readonly string VideosRoot = @"C:\Users\tehyy\CameraWebApp\Video";

        public class ImageData
        {
            public string ImageBase64 { get; set; } = string.Empty;
            public string? FileName { get; set; }
            public string? CameraName { get; set; } // 🆕 added
        }

        [HttpPost("save")]
        public IActionResult SaveImage([FromBody] ImageData data)
        {
            if (data == null || string.IsNullOrWhiteSpace(data.ImageBase64))
                return BadRequest("Invalid image data");

            // ✅ sanitize camera name for folder use
            string cameraName = SanitizeName(data.CameraName ?? "UnknownCamera");

            // Extract pure Base64 data (strip prefix)
            var base64Data = Regex.Replace(data.ImageBase64, @"^data:image\/[a-zA-Z]+;base64,", string.Empty);

            byte[] bytes;
            try
            {
                bytes = Convert.FromBase64String(base64Data);
            }
            catch
            {
                return BadRequest("Invalid base64 image data");
            }

            // ensure camera subfolder exists
            string targetFolder = Path.Combine(PicturesRoot, cameraName);
            Directory.CreateDirectory(targetFolder);

            string fileName = !string.IsNullOrWhiteSpace(data.FileName)
                ? data.FileName
                : $"photo_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.png";

            string filePath = Path.Combine(targetFolder, fileName);
            System.IO.File.WriteAllBytes(filePath, bytes);

            Console.WriteLine($"✅ Image saved: {filePath}");
            return Ok(new { path = filePath, camera = cameraName });
        }

        [HttpPost("savevideo")]
        public async Task<IActionResult> SaveVideo([FromForm] IFormFile? videoFile, [FromForm] string? cameraName)
        {
            if (videoFile == null || videoFile.Length == 0)
                return BadRequest("Invalid video file");

            string safeCamera = SanitizeName(cameraName ?? "UnknownCamera");
            string targetFolder = Path.Combine(VideosRoot, safeCamera);
            Directory.CreateDirectory(targetFolder);

            var fileName = Path.GetFileName(videoFile.FileName);
            if (string.IsNullOrWhiteSpace(fileName))
                fileName = $"video_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.webm";

            var filePath = Path.Combine(targetFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await videoFile.CopyToAsync(stream);
            }

            Console.WriteLine($"✅ Video saved: {filePath}");
            return Ok(new { path = filePath, camera = safeCamera });
        }

        // Helper: replace invalid path chars
        private static string SanitizeName(string name)
        {
            var invalid = Path.GetInvalidFileNameChars();
            foreach (var c in invalid)
                name = name.Replace(c, '_');
            return name.Trim();
        }

        [HttpGet("media")]
        public IActionResult GetMediaLibrary()
        {
            var result = new List<object>();

            void AddMediaFromFolder(string baseFolder, string type)
            {
                if (!Directory.Exists(baseFolder))
                    return;

                foreach (var cameraDir in Directory.GetDirectories(baseFolder))
                {
                    var cameraName = Path.GetFileName(cameraDir);
                    var files = Directory.GetFiles(cameraDir)
                        .Select(f => new
                        {
                            FileName = Path.GetFileName(f),
                            Type = type,
                            Path = f.Replace(@"\", "/")
                        });

                    result.Add(new
                    {
                        CameraName = cameraName,
                        Type = type,
                        Files = files
                    });
                }
            }

            AddMediaFromFolder(PicturesRoot, "photo");
            AddMediaFromFolder(VideosRoot, "video");

            return Ok(result);
        }

        [HttpGet("folders")]
        public IActionResult GetAvailableFolders()
        {
            var folders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (Directory.Exists(PicturesRoot))
            {
                foreach (var dir in Directory.GetDirectories(PicturesRoot))
                    folders.Add(Path.GetFileName(dir));
            }

            if (Directory.Exists(VideosRoot))
            {
                foreach (var dir in Directory.GetDirectories(VideosRoot))
                    folders.Add(Path.GetFileName(dir));
            }

            return Ok(folders.OrderBy(x => x));
        }

        [HttpGet("media/{type}/{folder}")]
        public IActionResult GetMediaByFolder(string type, string folder)
        {
            string basePath = type == "video" ? VideosRoot : PicturesRoot;
            string targetFolder = Path.Combine(basePath, folder);

            if (!Directory.Exists(targetFolder))
                return Ok(new List<object>());

            var files = Directory.GetFiles(targetFolder)
                .Select(f => new
                {
                    FileName = Path.GetFileName(f),
                    Path = f
                });

            return Ok(files);
        }

    }
}
