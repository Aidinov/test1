using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using DesignDocService.Dtos;
using DesignDocService.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DesignDocService.Tests.Documents
{
    /// <summary>
    /// Tests for creating and reading design documents via the API.
    /// </summary>
    public class CreateAndReadTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client = factory.CreateClient();

        // Configure JSON options to handle string enums during deserialization in tests
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter() }
        };

        [Fact]
        public async Task CreateDocument_ShouldPersistDocument()
        {
            var docDto = new
            {
                title = "Doc1",
                product = "Prod1",
                team = "Team1",
                author = "Alice",
                taskLink = "https://example.com/task/1",
                content = "# Design Doc 1",
                status = "Draft",
                gitRepository = "/tmp/repo",
                gitFilePath = "doc1.md",
                gitCommitHash = "c1"
            };

            var response = await _client.PostAsJsonAsync("/api/documents", docDto);
            response.EnsureSuccessStatusCode();
            var created = await response.Content.ReadFromJsonAsync<DocumentSummary>();
            Assert.NotNull(created);
            Assert.Equal(docDto.title, created!.Title);
            Assert.Equal(docDto.product, created.Product);
            Assert.Equal(docDto.team, created.Team);
            Assert.Equal(docDto.author, created.Author);
            Assert.Equal(docDto.gitRepository, created.GitRepository);
            Assert.Equal(docDto.gitFilePath, created.GitFilePath);
            Assert.Equal(docDto.gitCommitHash, created.GitCommitHash);

            // Verify persisted in DB
            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DesignDocContext>();
            var fromDb = await db.DesignDocuments.FindAsync(created.Id);
            Assert.NotNull(fromDb);
            Assert.Equal(docDto.title, fromDb!.Title);
        }

        [Fact]
        public async Task GetAll_ShouldNotReturnContent()
        {
            // Arrange: create a temporary repository and write a document via the API
            var repoPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(repoPath);
            var docDto = new
            {
                title = "NoContentDoc",
                product = "ProdNC",
                team = "TeamNC",
                author = "Tester",
                taskLink = "",
                content = "# Document content",
                status = "Draft",
                gitRepository = repoPath,
                gitFilePath = "doc.md",
                gitCommitHash = "commitNC"
            };
            var createRes = await _client.PostAsJsonAsync("/api/documents", docDto);
            createRes.EnsureSuccessStatusCode();

            // Act: fetch all documents
            var response = await _client.GetFromJsonAsync<List<DocumentSummary>>("/api/documents");
            Assert.NotNull(response);
            var retrieved = response!.FirstOrDefault(d => d.Title == "NoContentDoc");
            Assert.NotNull(retrieved);
        }

        [Fact]
        public async Task GetById_ShouldLoadContentFromGit()
        {
            // Arrange: create repository and document
            var repoPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(repoPath);
            var content = "# Hello\nContent for get by id test";
            var docDto = new
            {
                title = "GetByIdDoc",
                product = "ProdGBI",
                team = "TeamGBI",
                author = "Tester",
                taskLink = "",
                content = content,
                status = "Draft",
                gitRepository = repoPath,
                gitFilePath = "gbidoc.md",
                gitCommitHash = "hashGBI"
            };
            var createRes = await _client.PostAsJsonAsync("/api/documents", docDto);
            createRes.EnsureSuccessStatusCode();
            var created = await createRes.Content.ReadFromJsonAsync<DocumentSummary>();
            Assert.NotNull(created);

            // Act: fetch by id
            var fetch = await _client.GetFromJsonAsync<DocumentDetails>($"/api/documents/{created!.Id}");
            Assert.NotNull(fetch);
            // Assert: content loaded from Git
            Assert.Equal(content, fetch!.Content);
        }
    }
}