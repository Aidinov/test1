using System.Net.Http.Json;
using DesignDocService.Dtos;
using DesignDocService.Models;
using CommentDto = DesignDocService.Dtos.CommentResponse;
using DesignDocService.Tests.Infrastructure;
using Xunit;

namespace DesignDocService.Tests.Comments
{
    /// <summary>
    /// Tests for comment creation, versioning and resolution via the API.
    /// </summary>
    public class CommentCrudTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client = factory.CreateClient();

        [Fact]
        public async Task AddComment_ShouldPersistCommentWithVersion()
        {
            // Create doc with commit hash c1
            var docDto = new
            {
                title = "Doc4",
                product = "Prod4",
                team = "Team4",
                author = "Eve",
                taskLink = "",
                content = "# Doc4",
                status = "Draft",
                gitRepository = "/tmp/repo4",
                gitFilePath = "doc4.md",
                gitCommitHash = "c1"
            };
            var createRes = await _client.PostAsJsonAsync("/api/documents", docDto);
            createRes.EnsureSuccessStatusCode();
            var doc = await createRes.Content.ReadFromJsonAsync<DocumentSummary>();
            Assert.NotNull(doc);

            // Add comment
            var commentRequest = new CommentRequest
            {
                StartIndex = 0,
                EndIndex = 3,
                Type = CommentType.Question,
                Severity = RemarkSeverity.Opinion,
                Content = "What about this?",
                Author = "Reviewer"
            };
            var addRes = await _client.PostAsJsonAsync($"/api/documents/{doc!.Id}/comments", commentRequest);
            addRes.EnsureSuccessStatusCode();
            var createdComment = await addRes.Content.ReadFromJsonAsync<CommentDto>();
            Assert.NotNull(createdComment);
            Assert.Equal(commentRequest.Content, createdComment!.Content);
            Assert.Equal(doc.GitCommitHash, createdComment.DocumentVersion);

            // Update document commit hash
            // Update commit hash to c2 without changing content
            var updateRequest = new UpdateDocumentRequest
            {
                Title = doc.Title,
                Product = doc.Product,
                Team = doc.Team,
                Author = doc.Author,
                TaskLink = doc.TaskLink,
                Content = string.Empty, // content unchanged for this update
                GitRepository = doc.GitRepository,
                GitFilePath = doc.GitFilePath,
                GitCommitHash = "c2",
                Status = doc.Status
            };
            var updateRes = await _client.PutAsJsonAsync($"/api/documents/{doc!.Id}", updateRequest);
            updateRes.EnsureSuccessStatusCode();

            // Add another comment after update
            var commentRequest2 = new CommentRequest
            {
                StartIndex = 4,
                EndIndex = 8,
                Type = CommentType.Remark,
                Severity = RemarkSeverity.Critical,
                Content = "Needs improvement",
                Author = "Reviewer"
            };
            var addRes2 = await _client.PostAsJsonAsync($"/api/documents/{doc!.Id}/comments", commentRequest2);
            addRes2.EnsureSuccessStatusCode();
            var created2 = await addRes2.Content.ReadFromJsonAsync<CommentDto>();
            Assert.NotNull(created2);
            Assert.Equal("c2", created2!.DocumentVersion);

            // Fetch comments and verify count
            var comments = await _client.GetFromJsonAsync<List<CommentDto>>($"/api/documents/{doc.Id}/comments");
            Assert.NotNull(comments);
            Assert.Equal(2, comments!.Count);
        }

