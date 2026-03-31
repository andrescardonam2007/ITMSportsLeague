using Microsoft.EntityFrameworkCore;
using SportsLeague.DataAccess.Context;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Repositories;

namespace SportsLeague.DataAccess.Repositories
{
    public class TournamentSponsorRepository : ITournamentSponsorRepository
    {
        private readonly LeagueDbContext _context;

        public TournamentSponsorRepository(LeagueDbContext context)
        {
            _context = context;
        }

        public async Task<List<TournamentSponsor>> GetBySponsorIdAsync(int sponsorId)
        {
            return await _context.TournamentSponsors
                .Where(ts => ts.SponsorId == sponsorId)
                .Include(ts => ts.Tournament)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(int sponsorId, int tournamentId)
        {
            return await _context.TournamentSponsors
                .AnyAsync(ts => ts.SponsorId == sponsorId && ts.TournamentId == tournamentId);
        }

        public async Task AddAsync(TournamentSponsor entity)
        {
            _context.TournamentSponsors.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<TournamentSponsor?> GetAsync(int sponsorId, int tournamentId)
        {
            return await _context.TournamentSponsors
                .FirstOrDefaultAsync(ts => ts.SponsorId == sponsorId && ts.TournamentId == tournamentId);
        }

        public async Task DeleteAsync(TournamentSponsor entity)
        {
            _context.TournamentSponsors.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}