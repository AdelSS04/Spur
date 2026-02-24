using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;

namespace Spur.EntityFrameworkCore.Tests;

// ── Test DbContext ───────────────────────────────────────────────────────────

public class TestEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
    public DbSet<TestEntity> TestEntities => Set<TestEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TestEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
        });
    }
}

// ── QueryableExtensions Tests ────────────────────────────────────────────────

public class QueryableExtensionsTests : IDisposable
{
    private readonly TestDbContext _context;

    public QueryableExtensionsTests()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new TestDbContext(options);

        _context.TestEntities.AddRange(
            new TestEntity { Id = 1, Name = "Alice", Email = "alice@test.com" },
            new TestEntity { Id = 2, Name = "Bob", Email = "bob@test.com" },
            new TestEntity { Id = 3, Name = "Charlie", Email = "charlie@test.com" });
        _context.SaveChanges();
    }

    public void Dispose() => _context.Dispose();

    // ── FirstOrResultAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task FirstOrResultAsync_Found_ShouldReturnSuccess()
    {
        var result = await _context.TestEntities.FirstOrResultAsync();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task FirstOrResultAsync_NotFound_ShouldReturnNotFoundError()
    {
        var result = await _context.TestEntities
            .Where(e => e.Name == "Nobody")
            .FirstOrResultAsync();
        result.IsFailure.Should().BeTrue();
        result.Error.Category.Should().Be(ErrorCategory.NotFound);
    }

    [Fact]
    public async Task FirstOrResultAsync_NotFound_WithCustomError()
    {
        var customError = Error.NotFound("Custom not found", "CUSTOM_NF");
        var result = await _context.TestEntities
            .Where(e => e.Name == "Nobody")
            .FirstOrResultAsync(customError);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CUSTOM_NF");
    }

    [Fact]
    public async Task FirstOrResultAsync_WithPredicate_Found()
    {
        var result = await _context.TestEntities
            .FirstOrResultAsync(e => e.Name == "Alice");
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Alice");
    }

    [Fact]
    public async Task FirstOrResultAsync_WithPredicate_NotFound()
    {
        var result = await _context.TestEntities
            .FirstOrResultAsync(e => e.Name == "Nobody");
        result.IsFailure.Should().BeTrue();
        result.Error.Category.Should().Be(ErrorCategory.NotFound);
    }

    // ── SingleOrResultAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task SingleOrResultAsync_Found_ShouldReturnSuccess()
    {
        var result = await _context.TestEntities
            .Where(e => e.Name == "Alice")
            .SingleOrResultAsync();
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Alice");
    }

    [Fact]
    public async Task SingleOrResultAsync_NotFound_ShouldReturnNotFoundError()
    {
        var result = await _context.TestEntities
            .Where(e => e.Name == "Nobody")
            .SingleOrResultAsync();
        result.IsFailure.Should().BeTrue();
        result.Error.Category.Should().Be(ErrorCategory.NotFound);
    }

    [Fact]
    public async Task SingleOrResultAsync_Multiple_ShouldReturnUnexpectedError()
    {
        // Add a duplicate
        _context.TestEntities.Add(new TestEntity { Id = 10, Name = "Alice", Email = "alice2@test.com" });
        await _context.SaveChangesAsync();

        var result = await _context.TestEntities
            .Where(e => e.Name == "Alice")
            .SingleOrResultAsync();
        result.IsFailure.Should().BeTrue();
        result.Error.Category.Should().Be(ErrorCategory.Unexpected);
        result.Error.Code.Should().Be("MULTIPLE_RESULTS_ERROR");
    }

    [Fact]
    public async Task SingleOrResultAsync_WithPredicate_Found()
    {
        var result = await _context.TestEntities
            .SingleOrResultAsync(e => e.Name == "Bob");
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Bob");
    }

    [Fact]
    public async Task SingleOrResultAsync_WithPredicate_NotFound()
    {
        var result = await _context.TestEntities
            .SingleOrResultAsync(e => e.Name == "Nobody");
        result.IsFailure.Should().BeTrue();
        result.Error.Category.Should().Be(ErrorCategory.NotFound);
    }

    [Fact]
    public async Task SingleOrResultAsync_WithPredicate_Multiple_ShouldReturnUnexpected()
    {
        _context.TestEntities.Add(new TestEntity { Id = 11, Name = "Bob", Email = "bob2@test.com" });
        await _context.SaveChangesAsync();

        var result = await _context.TestEntities
            .SingleOrResultAsync(e => e.Name == "Bob");
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("MULTIPLE_RESULTS_ERROR");
    }

    // ── ExistsOrResultAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task ExistsOrResultAsync_Exists_ShouldReturnSuccess()
    {
        var result = await _context.TestEntities.ExistsOrResultAsync();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(Unit.Value);
    }

    [Fact]
    public async Task ExistsOrResultAsync_NotExists_ShouldReturnNotFound()
    {
        var result = await _context.TestEntities
            .Where(e => e.Name == "Nobody")
            .ExistsOrResultAsync();
        result.IsFailure.Should().BeTrue();
        result.Error.Category.Should().Be(ErrorCategory.NotFound);
    }

    [Fact]
    public async Task ExistsOrResultAsync_WithPredicate_Exists()
    {
        var result = await _context.TestEntities
            .ExistsOrResultAsync(e => e.Name == "Alice");
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsOrResultAsync_WithPredicate_NotExists()
    {
        var result = await _context.TestEntities
            .ExistsOrResultAsync(e => e.Name == "Nobody");
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsOrResultAsync_NotExists_WithCustomError()
    {
        var customError = Error.NotFound("Custom", "CUSTOM");
        var result = await _context.TestEntities
            .Where(e => e.Name == "Nobody")
            .ExistsOrResultAsync(customError);
        result.Error.Code.Should().Be("CUSTOM");
    }

    // ── ToResultListAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task ToResultListAsync_WithData_ShouldReturnSuccessWithList()
    {
        var result = await _context.TestEntities.ToResultListAsync();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
    }

    [Fact]
    public async Task ToResultListAsync_Empty_ShouldStillReturnSuccess()
    {
        var result = await _context.TestEntities
            .Where(e => e.Name == "Nobody")
            .ToResultListAsync();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    // ── ToResultListOrFailAsync ──────────────────────────────────────────────

    [Fact]
    public async Task ToResultListOrFailAsync_WithData_ShouldReturnSuccess()
    {
        var result = await _context.TestEntities.ToResultListOrFailAsync();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
    }

    [Fact]
    public async Task ToResultListOrFailAsync_Empty_ShouldReturnNotFound()
    {
        var result = await _context.TestEntities
            .Where(e => e.Name == "Nobody")
            .ToResultListOrFailAsync();
        result.IsFailure.Should().BeTrue();
        result.Error.Category.Should().Be(ErrorCategory.NotFound);
    }

    [Fact]
    public async Task ToResultListOrFailAsync_Empty_WithCustomError()
    {
        var customError = Error.NotFound("No items", "NO_ITEMS");
        var result = await _context.TestEntities
            .Where(e => e.Name == "Nobody")
            .ToResultListOrFailAsync(customError);
        result.Error.Code.Should().Be("NO_ITEMS");
    }
}

// ── DbContextExtensions Tests ────────────────────────────────────────────────

public class DbContextExtensionsTests : IDisposable
{
    private readonly TestDbContext _context;

    public DbContextExtensionsTests()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new TestDbContext(options);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task SaveChangesResultAsync_Success_ShouldReturnCount()
    {
        _context.TestEntities.Add(new TestEntity { Id = 1, Name = "Test", Email = "test@test.com" });
        var result = await _context.SaveChangesResultAsync();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task SaveChangesResultAsync_NoChanges_ShouldReturnZero()
    {
        var result = await _context.SaveChangesResultAsync();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0);
    }

    [Fact]
    public async Task SaveChangesResultAsync_AsUnit_ShouldReturnUnitOnSuccess()
    {
        _context.TestEntities.Add(new TestEntity { Id = 1, Name = "Test", Email = "test@test.com" });
        var result = await _context.SaveChangesResultAsync(acceptAllChangesOnSuccess: true);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(Unit.Value);
    }
}
