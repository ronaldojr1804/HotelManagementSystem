using Microsoft.EntityFrameworkCore;
using HotelManagementSystem.Web.Data;
using HotelManagementSystem.Web.Models;

namespace HotelManagementSystem.Web.Services;

public class ReservationService
{
    private readonly IDbContextFactory<HotelDbContext> _contextFactory;
    private readonly AuditService _auditService;

    public ReservationService(IDbContextFactory<HotelDbContext> contextFactory, AuditService auditService)
    {
        _contextFactory = contextFactory;
        _auditService = auditService;
    }

    public async Task<List<Reserva>> GetReservationsAsync()
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.Reservas
            .Include(r => r.Quarto)
            .Include(r => r.Hospede)
            .Include(r => r.Consumos)
            .ThenInclude(c => c.Produto)
            .OrderByDescending(r => r.DataEntrada)
            .ToListAsync();
    }

    public async Task<Reserva?> GetReservationAsync(int id)
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.Reservas
            .Include(r => r.Quarto)
            .Include(r => r.Hospede)
            .Include(r => r.Consumos)
            .ThenInclude(c => c.Produto)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<bool> IsRoomAvailableAsync(int roomId, DateTime start, DateTime end, int? excludeReservationId = null)
    {
        using var context = _contextFactory.CreateDbContext();
        var query = context.Reservas
            .Where(r => r.QuartoId == roomId && !r.Cancelada && !r.DataCheckOut.HasValue);

        if (excludeReservationId.HasValue)
        {
            query = query.Where(r => r.Id != excludeReservationId.Value);
        }

        // Adjust dates for Check-in (14h) and Check-out (12h)
        // Start date is the proposed check-in date. We assume check-in is at 14:00.
        // End date is the proposed check-out date. We assume check-out is at 12:00.
        
        // Existing reservations also follow this rule implicitly or explicitly.
        // We need to check if the time ranges overlap.
        
        // Range A (New): [Start + 14h, End + 12h]
        // Range B (Existing): [Entrada + 14h, Saida + 12h]
        
        // Overlap condition: StartA < EndB && EndA > StartB
        
        var startDt = start.Date.AddHours(14);
        var endDt = end.Date.AddHours(12);

        // We fetch potential conflicts and filter in memory or compose a complex query
        // Since EF Core might have issues with date additions in all providers, let's try to be safe.
        // But for SQLite/SQLServer, simple date comparison usually works if we treat them as dates.
        // However, the user wants precise time control.
        
        // Let's stick to the logic:
        // A reservation R overlaps if:
        // (NewStart < R.End) AND (NewEnd > R.Start)
        
        // Where R.Start = R.DataEntrada + 14h
        // Where R.End = R.DataSaida + 12h
        
        // Simplifying for LINQ translation (avoiding AddHours inside query if possible, but let's try):
        // We can just compare dates if we are careful.
        // If NewStartDay == ExistingEndDay:
        //    NewStart (14h) > ExistingEnd (12h) -> No Overlap.
        // If NewEndDay == ExistingStartDay:
        //    NewEnd (12h) < ExistingStart (14h) -> No Overlap.
        
        // So, strictly strictly:
        // Overlap if:
        // NewStartDay < ExistingEndDay AND NewEndDay > ExistingStartDay
        // BUT, if NewStartDay == ExistingEndDay, it is allowed (14h > 12h).
        // If NewEndDay == ExistingStartDay, it is allowed (12h < 14h).
        
        // So the condition for OVERLAP is actually:
        // NewStartDay < ExistingEndDay AND NewEndDay > ExistingStartDay
        // Wait, if NewStart = 05/11 (14h) and ExistingEnd = 05/11 (12h).
        // NewStart < ExistingEnd? No. 14h is not < 12h.
        // So they don't overlap.
        
        // If NewStart = 04/11 and ExistingEnd = 05/11.
        // NewStart < ExistingEnd. Yes.
        
        // So, the simple date comparison:
        // r.DataEntrada < end && r.DataSaida > start
        // This was preventing same day overlap.
        // Example: New(1/11 - 5/11), Existing(28/10 - 4/12).
        // Existing covers New. Overlap.
        
        // Example: New(1/11 - 5/11). Existing(28/10 - 1/11).
        // Existing ends 1/11 (12h). New starts 1/11 (14h).
        // Old Logic: 28/10 < 5/11 AND 1/11 > 1/11 (False). No Overlap. Correct.
        
        // Example: New(1/11 - 5/11). Existing(5/11 - 10/11).
        // Existing starts 5/11 (14h). New ends 5/11 (12h).
        // Old Logic: 5/11 < 5/11 (False). No Overlap. Correct.
        
        // So the strict inequality (>) and (<) actually handles the "touching" dates correctly IF we treat them as full days boundaries.
        // The user said: "when I select 1/11 to 5/11 I cannot select the room" (if there is a reservation 28/11 to 04/12?? No, that example was confusing).
        // User said: "reserva do dia 28/11 até 04/12 e quando seleciono 1/11 até 05/11 não consigo selecionar".
        // 28/11 to 04/12 is completely AFTER 1/11 to 05/11.
        // 28/11 < 05/11 (False). 04/12 > 01/11 (True).
        // Result: False. Should be available.
        
        // Wait, maybe the user meant 28/10 to 04/11?
        // If 28/10 to 04/11.
        // 28/10 < 05/11 (True). 04/11 > 01/11 (True).
        // Result: True (Overlap).
        // BUT 04/11 (End of existing) vs 01/11 (Start of new). They overlap 01, 02, 03. Yes.
        
        // Let's re-read the user example carefully:
        // "reserva do dia 28/11 até 04/12 e quando seleciono 1/11 até 05/11 não consigo selecionar"
        // 28 Nov - 04 Dec.
        // 01 Nov - 05 Nov.
        // These are completely disjoint. 05 Nov is weeks before 28 Nov.
        // Why would it be blocked?
        // Maybe the user meant 28/10? Or maybe the logic was just wrong?
        // Or maybe I was using OR instead of AND?
        // Code was: `r.DataEntrada < end && r.DataSaida > start`
        // Existing: Start=28/11, End=04/12.
        // New: Start=01/11, End=05/11.
        // 28/11 < 05/11 (False).
        // So it returns False (No overlap).
        
        // If the user says they couldn't select, maybe it was because of `EstaOcupado` flag?
        // YES! The `EstaOcupado` flag was preventing selection in the Modal!
        // I just removed that check in the previous step.
        
        // So the logic `r.DataEntrada < end && r.DataSaida > start` is actually correct for "touching" dates if we consider:
        // Start < End (Strict).
        // If Existing Ends on X, and New Starts on X.
        // Existing.Start < New.End AND Existing.End > New.Start
        // ... < ... AND X > X (False). No overlap.
        
        // So the logic IS correct for the 14h/12h rule implicitly, assuming the dates stored are just the dates.
        // I will keep the logic but ensure `EstaOcupado` is NOT used for future availability checks.
        
        return !await query.AnyAsync(r => r.DataEntrada < end && r.DataSaida > start);
    }

    public async Task<HashSet<int>> GetAvailableRoomIdsAsync(DateTime start, DateTime end)
    {
        using var context = _contextFactory.CreateDbContext();
        
        // Find all reservations that overlap with the requested period
        // and are NOT cancelled and NOT checked out (actually check out doesn't matter for future, 
        // but if it's checked out it means it's done, so it shouldn't block future. 
        // Wait, if I check out early, the room becomes free.
        // So !DataCheckOut.HasValue is correct for "active" reservations blocking the room.
        // BUT, for future reservations, DataCheckOut is obviously null.
        // So we just check for overlap.
        
        var conflictingReservations = await context.Reservas
            .Where(r => !r.Cancelada && !r.DataCheckOut.HasValue && r.DataEntrada < end && r.DataSaida > start)
            .Select(r => r.QuartoId)
            .ToListAsync();

        var allRoomIds = await context.Quartos.Select(q => q.Id).ToListAsync();
        
        var availableRoomIds = new HashSet<int>(allRoomIds);
        foreach (var id in conflictingReservations)
        {
            availableRoomIds.Remove(id);
        }
        
        return availableRoomIds;
    }

    public async Task CreateReservationAsync(Reserva reserva)
    {
        using var context = _contextFactory.CreateDbContext();
        
        // Snapshot room price
        var quarto = await context.Quartos.FindAsync(reserva.QuartoId);
        if (quarto != null)
        {
            reserva.ValorDiariaNoMomento = quarto.PrecoBase;
            
            // Only mark as occupied if it starts TODAY
            if (reserva.DataEntrada.Date <= DateTime.Today && reserva.DataSaida.Date > DateTime.Today)
            {
                quarto.EstaOcupado = true;
            }
        }

        if (reserva.HospedesSecundarios.Any())
        {
            foreach (var guest in reserva.HospedesSecundarios)
            {
                context.Attach(guest);
            }
        }

        reserva.DataCadastro = DateTime.Now;
        context.Reservas.Add(reserva);
        await context.SaveChangesAsync();
        await _auditService.LogAsync("Criar", "Reservas", $"Reserva criada para Quarto {reserva.QuartoId}.");
    }

    public async Task CheckInAsync(int reservaId)
    {
        using var context = _contextFactory.CreateDbContext();
        var reserva = await context.Reservas.Include(r => r.Quarto).FirstOrDefaultAsync(r => r.Id == reservaId);
        
        if (reserva != null && !reserva.DataRealCheckIn.HasValue)
        {
            reserva.DataRealCheckIn = DateTime.Now;
            if (reserva.Quarto != null)
            {
                reserva.Quarto.EstaOcupado = true;
            }
            await context.SaveChangesAsync();
            await _auditService.LogAsync("CheckIn", "Reservas", $"Check-in realizado para Reserva {reservaId}.");
        }
    }

    public async Task CancelReservationAsync(int id, string justificativa, string userId)
    {
        using var context = _contextFactory.CreateDbContext();
        var reserva = await context.Reservas.Include(r => r.Quarto).FirstOrDefaultAsync(r => r.Id == id);
        
        if (reserva != null)
        {
            reserva.Cancelada = true;
            reserva.JustificativaCancelamento = justificativa;
            reserva.UsuarioCancelamentoId = userId;
            
            if (reserva.Quarto != null)
            {
                // If it was occupied by this reservation, free it
                // We can check if today is within the range, or just force free if it was the active one
                // Simple approach: Set false. If there's another reservation today, it should have set it true, but this is a corner case.
                // Ideally we re-evaluate occupancy.
                reserva.Quarto.EstaOcupado = false;
            }
            
            await context.SaveChangesAsync();
            await _auditService.LogAsync("Cancelar", "Reservas", $"Reserva {id} cancelada. Motivo: {justificativa}");
        }
    }

    public async Task AddCleaningAsync(Limpeza limpeza)
    {
        using var context = _contextFactory.CreateDbContext();
        context.Limpezas.Add(limpeza);
        
        // Update Room Status
        var quarto = await context.Quartos.FindAsync(limpeza.QuartoId);
        if (quarto != null)
        {
            quarto.EstaLimpo = true;
        }
        
        await context.SaveChangesAsync();
        await _auditService.LogAsync("Limpeza", "Limpezas", $"Limpeza registrada para Quarto {limpeza.QuartoId}. Tipo: {limpeza.Tipo}");
    }

    public async Task<List<Limpeza>> GetCleaningsByReservationAsync(int reservationId)
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.Limpezas
            .Where(l => l.ReservaId == reservationId)
            .OrderByDescending(l => l.DataLimpeza)
            .ToListAsync();
    }

    public async Task<List<Limpeza>> GetAllCleaningsAsync()
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.Limpezas
            .Include(l => l.Quarto)
            .Include(l => l.Reserva)
                .ThenInclude(r => r.Hospede)
            .OrderByDescending(l => l.DataLimpeza)
            .ToListAsync();
    }

    public async Task AddConsumptionAsync(int reservaId, int produtoId, int quantidade)
    {
        using var context = _contextFactory.CreateDbContext();
        var produto = await context.Produtos.FindAsync(produtoId);
        
        if (produto != null)
        {
            var consumo = new ConsumoReserva
            {
                ReservaId = reservaId,
                ProdutoId = produtoId,
                Quantidade = quantidade,
                ValorUnitarioNoMomento = produto.Preco,
                DataConsumo = DateTime.Now
            };
            
            context.Consumos.Add(consumo);
            await context.SaveChangesAsync();
        }
    }

    public async Task CheckOutAsync(int reservaId)
    {
        using var context = _contextFactory.CreateDbContext();
        var reserva = await context.Reservas.Include(r => r.Quarto).FirstOrDefaultAsync(r => r.Id == reservaId);
        
        if (reserva != null)
        {
            reserva.DataCheckOut = DateTime.Now;
            if (reserva.Quarto != null)
            {
                reserva.Quarto.EstaOcupado = false;
                reserva.Quarto.EstaLimpo = false; // Needs cleaning
            }
            await context.SaveChangesAsync();
        }
    }
    public async Task<Reserva?> GetActiveReservationForRoomAsync(int roomId)
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.Reservas
            .Include(r => r.Hospede)
            .Where(r => r.QuartoId == roomId && !r.Cancelada && !r.DataCheckOut.HasValue && r.DataEntrada <= DateTime.Now && r.DataSaida >= DateTime.Today)
            .OrderByDescending(r => r.DataEntrada)
            .FirstOrDefaultAsync();
    }
}
