using System.Net;
using System.Net.Http.Json;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TaskFlow.Application.Boards.Commands;
using TaskFlow.Application.Columns.Commands;
using Xunit;

namespace TaskFlow.Tests.Integration.API.Columns;

[Collection("SequentialTests")]
public class ColumnsAPIIntegrationTests(TestWebApplicationFactory<TaskFlow.API.AssemblyReference> factory)
    : IClassFixture<TestWebApplicationFactory<TaskFlow.API.AssemblyReference>>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<Guid> CreateBoard()
    {
        var testScope = factory.GetTestScope();
        var mediator = testScope.ServiceScope.ServiceProvider.GetRequiredService<IMediator>();
        var boardId = await mediator.Send(new CreateBoardCommand("Test Board", true));
        return boardId;
    }

    [Fact]
    public async Task Post_CreateColumn_Returns201()
    {
        // Arrange
        var boardId = await CreateBoard();
        var payload = new CreateColumnCommand("Test Column", boardId, 0);

        // Act
        var result = await _client.PostAsJsonAsync("/api/columns", payload);

        // Assert
        Assert.Equal(HttpStatusCode.Created, result.StatusCode);
        Assert.NotNull(result.Headers.Location);
    }

    [Fact]
    public async Task Get_ColumnsByBoardId_Returns200()
    {
        // Arrange
        var boardId = await CreateBoard();
        var createPayload = new CreateColumnCommand("Test Column", boardId, 0);
        await _client.PostAsJsonAsync("/api/columns", createPayload);

        // Act
        var result = await _client.GetAsync($"/api/columns/board/{boardId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        var content = await result.Content.ReadAsStringAsync();
        Assert.Contains("Test Column", content);
    }

    [Fact]
    public async Task Get_ColumnById_Returns200()
    {
        // Arrange
        var boardId = await CreateBoard();
        var createPayload = new CreateColumnCommand("Test Column", boardId, 0);
        var createResponse = await _client.PostAsJsonAsync("/api/columns", createPayload);
        var createdColumnUrl = createResponse.Headers.Location;

        // Act
        var result = await _client.GetAsync(createdColumnUrl);

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        var content = await result.Content.ReadAsStringAsync();
        Assert.Contains("Test Column", content);
    }

    [Fact]
    public async Task Put_UpdateColumn_Returns204()
    {
        // Arrange
        var boardId = await CreateBoard();
        var createPayload = new CreateColumnCommand("Test Column", boardId, 0);
        var createResponse = await _client.PostAsJsonAsync("/api/columns", createPayload);
        var createdColumnUrl = createResponse.Headers.Location!;
        var idSegment = createdColumnUrl.Segments.Last();
        var createdColumnId = Guid.Parse(idSegment);

        var updatePayload = new UpdateColumnCommand(createdColumnId, "Updated Column", 1);

        // Act
        var result = await _client.PutAsJsonAsync($"/api/columns/{createdColumnId}", updatePayload);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
    }

    [Fact]
    public async Task Delete_Column_Returns204()
    {
        // Arrange
        var boardId = await CreateBoard();
        var createPayload = new CreateColumnCommand("Test Column", boardId, 0);
        var createResponse = await _client.PostAsJsonAsync("/api/columns", createPayload);
        var createdColumnUrl = createResponse.Headers.Location!;
        var idSegment = createdColumnUrl.Segments.Last();
        var createdColumnId = Guid.Parse(idSegment);

        // Act
        var result = await _client.DeleteAsync($"/api/columns/{createdColumnId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
    }
}