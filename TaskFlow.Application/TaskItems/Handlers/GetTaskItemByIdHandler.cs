using AutoMapper;
using MediatR;
using TaskFlow.Application.TaskItems.Queries;
using TaskFlow.Application.TaskItems.DTO;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.TaskItems.Handlers;

public class GetTaskItemByIdHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<GetTaskItemByIdQuery, TaskItemDTO>
{
    public async Task<TaskItemDTO> Handle(GetTaskItemByIdQuery request, CancellationToken cancellationToken)
    {
        return mapper.Map<TaskItemDTO>(request.TaskItem);
    }
}