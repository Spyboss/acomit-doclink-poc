using System.ComponentModel.DataAnnotations;

namespace DocLink.ViewModels;

public class CreateDocumentViewModel
{
    [Required(ErrorMessage = "Customer name is required")]
    [Display(Name = "Customer Name")]
    [MaxLength(200)]
    public string CustomerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone number is required")]
    [Display(Name = "Phone Number")]
    [Phone(ErrorMessage = "Invalid phone number")]
    [MaxLength(50)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Invoice number is required")]
    [Display(Name = "Invoice Number")]
    [MaxLength(100)]
    public string InvoiceNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Amount is required")]
    [Range(0.01, 99999999.99, ErrorMessage = "Amount must be between 0.01 and 99,999,999.99")]
    [DataType(DataType.Currency)]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Date is required")]
    [DataType(DataType.Date)]
    public DateTime Date { get; set; } = DateTime.Today;

    [Display(Name = "Address")]
    [MaxLength(500)]
    public string? Address { get; set; }

    [Display(Name = "Notes")]
    [MaxLength(2000)]
    public string? Notes { get; set; }

    [Display(Name = "Reference Number")]
    [MaxLength(100)]
    public string? ReferenceNumber { get; set; }
}