        [Fact]
        public async Task AddComment_ShouldStoreOriginalTextAndPreserveAfterUpdate()
        {
            // Arrange: create doc
            var repoPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(repoPath);
            var initialContent = "Sample text for comments";
            var docDto = new
            {
                title = "CommentDoc",
                product = "ProdC",
                team = "TeamC",
                author = "Tester",
                taskLink = "",
                content = initialContent,
                status = "Draft",
                gitRepository = repoPath,
                gitFilePath = "c.md",
                gitCommitHash = "c1"
            };
            var createRes = await _client.PostAsJsonAsync("/api/documents", docDto);
            createRes.EnsureSuccessStatusCode();
            var created = await createRes.Content.ReadFromJsonAsync<DocumentSummary>();
            Assert.NotNull(created);

            // Add a comment on "text"
            var start = initialContent.IndexOf("text");
            var end = start + "text".Length;
            var commentRequest = new CommentRequest
            {
                StartIndex = start,
                EndIndex = end,
                Type = CommentType.Remark,
                Severity = RemarkSeverity.Critical,
                Content = "This snippet",
                Author = "Reviewer"
            };
            var addRes = await _client.PostAsJsonAsync($"/api/documents/{created!.Id}/comments", commentRequest);
            addRes.EnsureSuccessStatusCode();
            var comment = await addRes.Content.ReadFromJsonAsync<CommentDto>();
            Assert.NotNull(comment);
            Assert.Equal("text", comment!.OriginalText);
            Assert.Equal("c1", comment.DocumentVersion);

            // Update document content; remove the commented word
            var newContent = initialContent.Replace("text", "word");
            var updateReq = new UpdateDocumentRequest
            {
                Title = created.Title,
                Product = created.Product,
                Team = created.Team,
                Author = created.Author,
                TaskLink = created.TaskLink,
                Content = newContent,
                GitRepository = created.GitRepository,
                GitFilePath = created.GitFilePath,
                GitCommitHash = "c2",
                Status = created.Status
            };
            var updateRes = await _client.PutAsJsonAsync($"/api/documents/{created!.Id}", updateReq);
            updateRes.EnsureSuccessStatusCode();

            // Fetch comments again; original comment should still be present with original text
            var comments = await _client.GetFromJsonAsync<List<CommentDto>>($"/api/documents/{created!.Id}/comments");
            Assert.NotNull(comments);
            var fetched = comments!.FirstOrDefault(c => c.Id == comment!.Id);
            Assert.NotNull(fetched);
            Assert.Equal("text", fetched!.OriginalText);
            Assert.Equal("c1", fetched.DocumentVersion);
        }

        [Fact]
        public async Task ResolveComment_ShouldMarkResolved()
        {
            // Create doc
            var docDto = new
            {
                title = "Doc5",
                product = "Prod5",
                team = "Team5",
                author = "Frank",
                taskLink = "",
                content = "# Doc5",
                status = "Draft",
                gitRepository = "/tmp/repo5",
                gitFilePath = "doc5.md",
                gitCommitHash = "c1"
            };
            var createRes = await _client.PostAsJsonAsync("/api/documents", docDto);
            createRes.EnsureSuccessStatusCode();
            var doc = await createRes.Content.ReadFromJsonAsync<DocumentSummary>();
            Assert.NotNull(doc);

            // Add comment
            var commentRequest = new CommentRequest
            {
                StartIndex = 0,
                EndIndex = 5,
                Type = CommentType.Remark,
                Severity = RemarkSeverity.Desirable,
                Content = "Looks good",
                Author = "Reviewer"
            };
            var addRes = await _client.PostAsJsonAsync($"/api/documents/{doc!.Id}/comments", commentRequest);
            addRes.EnsureSuccessStatusCode();
            var comment = await addRes.Content.ReadFromJsonAsync<CommentDto>();
            Assert.NotNull(comment);

            // Resolve
            var resolveRes = await _client.PostAsJsonAsync($"/api/comments/{comment!.Id}/resolve", new { resolvedBy = "Reviewer2" });
            resolveRes.EnsureSuccessStatusCode();
            var resolved = await resolveRes.Content.ReadFromJsonAsync<CommentDto>();
            Assert.NotNull(resolved);
            Assert.True(resolved!.IsResolved);
            Assert.Equal("Reviewer2", resolved.ResolvedBy);

            // Fetch comment and verify resolved
            var fetchedComments = await _client.GetFromJsonAsync<List<CommentDto>>($"/api/documents/{doc!.Id}/comments");
            Assert.NotNull(fetchedComments);
            var updatedComment = fetchedComments!.FirstOrDefault(c => c.Id == comment!.Id);
            Assert.NotNull(updatedComment);
            Assert.True(updatedComment!.IsResolved);
        }

        [Fact]
        public async Task AddComment_InvalidIndices_ShouldReturnBadRequest()
        {
            var docDto = new
            {
                title = "Doc6",
                product = "Prod6",
                team = "Team6",
                author = "George",
                taskLink = "",
                content = "# Doc6",
                status = "Draft",
                gitRepository = "/tmp/repo6",
                gitFilePath = "doc6.md",
                gitCommitHash = "c1"
            };
            var createRes = await _client.PostAsJsonAsync("/api/documents", docDto);
            createRes.EnsureSuccessStatusCode();
            var doc = await createRes.Content.ReadFromJsonAsync<DocumentSummary>();

            var invalidComment = new CommentRequest
            {
                StartIndex = 5,
                EndIndex = 2,
                Type = CommentType.Question,
                Severity = RemarkSeverity.Opinion,
                Content = "Invalid",
                Author = "Reviewer"
            };
            var addRes = await _client.PostAsJsonAsync($"/api/documents/{doc!.Id}/comments", invalidComment);
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, addRes.StatusCode);
        }
    }
}