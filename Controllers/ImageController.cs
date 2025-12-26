// File: Controllers/ImageController.cs
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;

[ApiController]
[Route("api/image")]
public class ImageController : ControllerBase
{
    private readonly string UploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

    // ✅ New endpoint to accept file uploads via multipart/form-data
    [HttpPost("upload")]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file uploaded" });

        try
        {
            if (!Directory.Exists(UploadFolder))
                Directory.CreateDirectory(UploadFolder);

            var fileName = $"crop_{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(UploadFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var fileUrl = $"/uploads/{fileName}";
            return Ok(new { fileUrl, size = file.Length });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // Optional: keep the Base64 endpoint if needed
    [HttpPost("uploadBase64")]
    public async Task<IActionResult> UploadBase64([FromBody] ImageUploadRequest request)
    {
        if (string.IsNullOrEmpty(request.Base64))
            return BadRequest(new { message = "No image provided" });

        try
        {
            if (!Directory.Exists(UploadFolder))
                Directory.CreateDirectory(UploadFolder);

            var bytes = Convert.FromBase64String(request.Base64.Replace("data:image/png;base64,", ""));
            var fileName = $"crop_{Guid.NewGuid():N}.png";
            var filePath = Path.Combine(UploadFolder, fileName);
            await System.IO.File.WriteAllBytesAsync(filePath, bytes);

            var fileUrl = $"/uploads/{fileName}";
            return Ok(new { fileUrl });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    public class ImageUploadRequest
    {
        public string Base64 { get; set; }
    }
}
