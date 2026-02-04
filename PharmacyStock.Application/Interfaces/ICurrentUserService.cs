namespace PharmacyStock.Application.Interfaces;

public interface ICurrentUserService
{
    string? GetCurrentUsername();
    int? GetCurrentUserId();
}
