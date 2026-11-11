using MailingSystem.Models;
using MailingSystem.ViewModel;
using Services.Interface;

namespace Services
{
    public class MockMailService : IMailService
    {
        private List<Mail> _mails = GenerateMail(60);
        // Create Random Date for the sake of the Design :)
        public static DateTime GetRandomDateTime()
        {
            var daysAgo = Random.Shared.Next(0, 30);
            var hoursAgo = Random.Shared.Next(0, 24);
            var minutesAgo = Random.Shared.Next(0, 60);

            return DateTime.Now
                .AddDays(-daysAgo)
                .AddHours(-hoursAgo)
                .AddMinutes(-minutesAgo);
        }

        private readonly object _lock = new();


        private static List<Mail> GenerateMail(int quantity)
        {
            return Enumerable.Range(1, quantity)
            .Select(i => new Mail
            {
                Id = Guid.NewGuid(),
                Subject = $"Sample Message {i:00}",
                Body = "Lorem ipsum dolor sit amet consectetur adipisicing elit. Consequatur, deserunt. Quasi veniam porro perspiciatis eum repellendus voluptatibus atque ex numquam inventore molestias maxime cupiditate, doloremque fuga consequuntur officia accusantium odio quidem iste tenetur illum cum commodi ipsa vitae? Magni dolor eos aliquid expedita voluptatibus libero!",
                From = $"sender{i:00}@example.com",
                To = $"recipient{i:00}@example.com",
                IsRead = i % 3 == 0,
                SentAt = GetRandomDateTime()
            })
            .OrderByDescending(m => m.SentAt)
            .ToList();
        }
        public Task<IReadOnlyList<Mail>> GetAllAsync()
        {
            // lock thread (thread safety);
            lock (_lock)
            {
                return Task.FromResult<IReadOnlyList<Mail>>(_mails.ToList());
            }
        }

        public Task<IReadOnlyDictionary<string, int>> GetUnreadCountsAsync()
        {
            lock (_lock)
            {
                IReadOnlyDictionary<string, int> counts = new Dictionary<string, int>
                {
                    ["Inbox"] = _mails.Count(mail => !mail.IsRead),
                    ["Sent"] = _mails.Skip(10).Take(10).Count(mail => !mail.IsRead),
                    ["Archive"] = _mails.Skip(20).Take(5).Count(mail => !mail.IsRead),
                    ["Drafts"] = _mails.Skip(30).Take(3).Count(mail => !mail.IsRead)
                };
                return Task.FromResult(counts);
            }
        }

        public Task CreateAsync(CreateMailViewModel model)
        {
            lock (_lock)
            {
                _mails.Add(new Mail
                {
                    Id = Guid.NewGuid(),
                    Subject = model.Subject,
                    Body = model.Content,
                    From = "demo@example.com",
                    To = model.Recipient,
                    IsRead = false,
                    SentAt = GetRandomDateTime()
                });
            }

            return Task.CompletedTask;
        }

        public Task<PaginatedResult<Mail>> GetInboxPaginatedAsync(int page, int pageSize)
        {
            lock (_lock)
            {
                page = Math.Max(1, page);
                pageSize = Math.Clamp(pageSize, 1, 100);

                var totalCount = _mails.Count;

                var items = _mails
                    .OrderByDescending(m => m.SentAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return Task.FromResult(new PaginatedResult<Mail>
                {
                    Items = items,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalCount = totalCount
                });
            }
        }

        public Task<Mail?> GetByIdAsync(Guid Id)
        {
            lock (_lock)
            {
                var mail = _mails.FirstOrDefault(m => m.Id == Id);
                return Task.FromResult(mail);
            }
        }

        public Task<PaginatedResult<Mail>> GetSentPaginatedAsync(int page, int pageSize)
        {
            lock (_lock)
            {

                var customMails = _mails.Slice(10, 10);
                page = Math.Max(1, page);

                pageSize = Math.Clamp(pageSize, 1, 100);

                var totalCount = customMails.Count;

                var items = customMails
                    .OrderByDescending(m => m.SentAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return Task.FromResult(new PaginatedResult<Mail>
                {
                    Items = items,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalCount = totalCount
                });
            }
        }

        public Task<PaginatedResult<Mail>> GetArchivePaginatedAsync(int page, int pageSize)
        {
            lock (_lock)
            {

                var customMails = _mails.Slice(20, 5);
                page = Math.Max(1, page);

                pageSize = Math.Clamp(pageSize, 1, 100);

                var totalCount = customMails.Count;

                var items = customMails
                    .OrderByDescending(m => m.SentAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return Task.FromResult(new PaginatedResult<Mail>
                {
                    Items = items,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalCount = totalCount
                });
            }
        }

        public Task<PaginatedResult<Mail>> GetDraftsPaginatedAsync(int page, int pageSize)
        {
            lock (_lock)
            {

                var customMails = _mails.Slice(30, 3);
                page = Math.Max(1, page);

                pageSize = Math.Clamp(pageSize, 1, 100);

                var totalCount = customMails.Count;

                var items = customMails
                    .OrderByDescending(m => m.SentAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return Task.FromResult(new PaginatedResult<Mail>
                {
                    Items = items,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalCount = totalCount
                });
            }
        }
    }
}
