# C# .NET Coding Standards — POCFlexora

## Framework
- ASP.NET Core Web API
- .NET 7 or 8
- Controllers inherit from ControllerBase

## Naming Conventions
- Controllers: PascalCase, suffix `Controller`
- Models: PascalCase, in `/Models` folder
- Methods: PascalCase
- Variables: camelCase

## Controller Pattern
```csharp
[ApiController]
[Route("api/[controller]")]
public class SampleController : ControllerBase
{
    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        // logic here
        return Ok(result);
    }
}
```

## Error Handling
- Return `NotFound()` for missing resources
- Return `BadRequest()` for invalid input
- Wrap logic in try/catch, return `StatusCode(500)` on exceptions

## Testing
- Framework: xUnit
- Naming: `MethodName_Scenario_ExpectedResult`
- Use Moq for mocking dependencies