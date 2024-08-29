using MongoDB.Bson;
using MongoDB.Driver;
using TodoApi.Models;

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

        public async Task<List<TestUser>> GetTestUsersAsync() =>
            await _testUsers.Find(user => true).ToListAsync();

        public async Task<TestUser> GetTestUserAsync(string id) =>
            await _testUsers.Find(user => user._id == ObjectId.Parse(id)).FirstOrDefaultAsync();

        public async Task CreateTestUserAsync(TestUser user) =>
            await _testUsers.InsertOneAsync(user);

        public async Task UpdateTestUserAsync(string id, TestUser user) =>
            await _testUsers.ReplaceOneAsync(u => u._id == ObjectId.Parse(id), user);

        public async Task DeleteTestUserAsync(string id) =>
            await _testUsers.DeleteOneAsync(u => u._id == ObjectId.Parse(id));

        public async Task CreatePostAsync(Post post)
        {
            // Insertar el post en la colección de Posts
            await _posts.InsertOneAsync(post);

            // Actualizar la lista de posts del usuario
            var filter = Builders<TestUser>.Filter.Eq(u => u._id, ObjectId.Parse(post.AuthorId));
            var update = Builders<TestUser>.Update.Push(u => u.Posts, post.Id);
            await _testUsers.UpdateOneAsync(filter, update);
        }

        public async Task<List<Post>> GetPostsAsync() =>
            await _posts.Find(post => true).ToListAsync();

        public async Task<Post> GetPostAsync(string id) =>
            await _posts.Find(post => post.Id == id).FirstOrDefaultAsync();
    }
}
