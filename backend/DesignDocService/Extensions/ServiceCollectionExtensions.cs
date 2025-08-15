using DesignDocService.Services;
// for DesignDocContext
using Microsoft.EntityFrameworkCore;

namespace DesignDocService.Extensions
{
    /// <summary>
    /// Provides extension methods for configuring dependency injection services for the
    /// DesignDocService application.  Encapsulating service registrations in this
    /// class keeps Program.cs concise and adheres to the Single Responsibility Principle.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers application services, database context and Git integration.  The
        /// configuration is supplied by ASP.NET Core and used to determine the
        /// connection string and GitLab settings.  If GitLab configuration is missing
        /// the file system based implementation will be used instead.
        /// </summary>
        /// <param name="services">Service collection to register with.</param>
        /// <param name="configuration">Application configuration (for connection strings and GitLab settings).</param>
        /// <returns>The same service collection for chaining.</returns>
        public static IServiceCollection AddDesignDocServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register EF Core context with PostgreSQL provider.  Read the connection string
            // from configuration, falling back to an environment variable when necessary.
            services.AddDbContext<DesignDocContext>(options =>
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                if (string.IsNullOrEmpty(connectionString))
                {
                    connectionString = configuration["DATABASE_CONNECTION"];
                }
                options.UseNpgsql(connectionString);
            });

            // Decide which Git service implementation to use based on configuration.  When
            // GitLab settings are provided (BaseUrl and AccessToken) register the GitLab
            // client using HttpClient.  Otherwise fall back to the file system based
            // implementation which stores files locally.
            var gitLabBaseUrl = configuration["GitLab:BaseUrl"];
            var gitLabAccessToken = configuration["GitLab:AccessToken"];
            if (!string.IsNullOrWhiteSpace(gitLabBaseUrl) && !string.IsNullOrWhiteSpace(gitLabAccessToken))
            {
                services.AddHttpClient<IGitService, GitLabService>();
            }
            else
            {
                services.AddScoped<IGitService, FileGitService>();
            }

            // Register domain services.  Use scoped lifetime for DesignDocumentService
            // because it depends on DbContext and Git service.  ProductService and
            // TeamService are stateless so singleton is appropriate.
            services.AddScoped<DesignDocumentService>();
            services.AddSingleton<ProductService>();
            services.AddSingleton<TeamService>();

            return services;
        }
    }
}