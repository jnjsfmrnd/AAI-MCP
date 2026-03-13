using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using AgentApi.Models;
using AgentApi.Services;
using Moq;
using Xunit;
using System.Net.Http;
using Microsoft.Extensions.Configuration;

namespace AgentApi.Tests.Services
{
    public class ExecutorServiceTests
    {
        [Fact]
        public async Task ExecuteAsync_ListsBlobs_WhenTaskIsListBlobs()
        {
            // Arrange
            var httpClient = new HttpClient();
            var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>()).Build();
            var planner = new PlannerService(new LlmProvider(httpClient, config));
            var mcpClientMock = new Mock<IMcpClient>(MockBehavior.Strict);

            mcpClientMock.Setup(x => x.ListBlobsAsync("datasets"))
                .ReturnsAsync(new ToolCall { Tool = "blob.list", Success = true, Data = JsonDocument.Parse("[\"file1.csv\"]").RootElement });

            var executor = new ExecutorService(planner, mcpClientMock.Object);

            // Act
            var response = await executor.ExecuteAsync("List all files in blob storage");

            // Assert
            Assert.NotNull(response);
            Assert.Contains(response.ToolCalls, c => c.Tool == "blob.list" && c.Success);
        }

        [Fact]
        public async Task ExecuteAsync_ProcessesCsvFiles_WhenTaskMentionsCsv()
        {
            // Arrange
            var httpClient = new HttpClient();
            var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>()).Build();
            var planner = new PlannerService(new LlmProvider(httpClient, config));
            var mcpClientMock = new Mock<IMcpClient>(MockBehavior.Strict);

            // Sequence of calls expected by ExecutorService
            mcpClientMock.Setup(x => x.ListBlobsAsync("datasets"))
                .ReturnsAsync(new ToolCall { Tool = "blob.list", Success = true, Data = JsonDocument.Parse("[\"file1.csv\"]").RootElement });

            mcpClientMock.Setup(x => x.ReadBlobAsync("datasets", "file1.csv"))
                .ReturnsAsync(new ToolCall { Tool = "blob.read", Success = true, Data = JsonDocument.Parse("\"a,b\\n1,2\\n\"").RootElement });

            mcpClientMock.Setup(x => x.TransformCsvAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new ToolCall { Tool = "csv.transform", Success = true, Data = JsonDocument.Parse("\"a,b\\n1,2\\n\"").RootElement });

            mcpClientMock.Setup(x => x.SummarizeAsync(It.IsAny<string>()))
                .ReturnsAsync(new ToolCall { Tool = "file.summarize", Success = true, Data = JsonDocument.Parse("\"summary\"").RootElement });

            mcpClientMock.Setup(x => x.WriteBlobAsync("outputs", "summary-file1.csv.txt", It.IsAny<string>()))
                .ReturnsAsync(new ToolCall { Tool = "blob.write", Success = true, Data = JsonDocument.Parse("\"ok\"").RootElement });

            var executor = new ExecutorService(planner, mcpClientMock.Object);

            // Act
            var response = await executor.ExecuteAsync("Read all CSVs, clean them, and upload summaries");

            // Assert
            Assert.NotNull(response);
            Assert.Contains(response.ToolCalls, c => c.Tool == "blob.list");
            Assert.Contains(response.ToolCalls, c => c.Tool == "blob.read");
            Assert.Contains(response.ToolCalls, c => c.Tool == "csv.transform");
            Assert.Contains(response.ToolCalls, c => c.Tool == "file.summarize");
            Assert.Contains(response.ToolCalls, c => c.Tool == "blob.write");
        }
    }
}
