namespace PharmacyStock.Application.DTOs;

public class SupplierDto
{
    public int Id { get; set; }
    public string SupplierCode { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? ContactInfo { get; set; }
    public bool IsActive { get; set; }
}

public class CreateSupplierDto
{
    public string SupplierCode { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? ContactInfo { get; set; }
}

public class UpdateSupplierDto
{
    public int Id { get; set; }
    public string SupplierCode { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? ContactInfo { get; set; }
    public bool IsActive { get; set; }
}
