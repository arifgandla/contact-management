using System.ComponentModel.DataAnnotations;
using ContactManagement.Core.Exceptions;
using ValidationException = ContactManagement.Core.Exceptions.ValidationException;

namespace ContactManagement.Services.Validators;

public static class ValidationService
{
    public static void Validate<T>(T model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var results = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        var context = new ValidationContext(model);
        if (!Validator.TryValidateObject(model, context, results, validateAllProperties: true))
        {
            var errors = results
                .Select(r => r.ErrorMessage ?? "Invalid value.")
                .Distinct()
                .ToArray();
            throw new ValidationException("Validation failed.", errors);
        }
    }

    public static void ValidateId(int id, string name = "Id")
    {
        if (id <= 0)
        {
            throw new ValidationException($"{name} must be a positive integer.");
        }
    }
}
