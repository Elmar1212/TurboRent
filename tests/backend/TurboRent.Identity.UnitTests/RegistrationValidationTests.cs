using TurboRent.Modules.Identity.Features.Register;
using TurboRent.Modules.Identity.Validation;

namespace TurboRent.Identity.UnitTests;

public class RegistrationValidationTests
{
    private static RegisterRequest Valid => new("person@example.test", "a long safe passphrase", "Test", "az", "customer", null, null, null);
    [Theory]
    [InlineData("Admin")]
    [InlineData("IndividualOwner")]
    [InlineData("unknown")]
    public void Privileged_or_unknown_account_types_are_rejected(string type) =>
        Assert.Contains("accountType", RegistrationValidation.Validate(Valid with { AccountType = type }));
    [Theory]
    [InlineData(14)]
    [InlineData(129)]
    public void Password_bounds_are_enforced(int length) =>
        Assert.Contains("password", RegistrationValidation.Validate(Valid with { Password = new string('a', length) }));
    [Fact]
    public void Unicode_passphrases_and_spaces_are_allowed() =>
        Assert.Empty(RegistrationValidation.Validate(Valid with { Password = "Mənim uzun təhlükəsiz parolum" }));
    [Fact]
    public void Company_requires_phone_legal_name_and_voen()
    {
        var errors = RegistrationValidation.Validate(Valid with { AccountType = "company" });
        Assert.Contains("contactPhone", errors.Keys);
        Assert.Contains("legalName", errors.Keys);
        Assert.Contains("voen", errors.Keys);
    }
}
