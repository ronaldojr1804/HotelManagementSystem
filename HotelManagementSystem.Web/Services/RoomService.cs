using Microsoft.EntityFrameworkCore;
using HotelManagementSystem.Web.Data;
using HotelManagementSystem.Web.Models;

namespace HotelManagementSystem.Web.Services;

public class RoomService
{
    private readonly IDbContextFactory<HotelDbContext> _contextFactory;
    private readonly AuditService _auditService;

    public RoomService(IDbContextFactory<HotelDbContext> contextFactory, AuditService auditService)
    {
        _contextFactory = contextFactory;
        _auditService = auditService;
    }

    public async Task<List<Quarto>> GetRoomsAsync()
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.Quartos.ToListAsync();
    }

    public async Task<Quarto?> GetRoomAsync(int id)
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.Quartos.FindAsync(id);
    }

    public async Task AddRoomAsync(Quarto quarto)
    {
        using var context = _contextFactory.CreateDbContext();
        context.Quartos.Add(quarto);
        await context.SaveChangesAsync();
        await _auditService.LogAsync("Criar", "Quartos", $"Quarto {quarto.Numero} criado.");
    }

    public async Task UpdateRoomAsync(Quarto quarto)
    {
        using var context = _contextFactory.CreateDbContext();
        context.Entry(quarto).State = EntityState.Modified;
        await context.SaveChangesAsync();
        await _auditService.LogAsync("Atualizar", "Quartos", $"Quarto {quarto.Numero} atualizado.");
    }

    public async Task DeleteRoomAsync(int id)
    {
        using var context = _contextFactory.CreateDbContext();
        var quarto = await context.Quartos.FindAsync(id);
        if (quarto != null)
        {
            context.Quartos.Remove(quarto);
            await context.SaveChangesAsync();
            await _auditService.LogAsync("Excluir", "Quartos", $"Quarto {quarto.Numero} excluído.");
        }
    }
}
