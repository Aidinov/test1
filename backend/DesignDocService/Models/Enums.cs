namespace DesignDocService.Models
{
    /// <summary>
    /// Represents the type of a comment.
    /// </summary>
    public enum CommentType
    {
        /// <summary>
        /// A clarification question.
        /// </summary>
        Question,

        /// <summary>
        /// A suggestion or remark about the document.
        /// </summary>
        Remark
    }

    /// <summary>
    /// Represents the severity level of a remark.
    /// </summary>
    public enum RemarkSeverity
    {
        /// <summary>
        /// A critical remark that must be addressed before approval.
        /// </summary>
        Critical,

        /// <summary>
        /// A desirable improvement that is recommended but not blocking.
        /// </summary>
        Desirable,

        /// <summary>
        /// A subjective opinion or minor stylistic suggestion.
        /// </summary>
        Opinion
    }

    /// <summary>
    /// Represents the status of a design document in the review workflow.
    /// </summary>
    public enum DocumentStatus
    {
        /// <summary>
        /// The document is being drafted by the author.
        /// </summary>
        InProgress,

        /// <summary>
        /// The document has been sent for review.
        /// </summary>
        UnderReview,

        /// <summary>
        /// The document has been approved.
        /// </summary>
        Approved,

        /// <summary>
        /// The document has been rejected.
        /// </summary>
        Rejected
    }
}