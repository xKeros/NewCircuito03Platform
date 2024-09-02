using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace TodoApi.Services
{
    public class ImageProcessingService
    {
        private readonly string _imagesPath;

        public ImageProcessingService(IWebHostEnvironment environment)
        {
            _imagesPath = Path.Combine(environment.ContentRootPath, "images");
        }

        public async Task<string> SaveImageAsync(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                throw new ArgumentException("Invalid file");
            }

            // Genera un nombre único para el archivo
            string fileName = $"{Path.GetFileNameWithoutExtension(imageFile.FileName)}_{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";

            string filePath = Path.Combine(_imagesPath, fileName);

            // Asegúrate de que el directorio existe
            Directory.CreateDirectory(_imagesPath);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            // Devuelve la ruta relativa de la imagen
            return $"/images/{fileName}";
        }
    }
}