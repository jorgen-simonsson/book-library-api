using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace BookLibrary.Application.Validation;

[AttributeUsage(AttributeTargets.Property)]
public sealed class ValidPublishedYearAttribute : ValidationAttribute
{
    private static readonly Regex FourDigitYear = new(@"^\d{4}$", RegexOptions.Compiled);

    public ValidPublishedYearAttribute()
    {
        ErrorMessage = "PublishedYear must be null or a 4-digit year (e.g. 2024).";
    }

    public override bool IsValid(object? value)
    {
        if (value is null) return true;
        return value is string str && FourDigitYear.IsMatch(str);
    }
}
