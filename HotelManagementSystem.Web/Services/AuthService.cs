using Microsoft.EntityFrameworkCore;
using HotelManagementSystem.Web.Data;
using HotelManagementSystem.Web.Models;
using Microsoft.AspNetCore.Identity;

namespace HotelManagementSystem.Web.Services;

public class AuthService
{
    private readonly IDbContextFactory<HotelDbContext> _contextFactory;

    public AuthService(IDbContextFactory<HotelDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<Usuario?> LoginAsync(string email, string password)
    {
        using var context = _contextFactory.CreateDbContext();
        var user = await context.Usuarios.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

        if (user == null) return null;

        if (user.SenhaHash == null) return null;

        var passwordHasher = new PasswordHasher<Usuario>();
        var result = passwordHasher.VerifyHashedPassword(user, user.SenhaHash, password);

        if (result == PasswordVerificationResult.Success)
        {
            return user;
        }

        return null;
    }

    public async Task<Usuario?> GetUserAsync(int id)
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.Usuarios.FindAsync(id);
    }

    public async Task<List<Usuario>> GetAllUsersAsync()
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.Usuarios.ToListAsync();
    }

    public async Task<(List<Usuario> Users, int TotalCount)> GetUsersAsync(int page, int pageSize,
        string? search = null)
    {
        using var context = _contextFactory.CreateDbContext();
        var query = context.Usuarios.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(u =>
                u.Nome.ToLower().Contains(search.ToLower()) || u.Email.ToLower().Contains(search.ToLower()));
        }

        var totalCount = await query.CountAsync();
        var users = await query.OrderBy(u => u.Nome)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (users, totalCount);
    }

    public async Task CreateUserAsync(Usuario user, string password)
    {
        using var context = _contextFactory.CreateDbContext();

        var passwordHasher = new PasswordHasher<Usuario>();
        user.SenhaHash = passwordHasher.HashPassword(user, password);

        context.Usuarios.Add(user);
        await context.SaveChangesAsync();
    }

    public async Task UpdateUserAsync(Usuario user, string? newPassword = null)
    {
        using var context = _contextFactory.CreateDbContext();
        var existingUser = await context.Usuarios.FindAsync(user.Id);

        if (existingUser != null)
        {
            existingUser.Nome = user.Nome;
            existingUser.Email = user.Email;
            existingUser.Role = user.Role;

            if (!string.IsNullOrEmpty(newPassword))
            {
                var passwordHasher = new PasswordHasher<Usuario>();
                existingUser.SenhaHash = passwordHasher.HashPassword(existingUser, newPassword);
            }

            await context.SaveChangesAsync();
        }
    }

    public async Task DeleteUserAsync(int id)
    {
        using var context = _contextFactory.CreateDbContext();
        var user = await context.Usuarios.FindAsync(id);
        if (user != null)
        {
            context.Usuarios.Remove(user);
            await context.SaveChangesAsync();
        }
    }
}
