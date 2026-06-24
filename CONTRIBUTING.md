# Contributing Guidelines

## Getting Started

1. Fork the repository
2. Clone your fork: `git clone https://github.com/YOUR-USERNAME/banking-api.git`
3. Create a feature branch: `git checkout -b feature/your-feature-name`
4. Make your changes
5. Run tests: `dotnet test`
6. Commit with clear messages: `git commit -m "Add your commit message"`
7. Push to your fork: `git push origin feature/your-feature-name`
8. Create a Pull Request

## Code Style

- Follow Microsoft C# Coding Conventions
- Use meaningful variable and method names
- Add XML documentation comments for public members
- Keep methods small and focused
- Use async/await for I/O operations

### Example:
```csharp
/// <summary>
/// Validates a transaction request
/// </summary>
/// <param name="request">The validation request</param>
/// <returns>The validation response</returns>
public async Task<ValidationResponse> ValidateAsync(ValidationRequest request)
{
    // Implementation
}
```

## Testing Requirements

- Write unit tests for all new features
- Maintain test coverage above 80%
- Test both success and failure scenarios
- Use descriptive test names

### Test Template:
```csharp
[Fact]
public void FeatureName_WhenCondition_ExpectedOutcome()
{
    // Arrange
    var input = new TestData();

    // Act
    var result = service.Method(input);

    // Assert
    Assert.NotNull(result);
}
```

## Commit Messages

- Use clear, descriptive messages
- Start with a verb: "Add", "Fix", "Update", "Remove"
- Keep first line under 50 characters
- Add detailed description if needed

Examples:
```
Add duplicate transaction detection
Fix validation error message formatting
Update documentation for encryption endpoints
Remove legacy code
```

## Pull Request Process

1. Update documentation if needed
2. Add tests for new functionality
3. Ensure all tests pass: `dotnet test`
4. Build in Release mode: `dotnet build -c Release`
5. Provide clear PR description
6. Reference related issues
7. Wait for review and approval

## Bug Reports

Include:
- Description of the bug
- Steps to reproduce
- Expected behavior
- Actual behavior
- .NET version and OS
- Error messages/logs

## Feature Requests

Include:
- Clear description of the feature
- Use case and motivation
- Proposed API/design
- Examples or wireframes

## Code Review

All code must be reviewed before merging. We look for:
- Functionality correctness
- Code quality and style
- Test coverage
- Documentation
- Performance impact
- Security considerations

## Documentation

Update the following when needed:
- README.md
- API_DOCUMENTATION.md
- SECURITY_COMPLIANCE.md
- PRODUCTION_DEPLOYMENT.md
- Code comments and XML docs

## Release Process

1. Update version in csproj
2. Update CHANGELOG.md
3. Create release tag
4. Build and publish Docker image
5. Create GitHub release

---

Thank you for contributing! 🙏
