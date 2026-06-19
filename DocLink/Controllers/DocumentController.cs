using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using DocLink.Services;
using DocLink.ViewModels;

namespace DocLink.Controllers;

public class DocumentController : Controller
{
    private readonly IDocumentService _documentService;
    private readonly ISmsService _smsService;

    public DocumentController(IDocumentService documentService, ISmsService smsService)
    {
        _documentService = documentService;
        _smsService = smsService;
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreateDocumentViewModel
        {
            Date = DateTime.Today
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("CreateDocument")]
    public async Task<IActionResult> Create(CreateDocumentViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var document = await _documentService.CreateDocumentAsync(model);

        var publicUrl = $"{Request.Scheme}://{Request.Host}/r/{document.PublicToken}";
        var message = $"Your {document.DocumentType} is ready.\n\nDocLink\n{publicUrl}";

        await _smsService.SendAsync(document.PhoneNumber, message);

        TempData["PublicToken"] = document.PublicToken;
        TempData["DocumentNumber"] = document.DocumentNumber;
        TempData["CustomerName"] = document.CustomerName;
        TempData["PhoneNumber"] = document.PhoneNumber;

        return RedirectToAction(nameof(Success));
    }

    [HttpGet]
    public IActionResult Success()
    {
        if (TempData["PublicToken"] is not string token)
            return RedirectToAction(nameof(Create));

        ViewData["PublicUrl"] = $"{Request.Scheme}://{Request.Host}/r/{token}";
        ViewData["PdfUrl"] = $"{Request.Scheme}://{Request.Host}/r/{token}/pdf";
        ViewData["DocumentNumber"] = TempData["DocumentNumber"];
        ViewData["CustomerName"] = TempData["CustomerName"];
        ViewData["PhoneNumber"] = TempData["PhoneNumber"];

        return View();
    }
}
