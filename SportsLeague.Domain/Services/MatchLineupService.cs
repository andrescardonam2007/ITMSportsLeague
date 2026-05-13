using Microsoft.Extensions.Logging;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Enums;
using SportsLeague.Domain.Interfaces.Repositories;
using SportsLeague.Domain.Interfaces.Services;

namespace SportsLeague.Domain.Services;

public class MatchLineupService : IMatchLineupService
{
    private readonly IMatchLineupRepository _matchLineupRepository;
    private readonly IMatchRepository _matchRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly ILogger<MatchLineupService> _logger;

    public MatchLineupService(
        IMatchLineupRepository matchLineupRepository,
        IMatchRepository matchRepository,
        IPlayerRepository playerRepository,
        ILogger<MatchLineupService> logger)
    {
        _matchLineupRepository = matchLineupRepository;
        _matchRepository = matchRepository;
        _playerRepository = playerRepository;
        _logger = logger;
    }

    public async Task<MatchLineup> RegisterLineupAsync(int matchId, MatchLineup lineup)
    {
        // V1: Validar que el partido exista
        var match = await _matchRepository.GetByIdAsync(matchId);
        if (match == null)
            throw new KeyNotFoundException(
                $"No se encontró el partido con ID {matchId}");

        // V6: Solo se permiten alineaciones cuando el partido está Scheduled
        if (match.Status != MatchStatus.Scheduled)
            throw new InvalidOperationException(
                "Solo se pueden registrar alineaciones en partidos con estado Scheduled");

        // V2: Validar que el jugador exista
        var player = await _playerRepository.GetByIdAsync(lineup.PlayerId);
        if (player == null)
            throw new KeyNotFoundException(
                $"No se encontró el jugador con ID {lineup.PlayerId}");

        // V3: Validar que el jugador pertenezca a uno de los equipos del partido
        if (player.TeamId != match.HomeTeamId &&
            player.TeamId != match.AwayTeamId)
        {
            throw new InvalidOperationException(
                "El jugador no pertenece a ninguno de los equipos del partido");
        }

        // V4: Validar que el jugador no esté ya registrado
        var alreadyExists = await _matchLineupRepository
            .ExistsByMatchAndPlayerAsync(matchId, lineup.PlayerId);

        if (alreadyExists)
            throw new InvalidOperationException(
                "El jugador ya está registrado en la alineación de este partido");
        // V5: Máximo 11 titulares por equipo
        if (lineup.IsStarter)
        {
            var currentLineup = await _matchLineupRepository
                .GetByMatchAndTeamAsync(matchId, player.TeamId);

            var startersCount = currentLineup.Count(l => l.IsStarter);

            if (startersCount >= 11)
                throw new InvalidOperationException(
                    "El equipo ya tiene 11 titulares registrados en este partido");
        }

        // Asignar el MatchId
        lineup.MatchId = matchId;

        _logger.LogInformation(
            "Registering lineup for Match {MatchId}, Player {PlayerId}",
            matchId, lineup.PlayerId);

        return await _matchLineupRepository.CreateAsync(lineup);
    }

    public async Task<IEnumerable<MatchLineup>> GetLineupByMatchAsync(int matchId)
    {
        _logger.LogInformation(
            "Retrieving lineup for Match {MatchId}", matchId);

        return await _matchLineupRepository.GetByMatchAsync(matchId);
    }

    public async Task<IEnumerable<MatchLineup>> GetLineupByMatchAndTeamAsync(
        int matchId,
        int teamId)
    {
        _logger.LogInformation(
            "Retrieving lineup for Match {MatchId} and Team {TeamId}",
            matchId, teamId);

        return await _matchLineupRepository.GetByMatchAndTeamAsync(
            matchId, teamId);
    }

    public async Task DeleteLineupAsync(int lineupId)
    {
        var lineup = await _matchLineupRepository.GetByIdAsync(lineupId);
        if (lineup == null)
            throw new KeyNotFoundException(
                $"No se encontró la alineación con ID {lineupId}");

        _logger.LogInformation(
            "Deleting lineup with ID {LineupId}", lineupId);

        await _matchLineupRepository.DeleteAsync(lineupId);
    }
}