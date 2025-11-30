using Microsoft.EntityFrameworkCore;
using HotelManagementSystem.Web.Data;
using HotelManagementSystem.Web.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace HotelManagementSystem.Web.Services;

public class AuditService
{
    private readonly IDbContextFactory<HotelDbContext> _contextFactory;
    private readonly AuthenticationStateProvider _authenticationStateProvider;

    public AuditService(IDbContextFactory<HotelDbContext> contextFactory,
        AuthenticationStateProvider authenticationStateProvider)
    {
        _contextFactory = contextFactory;
        _authenticationStateProvider = authenticationStateProvider;
    }

    public async Task LogAsync(string acao, string tabela, string detalhes, string? usuarioNome = null)
    {
        using var context = _contextFactory.CreateDbContext();

        // If user is not provided, try to get from auth state
        if (string.IsNullOrEmpty(usuarioNome))
        {
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            usuarioNome = authState.User.Identity?.Name ?? "Sistema";
        }

        var log = new AuditLog
        {
            Acao = acao,
            Tabela = tabela,
            Detalhes = detalhes,
            UsuarioNome = usuarioNome,
            DataHora = DateTime.Now
        };

        context.AuditLogs.Add(log);
        await context.SaveChangesAsync();
    }

    public async Task<(List<AuditLog> Logs, int TotalCount)> GetLogsAsync(int page, int pageSize, string? user = null,
        string? action = null, string? table = null, DateTime? startDate = null, DateTime? endDate = null,
        string? search = null)
    {
        using var context = _contextFactory.CreateDbContext();
        var query = context.AuditLogs.AsQueryable();

        if (!string.IsNullOrEmpty(user))
        {
            query = query.Where(l => l.UsuarioNome != null && l.UsuarioNome.ToLower().Contains(user.ToLower()));
        }

        if (!string.IsNullOrEmpty(action))
        {
            query = query.Where(l => l.Acao == action);
        }

        if (!string.IsNullOrEmpty(table))
        {
            query = query.Where(l => l.Tabela == table);
        }

        if (startDate.HasValue)
        {
            query = query.Where(l => l.DataHora >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(l => l.DataHora <= endDate.Value);
        }

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(l => l.Detalhes.ToLower().Contains(search.ToLower()));
        }

        var totalCount = await query.CountAsync();
        var logs = await query.OrderByDescending(l => l.DataHora)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (logs, totalCount);
    }
}
