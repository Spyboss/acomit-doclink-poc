using System.ComponentModel.DataAnnotations;

namespace DocLink.Models;

public class Document
{
    public Guid Id { get; set; }

    public DocumentType DocumentType { get; set; } = Models.DocumentType.Receipt;

    [MaxLength(100)]
    public string DocumentNumber { get; set; } = string.Empty;

    [MaxLength(200)]
    public string CustomerName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string PhoneNumber { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public DateTime Date { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = "Created";

    [MaxLength(100)]
    public string PublicToken { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
