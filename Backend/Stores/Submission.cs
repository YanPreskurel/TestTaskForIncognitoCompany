using System;
using LiteDB;

public class Submission
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public BsonDocument? Data { get; set; }
}
