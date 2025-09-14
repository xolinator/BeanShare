using MediatR;

namespace BeanShare.Application.Abstractions;

public interface IQuery<out TResponse> : IRequest<TResponse>
{
}