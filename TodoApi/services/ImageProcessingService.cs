using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Threading.Tasks;

public class ImageProcessingService
{
    private readonly string _imagesPath;

    public ImageProcessingService(string imagesPath)
    {
        _imagesPath = imagesPath ?? throw new ArgumentNullException(nameof(imagesPath), "Images path cannot be null.");
        if (!Directory.Exists(_imagesPath))
        {
            Directory.CreateDirectory(_imagesPath);
        }
    }

    public async Task<(string OptimizedImagePath, long OriginalSize, long OptimizedSize)> ProcessImageAsync(IFormFile imageFile, string uniqueFileName)
    {
        if (imageFile == null || imageFile.Length == 0)
            throw new ArgumentException("Invalid image file");

        var originalFilePath = Path.Combine(_imagesPath, uniqueFileName);
        var optimizedFilePath = Path.Combine(_imagesPath, Path.GetFileNameWithoutExtension(uniqueFileName) + ".webp");

        // Guardar la imagen original
        using (var stream = new FileStream(originalFilePath, FileMode.Create))
        {
            await imageFile.CopyToAsync(stream);
        }

        var originalSize = new FileInfo(originalFilePath).Length;

        // Convertir la imagen a WebP y optimizarla
        using (var image = Image.Load(originalFilePath))
        {
            await image.SaveAsync(optimizedFilePath, new WebpEncoder { Quality = 85 });
        }

        var optimizedSize = new FileInfo(optimizedFilePath).Length;

        // Eliminar la imagen original si no es necesaria
        File.Delete(originalFilePath);

        return (optimizedFilePath, originalSize, optimizedSize);
    }

}
