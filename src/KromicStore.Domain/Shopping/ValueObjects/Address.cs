namespace KromicStore.Domain.Shopping.ValueObjects;

/// <summary>
/// Address value object representing a shipping or billing address.
/// Immutable and ensures valid address data.
/// </summary>
public sealed class Address : IEquatable<Address>
{
    public string FirstName { get; }
    public string LastName { get; }
    public string Email { get; }
    public string PhoneNumber { get; }
    public string AddressLine1 { get; }
    public string? AddressLine2 { get; }
    public string City { get; }
    public string StateOrProvince { get; }
    public string PostalCode { get; }
    public string Country { get; }

    private Address(
        string firstName,
        string lastName,
        string email,
        string phoneNumber,
        string addressLine1,
        string? addressLine2,
        string city,
        string stateOrProvince,
        string postalCode,
        string country)
    {
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Email = email.Trim().ToLowerInvariant();
        PhoneNumber = phoneNumber.Trim();
        AddressLine1 = addressLine1.Trim();
        AddressLine2 = string.IsNullOrWhiteSpace(addressLine2) ? null : addressLine2.Trim();
        City = city.Trim();
        StateOrProvince = stateOrProvince.Trim();
        PostalCode = postalCode.Trim();
        Country = country.Trim();
    }

    /// <summary>
    /// Create a new Address value object.
    /// </summary>
    public static Address Create(
        string firstName,
        string lastName,
        string email,
        string phoneNumber,
        string addressLine1,
        string city,
        string stateOrProvince,
        string postalCode,
        string country,
        string? addressLine2 = null)
    {
        ValidateAddress(firstName, lastName, email, phoneNumber, addressLine1, city, stateOrProvince, postalCode, country);

        return new Address(
            firstName,
            lastName,
            email,
            phoneNumber,
            addressLine1,
            addressLine2,
            city,
            stateOrProvince,
            postalCode,
            country);
    }

    /// <summary>
    /// Get full name.
    /// </summary>
    public string GetFullName() => $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// Get formatted address as multiline string.
    /// </summary>
    public string GetFormatted()
    {
        var lines = new List<string>
        {
            GetFullName(),
            AddressLine1
        };

        if (!string.IsNullOrEmpty(AddressLine2))
            lines.Add(AddressLine2);

        lines.Add($"{City}, {StateOrProvince} {PostalCode}");
        lines.Add(Country);

        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// Validate all address fields.
    /// </summary>
    private static void ValidateAddress(
        string firstName,
        string lastName,
        string email,
        string phoneNumber,
        string addressLine1,
        string city,
        string stateOrProvince,
        string postalCode,
        string country)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty", nameof(firstName));

        if (firstName.Length > 100)
            throw new ArgumentException("First name cannot exceed 100 characters", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be empty", nameof(lastName));

        if (lastName.Length > 100)
            throw new ArgumentException("Last name cannot exceed 100 characters", nameof(lastName));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        if (!email.Contains("@"))
            throw new ArgumentException("Email must be valid", nameof(email));

        if (email.Length > 255)
            throw new ArgumentException("Email cannot exceed 255 characters", nameof(email));

        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number cannot be empty", nameof(phoneNumber));

        if (phoneNumber.Length > 20)
            throw new ArgumentException("Phone number cannot exceed 20 characters", nameof(phoneNumber));

        if (string.IsNullOrWhiteSpace(addressLine1))
            throw new ArgumentException("Address line 1 cannot be empty", nameof(addressLine1));

        if (addressLine1.Length > 255)
            throw new ArgumentException("Address line 1 cannot exceed 255 characters", nameof(addressLine1));

        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City cannot be empty", nameof(city));

        if (city.Length > 100)
            throw new ArgumentException("City cannot exceed 100 characters", nameof(city));

        if (string.IsNullOrWhiteSpace(stateOrProvince))
            throw new ArgumentException("State or province cannot be empty", nameof(stateOrProvince));

        if (stateOrProvince.Length > 100)
            throw new ArgumentException("State or province cannot exceed 100 characters", nameof(stateOrProvince));

        if (string.IsNullOrWhiteSpace(postalCode))
            throw new ArgumentException("Postal code cannot be empty", nameof(postalCode));

        if (postalCode.Length > 20)
            throw new ArgumentException("Postal code cannot exceed 20 characters", nameof(postalCode));

        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country cannot be empty", nameof(country));

        if (country.Length > 100)
            throw new ArgumentException("Country cannot exceed 100 characters", nameof(country));
    }

    public bool Equals(Address? other) =>
        other is not null &&
        FirstName == other.FirstName &&
        LastName == other.LastName &&
        Email == other.Email &&
        PhoneNumber == other.PhoneNumber &&
        AddressLine1 == other.AddressLine1 &&
        AddressLine2 == other.AddressLine2 &&
        City == other.City &&
        StateOrProvince == other.StateOrProvince &&
        PostalCode == other.PostalCode &&
        Country == other.Country;

    public override bool Equals(object? obj) =>
        obj is Address address && Equals(address);

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(FirstName);
        hashCode.Add(LastName);
        hashCode.Add(Email);
        hashCode.Add(PhoneNumber);
        hashCode.Add(AddressLine1);
        hashCode.Add(AddressLine2);
        hashCode.Add(City);
        hashCode.Add(StateOrProvince);
        hashCode.Add(PostalCode);
        hashCode.Add(Country);
        return hashCode.ToHashCode();
    }

    public override string ToString() =>
        GetFormatted();
}
