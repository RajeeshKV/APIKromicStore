using FluentValidation;

namespace KromicStore.Application.Features.Shopping.Validators;

/// <summary>
/// Reusable validator for address validation.
/// Used by checkout commands to validate billing and shipping addresses.
/// </summary>
public sealed class AddressValidator
{
    private const int StreetMaxLength = 255;
    private const int CityMaxLength = 100;
    private const int StateMaxLength = 100;
    private const int PostalCodeMaxLength = 20;
    private const int CountryCodeLength = 2;

    /// <summary>
    /// Validates address components.
    /// </summary>
    public static bool IsValid(string street, string city, string state, string postalCode, string country)
    {
        if (string.IsNullOrWhiteSpace(street) || street.Length > StreetMaxLength)
            return false;

        if (string.IsNullOrWhiteSpace(city) || city.Length > CityMaxLength)
            return false;

        if (string.IsNullOrWhiteSpace(state) || state.Length > StateMaxLength)
            return false;

        if (string.IsNullOrWhiteSpace(postalCode) || postalCode.Length > PostalCodeMaxLength)
            return false;

        if (string.IsNullOrWhiteSpace(country) || country.Length != CountryCodeLength)
            return false;

        return true;
    }

    /// <summary>
    /// Gets validation errors for address components.
    /// </summary>
    public static List<string> GetValidationErrors(string street, string city, string state, string postalCode, string country)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(street))
            errors.Add("Street is required");
        else if (street.Length > StreetMaxLength)
            errors.Add($"Street cannot exceed {StreetMaxLength} characters");

        if (string.IsNullOrWhiteSpace(city))
            errors.Add("City is required");
        else if (city.Length > CityMaxLength)
            errors.Add($"City cannot exceed {CityMaxLength} characters");

        if (string.IsNullOrWhiteSpace(state))
            errors.Add("State is required");
        else if (state.Length > StateMaxLength)
            errors.Add($"State cannot exceed {StateMaxLength} characters");

        if (string.IsNullOrWhiteSpace(postalCode))
            errors.Add("PostalCode is required");
        else if (postalCode.Length > PostalCodeMaxLength)
            errors.Add($"PostalCode cannot exceed {PostalCodeMaxLength} characters");

        if (string.IsNullOrWhiteSpace(country))
            errors.Add("Country is required");
        else if (country.Length != CountryCodeLength)
            errors.Add("Country must be a valid ISO 3166-1 alpha-2 code (2 characters)");

        return errors;
    }
}
