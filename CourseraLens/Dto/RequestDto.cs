using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using CourseraLens.Attributes;
using DefaultValueAttribute = System.ComponentModel.DefaultValueAttribute;
    
namespace CourseraLens.DTO;

public class RequestDto<T> : IValidatableObject
{
    [DefaultValue(0)] public int PageIndex { get; set; } = 0;

    [DefaultValue(10)] [Range(1, 100)] public int PageSize { get; set; } = 10;

    [DefaultValue(null)]
    public string? SortColumn { get; set; } = GetDefaultSortColumn<T>();

    [DefaultValue("ASC")]
    [SortOrderValidator]
    public string? SortOrder { get; set; } = "ASC";

    [DefaultValue(null)] public string? FilterQuery { get; set; } = null;

    // This is used to overcome the limitation of generic validation regarding attributes
    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        var validator = new SortColumnValidatorAttribute(typeof(T));
        var result = validator
            .GetValidationResult(SortColumn, validationContext);
        return result != null
            ? new[] { result }
            : new ValidationResult[0];
    }

    // This method is used to get the default sort column dynamically
    private static string? GetDefaultSortColumn<T>()
    {
        var properties = typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            // All fields being sortable is implicit, hence picking the first string or int property as a default value is reasonable
            .Where(p =>
                p.PropertyType == typeof(string) ||
                p.PropertyType == typeof(int))
            .ToList();

        return properties.FirstOrDefault()?.Name;
    }
}