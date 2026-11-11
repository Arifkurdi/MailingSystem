using MailingSystem.Models;
using MailingSystem.ViewModel;
using Services;

namespace MailingSystem.Tests.Services;

public class MailPaginationTests
{
    private static Task<PaginatedResult<Mail>> GetPage(
        MockMailService service, string folder, int page, int pageSize) => folder switch
        {
            "Inbox" => service.GetInboxPaginatedAsync(page, pageSize),
            "Sent" => service.GetSentPaginatedAsync(page, pageSize),
            "Archive" => service.GetArchivePaginatedAsync(page, pageSize),
            "Drafts" => service.GetDraftsPaginatedAsync(page, pageSize),
            _ => throw new ArgumentOutOfRangeException(nameof(folder))
        };

    [Theory]
    [InlineData("Inbox", 60)]
    [InlineData("Sent", 10)]
    [InlineData("Archive", 5)]
    [InlineData("Drafts", 3)]
    public async Task Pagination_CoversEveryMessageOnceInDateOrder(string folder, int expectedCount)
    {
        var service = new MockMailService();
        const int pageSize = 2;
        var first = await GetPage(service, folder, 1, pageSize);
        var allItems = new List<Mail>();
        var expectedPages = (int)Math.Ceiling((double)expectedCount / pageSize);

        for (var page = 1; page <= expectedPages; page++)
        {
            var result = await GetPage(service, folder, page, pageSize);
            Assert.Equal(page, result.CurrentPage);
            Assert.Equal(pageSize, result.PageSize);
            Assert.Equal(expectedCount, result.TotalCount);
            Assert.Equal(expectedPages, result.TotalPages);
            Assert.Equal(page > 1, result.HasPreviousPage);
            Assert.Equal(page < expectedPages, result.HasNextPage);
            Assert.Equal(Math.Min(pageSize, expectedCount - (page - 1) * pageSize), result.Items.Count);
            allItems.AddRange(result.Items);
        }

        Assert.Equal(expectedCount, allItems.Select(mail => mail.Id).Distinct().Count());
        Assert.Equal(allItems.OrderByDescending(mail => mail.SentAt).Select(mail => mail.Id),
            allItems.Select(mail => mail.Id));
        var repeated = await GetPage(service, folder, 1, pageSize);
        Assert.Equal(first.Items.Select(mail => mail.Id), repeated.Items.Select(mail => mail.Id));

        foreach (var mail in allItems)
        {
            var retrieved = await service.GetByIdAsync(mail.Id);
            Assert.NotNull(retrieved);
            Assert.Equal(mail.Subject, retrieved.Subject);
            Assert.Equal(mail.Body, retrieved.Body);
        }

        if (folder == "Inbox")
            Assert.Equal((await service.GetAllAsync()).Select(mail => mail.Id), allItems.Select(mail => mail.Id));
    }

    [Theory]
    [InlineData("Inbox")]
    [InlineData("Sent")]
    [InlineData("Archive")]
    [InlineData("Drafts")]
    public async Task UnreadCounts_CoversTheWholeFolderAcrossPagesAndReflectsReadStatus(string folder)
    {
        var service = new MockMailService();
        var wholeFolder = await GetPage(service, folder, 1, 100);
        var firstPage = await GetPage(service, folder, 1, 1);
        foreach (var mail in wholeFolder.Items)
            mail.IsRead = false;

        var counts = await service.GetUnreadCountsAsync();

        Assert.Equal(wholeFolder.Items.Count(mail => !mail.IsRead), counts[folder]);
        Assert.True(counts[folder] > firstPage.Items.Count(mail => !mail.IsRead));

        foreach (var mail in wholeFolder.Items)
            mail.IsRead = true;

        counts = await service.GetUnreadCountsAsync();
        Assert.Equal(0, counts[folder]);
    }

    [Fact]
    public async Task UnreadCounts_NewMailIncreasesInboxCount()
    {
        var service = new MockMailService();
        var before = await service.GetUnreadCountsAsync();

        await service.CreateAsync(new CreateMailViewModel
        {
            Subject = "Unread message",
            Content = "Body",
            Recipient = "recipient@example.com"
        });

        var after = await service.GetUnreadCountsAsync();
        Assert.Equal(before["Inbox"] + 1, after["Inbox"]);
        foreach (var folder in new[] { "Sent", "Archive", "Drafts" })
            Assert.Equal(before[folder], after[folder]);
    }

    public static IEnumerable<object[]> InvalidPaginationInputs()
    {
        foreach (var folder in new[] { "Inbox", "Sent", "Archive", "Drafts" })
        {
            yield return [folder, 0, 15, 15];
            yield return [folder, -5, 15, 15];
            yield return [folder, 1, 0, 1];
            yield return [folder, 1, -5, 1];
            yield return [folder, 1, 101, 100];
        }
    }

    [Theory]
    [MemberData(nameof(InvalidPaginationInputs))]
    public async Task Pagination_NormalizesNonpositivePagesAndBoundsPageSize(
        string folder, int page, int pageSize, int expectedPageSize)
    {
        var service = new MockMailService();

        var result = await GetPage(service, folder, page, pageSize);

        Assert.Equal(1, result.CurrentPage);
        Assert.Equal(expectedPageSize, result.PageSize);
        Assert.Equal(Math.Min(result.TotalCount, expectedPageSize), result.Items.Count);
    }

    [Fact]
    public async Task CreateAsync_NewMessageAppearsInPaginatedInboxWithoutLosingExistingMessages()
    {
        var service = new MockMailService();
        var originalIds = (await service.GetAllAsync()).Select(mail => mail.Id).ToHashSet();
        await service.CreateAsync(new CreateMailViewModel
        {
            Subject = "New message",
            Content = "New body",
            Recipient = "recipient@example.com"
        });

        var result = await service.GetInboxPaginatedAsync(1, 100);

        Assert.Equal(originalIds.Count + 1, result.TotalCount);
        Assert.True(originalIds.IsSubsetOf(result.Items.Select(mail => mail.Id)));
        var created = Assert.Single(result.Items, mail => !originalIds.Contains(mail.Id));
        Assert.Equal("New message", created.Subject);
        Assert.Equal("New body", created.Body);
        Assert.Equal("recipient@example.com", created.To);
    }
}
