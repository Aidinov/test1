namespace DesignDocService.Services
{
    /// <summary>
    /// Returns a static set of products for demonstration purposes.
    /// In a real system this would call an external API or database.
    /// </summary>
    public class ProductService
    {
        /// <summary>
        /// Returns a list of product names.
        /// </summary>
        public IEnumerable<string> GetProducts()
        {
            // In a future iteration this could fetch from an external service.
            return new List<string>
            {
                "Product Alpha",
                "Product Beta",
                "Product Gamma"
            };
        }
    }
}