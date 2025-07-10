﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BookSoulsApp.Domain.Entities;
public class Conversation
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public List<string> UserIds { get; set; } = []; // Exactly 2 users

    public LastMessage? LastMessage { get; set; }

    //[BsonRepresentation(BsonType.ObjectId)]
    //public Dictionary<string, bool> DeletedFor { get; set; } = []; // userId => true if deleted

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}

public class LastMessage
{
    public string Text { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string SenderId { get; set; }

    public DateTime SentAt { get; set; }

    public List<string> IsReadBy { get; set; } = [];
}
