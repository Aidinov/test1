namespace DesignDocService.Services
{
    /// <summary>
    /// Simple file system based implementation of <see cref="IGitService"/>.  Instead of
    /// interacting with a real Git repository, this service reads and writes files to
    /// the local file system.  Each write returns a new pseudo commit identifier which
    /// defaults to a newly generated GUID if the caller does not provide one.  This
    /// implementation is intended solely for the MVP and testing scenarios where a
    /// fully fledged Git integration is unnecessary.
    /// </summary>
    public class FileGitService : IGitService
    {
        public Task<string> ReadFileAsync(string repoPath, string filePath, string? commitId)
        {
            if (string.IsNullOrWhiteSpace(repoPath))
            {
                throw new ArgumentException("Repository path must be provided", nameof(repoPath));
            }
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path must be provided", nameof(filePath));
            }
            var fullPath = Path.Combine(repoPath, filePath);
            if (!File.Exists(fullPath))
            {
                // If the file does not exist, return empty string; the caller can decide how to handle it
                return Task.FromResult(string.Empty);
            }
            return File.ReadAllTextAsync(fullPath);
        }

        public async Task<string> WriteFileAsync(string repoPath, string filePath, string content)
        {
            if (string.IsNullOrWhiteSpace(repoPath))
            {
                throw new ArgumentException("Repository path must be provided", nameof(repoPath));
            }
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path must be provided", nameof(filePath));
            }
            var fullPath = Path.Combine(repoPath, filePath);
            // Ensure the directory exists
            var directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            await File.WriteAllTextAsync(fullPath, content ?? string.Empty);
            // Return a new commit identifier based on a GUID
            return Guid.NewGuid().ToString("N");
        }
    }
}