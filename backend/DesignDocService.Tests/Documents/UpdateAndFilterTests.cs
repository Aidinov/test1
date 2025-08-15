using System.Net.Http.Json;
using DesignDocService.Dtos;
using DesignDocService.Models;
using DesignDocService.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DesignDocService.Tests.Documents
{
    /// <summary>
    /// Tests for updating documents and filtering the list via the API.
    /// </summary>
    public class UpdateAndFilterTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client = factory.CreateClient();

        [Fact]
        public async Task UpdateDocument_ShouldModifyDocument()
        {
            // Create a document
            var docDto = new
            {
                title = "Doc2",
                product = "Prod2",
                team = "Team2",
                author = "Carol",
                taskLink = "",
                content = "# Doc2",
                status = "Draft",
                gitRepository = "/tmp/repo2",
                gitFilePath = "doc2.md",
                gitCommitHash = "c1"
            };
            var createRes = await _client.PostAsJsonAsync("/api/documents", docDto);
            createRes.EnsureSuccessStatusCode();
            var created = await createRes.Content.ReadFromJsonAsync<DocumentSummary>();
            Assert.NotNull(created);

            // Modify the document using an update request.  We reuse values from the created
            // response and supply the new content and commit hash.  Status is carried over
            // explicitly to avoid resetting it inadvertently.
            var updateRequest = new UpdateDocumentRequest
            {
                Title = "Doc2 Updated",
                Product = created!.Product,
                Team = created.Team,
                Author = created.Author,
                TaskLink = created.TaskLink,
                Content = "# Updated",
                GitRepository = created.GitRepository,
                GitFilePath = created.GitFilePath,
                GitCommitHash = "c2",
                Status = created.Status
            };
            var updateRes = await _client.PutAsJsonAsync($"/api/documents/{created.Id}", updateRequest);
            updateRes.EnsureSuccessStatusCode();
            var updated = await updateRes.Content.ReadFromJsonAsync<DocumentSummary>();
            Assert.NotNull(updated);
            Assert.Equal("Doc2 Updated", updated!.Title);
            Assert.Equal("c2", updated.GitCommitHash);

            // Verify DB
            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DesignDocContext>();
            var fromDb = await db.DesignDocuments.FindAsync(created!.Id);
            Assert.NotNull(fromDb);
            Assert.Equal("Doc2 Updated", fromDb!.Title);
            Assert.Equal("c2", fromDb.GitCommitHash);
        }

        [Fact]
        public async Task UpdateDocument_ShouldStoreContentInGitAndRespectCommitId()
        {
            // Arrange: create repo and initial document
            var repoPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(repoPath);
            var initialContent = "Initial content";
            var docDto = new
            {
                title = "UpdateDoc",
                product = "ProdU",
                team = "TeamU",
                author = "Tester",
                taskLink = "",
                content = initialContent,
                status = "Draft",
                gitRepository = repoPath,
                gitFilePath = "u.md",
                gitCommitHash = "cInit"
            };
            var createRes = await _client.PostAsJsonAsync("/api/documents", docDto);
            createRes.EnsureSuccessStatusCode();
            var created = await createRes.Content.ReadFromJsonAsync<DocumentSummary>();
            Assert.NotNull(created);

            // Act: update with new content and commit id using update request
            var updateRequest = new UpdateDocumentRequest
            {
                Title = created!.Title,
                Product = created.Product,
                Team = created.Team,
                Author = created.Author,
                TaskLink = created.TaskLink,
                Content = "Updated content",
                GitRepository = created.GitRepository,
                GitFilePath = created.GitFilePath,
                GitCommitHash = "cUpdated",
                Status = created.Status
            };
            var updateRes = await _client.PutAsJsonAsync($"/api/documents/{created.Id}", updateRequest);
            updateRes.EnsureSuccessStatusCode();
            var updated = await updateRes.Content.ReadFromJsonAsync<DocumentSummary>();
            Assert.NotNull(updated);
            // Assert: commit id respected
            Assert.Equal("cUpdated", updated!.GitCommitHash);
            // Load file directly from file system to ensure content persisted
            var filePath = Path.Combine(repoPath, "u.md");
            var fileContent = File.ReadAllText(filePath);
            Assert.Equal("Updated content", fileContent);
        }

        [Fact]
        public async Task UpdateStatus_ShouldChangeStatus()
        {
            var docDto = new
            {
                title = "Doc3",
                product = "Prod3",
                team = "Team3",
                author = "Dave",
                taskLink = "",
                content = "# Doc3",
                status = "Draft",
                gitRepository = "/tmp/repo3",
                gitFilePath = "doc3.md",
                gitCommitHash = "c1"
            };
            var createRes = await _client.PostAsJsonAsync("/api/documents", docDto);
            createRes.EnsureSuccessStatusCode();
            var created = await createRes.Content.ReadFromJsonAsync<DocumentSummary>();
            Assert.NotNull(created);

            // Change status to UnderReview
            var statusRes = await _client.PutAsync($"/api/documents/{created!.Id}/status?status=UnderReview", null);
            statusRes.EnsureSuccessStatusCode();
            var updated = await statusRes.Content.ReadFromJsonAsync<DocumentSummary>();
            Assert.NotNull(updated);
            Assert.Equal(DocumentStatus.UnderReview, updated!.Status);

            // Fetch document and assert status
            var fetch = await _client.GetFromJsonAsync<DocumentDetails>($"/api/documents/{created!.Id}");
            Assert.NotNull(fetch);
            Assert.Equal(DocumentStatus.UnderReview, fetch!.Status);
        }

        [Fact]
        public async Task FilterDocuments_ShouldReturnMatchingDocuments()
        {
            // Create documents with different teams/products/authors
            var docs = new[]
            {
                new { title="A", product="P1", team="T1", author="Ann", taskLink="", content="#A", status="Draft", gitRepository="/tmp/repo", gitFilePath="a.md", gitCommitHash="c" },
                new { title="B", product="P2", team="T1", author="Bob", taskLink="", content="#B", status="Draft", gitRepository="/tmp/repo", gitFilePath="b.md", gitCommitHash="c" },
                new { title="C", product="P1", team="T2", author="Ann", taskLink="", content="#C", status="Draft", gitRepository="/tmp/repo", gitFilePath="c.md", gitCommitHash="c" }
            };
            foreach (var dto in docs)
            {
                var res = await _client.PostAsJsonAsync("/api/documents", dto);
                res.EnsureSuccessStatusCode();
            }
            // No filter
            var noFilter = await _client.GetFromJsonAsync<List<DocumentSummary>>("/api/documents");
            Assert.NotNull(noFilter);
            Assert.Equal(3, noFilter.Count);
            
            // Filter by team T1
            var byTeam = await _client.GetFromJsonAsync<List<DocumentSummary>>("/api/documents?team=T1");
            Assert.NotNull(byTeam);
            Assert.Equal(2, byTeam!.Count);

            // Filter by product P1
            var byProduct = await _client.GetFromJsonAsync<List<DocumentSummary>>("/api/documents?product=P1");
            Assert.NotNull(byProduct);
            Assert.Equal(2, byProduct!.Count);

            // Filter by author Ann
            var byAuthor = await _client.GetFromJsonAsync<List<DocumentSummary>>("/api/documents?author=Ann");
            Assert.NotNull(byAuthor);
            Assert.Equal(2, byAuthor!.Count);

            // Filter by combination
            var combo = await _client.GetFromJsonAsync<List<DocumentSummary>>("/api/documents?team=T1&product=P2");
            Assert.NotNull(combo);
            Assert.Single(combo!);
            Assert.Equal("B", combo.First().Title);
        }
    }
}