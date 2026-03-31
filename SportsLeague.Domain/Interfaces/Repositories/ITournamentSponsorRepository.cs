using SportsLeague.Domain.Entities;

namespace SportsLeague.Domain.Interfaces.Repositories
{
    public interface ITournamentSponsorRepository
    {
        Task<List<TournamentSponsor>> GetBySponsorIdAsync(int sponsorId);
        Task<bool> ExistsAsync(int sponsorId, int tournamentId);
        Task AddAsync(TournamentSponsor entity);
        Task<TournamentSponsor?> GetAsync(int sponsorId, int tournamentId);
        Task DeleteAsync(TournamentSponsor entity);
    }
}
