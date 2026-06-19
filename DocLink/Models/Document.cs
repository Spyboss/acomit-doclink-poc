using System.ComponentModel.DataAnnotations;

namespace DocLink.Models;

public class Document
{
    public Guid Id { get; set; }

    [MaxLength(50)]
    public string Type { get; set; } = "Receipt";

    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(100)]
    public string DocumentNumber { get; set; } = string.Empty;

    [MaxLength(200)]
    public string CustomerName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string PhoneNumber { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public DateTime Date { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    [MaxLength(100)]
    public string? ReferenceNumber { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = "Created";

    [MaxLength(100)]
    public string PublicToken { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
