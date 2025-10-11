namespace TaskFlow.Application.Columns.DTO;

public class CreateColumnDTO
{
    public required string Name { get; set; }
    public required Guid BoardId { get; set; }
}