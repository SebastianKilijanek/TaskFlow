using AutoMapper;
using MediatR;
using TaskFlow.Application.Boards.Queries;
using TaskFlow.Application.Boards.DTO;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Boards.Handlers;

public class GetBoardByIdHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<GetBoardByIdQuery, BoardDTO>
{
    public async Task<BoardDTO> Handle(GetBoardByIdQuery request, CancellationToken cancellationToken)
    {
        return mapper.Map<BoardDTO>(request.Board);
    }
}