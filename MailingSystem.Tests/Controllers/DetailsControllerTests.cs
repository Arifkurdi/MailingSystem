using MailingSystem.Controllers;
using MailingSystem.Models;
using MailingSystem.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace MailingSystem.Tests.Controllers;

public class DetailsControllerTests
{
    [Theory]
    [InlineData("Inbox", "Index")]
    [InlineData("Sent", "Sent")]
    [InlineData("Archive", "Archive")]
    [InlineData("Drafts", "Drafts")]
    public async Task Index_PreservesOriginatingFolderAndPage(string folder, string expectedAction)
    {
        var mail = CreateMail();
        var controller = new DetailsController(new StubMailService { MailResult = mail });

        var result = await controller.Index(mail.Id, 3, folder);

        var model = Assert.IsType<MailViewModel>(Assert.IsType<ViewResult>(result).Model);
        Assert.Equal(folder, model.FolderName);
        Assert.Equal(expectedAction, model.FolderAction);
        Assert.Equal(3, model.FolderPage);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Unknown")]
    [InlineData("Error")]
    public async Task Index_UnsupportedFolder_FallsBackToInbox(string? folder)
    {
        var mail = CreateMail();
        var controller = new DetailsController(new StubMailService { MailResult = mail });

        var result = await controller.Index(mail.Id, 2, folder);

        var model = Assert.IsType<MailViewModel>(Assert.IsType<ViewResult>(result).Model);
        Assert.Equal("Inbox", model.FolderName);
        Assert.Equal("Index", model.FolderAction);
        Assert.Equal(2, model.FolderPage);
    }

    private static Mail CreateMail() => new()
    {
        Id = Guid.Parse("ae738bd5-dc08-44d4-8748-246bf693ae12"),
        Subject = "Test message",
        Body = "Test body",
        From = "sender@example.com",
        To = "recipient@example.com",
        SentAt = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc)
    };

    [Theory]
    [InlineData(1, 1)]
    [InlineData(4, 4)]
    [InlineData(0, 1)]
    [InlineData(-2, 1)]
    public async Task Index_WhenMailExists_ReturnsMailAndValidFolderPage(int page, int expectedFolderPage)
    {
        var mail = CreateMail();
        var service = new StubMailService { MailResult = mail };
        var controller = new DetailsController(service);

        var result = await controller.Index(mail.Id, page);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<MailViewModel>(view.Model);
        Assert.Same(mail, model.Mail);
        Assert.Equal(expectedFolderPage, model.FolderPage);
        Assert.Equal(mail.Id, service.RequestedId);
    }

    [Fact]
    public async Task Index_WhenPageIsOmitted_DefaultsToFirstFolderPage()
    {
        var mail = CreateMail();
        var controller = new DetailsController(new StubMailService { MailResult = mail });

        var result = await controller.Index(mail.Id);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<MailViewModel>(view.Model);
        Assert.Equal(1, model.FolderPage);
        Assert.Equal("Inbox", model.FolderName);
        Assert.Equal("Index", model.FolderAction);
    }

    [Fact]
    public async Task Index_WhenMailDoesNotExist_ReturnsNotFound()
    {
        var service = new StubMailService();
        var controller = new DetailsController(service);
        var id = CreateMail().Id;

        var result = await controller.Index(id, 4);

        Assert.IsType<NotFoundResult>(result);
        Assert.Equal(id, service.RequestedId);
    }
}
