using DocLink.Models;

namespace DocLink.ViewModels;

public class PublicDocumentViewModel
{
    public DocumentType DocumentType { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PublicToken { get; set; } = string.Empty;
}
