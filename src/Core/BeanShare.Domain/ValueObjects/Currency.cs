namespace BeanShare.Domain.ValueObjects;

/// <summary>
/// Represents a currency using ISO 4217 currency codes
/// </summary>
public sealed record Currency
{
    private static readonly HashSet<string> ValidCurrencies = new()
    {
        "USD", "EUR", "GBP", "JPY", "CHF", "CAD", "AUD", "NZD", "SEK", "NOK", "DKK",
        "CZK", "PLN", "HUF", "RON", "BGN", "HRK", "RUB", "TRY", "CNY", "INR", "KRW",
        "SGD", "HKD", "TWD", "THB", "IDR", "MYR", "PHP", "MXN", "BRL", "ARS", "CLP",
        "PEN", "COP", "ZAR", "EGP", "ILS", "AED", "SAR", "QAR", "KWD", "BHD", "OMR"
    };

    public string Code { get; }
    public string Symbol { get; }
    public string Name { get; }
    public int DecimalPlaces { get; }

    private Currency(string code)
    {
        Code = code.ToUpperInvariant();

        (Symbol, Name, DecimalPlaces) = Code switch
        {
            "USD" => ("$", "US Dollar", 2),
            "EUR" => ("€", "Euro", 2),
            "GBP" => ("£", "British Pound", 2),
            "JPY" => ("¥", "Japanese Yen", 0),
            "CHF" => ("Fr", "Swiss Franc", 2),
            "CAD" => ("C$", "Canadian Dollar", 2),
            "AUD" => ("A$", "Australian Dollar", 2),
            "NZD" => ("NZ$", "New Zealand Dollar", 2),
            "SEK" => ("kr", "Swedish Krona", 2),
            "NOK" => ("kr", "Norwegian Krone", 2),
            "DKK" => ("kr", "Danish Krone", 2),
            "CZK" => ("Kč", "Czech Koruna", 2),
            "PLN" => ("zł", "Polish Zloty", 2),
            "HUF" => ("Ft", "Hungarian Forint", 0),
            "RON" => ("lei", "Romanian Leu", 2),
            "BGN" => ("лв", "Bulgarian Lev", 2),
            "HRK" => ("kn", "Croatian Kuna", 2),
            "RUB" => ("₽", "Russian Ruble", 2),
            "TRY" => ("₺", "Turkish Lira", 2),
            "CNY" => ("¥", "Chinese Yuan", 2),
            "INR" => ("₹", "Indian Rupee", 2),
            "KRW" => ("₩", "South Korean Won", 0),
            "SGD" => ("S$", "Singapore Dollar", 2),
            "HKD" => ("HK$", "Hong Kong Dollar", 2),
            "TWD" => ("NT$", "Taiwan Dollar", 0),
            "THB" => ("฿", "Thai Baht", 2),
            "IDR" => ("Rp", "Indonesian Rupiah", 0),
            "MYR" => ("RM", "Malaysian Ringgit", 2),
            "PHP" => ("₱", "Philippine Peso", 2),
            "MXN" => ("$", "Mexican Peso", 2),
            "BRL" => ("R$", "Brazilian Real", 2),
            "ARS" => ("$", "Argentine Peso", 2),
            "CLP" => ("$", "Chilean Peso", 0),
            "PEN" => ("S/", "Peruvian Sol", 2),
            "COP" => ("$", "Colombian Peso", 0),
            "ZAR" => ("R", "South African Rand", 2),
            "EGP" => ("£", "Egyptian Pound", 2),
            "ILS" => ("₪", "Israeli Shekel", 2),
            "AED" => ("د.إ", "UAE Dirham", 2),
            "SAR" => ("﷼", "Saudi Riyal", 2),
            "QAR" => ("﷼", "Qatari Riyal", 2),
            "KWD" => ("د.ك", "Kuwaiti Dinar", 3),
            "BHD" => ("د.ب", "Bahraini Dinar", 3),
            "OMR" => ("﷼", "Omani Rial", 3),
            _ => (Code, Code, 2)
        };
    }

    public static Currency Create(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Currency code cannot be empty");

        if (code.Length != 3)
            throw new ArgumentException("Currency code must be exactly 3 characters (ISO 4217)");

        var upperCode = code.ToUpperInvariant();

        if (!ValidCurrencies.Contains(upperCode))
            throw new ArgumentException($"'{upperCode}' is not a valid ISO 4217 currency code");

        return new Currency(upperCode);
    }

    /// <summary>
    /// Common currencies for easy access
    /// </summary>
    public static Currency USD => new("USD");
    public static Currency EUR => new("EUR");
    public static Currency GBP => new("GBP");
    public static Currency CZK => new("CZK");
    public static Currency JPY => new("JPY");

    public string FormatAmount(decimal amount)
    {
        var format = DecimalPlaces > 0 ? $"N{DecimalPlaces}" : "N0";
        return $"{Symbol}{amount.ToString(format)}";
    }

    public override string ToString() => Code;

    public static implicit operator Currency(string code) => Create(code);

    public static implicit operator string(Currency currency) => currency.Code;
}