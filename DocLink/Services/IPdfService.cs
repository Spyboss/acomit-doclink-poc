using DocLink.Models;

namespace DocLink.Services;

public interface IPdfService
{
    byte[] GenerateReceiptPdf(Document document);
}
