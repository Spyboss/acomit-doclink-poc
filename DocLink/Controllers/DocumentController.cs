using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using DocLink.Services;
using DocLink.ViewModels;

namespace DocLink.Controllers;

public class DocumentController : Controller
{
    private readonly IDocumentService _documentService;
    private readonly IPdfService _pdfService;

    public DocumentController(IDocumentService documentService, IPdfService pdfService)
    {
        _documentService = documentService;
        _pdfService = pdfService;
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

        TempData["PublicToken"] = document.PublicToken;
        TempData["Title"] = document.Title;
        TempData["DocumentNumber"] = document.DocumentNumber;
        TempData["CustomerName"] = document.CustomerName;

        return RedirectToAction(nameof(Success));
    }

    [HttpGet]
    public IActionResult Success()
    {
        if (TempData["PublicToken"] is not string token)
            return RedirectToAction(nameof(Create));

        ViewData["PublicUrl"] = $"{Request.Scheme}://{Request.Host}/r/{token}";
        ViewData["PdfUrl"] = $"{Request.Scheme}://{Request.Host}/r/{token}/pdf";
        ViewData["Title"] = TempData["Title"];
        ViewData["DocumentNumber"] = TempData["DocumentNumber"];
        ViewData["CustomerName"] = TempData["CustomerName"];

        return View();
    }
}
