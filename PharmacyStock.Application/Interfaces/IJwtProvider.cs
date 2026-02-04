using PharmacyStock.Domain.Entities;

namespace PharmacyStock.Application.Interfaces;

public interface IJwtProvider
{
    string GenerateAccessToken(User user, string roleName, IEnumerable<string> permissions, bool isPersistent);
    string GenerateRefreshToken();
    (int userId, bool isPersistent)? ValidateExpiredAccessToken(string accessToken);
}
