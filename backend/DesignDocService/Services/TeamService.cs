namespace DesignDocService.Services
{
    /// <summary>
    /// Returns a static set of teams for demonstration purposes.
    /// </summary>
    public class TeamService
    {
        public IEnumerable<string> GetTeams()
        {
            return new List<string>
            {
                "Team Rocket",
                "Team Delta",
                "Team Sigma"
            };
        }
    }
}