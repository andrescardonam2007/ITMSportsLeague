using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Repositories;
using SportsLeague.Domain.Interfaces.Services;

namespace SportsLeague.Domain.Services
{
    public class SponsorService : ISponsorService
    {
        private readonly ISponsorRepository _sponsorRepository;
        private readonly ITournamentRepository _tournamentRepository;
        private readonly ITournamentSponsorRepository _tournamentSponsorRepository;
        private readonly ILogger<SponsorService> _logger;

        public SponsorService(
            ISponsorRepository sponsorRepository,
            ITournamentRepository tournamentRepository,
            ITournamentSponsorRepository tournamentSponsorRepository,
            ILogger<SponsorService> logger)
        {
            _sponsorRepository = sponsorRepository;
            _tournamentRepository = tournamentRepository;
            _tournamentSponsorRepository = tournamentSponsorRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<Sponsor>> GetAllAsync()
        {
            return await _sponsorRepository.GetAllAsync();
        }

        public async Task<Sponsor?> GetByIdAsync(int id)
        {
            return await _sponsorRepository.GetByIdAsync(id);
        }

        public async Task<Sponsor> CreateAsync(Sponsor sponsor)
        {
            if (await _sponsorRepository.ExistsByNameAsync(sponsor.Name))
                throw new InvalidOperationException($"Ya existe un sponsor con el nombre '{sponsor.Name}'");

            if (!new EmailAddressAttribute().IsValid(sponsor.ContactEmail))
                throw new InvalidOperationException("Email inválido");

            sponsor.CreatedAt = DateTime.UtcNow;

            return await _sponsorRepository.CreateAsync(sponsor);
        }

        public async Task UpdateAsync(int id, Sponsor sponsor)
        {
            var existing = await _sponsorRepository.GetByIdAsync(id);

            if (existing == null)
                throw new KeyNotFoundException($"No se encontró el sponsor con ID {id}");

            if (existing.Name != sponsor.Name &&
                await _sponsorRepository.ExistsByNameAsync(sponsor.Name))
            {
                throw new InvalidOperationException($"Ya existe un sponsor con el nombre '{sponsor.Name}'");
            }

            if (!new EmailAddressAttribute().IsValid(sponsor.ContactEmail))
                throw new InvalidOperationException("Email inválido");

            existing.Name = sponsor.Name;
            existing.ContactEmail = sponsor.ContactEmail;
            existing.Phone = sponsor.Phone;
            existing.WebsiteUrl = sponsor.WebsiteUrl;
            existing.Category = sponsor.Category;
            existing.UpdatedAt = DateTime.UtcNow;

            await _sponsorRepository.UpdateAsync(existing);
        }

        public async Task DeleteAsync(int id)
        {
            var exists = await _sponsorRepository.GetByIdAsync(id);

            if (exists == null)
                throw new KeyNotFoundException($"No se encontró el sponsor con ID {id}");

            await _sponsorRepository.DeleteAsync(id);
        }

        public async Task<List<TournamentSponsor>> GetTournamentsBySponsorIdAsync(int sponsorId)
        {
            var sponsor = await _sponsorRepository.GetByIdAsync(sponsorId);
            if (sponsor == null)
                throw new KeyNotFoundException($"No se encontró el sponsor con ID {sponsorId}");

            return (await _tournamentSponsorRepository
                .GetBySponsorIdAsync(sponsorId))
                .ToList();
        }

        public async Task<TournamentSponsor> AddSponsorToTournamentAsync(int sponsorId, int tournamentId, decimal contractAmount)
        {
            var sponsor = await _sponsorRepository.GetByIdAsync(sponsorId);
            if (sponsor == null)
                throw new KeyNotFoundException($"No se encontró el sponsor con ID {sponsorId}");

            var tournament = await _tournamentRepository.GetByIdAsync(tournamentId);
            if (tournament == null)
                throw new KeyNotFoundException($"No se encontró el torneo con ID {tournamentId}");

            if (contractAmount <= 0)
                throw new InvalidOperationException("El monto debe ser mayor que 0");

            if (await _tournamentSponsorRepository.ExistsAsync(tournamentId, sponsorId))
                throw new InvalidOperationException("El sponsor ya está asociado");

            var relation = new TournamentSponsor
            {
                SponsorId = sponsorId,
                TournamentId = tournamentId,
                ContractAmount = contractAmount,
                JoinedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            await _tournamentSponsorRepository.AddAsync(relation);

            return relation;
        }

        public async Task RemoveSponsorFromTournamentAsync(int sponsorId, int tournamentId)
        {
            var existingList = await _tournamentSponsorRepository.GetBySponsorIdAsync(sponsorId);

            var relation = existingList.FirstOrDefault(ts => ts.TournamentId == tournamentId);

            if (relation == null)
                throw new KeyNotFoundException("Relación no encontrada");

            await _tournamentSponsorRepository.DeleteAsync(relation);
        }
    }
}