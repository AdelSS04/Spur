using Xunit;
using FluentAssertions;
using Spur.Pipeline;
using Spur.Tests.Helpers;

namespace Spur.Tests.Pipeline;

public class MapExtensionsTests
{
    [Fact]
    public void Map_OnSuccess_ShouldTransformValue()
    {
        var result = Result.Success(42).Map(x => x.ToString());
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("42");
    }

    [Fact]
    public void Map_OnFailure_ShouldPropagateError()
    {
        var result = Result.Failure<int>(TestData.Errors.NotFound).Map(x => x.ToString());
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TestData.Errors.NotFound);
    }

    [Fact]
    public void Map_NullFunc_ShouldThrowArgumentNullException()
    {
        var result = Result.Success(1);
        var act = () => result.Map<int, string>(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Map_Chain_ShouldApplyAll()
    {
        var result = Result.Success(5)
            .Map(x => x * 2)
            .Map(x => x + 10)
            .Map(x => x.ToString());
        result.Value.Should().Be("20");
    }

    [Fact]
    public void Map_WithComplexType_ShouldWork()
    {
        var result = Result.Success(TestData.SampleUser).Map(u => u.Email);
        result.Value.Should().Be("test@example.com");
    }

    [Fact]
    public void Map_ToSameType_ShouldWork()
    {
        var result = Result.Success(5).Map(x => x * 3);
        result.Value.Should().Be(15);
    }

    // Async: Result<T> + async transform
    [Fact]
    public async Task MapAsync_ResultWithAsyncFunc_OnSuccess()
    {
        var mapped = await Result.Success(42).MapAsync(async x =>
        {
            await Task.Yield();
            return x.ToString();
        });
        mapped.Value.Should().Be("42");
    }

    [Fact]
    public async Task MapAsync_ResultWithAsyncFunc_OnFailure_ShouldPropagateError()
    {
        var mapped = await Result.Failure<int>(TestData.Errors.Validation).MapAsync(async x =>
        {
            await Task.Yield();
            return x.ToString();
        });
        mapped.IsFailure.Should().BeTrue();
        mapped.Error.Should().Be(TestData.Errors.Validation);
    }

    // Async: Task<Result<T>> + sync transform
    [Fact]
    public async Task MapAsync_TaskResultWithSyncFunc_OnSuccess()
    {
        var mapped = await Task.FromResult(Result.Success(42)).MapAsync(x => x * 2);
        mapped.Value.Should().Be(84);
    }

    [Fact]
    public async Task MapAsync_TaskResultWithSyncFunc_OnFailure()
    {
        var mapped = await Task.FromResult(Result.Failure<int>(TestData.Errors.Conflict))
            .MapAsync(x => x * 2);
        mapped.IsFailure.Should().BeTrue();
    }

    // Async: Task<Result<T>> + async transform
    [Fact]
    public async Task MapAsync_TaskResultWithAsyncFunc_ShouldWork()
    {
        var mapped = await Task.FromResult(Result.Success(10))
            .MapAsync(async x =>
            {
                await Task.Yield();
                return x + 5;
            });
        mapped.Value.Should().Be(15);
    }

    [Fact]
    public async Task MapAsync_NullFunc_ShouldThrowArgumentNullException()
    {
        Func<int, Task<string>> fn = null!;
        var act = async () => await Result.Success(1).MapAsync(fn);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public void Map_FailureChain_ShouldPropagateOriginalError()
    {
        var error = Error.Unauthorized("no auth");
        var result = Result.Failure<int>(error)
            .Map(x => x * 2)
            .Map(x => x.ToString())
            .Map(s => s.Length);
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }
}
