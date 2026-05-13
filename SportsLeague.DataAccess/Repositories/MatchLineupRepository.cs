using Microsoft.EntityFrameworkCore;
using SportsLeague.DataAccess.Context;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Repositories;

namespace SportsLeague.DataAccess.Repositories
{
    public class MatchLineupRepository
        : GenericRepository<MatchLineup>, IMatchLineupRepository
    {
        private readonly LeagueDbContext _context;

        public MatchLineupRepository(LeagueDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MatchLineup>> GetByMatchAsync(int matchId)
        {
            return await _context.MatchLineups
                .Include(ml => ml.Player)
                    .ThenInclude(p => p.Team)
                .Where(ml => ml.MatchId == matchId)
                .OrderByDescending(ml => ml.IsStarter)
                .ThenBy(ml => ml.Position)
                .ToListAsync();
        }

        public async Task<IEnumerable<MatchLineup>> GetByMatchAndTeamAsync(int matchId, int teamId)
        {
            return await _context.MatchLineups
                .Include(ml => ml.Player)
                    .ThenInclude(p => p.Team)
                .Where(ml => ml.MatchId == matchId &&
                             ml.Player.TeamId == teamId)
                .OrderByDescending(ml => ml.IsStarter)
                .ThenBy(ml => ml.Position)
                .ToListAsync();
        }

        public async Task<bool> ExistsByMatchAndPlayerAsync(int matchId, int playerId)
        {
            return await _context.MatchLineups
                .AnyAsync(ml => ml.MatchId == matchId &&
                                ml.PlayerId == playerId);
        }
    }
}