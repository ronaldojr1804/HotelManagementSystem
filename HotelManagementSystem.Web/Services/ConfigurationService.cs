using HotelManagementSystem.Web.Data;
using HotelManagementSystem.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem.Web.Services;

public class ConfigurationService
{
    private readonly IDbContextFactory<HotelDbContext> _contextFactory;
    private readonly AuditService _auditService;

    public ConfigurationService(IDbContextFactory<HotelDbContext> contextFactory, AuditService auditService)
    {
        _contextFactory = contextFactory;
        _auditService = auditService;
    }

    public async Task<HotelConfiguration> GetConfigurationAsync()
    {
        using var context = _contextFactory.CreateDbContext();
        var config = await context.Configuracoes.FirstOrDefaultAsync();

        if (config == null)
        {
            config = new HotelConfiguration();
            context.Configuracoes.Add(config);
            await context.SaveChangesAsync();
        }

        return config;
    }

    public async Task UpdateConfigurationAsync(HotelConfiguration config)
    {
        using var context = _contextFactory.CreateDbContext();
        context.Configuracoes.Update(config);
        await context.SaveChangesAsync();
        
        await _auditService.LogAsync("Update", "Configuracoes", $"Configurações do hotel atualizadas.");
    }
}
