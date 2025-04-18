using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace CourseraLens.Utils;

public class EnrolledConverter : ITypeConverter
{
    public object? ConvertFromString(string? text, IReaderRow row,
        MemberMapData memberMapData)
    {
        if (string.IsNullOrEmpty(text))
            return null;

        text = text.Trim().ToLowerInvariant();

        if (text.EndsWith("k"))
        {
            var value = text[..^1];
            if (double.TryParse(value, NumberStyles.Any,
                    CultureInfo.InvariantCulture, out var result))
                return (int)(result * 1000);
        }
        else if (text.EndsWith("m"))
        {
            var value = text[..^1];
            if (double.TryParse(value, NumberStyles.Any,
                    CultureInfo.InvariantCulture, out var result))
                return (int)(result * 1000000);
        }
        else if (text.EndsWith("b"))
        {
            var value = text[..^1];
            if (double.TryParse(value, NumberStyles.Any,
                    CultureInfo.InvariantCulture, out var result))
                return (int)(result * 1000000000);
        }

        // If it's not in the "k", "m" or "b" format, just try to parse it as a normal integer
        if (int.TryParse(text, out var intValue)) return intValue;

        return null;
    }

    public string? ConvertToString(object? value, IWriterRow row,
        MemberMapData memberMapData)
    {
        return value?.ToString();
    }
}