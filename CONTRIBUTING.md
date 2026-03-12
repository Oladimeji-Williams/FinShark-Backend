# FinShark Contributing Guide

Guidelines for contributing to the FinShark project.

## Code of Conduct

- Be respectful and inclusive
- Focus on code quality
- Ask for help when needed
- Share knowledge and learn together

---

## Getting Started

### 1. Fork & Clone

```bash
# Fork on GitHub
# Clone your fork
git clone https://github.com/your-username/finshark-backend.git
cd finshark-backend

# Add upstream remote
git remote add upstream https://github.com/original-owner/finshark-backend.git
```

### 2. Create Feature Branch

```bash
# Update main branch
git fetch upstream
git checkout main
git merge upstream/main

# Create feature branch (from main)
git checkout -b feature/stock-dividends

# Branch naming conventions:
# - feature/description     - New features
# - bugfix/description      - Bug fixes
# - docs/description        - Documentation
# - refactor/description    - Code refactoring
```

### 3. Setup Local Environment

```bash
# Restore packages
dotnet restore

# Configure database
# Update appsettings.Development.json with your connection string

# Run migrations
cd src/FinShark.API
dotnet ef database update

# Run tests
cd ../..
dotnet test
```

---

## Development Workflow

### Writing Code

**Follow these standards:**

1. **C# Naming Conventions**
   - Classes: `PascalCase` → `CreateStockCommand`
   - Methods: `PascalCase` → `GetAllAsync()`
   - Properties: `PascalCase` → `CompanyName`
   - Private fields: `_camelCase` → `_logger`
   - Local variables: `camelCase` → `stockId`
   - Constants: `UPPER_SNAKE_CASE` → `MAX_PAGE_SIZE`

2. **Architecture Rules**
   ```
   Dependencies ONLY flow inward:
   
   API → Application → Domain
   API → Persistence
   Application → Domain
   Persistence → Domain
   
   Domain depends on NOTHING
   ```

3. **Code Style**
   ```csharp
   // ✅ Use required keyword for entity properties
   public required string Symbol { get; init; }
   
   // ✅ Use sealed classes
   public sealed class CreateStockCommand { }
   
   // ✅ Use init-only properties
   public string Name { get; init; }
   
   // ✅ Use records for immutable data
   public sealed record StockDto { }
   
   // ❌ Avoid nullable reference types annotation
   #nullable enable  // DON'T USE
   
   // ❌ Avoid mutable public properties
   public string Symbol { get; set; }  // DON'T USE
   ```

### Project Structure

Maintain this organization:

```
FinShark.Application/
├── Stocks/
│   ├── Commands/
│   │   ├── CreateStock/
│   │   │   ├── CreateStockCommand.cs
│   │   │   └── CreateStockCommandHandler.cs
│   │   └── UpdateStock/
│   ├── Queries/
│   │   ├── GetStocks/
│   │   └── GetStockById/
│   └── Validators/
│       ├── CreateStockValidator.cs
│       └── UpdateStockValidator.cs
├── Stocks/
├── Dtos/
└── ServiceCollectionExtensions.cs
```

**Rules**:
- One command per folder
- Handler in same folder as command
- Validators in separate folder
- DTOs in centralized folder

### Commits

Write clear, descriptive commit messages:

```bash
# ✅ Good
git commit -m "feat: add dividend support for stocks"
git commit -m "fix: resolve null reference in stock mapper"
git commit -m "docs: update API endpoints documentation"
git commit -m "refactor: simplify stock validation logic"

# ❌ Bad
git commit -m "update code"
git commit -m "fix bug"
git commit -m "changes"
```

**Format**:
```
<type>: <subject> (50 chars max)

<body> (optional, wrap at 72 chars)

<footer> (optional)

Types:
- feat: New feature
- fix: Bug fix
- docs: Documentation
- style: Formatting
- refactor: Code restructuring
- test: Test additions
- chore: Build, dependencies
```

---

## Testing Requirements

All code must include tests.

### Unit Tests

Test business logic in isolation:

