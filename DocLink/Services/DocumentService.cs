using Microsoft.EntityFrameworkCore;
using DocLink.Data;
using DocLink.Models;
using DocLink.ViewModels;

namespace DocLink.Services;

public class DocumentService : IDocumentService
{
    private readonly AppDbContext _db;
    private readonly ITokenService _tokenService;

    public DocumentService(AppDbContext db, ITokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    public async Task<Document> CreateDocumentAsync(CreateDocumentViewModel model)
    {
        var document = new Document
        {
            Id = Guid.NewGuid(),
            DocumentType = Models.DocumentType.Receipt,
            DocumentNumber = model.InvoiceNumber,
            CustomerName = model.CustomerName,
            PhoneNumber = model.PhoneNumber,
            Amount = model.Amount,
            Date = model.Date,
            Status = "Created",
            PublicToken = _tokenService.GenerateToken(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Documents.Add(document);
        await _db.SaveChangesAsync();

        return document;
    }

    public async Task<Document?> GetByTokenAsync(string token)
    {
        return await _db.Documents
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.PublicToken == token);
    }
}
