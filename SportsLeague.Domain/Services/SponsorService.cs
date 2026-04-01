using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Repositories;
using SportsLeague.Domain.Interfaces.Services;


namespace SportsLeague.Domain.Services
{
    public class SponsorService : ISponsorService
    {
        private readonly ISponsorRepository _sponsorRepository;
        private readonly ILogger<SponsorService> _logger;
        private readonly ITournamentRepository _tournamentRepository;
        private readonly ITournamentSponsorRepository _tournamentSponsorRepository;

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
            _logger.LogInformation("Retrieving all sponsors");
            return await _sponsorRepository.GetAllAsync();
        }

        public async Task<Sponsor?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Retrieving sponsor with ID: {SponsorId}", id);

            var sponsor = await _sponsorRepository.GetByIdAsync(id);

            if (sponsor == null)
                _logger.LogWarning("Sponsor with ID {SponsorId} not found", id);

            return sponsor;
        }

        public async Task<Sponsor> CreateAsync(Sponsor sponsor)
        {
            // 🔴 Validación: nombre duplicado
            if (await _sponsorRepository.ExistsByNameAsync(sponsor.Name))
            {
                _logger.LogWarning("Sponsor with name '{Name}' already exists", sponsor.Name);
                throw new InvalidOperationException($"Ya existe un sponsor con el nombre '{sponsor.Name}'");
            }

            // 🔴 Validación: email válido
            if (!new EmailAddressAttribute().IsValid(sponsor.ContactEmail))
            {
                throw new InvalidOperationException("El email no tiene un formato válido");
            }

            sponsor.CreatedAt = DateTime.UtcNow;

            _logger.LogInformation("Creating sponsor: {Name}", sponsor.Name);

            return await _sponsorRepository.CreateAsync(sponsor);
        }

        public async Task UpdateAsync(int id, Sponsor sponsor)
        {
            var existingSponsor = await _sponsorRepository.GetByIdAsync(id);

            if (existingSponsor == null)
            {
                _logger.LogWarning("Sponsor with ID {SponsorId} not found for update", id);
                throw new KeyNotFoundException($"No se encontró el sponsor con ID {id}");
            }

            // 🔴 Validar nombre único (si cambió)
            if (existingSponsor.Name != sponsor.Name)
            {
                if (await _sponsorRepository.ExistsByNameAsync(sponsor.Name))
                {
                    throw new InvalidOperationException($"Ya existe un sponsor con el nombre '{sponsor.Name}'");
                }
            }

            // 🔴 Validar email
            if (!new EmailAddressAttribute().IsValid(sponsor.ContactEmail))
            {
                throw new InvalidOperationException("El email no tiene un formato válido");
            }

            // 🔄 Actualizar campos
            existingSponsor.Name = sponsor.Name;
            existingSponsor.ContactEmail = sponsor.ContactEmail;
            existingSponsor.Phone = sponsor.Phone;
            existingSponsor.WebsiteUrl = sponsor.WebsiteUrl;
            existingSponsor.Category = sponsor.Category;
            existingSponsor.UpdatedAt = DateTime.UtcNow;

            _logger.LogInformation("Updating sponsor with ID: {SponsorId}", id);

            await _sponsorRepository.UpdateAsync(existingSponsor);
        }

        public async Task DeleteAsync(int id)
        {
            var sponsor = await _sponsorRepository.GetByIdAsync(id);

            if (sponsor == null)
            {
                _logger.LogWarning("Sponsor with ID {SponsorId} not found for deletion", id);
                throw new KeyNotFoundException($"No se encontró el sponsor con ID {id}");
            }

            _logger.LogInformation("Deleting sponsor with ID: {SponsorId}", id);

            await _sponsorRepository.DeleteAsync(id);
        }

        public async Task<List<TournamentSponsor>> GetTournamentsBySponsorIdAsync(int sponsorId)
        {
            _logger.LogInformation("Retrieving tournaments for sponsor ID: {SponsorId}", sponsorId);

            var sponsor = await _sponsorRepository.GetByIdAsync(sponsorId);
            if (sponsor == null)
            {
                _logger.LogWarning("Sponsor with ID {SponsorId} not found", sponsorId);
                throw new KeyNotFoundException($"No se encontró el sponsor con ID {sponsorId}");
            }

            return sponsor.TournamentSponsors?.ToList() ?? new List<TournamentSponsor>();
        }

        public async Task<TournamentSponsor> AddSponsorToTournamentAsync(int sponsorId, int tournamentId, decimal contractAmount)
        {
            _logger.LogInformation("Adding sponsor {SponsorId} to tournament {TournamentId} with amount {Amount}", sponsorId, tournamentId, contractAmount);

            var sponsor = await _sponsorRepository.GetByIdAsync(sponsorId);
            if (sponsor == null) throw new KeyNotFoundException($"No se encontró el sponsor con ID {sponsorId}");

            var tournament = await _tournamentRepository.GetByIdAsync(tournamentId);
            if (tournament == null) throw new KeyNotFoundException($"No se encontró el torneo con ID {tournamentId}");

            if (contractAmount <= 0) throw new InvalidOperationException("El monto del contrato debe ser mayor que 0");

            if (await _tournamentSponsorRepository.ExistsAsync(sponsorId, tournamentId))
                throw new InvalidOperationException("El sponsor ya está asociado al torneo");

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
            _logger.LogInformation("Removing sponsor {SponsorId} from tournament {TournamentId}", sponsorId, tournamentId);

            var sponsor = await _sponsorRepository.GetByIdAsync(sponsorId);
            if (sponsor == null) throw new KeyNotFoundException($"No se encontró el sponsor con ID {sponsorId}");

            var tournament = await _tournamentRepository.GetByIdAsync(tournamentId);
            if (tournament == null) throw new KeyNotFoundException($"No se encontró el torneo con ID {tournamentId}");

            var existing = await _tournamentSponsorRepository.GetAsync(sponsorId, tournamentId);
            if (existing == null) throw new KeyNotFoundException("La relación sponsor-torneo no existe");

            await _tournamentSponsorRepository.DeleteAsync(existing);
        }
    }
}