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
    public async Task<IActionResult> UploadImage([FromForm] List<IFormFile> images)
    {
        try
        {
            if (images == null || images.Count == 0)
                return BadRequest("Please upload at least one valid image file.");

            var savedImages = new List<string>();

            foreach (var image in images)
            {
                // Generar un nombre de archivo único para evitar sobrescritura
                var uniqueFileName = Path.GetFileNameWithoutExtension(image.FileName) + "_" + Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                var (optimizedImagePath, originalSize, optimizedSize) = await _imageProcessingService.ProcessImageAsync(image, uniqueFileName);

                savedImages.Add(optimizedImagePath); // Guardar la ruta de la imagen optimizada
            }

            return Ok(new { Message = "Images processed successfully", SavedImages = savedImages });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }


}
