namespace SportsLeague.Domain.DTOs.MatchLineup.Request
{
    public class CreateMatchLineupDTO
    {
        public int PlayerId { get; set; }

        public bool IsStarter { get; set; }

        public string Position { get; set; } = string.Empty;
    }
}