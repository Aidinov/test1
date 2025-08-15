using DesignDocService.Services;

namespace DesignDocService.Endpoints
{
    /// <summary>
    /// Extension methods for registering product-related API endpoints.
    /// </summary>
    public static class ProductEndpoints
    {
        public static void MapProductEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/products", (ProductService productService) => productService.GetProducts());
        }
    }
}