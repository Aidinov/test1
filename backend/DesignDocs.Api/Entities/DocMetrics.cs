namespace DesignDocs.Api.Entities;

public class DocMetrics
{
    public Guid DocumentId { get; set; }
    public Document? Document { get; set; }
    public int OpenBlockers { get; set; }
    public int OpenShould { get; set; }
    public int OpenNit { get; set; }
    public int DetachedCount { get; set; }
    public int NeedsRecheckCount { get; set; }
    public TimeSpan? MedianTimeToFirstReply { get; set; }
}
