namespace DocLink.Models.Configuration;

public class CompanyBranding
{
    public const string SectionName = "CompanyBranding";

    public string Name { get; set; } = "DocLink";
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? LogoUrl { get; set; }
}
