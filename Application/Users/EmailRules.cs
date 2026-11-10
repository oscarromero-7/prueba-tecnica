using System.Text.RegularExpressions;
using FluentValidation;

namespace PruebaTecnica.Application.Users;

public static partial class EmailRules
{
    // EmailAddress() de FluentValidation solo exige un "@" que no esté al inicio ni al final
    // (acepta "juan@test" o "juan perez@test.com"), por eso se usa una regla propia.
    [GeneratedRegex(@"^[A-Za-z0-9._%+-]+@([A-Za-z0-9-]+\.)+[A-Za-z]{2,}$")]
    private static partial Regex EmailRegex();

    public static IRuleBuilderOptions<T, string> ValidEmail<T>(this IRuleBuilderInitial<T, string> rule) =>
        rule.Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MaximumLength(200)
            .Must(email => EmailRegex().IsMatch(email.Trim()))
            .WithMessage("'Email' no tiene un formato válido.");
}
