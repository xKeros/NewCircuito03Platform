using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class ImagesController : ControllerBase
{
    private readonly ImageProcessingService _imageProcessingService;

    public ImagesController(ImageProcessingService imageProcessingService)
    {
        _imageProcessingService = imageProcessingService;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadImage([FromForm] IFormFile image)
    {
        try
        {
            if (image == null || image.Length == 0)
                return BadRequest("Please upload a valid image file.");

            var (optimizedImagePath, originalSize, optimizedSize) = await _imageProcessingService.ProcessImageAsync(image);

            return Ok(new
            {
                Message = "Image processed successfully",
                OriginalSizeKB = originalSize / 1024,
                OptimizedSizeKB = optimizedSize / 1024,
                OptimizedImagePath = optimizedImagePath
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}
