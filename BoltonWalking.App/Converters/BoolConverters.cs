using System.Globalization;
using BoltonWalking.App.Models;

namespace BoltonWalking.App.Converters;

/// <summary>True if the bound string is non-empty - used to hide labels/images with no content.</summary>
public class StringToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => !string.IsNullOrWhiteSpace(value as string);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>True if the bound int is greater than zero - used to hide a "Downloads" heading when there are none.</summary>
public class IntToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is int i && i > 0;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>True if the bound int is zero - used to show an empty-state message.</summary>
public class IntToInverseBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is int i && i == 0;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>Maps a route's difficulty tier to a badge colour - green/orange/red, matching the map pins.</summary>
public class DifficultyToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value switch
        {
            RouteDifficulty.Easy => Color.FromArgb("#2E8B3D"),
            RouteDifficulty.Moderate => Color.FromArgb("#E8850C"),
            RouteDifficulty.Hard => Color.FromArgb("#D0202B"),
            _ => Colors.Gray
        };

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
