﻿using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

public class TestUser
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId _id { get; set; }

    [BsonElement("fullname")]
    public string Fullname { get; set; }

    [BsonElement("username")]
    public string Username { get; set; }

    [BsonElement("password")]
    public string Password { get; set; }

    [BsonElement("age")]
    public int Age { get; set; }

    [BsonElement("posts")]
    public List<string> Posts { get; set; } = new List<string>();

    [BsonElement("refreshToken")]
    public string RefreshToken { get; set; } // New field for Refresh Token

    [BsonElement("oAuthProvider")]
    public string OAuthProvider { get; set; }

    [BsonElement("oAuthProviderId")]
    public string OAuthProviderId { get; set; }

    public void SetPassword(string plainTextPassword)
    {
        using (var md5 = System.Security.Cryptography.MD5.Create())
        {
            var inputBytes = System.Text.Encoding.ASCII.GetBytes(plainTextPassword);
            var hashBytes = md5.ComputeHash(inputBytes);
            Password = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
    }
}
