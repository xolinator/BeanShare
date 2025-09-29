namespace BeanShare.Application.Common;

public readonly record struct Result
{
    private readonly Error[] _errors;

    private Result(bool isSuccess, Error[] errors)
    {
        IsSuccess = isSuccess;
        _errors = errors ?? [];
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public IReadOnlyList<Error> Errors => _errors;

    public static Result Success() => new(true, []);

    public static Result Failure(Error error) => new(false, new[] { error });

    public static Result Failure(IEnumerable<Error> errors) => 
        new(false, errors.ToArray());

    public Result<T> Map<T>(T value) => IsSuccess 
        ? Result<T>.Success(value) 
        : Result<T>.Failure(_errors);

    public Result Bind(Func<Result> func) => IsSuccess ? func() : this;

    public Result<T> Bind<T>(Func<Result<T>> func) => IsSuccess 
        ? func() 
        : Result<T>.Failure(_errors);

    public static Result Combine(params Result[] results)
    {
        var errors = results.Where(r => r.IsFailure).SelectMany(r => r.Errors).ToArray();
        return errors.Length == 0 ? Success() : Failure(errors);
    }

    public T Match<T>(Func<T> onSuccess, Func<IReadOnlyList<Error>, T> onFailure) =>
        IsSuccess ? onSuccess() : onFailure(Errors);
}

public readonly record struct Result<T>
{
    private readonly T? _value;
    private readonly Error[] _errors;

    private Result(bool isSuccess, T? value, Error[] errors)
    {
        IsSuccess = isSuccess;
        _value = value;
        _errors = errors ?? [];
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T Value => IsSuccess ? _value! : throw new InvalidOperationException("Cannot access value of failed result");
    public IReadOnlyList<Error> Errors => _errors;

    public static Result<T> Success(T value) => new(true, value, []);

    public static Result<T> Failure(Error error) => new(false, default, new[] { error });

    public static Result<T> Failure(IEnumerable<Error> errors) => 
        new(false, default, errors.ToArray());

    public Result<TNew> Map<TNew>(Func<T, TNew> mapper) => IsSuccess 
        ? Result<TNew>.Success(mapper(Value)) 
        : Result<TNew>.Failure(_errors);

    public Result<TNew> Bind<TNew>(Func<T, Result<TNew>> func) => IsSuccess 
        ? func(Value) 
        : Result<TNew>.Failure(_errors);

    public Result Bind(Func<T, Result> func) => IsSuccess 
        ? func(Value) 
        : Result.Failure(_errors);

    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<IReadOnlyList<Error>, TResult> onFailure) =>
        IsSuccess ? onSuccess(Value) : onFailure(Errors);

    public static implicit operator Result<T>(T value) => Success(value);
}