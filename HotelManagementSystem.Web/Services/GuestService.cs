using Microsoft.EntityFrameworkCore;
using HotelManagementSystem.Web.Data;
using HotelManagementSystem.Web.Models;

namespace HotelManagementSystem.Web.Services;

public class GuestService
{
    private readonly IDbContextFactory<HotelDbContext> _contextFactory;
    private readonly AuditService _auditService;

    public GuestService(IDbContextFactory<HotelDbContext> contextFactory, AuditService auditService)
    {
        _contextFactory = contextFactory;
        _auditService = auditService;
    }

    public async Task<List<Hospede>> GetGuestsAsync()
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.Hospedes.Include(h => h.Familiares).ToListAsync();
    }

    public async Task<(List<Hospede> Guests, int TotalCount)> GetGuestsAsync(int page, int pageSize,
        string? search = null)
    {
        using var context = _contextFactory.CreateDbContext();
        var query = context.Hospedes.Include(h => h.Familiares).AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(h => h.Nome.ToLower().Contains(search.ToLower()) || h.CPF.Contains(search));
        }

        var totalCount = await query.CountAsync();
        var guests = await query.OrderBy(h => h.Nome)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (guests, totalCount);
    }

    public async Task<Hospede?> GetGuestAsync(int id)
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.Hospedes.Include(h => h.Familiares).FirstOrDefaultAsync(h => h.Id == id);
    }

    public async Task AddGuestAsync(Hospede hospede)
    {
        using var context = _contextFactory.CreateDbContext();
        context.Hospedes.Add(hospede);
        await context.SaveChangesAsync();
        await _auditService.LogAsync("Criar", "Hóspedes", $"Hóspede {hospede.Nome} criado.");
    }

    public async Task UpdateGuestAsync(Hospede hospede)
    {
        using var context = _contextFactory.CreateDbContext();

        // Fetch existing to handle relations properly if needed, or just Update
        // For simplicity with disconnected graph:
        var existingGuest = await context.Hospedes
            .Include(h => h.Familiares)
            .FirstOrDefaultAsync(h => h.Id == hospede.Id);

        if (existingGuest != null)
        {
            // Update scalar properties
            context.Entry(existingGuest).CurrentValues.SetValues(hospede);

            // Handle collection: Remove missing
            foreach (var existingFamiliar in existingGuest.Familiares.ToList())
            {
                if (!hospede.Familiares.Any(f => f.Id == existingFamiliar.Id))
                {
                    context.Familiares.Remove(existingFamiliar);
                }
            }

            // Add/Update
            foreach (var familiar in hospede.Familiares)
            {
                var existingFamiliar = existingGuest.Familiares.FirstOrDefault(f => f.Id == familiar.Id);
                if (existingFamiliar != null)
                {
                    context.Entry(existingFamiliar).CurrentValues.SetValues(familiar);
                }
                else
                {
                    existingGuest.Familiares.Add(familiar);
                }
            }

            await context.SaveChangesAsync();
        }
    }

    public async Task DeleteGuestAsync(int id)
    {
        using var context = _contextFactory.CreateDbContext();
        var hospede = await context.Hospedes.FindAsync(id);
        if (hospede != null)
        {
            context.Hospedes.Remove(hospede);
            await context.SaveChangesAsync();
        }
    }
}
