using System.Net;
using Xunit;
using System.Text.Json;
using System.Text;

namespace TaskFlow.Tests.Integration.API.Boards;

[Collection("SequentialTests")]
public class BoardsAPIIntegrationTests(TestWebApplicationFactory<TaskFlow.API.AssemblyReference> factory)
    : IClassFixture<TestWebApplicationFactory<TaskFlow.API.AssemblyReference>>
{
    private readonly HttpClient _client = factory.CreateClient();
    
    [Fact]
    public async Task Post_CreateBoard_Returns201()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        var payload = new { name = "API Test Board", isPublic = true };
        var json = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        // Act
        var result = await _client.PostAsync("/api/boards", json);

        // Assert
        Assert.Equal(HttpStatusCode.Created, result.StatusCode);
    }
        
    [Fact]
    public async Task Get_Boards_Returns200()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        
        // Act
        var result = await _client.GetAsync("/api/boards");

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        var content = await result.Content.ReadAsStringAsync();
        Assert.NotEmpty(content);
    }
        
    [Fact]
    public async Task Get_BoardById_Returns200()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        var createPayload = new { name = "API Test Board", isPublic = true };
        var createJson = new StringContent(JsonSerializer.Serialize(createPayload), Encoding.UTF8, "application/json");
        var createResponse = await _client.PostAsync("/api/boards", createJson);
        createResponse.EnsureSuccessStatusCode();

        var location = createResponse.Headers.Location;
        Assert.NotNull(location);
        var idSegment = location!.Segments.Last();
        var createdBoardId = Guid.Parse(idSegment);

        // Act
        var result = await _client.GetAsync($"/api/boards/{createdBoardId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }
        
    [Fact]
    public async Task Put_UpdateBoard_Returns204()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        var createPayload = new { name = "API Test Board", isPublic = true };
        var createJson = new StringContent(JsonSerializer.Serialize(createPayload), Encoding.UTF8, "application/json");
        var createResponse = await _client.PostAsync("/api/boards", createJson);
        createResponse.EnsureSuccessStatusCode();

        var location = createResponse.Headers.Location;
        Assert.NotNull(location);
        var idSegment = location!.Segments.Last();
        var createdBoardId = Guid.Parse(idSegment);

        var updatePayload = new { name = "Updated Board", isPublic = false };
        var updateJson = new StringContent(JsonSerializer.Serialize(updatePayload), Encoding.UTF8, "application/json");
        
        // Act
        var result = await _client.PutAsync($"/api/boards/{createdBoardId}", updateJson);
        
        // Assert
        Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
    }
        
    [Fact]
    public async Task Delete_Board_Returns204()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        var createPayload = new { name = "API Test Board", isPublic = true };
        var createJson = new StringContent(JsonSerializer.Serialize(createPayload), Encoding.UTF8, "application/json");
        var createResponse = await _client.PostAsync("/api/boards", createJson);
        createResponse.EnsureSuccessStatusCode();

        var location = createResponse.Headers.Location;
        Assert.NotNull(location);
        var idSegment = location!.Segments.Last();
        var createdBoardId = Guid.Parse(idSegment);
        
        // Act
        var result = await _client.DeleteAsync($"/api/boards/{createdBoardId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
    }
}