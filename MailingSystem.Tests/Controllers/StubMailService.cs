using MailingSystem.Models;
using MailingSystem.ViewModel;
using Services.Interface;

namespace MailingSystem.Tests.Controllers;

internal sealed class StubMailService : IMailService
{
    public PaginatedResult<Mail> PageResult { get; init; } = new()
    {
        CurrentPage = 1,
        PageSize = 15
    };

    public Mail? MailResult { get; init; }
    public int? RequestedPage { get; private set; }
    public int? RequestedPageSize { get; private set; }
    public Guid? RequestedId { get; private set; }
    public string? RequestedFolder { get; private set; }

    public Task<PaginatedResult<Mail>> GetInboxPaginatedAsync(int page, int pageSize)
        => GetPage("Inbox", page, pageSize);

    public Task<PaginatedResult<Mail>> GetSentPaginatedAsync(int page, int pageSize)
        => GetPage("Sent", page, pageSize);

    public Task<PaginatedResult<Mail>> GetArchivePaginatedAsync(int page, int pageSize)
        => GetPage("Archive", page, pageSize);

    public Task<PaginatedResult<Mail>> GetDraftsPaginatedAsync(int page, int pageSize)
        => GetPage("Drafts", page, pageSize);

    private Task<PaginatedResult<Mail>> GetPage(string folder, int page, int pageSize)
    {
        RequestedFolder = folder;
        RequestedPage = page;
        RequestedPageSize = pageSize;
        return Task.FromResult(PageResult);
    }

    public Task<Mail?> GetByIdAsync(Guid id)
    {
        RequestedId = id;
        return Task.FromResult(MailResult);
    }

    public Task<IReadOnlyList<Mail>> GetAllAsync() => throw new NotSupportedException();

    public Task CreateAsync(CreateMailViewModel model) => throw new NotSupportedException();
}
