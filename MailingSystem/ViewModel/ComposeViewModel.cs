namespace ViewModel;

public class ComposeViewModel
{
    public Guid ReplyToId { get; init; }
    public string ReplyToTitle { get; init; } = string.Empty;
    public string ReplyToAddress { get; init; } = string.Empty;
}
