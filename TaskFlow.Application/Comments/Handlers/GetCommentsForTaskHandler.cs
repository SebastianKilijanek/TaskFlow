using AutoMapper;
using MediatR;
using TaskFlow.Application.Comments.DTO;
using TaskFlow.Application.Comments.Queries;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Comments.Handlers;

public class GetCommentsForTaskHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<GetCommentsForTaskQuery, IReadOnlyList<CommentDTO>>
{
    public async Task<IReadOnlyList<CommentDTO>> Handle(GetCommentsForTaskQuery request, CancellationToken cancellationToken)
    {
        var comments = await unitOfWork.Repository<Comment>().ListAsync(c => c.TaskItemId == request.TaskItemId);
        return mapper.Map<IReadOnlyList<CommentDTO>>(comments);
    }
}