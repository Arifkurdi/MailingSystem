using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MailingSystem.Models;
using MailingSystem.ViewModel;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Services;
using Services.Interface;
using ViewModel;

namespace MailingSystem.Controllers;

public class DetailsController(IMailService mailService) : Controller
{
    private readonly IMailService _mailService = mailService;

    [HttpGet]
    public async Task<IActionResult> Index(Guid id, int page = 1, string? folder = "Inbox")

    {

        var mail = await _mailService.GetByIdAsync(id);

        if (mail is null) return NotFound();

        return View(new MailViewModel
        {
            Mail = mail,
            FolderPage = Math.Max(1, page),
            FolderName = folder is "Sent" or "Archive" or "Drafts" ? folder : "Inbox"
        });

    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
