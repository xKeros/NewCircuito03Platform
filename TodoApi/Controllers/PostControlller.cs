using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using TodoApi.Models;
using TodoApi.Services;

namespace TodoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly MongoDbService _mongoDbService;

        public PostsController(MongoDbService mongoDbService)
        {
            _mongoDbService = mongoDbService;
        }

        // GET: api/Posts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Post>>> GetPosts()
        {
            var posts = await _mongoDbService.GetPostsAsync();
            return Ok(posts);
        }

        // GET: api/Posts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Post>> GetPost(string id)
        {
            var post = await _mongoDbService.GetPostAsync(id);

            if (post == null)
            {
                return NotFound();
            }

            return Ok(post);
        }
        // POST: api/Post
        [HttpPost]
        [HttpPost]
        [HttpPost]
        public async Task<ActionResult<Post>> PostPost(Post post)
        {
            try
            {
                // Validar y convertir la fecha a timestamp
                if (post.Timestamp.HasValue && post.Timestamp.Value > 0)
                {
                    // Ya se recibió un timestamp en el request body, por lo tanto, se asume que es correcto.
                    // No se hace conversión adicional aquí si ya es un Unix timestamp.
                }
                else if (!string.IsNullOrWhiteSpace(post.Title)) // Asumiendo que tienes algún criterio para validar
                {
                    return BadRequest("The timestamp field is required and must be a valid timestamp.");
                }

                // Intentar crear el post en la base de datos
                await _mongoDbService.CreatePostAsync(post);

                // Devolver respuesta exitosa
                return CreatedAtAction(nameof(GetPost), new { id = post.Id }, post);
            }
            catch (FormatException ex)
            {
                // Error específico de formato
                return BadRequest($"Date format error: {ex.Message}");
            }
            catch (MongoException ex)
            {
                // Error relacionado con MongoDB
                return StatusCode(500, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Cualquier otro tipo de excepción
                return StatusCode(500, $"An unexpected error occurred: {ex.Message}");
            }
        }




    }
}