```csharp
// FinShark.Tests/Unit/Stocks/Validators/CreateStockValidatorTests.cs
public class CreateStockValidatorTests
{
    private readonly CreateStockValidator _validator;

    public CreateStockValidatorTests()
    {
        var mockRepository = new Mock<IStockRepository>();
        mockRepository
            .Setup(r => r.SymbolExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _validator = new CreateStockValidator(mockRepository.Object);
    }

    [Fact]
    public async Task Validate_WithValidCommand_ReturnsSuccess()
    {
        // Arrange
        var command = new CreateStockCommand(
            Symbol: "AAPL",
            CompanyName: "Apple Inc.",
            CurrentPrice: 150m,
            Industry: Industry.Technology,
            MarketCap: 2500000000000m);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithEmptySymbol_ReturnsFail()
    {
        // Arrange
        var command = new CreateStockCommand(
            Symbol: "",
            CompanyName: "Apple Inc.",
            CurrentPrice: 150m,
            Industry: Industry.Technology,
            MarketCap: 2500000000000m);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.Symbol));
    }
}
```

**Test Coverage Goals**:
- Unit tests: >80% coverage
- Happy path tests
- Error condition tests
- Edge case tests

### Integration Tests

Test system components together:

```csharp
// FinShark.Tests/Integration/Stocks/StockRepositoryTests.cs
public class StockRepositoryTests
{
    private readonly AppDbContext _context;
    private readonly StockRepository _repository;

    public StockRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("TestDb" + Guid.NewGuid())
            .Options;

        _context = new AppDbContext(options);
        _repository = new StockRepository(_context);
    }

    [Fact]
    public async Task AddAsync_WithValidStock_SavesSuccessfully()
    {
        // Arrange
        var stock = new Stock("AAPL", "Apple Inc.", 150m, Industry.Technology, 2500000000000m);

        // Act
        await _repository.AddAsync(stock);

        // Assert
        var savedStock = await _repository.GetByIdAsync(stock.Id);
        Assert.NotNull(savedStock);
        Assert.Equal("AAPL", savedStock.Symbol);
    }
}
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test src/FinShark.Tests

# Run specific test class
dotnet test --filter ClassName=StockRepositoryTests

# Run with coverage
dotnet test /p:CollectCoverage=true
```

---

## Pull Request Process

### 1. Before Submitting

- [ ] Tests pass: `dotnet test`
- [ ] Code builds: `dotnet build`
- [ ] No compiler warnings
- [ ] Code follows conventions
- [ ] Documentation updated
- [ ] Commit messages clear
- [ ] Branch is up to date with main

### 2. Create Pull Request

```bash
# Push feature branch
git push origin feature/stock-dividends

# Create PR on GitHub
# Fill in PR template (see below)
```

### 3. PR Template

```markdown
## Description
Brief description of changes.

## Type of Change
- [ ] New feature
- [ ] Bug fix
- [ ] Documentation update
- [ ] Breaking change

## Related Issue
Closes #123

## Testing
- [ ] Unit tests added
- [ ] Integration tests added
- [ ] All tests pass
- [ ] Manual testing done

## Checklist
- [ ] Code follows project style
- [ ] No new warnings
- [ ] Documentation updated
- [ ] Tests added/updated
- [ ] No breaking changes (unless necessary)

## Screenshots (if applicable)
Include API response examples or UI changes.
```

### 4. Review Process

- Code review by maintainers
- Address feedback
- Keep branch up to date
- Wait for approval
- Squash commits before merging

---

## Adding New Features

### Example: Add Dividend Support

**Step 1**: Create Domain Entity

```csharp
// FinShark.Domain/Entities/Dividend.cs
public sealed class Dividend : BaseEntity
{
    public required int StockId { get; set; }
    public required decimal Amount { get; set; }
    public required DateTime PaymentDate { get; set; }
    
    // ... rest of implementation
}
```

**Step 2**: Create Queries/Commands

