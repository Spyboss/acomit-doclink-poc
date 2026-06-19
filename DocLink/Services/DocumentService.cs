using Microsoft.EntityFrameworkCore;
using DocLink.Data;
using DocLink.Models;
using DocLink.ViewModels;

namespace DocLink.Services;

public class DocumentService : IDocumentService
{
    private readonly AppDbContext _db;
    private readonly ITokenService _tokenService;
    private readonly ISmsService _smsService;

    public DocumentService(AppDbContext db, ITokenService tokenService, ISmsService smsService)
    {
        _db = db;
        _tokenService = tokenService;
        _smsService = smsService;
    }

    public async Task<Document> CreateDocumentAsync(CreateDocumentViewModel model)
    {
        var document = new Document
        {
            Id = Guid.NewGuid(),
            Type = "Receipt",
            Title = "Payment Receipt",
            DocumentNumber = model.InvoiceNumber,
            CustomerName = model.CustomerName,
            PhoneNumber = model.PhoneNumber,
            Amount = model.Amount,
            Date = model.Date,
            Address = model.Address,
            Notes = model.Notes,
            ReferenceNumber = model.ReferenceNumber,
            Status = "Created",
            PublicToken = _tokenService.GenerateToken(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Documents.Add(document);
        await _db.SaveChangesAsync();

        var publicUrl = $"/r/{document.PublicToken}";
        var message = $"Your {document.Title} is ready.\n\nDocLink\n{publicUrl}";

        await _smsService.SendAsync(document.PhoneNumber, message);

        return document;
    }

    public async Task<Document?> GetByTokenAsync(string token)
    {
        return await _db.Documents
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.PublicToken == token);
    }
}
