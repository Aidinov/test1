namespace DesignDocService.Dtos
{
    /// <summary>
    /// Request payload used to update an existing design document. Content represents the new
    /// markdown to be written to the Git repository. Fields not supplied will not be updated.
    /// </summary>
    public class UpdateDesignDocumentRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Product { get; set; } = string.Empty;
        public string Team { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string TaskLink { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string GitRepository { get; set; } = string.Empty;
        public string GitFilePath { get; set; } = string.Empty;
        /// <summary>
        /// Optional new commit hash to assign to this update. If omitted, the Git service
        /// will generate a new commit identifier.
        /// </summary>
        public string? GitCommitHash { get; set; }
        public Models.DocumentStatus Status { get; set; }
    }
}