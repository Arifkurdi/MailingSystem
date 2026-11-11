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
    public async Task Folder_ReturnsItsOwnServiceResultAndFolderName(string folder, int requestedPage)
    {
        var page = new PaginatedResult<MailingSystem.Models.Mail>
        {
            CurrentPage = requestedPage,
            PageSize = 15,
            TotalCount = 60
        };
        var service = new StubMailService { PageResult = page };
        var controller = new HomeController(service);

        var result = folder switch
        {
            "Inbox" => await controller.Index(requestedPage),
            "Sent" => await controller.Sent(requestedPage),
            "Archive" => await controller.Archive(requestedPage),
            "Drafts" => await controller.Drafts(requestedPage),
            _ => throw new ArgumentOutOfRangeException(nameof(folder))
        };

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<HomeViewModel>(view.Model);
        Assert.Equal(folder, service.RequestedFolder);
        Assert.Equal(folder, model.FolderName);
        Assert.Equal(folder == "Inbox" ? "Index" : folder, model.FolderAction);
        Assert.Same(page, model.Mails);
        Assert.Equal(requestedPage, service.RequestedPage);
        Assert.Equal(15, service.RequestedPageSize);
        if (folder != "Inbox") Assert.Equal("Index", view.ViewName);
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
            _ => throw new ArgumentOutOfRangeException(nameof(folder))
        };

        Assert.Equal(folder, service.RequestedFolder);
        Assert.Equal(1, service.RequestedPage);
        Assert.Equal(15, service.RequestedPageSize);
    }

    [Fact]
    public async Task Index_WhenPageIsOmitted_RequestsFirstPage()
    {
        var service = new StubMailService();
        var controller = new HomeController(service);

        await controller.Index();

        Assert.Equal(1, service.RequestedPage);
        Assert.Equal(15, service.RequestedPageSize);
    }

    [Fact]
    public async Task Index_WhenPageIsSpecified_ReturnsTheRequestedPageInTheViewModel()
    {
        var page = new PaginatedResult<MailingSystem.Models.Mail>
        {
            CurrentPage = 3,
            PageSize = 15,
            TotalCount = 60
        };
        var service = new StubMailService { PageResult = page };
        var controller = new HomeController(service);

        var result = await controller.Index(3);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<HomeViewModel>(view.Model);
        Assert.Same(page, model.Mails);
        Assert.Equal(3, service.RequestedPage);
        Assert.Equal(15, service.RequestedPageSize);
    }

    [Fact]
    public async Task Index_WhenInboxIsEmpty_ReturnsAnEmptyPage()
    {
        var controller = new HomeController(new StubMailService());

        var result = await controller.Index();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<HomeViewModel>(view.Model);
        Assert.Empty(model.Mails.Items);
        Assert.Equal(0, model.Mails.TotalCount);
    }
}
