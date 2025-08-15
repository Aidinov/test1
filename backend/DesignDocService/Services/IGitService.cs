namespace DesignDocService.Services
{
    /// <summary>
    /// Abstraction for reading and writing design document content from a Git repository.
    /// In this MVP implementation the repository is represented by a file system path.
    /// </summary>
    public interface IGitService
    {
        /// <summary>
        /// Reads the contents of the specified file from the repository.
        /// </summary>
        /// <param name="repoPath">Root path to the repository.</param>
        /// <param name="filePath">Relative path to the file within the repository.</param>
        /// <param name="commitId">Optional commit identifier; not used in the file system implementation.</param>
        /// <returns>The file contents as a string. Returns empty string if the file does not exist.</returns>
        Task<string> ReadFileAsync(string repoPath, string filePath, string? commitId);

        /// <summary>
        /// Writes the contents to the specified file in the repository and returns a new commit identifier.
        /// </summary>
        /// <param name="repoPath">Root path to the repository.</param>
        /// <param name="filePath">Relative path to the file within the repository.</param>
        /// <param name="content">File contents to write.</param>
        /// <returns>A commit identifier representing this write. This may be a GUID or timestamp.</returns>
        Task<string> WriteFileAsync(string repoPath, string filePath, string content);
    }
}