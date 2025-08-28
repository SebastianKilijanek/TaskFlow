using AutoMapper;
using MediatR;
using TaskFlow.Application.Comments.DTO;
using TaskFlow.Application.Comments.Queries;

namespace TaskFlow.Application.Comments.Handlers;

public class GetCommentByIdHandler(IMapper mapper) : IRequestHandler<GetCommentByIdQuery, CommentDTO>
{
    public Task<CommentDTO> Handle(GetCommentByIdQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(mapper.Map<CommentDTO>(request.Entity));
    }
}