using System.ComponentModel.DataAnnotations;

namespace MailingSystem.ViewModel;

public class CreateMailViewModel

{

    [Required]

    [StringLength(200)]

    public string Subject { get; set; } = string.Empty;

    [Required]

    public string Recipient { get; set; } = string.Empty;

    [Required]

    public string Content { get; set; } = string.Empty;

}