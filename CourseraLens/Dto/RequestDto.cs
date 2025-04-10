using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using CourseraLens.Attributes;

namespace CourseraLens.DTO;

public class RequestDto<T> : IValidatableObject
{
    [DefaultValue(0)] 
    public int PageIndex { get; set; } = 0;
    
    [DefaultValue(10)] 
    [Range(1, 100)] 
    public int PageSize { get; set; } = 10;
    
    [DefaultValue("Title")] 
    public string? SortColumn { get; set; } = "Title";
    
    [DefaultValue("ASC")] 
    [SortOrderValidator] 
    public string? SortOrder { get; set; } = "ASC";
    
    [DefaultValue(null)]
    public string? FilterQuery { get; set; } = null;
    
    // This is used to overcome the limitation of generic validation regarding attributes
    public IEnumerable<ValidationResult> Validate( 
    ValidationContext validationContext)
    {
        var validator = new SortColumnValidatorAttribute(typeof(T));
        var result = validator
            .GetValidationResult(SortColumn, validationContext);
        return (result != null)
            ? new [] { result }
            : new ValidationResult[0];
    }
}