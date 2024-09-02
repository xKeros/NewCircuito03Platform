using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TodoApi.Models;
using TodoApi.Services;

namespace TodoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestUsersController : ControllerBase
    {
        private readonly MongoDbService _mongoDbService;
        private readonly IConfiguration _configuration;

        public TestUsersController(MongoDbService mongoDbService, IConfiguration configuration)
        {
            _mongoDbService = mongoDbService;
            _configuration = configuration;
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
                return NotFound("User not found");
            }

            return Ok(user);
        }

        // POST: api/TestUsers
        [HttpPost]
        public async Task<ActionResult<TestUser>> PostTestUser([FromBody] TestUser testUser)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            testUser.SetPassword(testUser.Password);
            await _mongoDbService.CreateTestUserAsync(testUser);

            return CreatedAtAction(nameof(GetTestUser), new { id = testUser._id.ToString() }, testUser);
        }

        // PUT: api/TestUsers/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTestUser(string id, [FromBody] TestUser testUser)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingUser = await _mongoDbService.GetTestUserAsync(id);

            if (existingUser == null)
            {
                return NotFound("User not found");
            }

            testUser._id = ObjectId.Parse(id); // Ensure the ID is correct
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
                return NotFound("User not found");
            }

            await _mongoDbService.DeleteTestUserAsync(id);

            return NoContent();
        }

        // POST: api/TestUsers/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel login)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _mongoDbService.GetTestUserByUsernameAsync(login.Username);

            if (user == null || !_mongoDbService.VerifyPassword(user, login.Password))
            {
                return Unauthorized("Invalid username or password.");
            }

            // Generate JWT token and Refresh Token
            var token = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            // Save the refresh token in the user's record in the database
            user.RefreshToken = refreshToken;
            await _mongoDbService.UpdateTestUserAsync(user._id.ToString(), user);

            // Set the tokens as HttpOnly cookies with expiration times
            var tokenCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Asegúrate de que esto esté en true en producción
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddHours(1) // Duración de 1 hora para el token
            };
            Response.Cookies.Append("token", token, tokenCookieOptions);

            var refreshTokenCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Asegúrate de que esto esté en true en producción
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(7) // Duración de 7 días para el refresh token
            };
            Response.Cookies.Append("refreshToken", refreshToken, refreshTokenCookieOptions);

            // Set the userId as an HttpOnly cookie with SameSiteMode.None
            var userIdCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Asegúrate de que esto esté en true en producción
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(7) // Ajustar la duración según sea necesario
            };
            Response.Cookies.Append("userId", user._id.ToString(), userIdCookieOptions);

            // Return the user ID in the response body
            return Ok(new { UserId = user._id.ToString() });
        }





        // POST: api/TestUsers/refresh
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenModel refreshModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _mongoDbService.GetUserByRefreshTokenAsync(refreshModel.RefreshToken);

            if (user == null || user.RefreshToken != refreshModel.RefreshToken)
            {
                return Unauthorized("Invalid refresh token.");
            }

            // Generate new JWT token and refresh token
            var newJwtToken = GenerateJwtToken(user);
            var newRefreshToken = GenerateRefreshToken();

            // Update the refresh token in the user's record in the database
            user.RefreshToken = newRefreshToken;
            await _mongoDbService.UpdateTestUserAsync(user._id.ToString(), user);

            // Return the new tokens
            return Ok(new { Token = newJwtToken, RefreshToken = newRefreshToken });
        }

        private string GenerateJwtToken(TestUser user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings").Get<JwtSettings>();

            if (jwtSettings == null || string.IsNullOrEmpty(jwtSettings.Secret))
            {
                throw new InvalidOperationException("JWT settings are not configured properly.");
            }

            // Create claims for the token
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Name, user.Fullname),
                // Add other claims if needed
            };

            // Generate signing key and token
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings.Issuer,
                audience: jwtSettings.Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(30), // JWT is valid for 30 minutes
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
    }

    public class LoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class RefreshTokenModel
    {
        public string RefreshToken { get; set; }
    }

    public class JwtSettings
    {
        public string Secret { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
    }
}
