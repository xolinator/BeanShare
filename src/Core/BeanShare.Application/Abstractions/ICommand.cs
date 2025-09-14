using MediatR;

namespace BeanShare.Application.Abstractions;

public interface ICommand<out TResponse> : IRequest<TResponse>
{
}