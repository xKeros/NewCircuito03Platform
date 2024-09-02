using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using TodoApi.Services;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddNewtonsoftJson(); // Agregar Newtonsoft.Json para la serialización y deserialización de JSON

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configurar CORS para permitir cualquier origen
// Configurar CORS para permitir el origen específico de tu frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder =>
        {
            builder.WithOrigins("http://localhost:5173") // Especifica la URL del frontend
                   .AllowAnyHeader()
                   .AllowAnyMethod()
                   .AllowCredentials(); // Permitir el envío de credenciales (cookies, tokens, etc.)
        });
});


// Configurar la conexión a MongoDB y registrar el servicio en el contenedor de dependencias
builder.Services.AddSingleton<IMongoClient, MongoClient>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("MongoDb");
    return new MongoClient(connectionString);
});

builder.Services.AddSingleton<MongoDbService>();

// Registrar el servicio `ImageProcessingService` con la ruta de imágenes
builder.Services.AddSingleton<ImageProcessingService>(sp =>
{
    var env = sp.GetRequiredService<IWebHostEnvironment>();
    // Usar ContentRootPath en lugar de WebRootPath
    var imagesPath = Path.Combine(env.ContentRootPath, "images");
    return new ImageProcessingService(imagesPath);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Aplica la política de CORS específica
app.UseCors("AllowSpecificOrigin");

app.UseAuthorization();

app.MapControllers();

app.Run();

