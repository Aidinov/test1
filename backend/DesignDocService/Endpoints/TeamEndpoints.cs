using DesignDocService.Services;

namespace DesignDocService.Endpoints
{
    /// <summary>
    /// Extension methods for registering team-related API endpoints.
    /// </summary>
    public static class TeamEndpoints
    {
        public static void MapTeamEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/teams", (TeamService teamService) => teamService.GetTeams());
        }
    }
}