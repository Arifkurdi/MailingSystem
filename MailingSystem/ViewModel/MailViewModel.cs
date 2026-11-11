using MailingSystem.Models;

namespace MailingSystem.ViewModel;

public class MailViewModel
{
    public required Mail Mail { get; init; }
    public int FolderPage { get; init; } = 1;
    public string FolderName { get; init; } = "Inbox";
    public string FolderAction => FolderName == "Inbox" ? "Index" : FolderName;
}
