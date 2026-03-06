# Route Entity Model Binder 🚀

A Laravel-inspired Route Model Binder for ASP.NET Core. Automatically resolves database entities from route parameters, eliminating repetitive lookup code and streamlining your API controllers.

## Why EntityModelBinder?

In RESTful APIs, you often need to fetch database entities based on route parameters. Instead of writing repetitive lookup code in every endpoint, EntityModelBinder automatically resolves the entity for you—exactly like Laravel's route-model binding.

## Installation

```bash
dotnet add package EntityModelBinder
```

## Quick Start

### 1. Implement `IEntity` on your database model

```csharp
using EntityModelBinder;

public class Product : IEntity
{
    public int Id { get; set; }
    public string Slug { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}
```

### 2. Register the service in your `Program.cs`

```csharp
builder.Services.AddEntityModelBinder<SomePostgresContext>(options =>
{
    options.EnableSwaggerSchemaIds = true;
    options.SuppressModelStateInvalidFilter = true;
});
```

### 3. Use in your API controllers

```csharp
[HttpGet("products/{product}")]
public IActionResult Get([FromRoute] Product product)
{
    if (product == null) return NotFound();
    return Ok(product);
}
```

That's it! The model is automatically fetched from the database. ✌️

## Advanced Usage

### Using Custom Column Keys with `[EntityKey]`

By default, the binder searches by `Id`. Use the `[EntityKey]` attribute to bind by any column:

```csharp
[HttpGet("products/{product}")]
public IActionResult Get([FromRoute, EntityKey("Slug")] Product product)
{
    if (product == null) return NotFound();
    return Ok(product);
}
```

Supported types: `int`, `Guid`, `string` (slugs).

### Real-World Example

**Before** (without EntityModelBinder):

```csharp
[HttpGet("{surveyId:int}")]
public async Task<IActionResult> Show([FromRoute] int surveyId)
{
    var survey = await _dbContext.Survey.FindAsync(surveyId);
    return Ok(survey);
}

[HttpPatch("{surveyId:int}")]
public async Task<IActionResult> Update([FromRoute] int surveyId, [FromBody] SurveyRequest request)
{
    var survey = await _dbContext.Survey.FindAsync(surveyId);
    survey.Name = request.name;
    survey.Json = request.jsonData;
    survey.UpdatedAt = DateTime.UtcNow;
    await _dbContext.SaveChangesAsync();
    return Ok(new { id = survey.Id });
}
```

**After** (with EntityModelBinder):

```csharp
[HttpGet("{survey}")]
public IActionResult Show([FromRoute] Survey survey)
{
    if (survey == null) return NotFound();
    return Ok(survey);
}
```

or in some cases

```C#
[HttpGet("{survey}")]
public IActionResult Show([FromRoute] Survey survey)
    => Ok(survey);
```

**Adding a new resource**

```C#
[HttpPost]
public async Task<IActionResult> Store([FromBody] SurveyRequest request)
{
    var entity = new Survey
    {
        Name = request.name,
        Json = request.jsonData,
        StructureId = _identityProvider.CurrentStructureId,
        CreatedAt = DateTime.UtcNow
    };
    _dbContext.Surveys.Add(entity);
    await _dbContext.SaveChangesAsync();
    return CreatedAtAction(nameof(Show), new { survey = entity.Id }, entity);
}

[HttpPatch("{survey}")]
public async Task<IActionResult> Update([FromRoute] Survey survey, [FromBody] SurveyRequest request)
{
    if (survey == null) return NotFound();

    survey.Name = request.name;
    survey.Json = request.jsonData;
    survey.UpdatedAt = DateTime.UtcNow;
    await _dbContext.SaveChangesAsync();
    return Ok(survey);
}
```

## Configuration

### Swagger Support

If you're using Swagger/OpenAPI and encounter schema conflicts, enable the option:

```csharp
builder.Services.AddEntityModelBinder(options =>
{
    options.EnableSwaggerSchemaIds = true;
});
```

Then in your Swagger configuration:

```csharp
c.CustomSchemaIds(type => type.ToString());
```

## How It Works

1. When a request hits a route like `/products/my-product-slug`, the binder intercepts the parameter
2. It checks for `[EntityKey("ColumnName")]` on the action parameter
3. If found, uses that column; otherwise defaults to `Id`
4. Determines the column type (`int`, `Guid`, or `string`)
5. Queries the database using Entity Framework Core
6. Returns the entity directly to your controller method
7. ~Returns `404 Not Found` if the entity doesn't exist~

## Requirements

- .NET 8.0+
- ASP.NET Core
- Entity Framework Core 8.0+

## License

MIT License - see [LICENSE](LICENSE) file for details.
