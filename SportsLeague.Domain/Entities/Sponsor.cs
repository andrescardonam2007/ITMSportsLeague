using SportsLeague.Domain.Enums;

namespace SportsLeague.Domain.Entities
{
    public class Sponsor
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string ContactEmail { get; set; } = null!;

        public string? Phone { get; set; }

        public string? WebsiteUrl { get; set; }

        public SponsorCategory Category { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
        public ICollection<TournamentSponsor> TournamentSponsors { get; set; } = new List<TournamentSponsor>();
    }
}


