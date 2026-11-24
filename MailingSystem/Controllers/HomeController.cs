using System.Diagnostics;
using MailingSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;
using ViewModel;

namespace MailingSystem.Controllers;

public class HomeController(IMailService mailService) : Controller
{
    private readonly IMailService _mailService = mailService;

    [HttpGet]
    public async Task<IActionResult> Inbox(int page = 1)
    {
        const int pageSize = 15;

        var mails = await _mailService.GetInboxPaginatedAsync(page, pageSize);
        var model = new HomeViewModel { FolderName = "Inbox", Mails = mails };

        return View(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(
            new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }
        );
    }

    [HttpGet]
    public async Task<IActionResult> Sent(int page = 1)
    {
        const int pageSize = 15;

        var mails = await _mailService.GetSentPaginatedAsync(page, pageSize);
        var model = new HomeViewModel { FolderName = "Sent", Mails = mails };

        return View("Inbox", model);
    }

    [HttpGet]
    public async Task<IActionResult> Archive(int page = 1)
    {
        const int pageSize = 15;

        var mails = await _mailService.GetArchivePaginatedAsync(page, pageSize);
        var model = new HomeViewModel { FolderName = "Archive", Mails = mails };

        return View("Inbox", model);
    }

    [HttpGet]
    public async Task<IActionResult> Drafts(int page = 1)
    {
        const int pageSize = 15;

        var mails = await _mailService.GetDraftsPaginatedAsync(page, pageSize);
        var model = new HomeViewModel { FolderName = "Drafts", Mails = mails };

        return View("Inbox", model);
    }

    [HttpGet]
    public async Task<IActionResult> Compose(Guid id = default)
    {
        if (id == Guid.Empty)
            return View(new ComposeViewModel());

        var mail = await _mailService.GetByIdAsync(id);
        if (mail is null)
            return NotFound();

        var model = new ComposeViewModel
        {
            ReplyToId = mail.Id,
            ReplyToTitle = mail.Subject,
            ReplyToAddress = mail.From,
        };

        return View(model);
    }
}
