using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using TodoApi.Models;
using TodoApi.Services;

namespace TodoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestUsersController : ControllerBase
    {
        private readonly MongoDbService _mongoDbService;

        public TestUsersController(MongoDbService mongoDbService)
        {
            _mongoDbService = mongoDbService;
        }

        // GET: api/TestUsers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TestUser>>> GetTestUsers()
        {
            var users = await _mongoDbService.GetTestUsersAsync();
            return Ok(users);
        }

        // GET: api/TestUsers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TestUser>> GetTestUser(string id)
        {
            var user = await _mongoDbService.GetTestUserAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        // POST: api/TestUsers
        [HttpPost]
        public async Task<ActionResult<TestUser>> PostTestUser(TestUser testUser)
        {
            testUser.SetPassword(testUser.Password);
            await _mongoDbService.CreateTestUserAsync(testUser);

            return CreatedAtAction(nameof(GetTestUser), new { id = testUser._id.ToString() }, testUser);
        }

        // PUT: api/TestUsers/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTestUser(string id, TestUser testUser)
        {
            var existingUser = await _mongoDbService.GetTestUserAsync(id);

            if (existingUser == null)
            {
                return NotFound();
            }

            testUser._id = ObjectId.Parse(id); // Asegura que el _id sea correcto
            await _mongoDbService.UpdateTestUserAsync(id, testUser);

            return NoContent();
        }

        // DELETE: api/TestUsers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTestUser(string id)
        {
            var user = await _mongoDbService.GetTestUserAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            await _mongoDbService.DeleteTestUserAsync(id);

            return NoContent();
        }
    }
}
