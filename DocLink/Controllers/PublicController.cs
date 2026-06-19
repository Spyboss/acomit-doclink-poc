using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using DocLink.Services;
using DocLink.ViewModels;

namespace DocLink.Controllers;

[EnableRateLimiting("PublicRead")]
public class PublicController : Controller
{
    private readonly IDocumentService _documentService;
    private readonly IPdfService _pdfService;

    public PublicController(IDocumentService documentService, IPdfService pdfService)
    {
        _documentService = documentService;
        _pdfService = pdfService;
    }

    [HttpGet("/r/{token}")]
    public async Task<IActionResult> Index(string token)
    {
        var document = await _documentService.GetByTokenAsync(token);
        if (document is null)
            return NotFound();

        var viewModel = new PublicDocumentViewModel
        {
            Title = document.Title,
            DocumentNumber = document.DocumentNumber,
            CustomerName = document.CustomerName,
            Amount = document.Amount,
            Date = document.Date,
            Address = document.Address,
            Notes = document.Notes,
            ReferenceNumber = document.ReferenceNumber,
            Status = document.Status,
            PublicToken = document.PublicToken
        };

        return View(viewModel);
    }

    [HttpGet("/r/{token}/pdf")]
    public async Task<IActionResult> Pdf(string token)
    {
        var document = await _documentService.GetByTokenAsync(token);
        if (document is null)
            return NotFound();

        var pdf = _pdfService.GenerateReceiptPdf(document);

        return File(pdf, "application/pdf", $"{document.DocumentNumber}.pdf");
    }
}
