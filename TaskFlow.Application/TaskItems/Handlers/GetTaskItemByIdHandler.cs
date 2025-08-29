using AutoMapper;
using MediatR;
using TaskFlow.Application.TaskItems.Queries;
using TaskFlow.Application.TaskItems.DTO;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.TaskItems.Handlers;

public class GetTaskItemByIdHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<GetTaskItemByIdQuery, TaskItemDTO>
{
    public Task<TaskItemDTO> Handle(GetTaskItemByIdQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(mapper.Map<TaskItemDTO>(request.Entity));
    }
}