using System.Linq.Expressions;

namespace BeanShare.Domain.Specifications;

public interface ISpec<T>
{
    Expression<Func<T, bool>> Criteria { get; }
    string? Reason { get; }
}

public abstract class Spec<T> : ISpec<T>
{
    public abstract Expression<Func<T, bool>> Criteria { get; }
    public abstract string? Reason { get; }
}

public sealed class PredicateSpec<T> : ISpec<T>
{
    public PredicateSpec(Expression<Func<T, bool>> criteria, string? reason = null)
    {
        Criteria = criteria ?? throw new ArgumentNullException(nameof(criteria));
        Reason = reason;
    }

    public Expression<Func<T, bool>> Criteria { get; }
    public string? Reason { get; }
}

public static class Spec
{
    public static ISpec<T> Where<T>(Expression<Func<T, bool>> predicate, string? reason = null)
        => new PredicateSpec<T>(predicate, reason);

    public static ISpec<T> And<T>(this ISpec<T> left, ISpec<T> right, string? reason = null)
        => new PredicateSpec<T>(left.Criteria.And(right.Criteria), reason ?? CombineReason("AND", left, right));

    public static ISpec<T> Or<T>(this ISpec<T> left, ISpec<T> right, string? reason = null)
        => new PredicateSpec<T>(left.Criteria.Or(right.Criteria), reason ?? CombineReason("OR", left, right));

    public static ISpec<T> Not<T>(this ISpec<T> spec, string? reason = null)
        => new PredicateSpec<T>(spec.Criteria.Not(), reason ?? $"NOT({spec.Reason ?? spec.Criteria.Body.ToString()})");

    private static string CombineReason<T>(string op, ISpec<T> a, ISpec<T> b)
        => $"({a.Reason ?? a.Criteria.Body.ToString()} {op} {b.Reason ?? b.Criteria.Body.ToString()})";
}

internal static class ExpressionCombiner
{
    public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> left,
                                                   Expression<Func<T, bool>> right)
    {
        var param = Expression.Parameter(typeof(T), "x");
        var body = Expression.AndAlso(
            left.Body.Replace(left.Parameters[0], param),
            right.Body.Replace(right.Parameters[0], param));
        return Expression.Lambda<Func<T, bool>>(body, param);
    }

    public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> left,
                                                  Expression<Func<T, bool>> right)
    {
        var param = Expression.Parameter(typeof(T), "x");
        var body = Expression.OrElse(
            left.Body.Replace(left.Parameters[0], param),
            right.Body.Replace(right.Parameters[0], param));
        return Expression.Lambda<Func<T, bool>>(body, param);
    }

    public static Expression<Func<T, bool>> Not<T>(this Expression<Func<T, bool>> expr)
    {
        var param = expr.Parameters[0];
        var body = Expression.Not(expr.Body);
        return Expression.Lambda<Func<T, bool>>(body, param);
    }

    private static Expression Replace(this Expression expression, Expression source, Expression target)
        => new RebindVisitor(source, target).Visit(expression)!;

    private sealed class RebindVisitor : ExpressionVisitor
    {
        private readonly Expression _from;
        private readonly Expression _to;
        public RebindVisitor(Expression from, Expression to) { _from = from; _to = to; }
        public override Expression? Visit(Expression? node) => node == _from ? _to : base.Visit(node);
    }
}