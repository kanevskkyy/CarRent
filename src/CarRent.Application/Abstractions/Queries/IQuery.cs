using MediatR;

namespace CarRent.Application.Abstractions.Queries;

public interface IQuery<out TResponse> : IRequest<TResponse>
{
    
}