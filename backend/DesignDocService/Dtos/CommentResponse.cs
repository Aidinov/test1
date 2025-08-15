namespace DesignDocService.Dtos
{
    /// <summary>
    /// Comment returned from APIs including potential replies.
    /// </summary>
    public class CommentResponse : Comment
    {
        public List<CommentReply> Replies { get; set; } = new();
    }
}
