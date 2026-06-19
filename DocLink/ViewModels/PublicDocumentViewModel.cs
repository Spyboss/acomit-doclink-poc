namespace DocLink.ViewModels;

public class PublicDocumentViewModel
{
    public string Title { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string? Address { get; set; }
    public string? Notes { get; set; }
    public string? ReferenceNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PublicToken { get; set; } = string.Empty;
}
