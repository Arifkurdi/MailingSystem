using MailingSystem.Models;

namespace ViewModel;

public class HomeViewModel
{
    public string FolderName { get; set; } = string.Empty;
    public string FolderAction => FolderName == "Inbox" ? "Index" : FolderName;
    public PaginatedResult<Mail> Mails { get; set; } = new();
}
