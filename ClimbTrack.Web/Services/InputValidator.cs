using System.ComponentModel.DataAnnotations;

namespace ClimbTrack.Web.Services;

/// <summary>Runs DataAnnotations validation in the service layer so rules hold even when the UI is bypassed (e.g. API calls).</summary>
public static class InputValidator
{
    public static string? Validate(object input)
    {
        var results = new List<ValidationResult>();
        var ok = Validator.TryValidateObject(input, new ValidationContext(input), results, validateAllProperties: true);
        return ok ? null : string.Join(" ", results.Select(r => r.ErrorMessage));
    }
}
