using AutoMapper;
using TaskFlow.Domain.Entities;
using TaskFlow.Application.Boards.DTO;
using TaskFlow.Application.Columns.DTO;
using TaskFlow.Application.Users.DTO;
using TaskFlow.Application.TaskItems.DTO;
using TaskFlow.Application.Comments.DTO;
using TaskFlow.Application.UserBoards.DTO;

namespace TaskFlow.Application.Common.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User
        CreateMap<User, UserDTO>();
        CreateMap<UserDTO, User>();

        // Board
        CreateMap<Board, BoardDTO>();
        CreateMap<BoardDTO, Board>();

        // Column
        CreateMap<Column, ColumnDTO>();
        CreateMap<ColumnDTO, Column>();

        // TaskItem
        CreateMap<TaskItem, TaskItemDTO>()
            .ForMember(dest => dest.AssignedUserName, opt => opt.MapFrom(src => src.AssignedUser != null ? src.AssignedUser.UserName : null))
            .ForMember(dest => dest.Comments, opt => opt.MapFrom(src => src.Comments));
        CreateMap<TaskItemDTO, TaskItem>();

        // Comment
        CreateMap<Comment, CommentDTO>()
            .ForMember(dest => dest.AuthorUserName, opt => opt.MapFrom(src => src.Author.UserName));
        CreateMap<CommentDTO, Comment>();
        
        CreateMap<UserBoard, UserBoardDTO>();
        CreateMap<UserBoardDTO, UserBoard>();
    }
}