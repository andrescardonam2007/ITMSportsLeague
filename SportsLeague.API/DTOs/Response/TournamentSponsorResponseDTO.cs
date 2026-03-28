namespace SportsLeague.API.DTOs.Response
{
    public class TournamentSponsorResponseDTO
    {
        public int Id { get; set; }

        public int TournamentId { get; set; }
        public string TournamentName { get; set; } = null!;

        public int SponsorId { get; set; }
        public string SponsorName { get; set; } = null!;

        public decimal ContractAmount { get; set; }

        public DateTime JoinedAt { get; set; }
    }
}