```csharp
// FinShark.Application/Stocks/Commands/CreateDividend/
public sealed record CreateDividendCommand(
    int StockId,
    decimal Amount,
    DateTime PaymentDate) : IRequest<int>;

public sealed class CreateDividendCommandHandler : IRequestHandler<CreateDividendCommand, int>
{
    // ... implementation
}
```

**Step 3**: Create Validators

```csharp
// FinShark.Application/Stocks/Validators/CreateDividendValidator.cs
public sealed class CreateDividendValidator : AbstractValidator<CreateDividendCommand>
{
    // ... validation rules
}
```

**Step 4**: Create DTOs

```csharp
// FinShark.Application/Dtos/DividendDto.cs
public sealed record DividendDto
{
    public required int Id { get; init; }
    // ... other properties
}
```

**Step 5**: Create Tests

```csharp
// Unit tests for validator
// Integration tests for handler
// Repository tests
```

**Step 6**: Add Endpoint

```csharp
// FinShark.API/Controllers/StocksController.cs
[HttpPost("{stockId}/dividends")]
public async Task<ActionResult<ApiResponse<int>>> CreateDividend(
    int stockId,
    [FromBody] CreateDividendRequestDto request)
{
    var command = new CreateDividendCommand(stockId, request.Amount, request.PaymentDate);
    var result = await _mediator.Send(command);
    return CreatedAtAction(nameof(GetDividend), new { stockId, dividendId = result });
}
```

**Step 7**: Create Migration

```bash
dotnet ef migrations add AddDividendSupport -p src/FinShark.Persistence
```

**Step 8**: Update Documentation

- Update [API_ENDPOINTS.md](API_ENDPOINTS.md)
- Update [DATABASE_SCHEMA.md](DATABASE_SCHEMA.md)
- Add implementation examples

---

## Code Review Checklist

When reviewing code:

- [ ] Code follows project conventions
- [ ] Architecture principles maintained
- [ ] Tests are adequate
- [ ] Error handling proper
- [ ] No security vulnerabilities
- [ ] Performance acceptable
- [ ] Documentation clear
- [ ] No breaking changes (if minor)

---

## Release Process

### Version Numbers

Follow Semantic Versioning: MAJOR.MINOR.PATCH

- **MAJOR**: Breaking changes
- **MINOR**: New features (backwards compatible)
- **PATCH**: Bug fixes

### Release Checklist

```bash
# 1. Update version numbers
# src/FinShark.API/FinShark.API.csproj
# <Version>1.1.0</Version>

# 2. Update CHANGELOG.md
# - New features
# - Bug fixes
# - Breaking changes

# 3. Create release tag
git tag -a v1.1.0 -m "Release version 1.1.0"
git push origin v1.1.0

# 4. Build release
dotnet publish -c Release

# 5. Create GitHub Release
# Upload artifacts, add release notes
```

---

## Getting Help

- **Questions**: Open a discussion on GitHub
- **Issues**: Create an issue with details
- **Documentation**: Check existing docs
- **Code Examples**: Look at tests

---

## Style Guide Quick Reference

| Element | Style | Example |
|---------|-------|---------|
| Namespace | PascalCase | `FinShark.Application.Stocks.Commands` |
| Class | PascalCase | `CreateStockCommand` |
| Method | PascalCase | `GetAllAsync()` |
| Property | PascalCase | `CompanyName` |
| Private field | _camelCase | `_logger` |
| Local variable | camelCase | `stockId` |
| Constant | UPPER_SNAKE | `MAX_PRICE` |
| Interface | IPascalCase | `IStockRepository` |
| Enum | PascalCase | `Industry.Technology` |

---

## Resources

- **Architecture**: See [ARCHITECTURE.md](ARCHITECTURE.md)
- **Testing**: See [TESTING.md](TESTING.md)
- **API Design**: See [API_ENDPOINTS.md](API_ENDPOINTS.md)
- **Validation**: See [VALIDATION_RULES.md](VALIDATION_RULES.md)
- **Database**: See [DATABASE_SCHEMA.md](DATABASE_SCHEMA.md)

---

## Thank You

Thank you for contributing to FinShark! Your efforts help make this project better for everyone.
