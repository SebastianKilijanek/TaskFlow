using AutoMapper;
using TaskFlow.Domain.Entities;
using TaskFlow.Application.Boards.DTO;
using TaskFlow.Application.Columns.DTO;
using TaskFlow.Application.Users.DTO;

namespace TaskFlow.Application.Common.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDTO>();
        CreateMap<UserDTO, User>();
        
        CreateMap<Board, BoardDTO>();
        CreateMap<BoardDTO, Board>();

        CreateMap<Column, ColumnDTO>();
        CreateMap<ColumnDTO, Column>();
        
    }
}
