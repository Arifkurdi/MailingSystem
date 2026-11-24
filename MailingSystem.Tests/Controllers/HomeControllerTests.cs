using MailingSystem.Controllers;
using Microsoft.AspNetCore.Mvc;
using ViewModel;

namespace MailingSystem.Tests.Controllers;

public class HomeControllerTests
{
    [Theory]
    [InlineData("Inbox", 1)]
    [InlineData("Inbox", 3)]
    [InlineData("Sent", 1)]
    [InlineData("Sent", 3)]
    [InlineData("Archive", 1)]
    [InlineData("Archive", 3)]
    [InlineData("Drafts", 1)]
    [InlineData("Drafts", 3)]
    public async Task Folder_ReturnsItsOwnServiceResultAndFolderName(
        string folder,
        int requestedPage
    )
    {
        var page = new PaginatedResult<MailingSystem.Models.Mail>
        {
            CurrentPage = requestedPage,
            PageSize = 15,
            TotalCount = 60,
        };
        var service = new StubMailService { PageResult = page };
        var controller = new HomeController(service);

        var result = folder switch
        {
            "Inbox" => await controller.Inbox(requestedPage),
            "Sent" => await controller.Sent(requestedPage),
            "Archive" => await controller.Archive(requestedPage),
            "Drafts" => await controller.Drafts(requestedPage),
            _ => throw new ArgumentOutOfRangeException(nameof(folder)),
        };

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<HomeViewModel>(view.Model);
        Assert.Equal(folder, service.RequestedFolder);
        Assert.Equal(folder, model.FolderName);
        Assert.Equal(folder == "Inbox" ? "Inbox" : folder, model.FolderAction);
        Assert.Same(page, model.Mails);
        Assert.Equal(requestedPage, service.RequestedPage);
        Assert.Equal(15, service.RequestedPageSize);
        if (folder != "Inbox")
            Assert.Equal("Inbox", view.ViewName);
    }

    [Theory]
    [InlineData("Sent")]
    [InlineData("Archive")]
    [InlineData("Drafts")]
    public async Task Folder_WhenPageIsOmitted_RequestsFirstPage(string folder)
    {
        var service = new StubMailService();
        var controller = new HomeController(service);

        _ = folder switch
        {
            "Sent" => await controller.Sent(),
            "Archive" => await controller.Archive(),
            "Drafts" => await controller.Drafts(),
            _ => throw new ArgumentOutOfRangeException(nameof(folder)),
        };

        Assert.Equal(folder, service.RequestedFolder);
        Assert.Equal(1, service.RequestedPage);
        Assert.Equal(15, service.RequestedPageSize);
    }

    [Fact]
    public async Task Inbox_WhenPageIsOmitted_RequestsFirstPage()
    {
        var service = new StubMailService();
        var controller = new HomeController(service);

        await controller.Inbox();

        Assert.Equal(1, service.RequestedPage);
        Assert.Equal(15, service.RequestedPageSize);
    }

    [Fact]
    public async Task Inbox_WhenPageIsSpecified_ReturnsTheRequestedPageInTheViewModel()
    {
        var page = new PaginatedResult<MailingSystem.Models.Mail>
        {
            CurrentPage = 3,
            PageSize = 15,
            TotalCount = 60,
        };
        var service = new StubMailService { PageResult = page };
        var controller = new HomeController(service);

        var result = await controller.Inbox(3);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<HomeViewModel>(view.Model);
        Assert.Same(page, model.Mails);
        Assert.Equal(3, service.RequestedPage);
        Assert.Equal(15, service.RequestedPageSize);
    }

    [Fact]
    public async Task Inbox_WhenInboxIsEmpty_ReturnsAnEmptyPage()
    {
        var controller = new HomeController(new StubMailService());

        var result = await controller.Inbox();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<HomeViewModel>(view.Model);
        Assert.Empty(model.Mails.Items);
        Assert.Equal(0, model.Mails.TotalCount);
    }

    [Fact]
    public async Task Compose_WhenReplying_ProvidesOriginalSubjectAndSender()
    {
        var mail = new MailingSystem.Models.Mail
        {
            Id = Guid.NewGuid(),
            Subject = "Project update",
            From = "sender@example.com",
            To = "inbox@example.com",
            Body = "Original message\nSecond line",
        };
        var service = new StubMailService { MailResult = mail };
        var controller = new HomeController(service);

        var view = Assert.IsType<ViewResult>(await controller.Compose(mail.Id));
        var model = Assert.IsType<ComposeViewModel>(view.Model);

        Assert.Equal(mail.Id, service.RequestedId);
        Assert.Equal(mail.Id, model.ReplyToId);
        Assert.Equal(mail.From, model.ReplyToAddress);
        Assert.Equal(mail.Subject, model.ReplyToTitle);
    }
}
