using Microsoft.EntityFrameworkCore;
using HotelManagementSystem.Web.Data;
using HotelManagementSystem.Web.Models;

namespace HotelManagementSystem.Web.Services;

public class ProductService
{
    private readonly IDbContextFactory<HotelDbContext> _contextFactory;
    private readonly AuditService _auditService;

    public ProductService(IDbContextFactory<HotelDbContext> contextFactory, AuditService auditService)
    {
        _contextFactory = contextFactory;
        _auditService = auditService;
    }

    public async Task<List<Produto>> GetProductsAsync()
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.Produtos.ToListAsync();
    }

    public async Task<(List<Produto> Products, int TotalCount)> GetProductsAsync(int page, int pageSize,
        string? search = null)
    {
        using var context = _contextFactory.CreateDbContext();
        var query = context.Produtos.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(p => p.Nome.ToLower().Contains(search.ToLower()));
        }

        var totalCount = await query.CountAsync();
        var products = await query.OrderBy(p => p.Nome)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (products, totalCount);
    }

    public async Task AddProductAsync(Produto produto)
    {
        using var context = _contextFactory.CreateDbContext();
        context.Produtos.Add(produto);
        await context.SaveChangesAsync();
        await _auditService.LogAsync("Criar", "Produtos", $"Produto {produto.Nome} criado.");
    }

    // Add other CRUD if needed
}
