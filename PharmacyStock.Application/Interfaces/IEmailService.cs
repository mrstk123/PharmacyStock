using System.Threading.Tasks;

namespace PharmacyStock.Application.Interfaces;

public interface IEmailService
{
    Task<bool> SendEmailAsync(string to, string subject, string body);
}
