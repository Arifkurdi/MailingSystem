using MailingSystem.Models;
using MailingSystem.ViewModel;

namespace Services.Interface;

public interface IMailService
{
    public Task<IReadOnlyList<Mail>> GetAllAsync();

    Task<IReadOnlyDictionary<string, int>> GetUnreadCountsAsync();

    Task<PaginatedResult<Mail>> GetInboxPaginatedAsync(int page, int pageSize);

    Task<PaginatedResult<Mail>> GetSentPaginatedAsync(int page, int pageSize);
    Task<PaginatedResult<Mail>> GetArchivePaginatedAsync(int page, int pageSize);
    Task<PaginatedResult<Mail>> GetDraftsPaginatedAsync(int page, int pageSize);

    public Task CreateAsync(CreateMailViewModel model);

    public Task<Mail?> GetByIdAsync(Guid Id);
}
