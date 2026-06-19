using DocLink.Models;
using DocLink.ViewModels;

namespace DocLink.Services;

public interface IDocumentService
{
    Task<Document> CreateDocumentAsync(CreateDocumentViewModel model);
    Task<Document?> GetByTokenAsync(string token);
}
