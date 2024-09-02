using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using TodoApi.Services;

[ApiController]
[Route("api/[controller]")]
public class ImagesController : ControllerBase
{
    private readonly ImageProcessingService _imageProcessingService;

    public ImagesController(ImageProcessingService imageProcessingService)
    {
        _imageProcessingService = imageProcessingService;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadImage(IFormFile image)
    {
        if (image == null || image.Length == 0)
        {
            return BadRequest("No image file provided");
        }

        try
        {
            string imagePath = await _imageProcessingService.SaveImageAsync(image);
            return Ok(new { imagePath });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex}");
        }
    }
}
