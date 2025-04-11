using CourseraLens.Attributes;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CourseraLens.Swagger;

public class SortColumnFilter : IParameterFilter
{
    public void Apply(OpenApiParameter parameter,
        ParameterFilterContext context)
    {
        // ATTENTION: This is a temporary placeholder filter
        // Currently NON-FUNCTIONAL for SortColumn validation because:
        // 1. [SortColumnValidator] isn't applied to properties (uses IValidatableObject instead)
        // 2. The required EntityType information isn't available at Swagger generation time

        // Standard pattern for checking parameter-level attributes
        var paramAttributes = context.ParameterInfo
            .GetCustomAttributes(true)
            .OfType<SortColumnValidatorAttribute>(); // Will always be empty

        // Standard pattern for checking property-level attributes (in DTOs)
        var propAttributes = context.ParameterInfo.ParameterType
            .GetProperties()
            .Where(p =>
                p.Name.Equals(parameter.Name,
                    StringComparison.OrdinalIgnoreCase))
            .SelectMany(p => p.GetCustomAttributes(true))
            .OfType<SortColumnValidatorAttribute>(); // Will always be empty

        // This union exists solely to maintain structural parity with SortOrderFilter
        var attributes = paramAttributes.Union(propAttributes);

        // NOTE: This block will NEVER execute currently
        if (attributes.Any())
            // Reserved space for future implementation
            parameter.Description = "Sort column for the target entity (WIP)";

        // TEMPORARY WORKAROUND: 
        // For now, this filter exists only to maintain code consistency
        // Actual validation happens in RequestDto<T>.Validate()
    }
}