namespace BeanShare.Application.Constants;

// Cache settings for various services
public static class CacheSettings
{
    public const int UserCacheDurationMinutes = 5;
    public const int ExchangeRateCacheDurationMinutes = 10;
    public const int ExchangeRateMaxStalenessHours = 24;
    public const int ExchangeRateUpdateIntervalHours = 24;
}

// Auth token configuration
public static class AuthenticationSettings
{
    public const int TokenClockSkewMinutes = 5;
    public const int AccessTokenExpirationMinutes = 60;
    public const int RefreshTokenExpirationMinutes = 1440;
}

public static class PdfLayoutConstants
{
    public const int PageMargin = 40;
    public const int HeaderTitleFontSize = 24;
    public const int HeaderSubtitleFontSize = 16;
    public const int SpaceNameFontSize = 14;
    public const int SectionTitleFontSize = 11;
    public const int SummaryValueFontSize = 18;
    public const int BodyFontSize = 10;
    public const int LabelFontSize = 9;
    public const int SmallFontSize = 8;
    public const int TableCellPadding = 5;
    public const int SectionPadding = 15;
    public const int SectionSpacing = 10;
    public const int SmallSpacing = 5;
    public const int BorderWidth = 1;
}

public static class ExcelStyleConstants
{
    public const int TitleFontSize = 18;
    public const int SectionHeaderFontSize = 14;
    public const int HeaderFontSize = 12;
    public const int BodyFontSize = 10;
    public const int StandardColumnWidth = 20;
    public const int WideColumnWidth = 30;

    public const string PrimaryColor = "#5D4037";
    public const string LightBackgroundColor = "#EFEBE9";
    public const string AlternateRowColor = "#FAFAFA";
    public const string PaidRowColor = "#E8F5E9";
    public const string TotalRowColor = "#D7CCC8";
    public const string CurrencyFormat = "#,##0.00";
}

public static class BusinessRuleConstants
{
    public const int MaxSpaceMembers = 50;
    public const decimal MinConsumptionGrams = 0.1m;
    public const decimal MaxConsumptionGrams = 1000m;
    public const decimal DefaultPresetGrams = 18m; // Standard espresso dose
}
