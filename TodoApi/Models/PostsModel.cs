using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TodoApi.Models
{
    public class Post
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("title")]
        public required string Title { get; set; }

        [BsonElement("content")]
        public required string Content { get; set; }

        [BsonElement("authorId")]
        public required string AuthorId { get; set; }

        [BsonElement("timestamp")]
        public long? Timestamp { get; set; }

        [BsonElement("images")]
        public List<string> Images { get; set; } = new List<string>();
    }
}
