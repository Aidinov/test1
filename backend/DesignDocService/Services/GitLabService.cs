using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace DesignDocService.Services
{
    /// <summary>
    /// Implementation of <see cref="IGitService"/> that integrates with a real GitLab instance.
    /// This service interacts with the GitLab REST API to read and write files in a repository.
    /// The repository path supplied to the methods should correspond to a valid GitLab project
    /// identifier (either the numeric project ID or a URL encoded path such as "group%2Fproject").
    /// Authentication and API base URL are supplied via configuration under the "GitLab" section.
    /// </summary>
    public class GitLabService : IGitService
    {
        private readonly HttpClient _httpClient;
        private readonly string _defaultBranch;

        /// <summary>
        /// Initializes a new instance of the <see cref="GitLabService"/> class.
        /// </summary>
        /// <param name="httpClient">An <see cref="HttpClient"/> configured for GitLab API calls.</param>
        /// <param name="configuration">Application configuration used to retrieve GitLab settings.</param>
        public GitLabService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            // Read configuration settings
            var baseUrl = configuration["GitLab:BaseUrl"];
            var accessToken = configuration["GitLab:AccessToken"];
            _defaultBranch = configuration["GitLab:DefaultBranch"] ?? "main";
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new ArgumentException("GitLab base URL must be configured (GitLab:BaseUrl)");
            }
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new ArgumentException("GitLab access token must be configured (GitLab:AccessToken)");
            }
            // Ensure base URL ends with a slash for proper relative URI resolution
            _httpClient.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        /// <inheritdoc />
        public async Task<string> ReadFileAsync(string repoPath, string filePath, string? commitId)
        {
            if (string.IsNullOrWhiteSpace(repoPath))
                throw new ArgumentException("Repository path must be provided", nameof(repoPath));
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path must be provided", nameof(filePath));
            // URL encode project and file identifiers per GitLab API requirements
            var projectId = Uri.EscapeDataString(repoPath);
            var encodedFilePath = Uri.EscapeDataString(filePath);
            var refName = !string.IsNullOrWhiteSpace(commitId) ? commitId : _defaultBranch;
            // Build request URI to fetch the raw file contents
            var requestUri = $"api/v4/projects/{projectId}/repository/files/{encodedFilePath}/raw?ref={Uri.EscapeDataString(refName)}";
            var response = await _httpClient.GetAsync(requestUri);
            if (!response.IsSuccessStatusCode)
            {
                // If the file is not found or another error occurs, return an empty string.
                return string.Empty;
            }
            return await response.Content.ReadAsStringAsync();
        }

        /// <inheritdoc />
        public async Task<string> WriteFileAsync(string repoPath, string filePath, string content)
        {
            if (string.IsNullOrWhiteSpace(repoPath))
                throw new ArgumentException("Repository path must be provided", nameof(repoPath));
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path must be provided", nameof(filePath));
            // Determine whether to create or update the file.  Attempt to fetch file metadata on
            // the default branch; if not found the file will be created.
            var projectId = Uri.EscapeDataString(repoPath);
            var encodedFilePath = Uri.EscapeDataString(filePath);
            var branch = _defaultBranch;
            string action = "update";
            var metadataResponse = await _httpClient.GetAsync($"api/v4/projects/{projectId}/repository/files/{encodedFilePath}?ref={Uri.EscapeDataString(branch)}");
            if (!metadataResponse.IsSuccessStatusCode)
            {
                action = "create";
            }
            // Construct a commit with a single file action.  Using the commits API allows us
            // to receive a commit identifier back in the response.
            var commitRequest = new
            {
                branch,
                commit_message = $"{(action == "create" ? "Create" : "Update")} {filePath} via API",
                actions = new[]
                {
                    new
                    {
                        action,
                        file_path = filePath,
                        content,
                        encoding = "text"
                    }
                }
            };
            var requestJson = JsonSerializer.Serialize(commitRequest);
            using var httpContent = new StringContent(requestJson, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"api/v4/projects/{projectId}/repository/commits", httpContent);
            response.EnsureSuccessStatusCode();
            // Parse commit id from response JSON.  If absent, return a new GUID as fallback.
            var body = await response.Content.ReadFromJsonAsync<JsonElement>();
            if (body.ValueKind == JsonValueKind.Object && body.TryGetProperty("id", out var idProp) && idProp.ValueKind == JsonValueKind.String)
            {
                var commitId = idProp.GetString();
                if (!string.IsNullOrWhiteSpace(commitId))
                {
                    return commitId;
                }
            }
            // Fallback commit id when not provided in response
            return Guid.NewGuid().ToString("N");
        }
    }
}