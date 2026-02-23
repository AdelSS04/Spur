using BenchmarkDotNet.Attributes;

namespace Spur.Benchmarks;

[MemoryDiagnoser]
[ShortRunJob]
public class VsExceptionBenchmarks
{
    [Benchmark(Baseline = true)]
    public int ResultSuccessPath()
    {
        var result = GetResultSuccess();
        return result.IsSuccess ? result.Value : 0;
    }

    [Benchmark]
    public int ResultFailurePath()
    {
        var result = GetResultFailure();
        return result.IsSuccess ? result.Value : 0;
    }

    [Benchmark]
    public int ExceptionSuccessPath()
    {
        try
        {
            return GetValueOrThrowSuccess();
        }
        catch
        {
            return 0;
        }
    }

    [Benchmark]
    public int ExceptionFailurePath()
    {
        try
        {
            return GetValueOrThrowFailure();
        }
        catch
        {
            return 0;
        }
    }

    [Benchmark]
    public Result<int> ResultTryCatchSuccess()
    {
        return Result.Try(() => GetValueOrThrowSuccess());
    }

    [Benchmark]
    public Result<int> ResultTryCatchFailure()
    {
        return Result.Try(() => GetValueOrThrowFailure());
    }

    private static Result<int> GetResultSuccess() => Result.Success(42);
    private static Result<int> GetResultFailure() => Result.Failure<int>(Error.NotFound("Not found"));
    private static int GetValueOrThrowSuccess() => 42;
    private static int GetValueOrThrowFailure() => throw new InvalidOperationException("Not found");
}
