using Microsoft.AspNetCore.Mvc;
using LiteDB;
using System.Text.Json;
using BsonValue = LiteDB.BsonValue;

[ApiController]
[Route("api/[controller]")]
public class SubmissionsController : ControllerBase
{
    private readonly string _dbPath = "submissions.db";

    [HttpPost]
    public IActionResult Post([FromBody] JsonElement body)
    {
        using var db = new LiteDatabase(_dbPath);
        var col = db.GetCollection<Submission>("submissions");

        // Конвертация JSON → BSON
        var bson = ConvertJsonElementToBson(body);

        var submission = new Submission
        {
            Data = (BsonDocument)bson,
            CreatedAt = DateTime.UtcNow
        };

        col.Insert(submission);

        return CreatedAtAction(nameof(GetById), new { id = submission.Id }, submission.Id);
    }

    [HttpGet]
    public IActionResult Get()
    {
        using var db = new LiteDatabase(_dbPath);
        var col = db.GetCollection<Submission>("submissions");

        var all = col.FindAll().ToList();
        return Ok(all);
    }

    [HttpGet("{id}")]
    public IActionResult GetById(Guid id)
    {
        using var db = new LiteDatabase(_dbPath);
        var col = db.GetCollection<Submission>("submissions");

        var found = col.FindById(id);
        if (found == null) return NotFound();

        return Ok(found);
    }

    // --------------------------
    // JSON → BSON конвертация
    // --------------------------
    private BsonValue ConvertJsonElementToBson(JsonElement json)
    {
        switch (json.ValueKind)
        {
            case JsonValueKind.Object:
                var doc = new BsonDocument();
                foreach (var prop in json.EnumerateObject())
                {
                    doc[prop.Name] = ConvertJsonElementToBson(prop.Value);
                }
                return doc;

            case JsonValueKind.Array:
                var array = new BsonArray();
                foreach (var item in json.EnumerateArray())
                {
                    array.Add(ConvertJsonElementToBson(item));
                }
                return array;

            case JsonValueKind.String:
                return json.GetString();

            case JsonValueKind.Number:
                if (json.TryGetInt64(out long l)) return l;
                if (json.TryGetDouble(out double d)) return d;
                break;

            case JsonValueKind.True:
                return true;

            case JsonValueKind.False:
                return false;

            case JsonValueKind.Null:
                return BsonValue.Null;
        }

        return BsonValue.Null;
    }
}
