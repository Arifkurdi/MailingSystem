using Microsoft.AspNetCore.Mvc;

[Route("api/recipients")]
public class RecipientsController : Controller
{
    private static readonly List<RecipientDto> Recipients =
    [
        new("1", "Ahmed Ali", "ahmed@example.com"),
        new("2", "Sara Hassan", "sara@example.com"),
        new("3", "Mohammed Ibrahim", "mohammed@example.com"),
        new("4", "Mona Ahmed", "mona@example.com")
    ];

    [HttpGet("search")]
    public IActionResult Search(string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Ok(Array.Empty<RecipientDto>());

        var results = Recipients
            .Where(r =>
                r.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                r.Email.Contains(query, StringComparison.OrdinalIgnoreCase))
            .Take(10)
            .ToList();

        return Ok(results);
    }
}