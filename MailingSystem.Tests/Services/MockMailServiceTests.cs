using Xunit;
using Services;
using MailingSystem.Models;
using MailingSystem.ViewModel;

namespace MailingSystem.Tests.Services;

public class MockMailServiceTests
{
    [Fact]
    public async Task GetByIdAsync_WhenMailDoesNotExist_ReturnsNull()
    {
        // Given
        var service = new MockMailService();
        var missingId = Guid.Empty;
        // When
        var result = await service.GetByIdAsync(missingId);
        // Then
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMailExists_ReturnsMatchingMail()
    {
        var service = new MockMailService();
        var mails = await service.GetAllAsync();
        var expected = mails[0];

        var result = await service.GetByIdAsync(expected.Id);

        Assert.NotNull(result);
        Assert.Equal(expected.Id, result.Id);
        Assert.Equal(expected.Subject, result.Subject);
        Assert.Equal(expected.Body, result.Body);
        Assert.Equal(expected.From, result.From);
        Assert.Equal(expected.To, result.To);
        Assert.Equal(expected.IsRead, result.IsRead);
        Assert.Equal(expected.SentAt, result.SentAt);
    }

    [Fact]
    public async Task GetAllAsync_WhenServiceIsNew_ReturnsSixtyMailsWithUniqueIds()
    {
        var service = new MockMailService();

        var mails = await service.GetAllAsync();

        Assert.Equal(60, mails.Count);
        Assert.Equal(mails.Count, mails.Select(mail => mail.Id).Distinct().Count());
        Assert.All(mails, mail => Assert.NotEqual(Guid.Empty, mail.Id));
    }

    [Fact]
    public async Task GetAllAsync_WhenServiceIsNew_ReturnsNewestMailsFirst()
    {
        var service = new MockMailService();

        var mails = await service.GetAllAsync();

        Assert.Equal(mails.OrderByDescending(mail => mail.SentAt), mails);
    }

    [Fact]
    public async Task GetAllAsync_WhenReturnedCollectionIsCleared_DoesNotRemoveStoredMails()
    {
        var service = new MockMailService();
        var mails = await service.GetAllAsync();
        var expectedIds = mails.Select(mail => mail.Id).ToArray();

        if (mails is ICollection<Mail> { IsReadOnly: false } collection)
        {
            collection.Clear();
        }

        var storedMails = await service.GetAllAsync();
        Assert.Equal(expectedIds, storedMails.Select(mail => mail.Id));
    }

    [Fact]
    public async Task CreateAsync_WithValidModel_AddsUnreadMailThatCanBeRetrievedById()
    {
        var service = new MockMailService();
        var originalMails = await service.GetAllAsync();
        var originalIds = originalMails.Select(mail => mail.Id).ToHashSet();
        var model = new CreateMailViewModel
        {
            Subject = "Test subject",
            Content = "Test message body",
            Recipient = "recipient@example.com"
        };

        await service.CreateAsync(model);

        var mails = await service.GetAllAsync();
        Assert.Equal(originalMails.Count + 1, mails.Count);
        var created = Assert.Single(mails, mail => !originalIds.Contains(mail.Id));
        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal(model.Subject, created.Subject);
        Assert.Equal(model.Content, created.Body);
        Assert.Equal(model.Recipient, created.To);
        Assert.Equal("demo@example.com", created.From);
        Assert.False(created.IsRead);
        Assert.NotEqual(default, created.SentAt);

        var retrieved = await service.GetByIdAsync(created.Id);
        Assert.NotNull(retrieved);
        Assert.Equal(created.Id, retrieved.Id);
    }

    [Fact]
    public async Task CreateAsync_WhenCalledTwice_CreatesSeparateMails()
    {
        var service = new MockMailService();
        var originalIds = (await service.GetAllAsync()).Select(mail => mail.Id).ToHashSet();
        var model = new CreateMailViewModel
        {
            Subject = "Repeated subject",
            Content = "Repeated content",
            Recipient = "recipient@example.com"
        };

        await service.CreateAsync(model);
        await service.CreateAsync(model);

        var mails = await service.GetAllAsync();
        var created = mails.Where(mail => !originalIds.Contains(mail.Id)).ToList();
        Assert.Equal(2, created.Count);
        Assert.NotEqual(created[0].Id, created[1].Id);
        Assert.All(created, mail => Assert.Equal(model.Subject, mail.Subject));
    }
}
