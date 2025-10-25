using DesignDocService.Models;

namespace DesignDocService.Dtos
{
    /// <summary>
    /// Query parameters for filtering and paging design documents. All properties are optional;
    /// when provided, the API will return documents matching the specified criteria.
    /// </summary>
    public class DesignDocumentQuery
    {
        public string? Team { get; set; }
        public string? Product { get; set; }
        public string? Author { get; set; }
        public DocumentStatus? Status { get; set; }
        public int? Page { get; set; }
        public int? PageSize { get; set; }
    }
}