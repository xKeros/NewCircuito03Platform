using MongoDB.Bson;
using MongoDB.Driver;
using TodoApi.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace TodoApi.Services
{
    public class MongoDbService
    {
        private readonly IMongoCollection<TestUser> _testUsers;
        private readonly IMongoCollection<Post> _posts;

        public MongoDbService(IConfiguration config)
        {
            var client = new MongoClient(config.GetConnectionString("MongoDb"));
            var database = client.GetDatabase("test-database");

            _testUsers = database.GetCollection<TestUser>("TestUsers");
            _posts = database.GetCollection<Post>("Posts");
        }

        // Método para obtener todos los usuarios
        public async Task<List<TestUser>> GetTestUsersAsync() =>
            await _testUsers.Find(user => true).ToListAsync();

        // Método para obtener un usuario por ID
        public async Task<TestUser> GetTestUserAsync(string id) =>
            await _testUsers.Find(user => user._id == ObjectId.Parse(id)).FirstOrDefaultAsync();

        // Método para crear un nuevo usuario
        public async Task CreateTestUserAsync(TestUser user) =>
            await _testUsers.InsertOneAsync(user);

        // Método para actualizar un usuario
        public async Task UpdateTestUserAsync(string id, TestUser user) =>
            await _testUsers.ReplaceOneAsync(u => u._id == ObjectId.Parse(id), user);

        // Método para eliminar un usuario
        public async Task DeleteTestUserAsync(string id) =>
            await _testUsers.DeleteOneAsync(u => u._id == ObjectId.Parse(id));

        // Método para crear un post y actualizar la lista de posts del usuario
        public async Task CreatePostAsync(Post post)
        {
            await _posts.InsertOneAsync(post);
            var filter = Builders<TestUser>.Filter.Eq(u => u._id, ObjectId.Parse(post.AuthorId));
            var update = Builders<TestUser>.Update.Push(u => u.Posts, post.Id);
            await _testUsers.UpdateOneAsync(filter, update);
        }

        // Método para obtener un usuario por username
        public async Task<TestUser> GetTestUserByUsernameAsync(string username)
        {
            return await _testUsers.Find(user => user.Username == username).FirstOrDefaultAsync();
        }

        // Método para verificar la contraseña
        public bool VerifyPassword(TestUser user, string plainTextPassword)
        {
            var hashedPassword = HashPassword(plainTextPassword);
            return user.Password == hashedPassword;
        }

        // Método para obtener todos los posts
        public async Task<List<Post>> GetPostsAsync() =>
            await _posts.Find(post => true).ToListAsync();

        // Método para obtener un post por ID
        public async Task<Post> GetPostAsync(string id) =>
            await _posts.Find(post => post.Id == id).FirstOrDefaultAsync();

        // Método privado para hashear la contraseña
        private string HashPassword(string plainTextPassword)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                var inputBytes = System.Text.Encoding.ASCII.GetBytes(plainTextPassword);
                var hashBytes = md5.ComputeHash(inputBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }
        public async Task<TestUser> GetUserByRefreshTokenAsync(string refreshToken)
        {
            return await _testUsers.Find(user => user.RefreshToken == refreshToken).FirstOrDefaultAsync();
        }

    }
}

