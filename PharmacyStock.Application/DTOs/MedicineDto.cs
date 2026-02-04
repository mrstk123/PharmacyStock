namespace PharmacyStock.Application.DTOs;

public class MedicineDto
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public string MedicineCode { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? GenericName { get; set; }
    public string? Manufacturer { get; set; }
    public string? StorageCondition { get; set; }
    public string UnitOfMeasure { get; set; } = null!;
    public int LowStockThreshold { get; set; } = 50;
    public bool IsActive { get; set; }
}

public class CreateMedicineDto
{
    public int CategoryId { get; set; }
    public string MedicineCode { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? GenericName { get; set; }
    public string? Manufacturer { get; set; }
    public string? StorageCondition { get; set; }
    public string UnitOfMeasure { get; set; } = null!;
    public int LowStockThreshold { get; set; } = 50;
}

public class UpdateMedicineDto
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string MedicineCode { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? GenericName { get; set; }
    public string? Manufacturer { get; set; }
    public string? StorageCondition { get; set; }
    public string UnitOfMeasure { get; set; } = null!;
    public int LowStockThreshold { get; set; } = 50;
    public bool IsActive { get; set; }
}
