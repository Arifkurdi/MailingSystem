using System.ComponentModel.DataAnnotations;

namespace MailingSystem.Models;

public class Mail
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Subject { get; set; } = String.Empty;

    [Required]
    public required string Body { get; set; }

    [Required]
    public required string From { get; set; }

    [Required]
    public required string To { get; set; }

    [Required]
    public bool IsRead { get; set; } = false;

    [Required]
    public DateTime SentAt { get; set; }
}