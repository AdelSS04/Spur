using BenchmarkDotNet.Attributes;

namespace Spur.Benchmarks;

[MemoryDiagnoser]
[ShortRunJob]
public class ResultCreationBenchmarks
{
    private const int Value = 42;
    private readonly Error _error = Error.NotFound("Resource not found");

    [Benchmark(Baseline = true)]
    public Result<int> CreateSuccess()
    {
        return Result.Success(Value);
    }

    [Benchmark]
    public Result<int> CreateFailure()
    {
        return Result.Failure<int>(_error);
    }

    [Benchmark]
    public Result<int> ImplicitConversionFromValue()
    {
        Result<int> result = Value;
        return result;
    }

    [Benchmark]
    public Result<int> ImplicitConversionFromError()
    {
        Result<int> result = _error;
        return result;
    }

    [Benchmark]
    public int GetValueOnSuccess()
    {
        var result = Result.Success(Value);
        return result.Value;
    }

    [Benchmark]
    public Error GetErrorOnFailure()
    {
        var result = Result.Failure<int>(_error);
        return result.Error;
    }

    [Benchmark]
    public int MatchOnSuccess()
    {
        var result = Result.Success(Value);
        return result.Match(
            onSuccess: v => v,
            onFailure: _ => 0);
    }

    [Benchmark]
    public int MatchOnFailure()
    {
        var result = Result.Failure<int>(_error);
        return result.Match(
            onSuccess: v => v,
            onFailure: _ => 0);
    }
}
