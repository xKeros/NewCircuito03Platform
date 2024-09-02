using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using TodoApi.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddNewtonsoftJson(); // Agregar Newtonsoft.Json para la serialización y deserialización de JSON
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("MongoDb");
    return new MongoClient(connectionString);
});

builder.Services.AddSingleton<MongoDbService>();

// Registrar el servicio `ImageProcessingService`
builder.Services.AddSingleton<ImageProcessingService>();

// Configurar autenticación con Cookies, Google y Microsoft
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie()
.AddGoogle(googleOptions =>
{
    googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
})
.AddMicrosoftAccount(microsoftOptions =>
{
    microsoftOptions.ClientId = builder.Configuration["Authentication:Microsoft:ClientId"];
    microsoftOptions.ClientSecret = builder.Configuration["Authentication:Microsoft:ClientSecret"];
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Configurar el uso de archivos estáticos
app.UseStaticFiles(); // Esto permite servir archivos estáticos desde la carpeta wwwroot

// Configurar una ruta específica para las imágenes
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(app.Environment.ContentRootPath, "images")),
    RequestPath = "/images"
});

// Aplica la política de CORS específica
app.UseCors("AllowSpecificOrigin");

// Añadir autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
