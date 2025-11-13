using FluentValidation;
using System.Net;
using WordMaster.Application.Requests.AllowedIpAddress;

namespace WordMaster.Application.Validation.AllowedIpAddress;

public class AllowedIpAddressCreateRequestValidator : AbstractValidator<AllowedIpAddressCreateRequest>
{
    public AllowedIpAddressCreateRequestValidator()
    {
        RuleFor(x => x.IpAddress)
            .NotEmpty().WithMessage("IP adresi gereklidir.")
            .Must(BeValidIpAddress).WithMessage("Geçerli bir IP adresi giriniz (IPv4 veya IPv6).");

        RuleFor(x => x.Description)
            .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("Açıklama en fazla 500 karakter olabilir.");
    }

    private bool BeValidIpAddress(string? ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
            return false;

        // IPAddress.TryParse hem IPv4 hem IPv6'yı doğrular
        return IPAddress.TryParse(ipAddress, out IPAddress? parsedAddress) &&
               (parsedAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork ||
                parsedAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6);
    }
}

