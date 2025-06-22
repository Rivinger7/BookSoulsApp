using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BookSoulsApp.Domain.Enums.SchemaFilter;

public class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type.IsEnum)
        {
            schema.Type = "string";
            schema.Enum = context.Type
                .GetEnumNames()
                .Select(name => (IOpenApiAny)new OpenApiString(name))
                .ToList();
        }
    }
}
