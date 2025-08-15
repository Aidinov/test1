using DesignDocService.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DesignDocService.Tests.Infrastructure
{
    /// <summary>
    /// A custom WebApplicationFactory that configures the API to use SQLite instead of PostgreSQL for testing.
    /// It also ensures the database is created for each test run.
    /// </summary>
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing DesignDocContext registration
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<DesignDocContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Remove any existing IGitService registration so that tests always use the file based implementation.
                var gitServiceDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IGitService));
                if (gitServiceDescriptor != null)
                {
                    services.Remove(gitServiceDescriptor);
                }

                // Register SQLite database for tests
                // Use a file based database to persist across the lifetime of the server
                var tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".db");
                services.AddDbContext<DesignDocContext>(options => options.UseSqlite($"Data Source={tempFile}"));

                // Register the file based Git service.  Tests should never attempt to hit a real GitLab instance.
                services.AddScoped<IGitService, FileGitService>();

                // Build the service provider and initialize the database
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<DesignDocContext>();
                context.Database.EnsureCreated();
            });
        }
    }
}