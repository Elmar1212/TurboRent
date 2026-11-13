using System.ComponentModel.DataAnnotations;
using TurboRent.Modules.Identity.Features.Register;

namespace TurboRent.Modules.Identity.Validation;

public static class RegistrationValidation
{
    public static bool ValidEmail(string? email) =>
        email is { Length: > 0 and <= 254 } && email == email.Trim() && new EmailAddressAttribute().IsValid(email);

    public static Dictionary<string, string[]> Validate(RegisterRequest request)
    {
        var errors = new Dictionary<string, string[]>();
        if (!ValidEmail(request.Email)) errors["email"] = ["invalid_email"];
        if (request.Password is not { Length: >= 15 and <= 128 }) errors["password"] = ["password_length"];
        if (string.IsNullOrWhiteSpace(request.DisplayName) || request.DisplayName.Trim().Length > 100)
            errors["displayName"] = ["invalid_name"];
        if (request.Locale is not ("az" or "en")) errors["locale"] = ["invalid_locale"];
        if (request.AccountType is not ("customer" or "owner" or "company")) errors["accountType"] = ["invalid_account_type"];
        if (request.AccountType is "owner" or "company")
        {
            var phone = request.ContactPhone;
            if (phone is null || phone.Length != 13 || !phone.StartsWith("+994", StringComparison.Ordinal) ||
                !phone[4..].All(char.IsAsciiDigit)) errors["contactPhone"] = ["invalid_phone"];
        }
        if (request.AccountType == "company")
        {
            if (string.IsNullOrWhiteSpace(request.LegalName) || request.LegalName.Trim().Length > 200)
                errors["legalName"] = ["invalid_company_name"];
            if (request.Voen is not { Length: 10 } || !request.Voen.All(char.IsAsciiDigit))
                errors["voen"] = ["invalid_voen"];
        }
        return errors;
    }
}
