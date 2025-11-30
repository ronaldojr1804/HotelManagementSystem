using Microsoft.EntityFrameworkCore;
using HotelManagementSystem.Web.Models;
using Microsoft.AspNetCore.Identity;

namespace HotelManagementSystem.Web.Data;

public class HotelDbContext : DbContext
{
    public HotelDbContext(DbContextOptions<HotelDbContext> options) : base(options)
    {
    }

    public DbSet<Quarto> Quartos { get; set; }
    public DbSet<Hospede> Hospedes { get; set; }
    public DbSet<Familiar> Familiares { get; set; }
    public DbSet<Produto> Produtos { get; set; }
    public DbSet<Reserva> Reservas { get; set; }
    public DbSet<ConsumoReserva> Consumos { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<Limpeza> Limpezas { get; set; }
    public DbSet<HotelConfiguration> Configuracoes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurações adicionais se necessário
        modelBuilder.Entity<Reserva>()
            .HasOne(r => r.Quarto)
            .WithMany()
            .HasForeignKey(r => r.QuartoId);

        modelBuilder.Entity<Reserva>()
            .HasOne(r => r.Hospede)
            .WithMany()
            .HasForeignKey(r => r.HospedeId);

        modelBuilder.Entity<Familiar>()
            .HasOne(f => f.Hospede)
            .WithMany(h => h.Familiares)
            .HasForeignKey(f => f.HospedeId);

        // Seed Default User (admin/admin)
        var admin = new Usuario
        {
            Id = 1,
            Nome = "Administrador",
            Email = "admin@admin",
            Role = UserRole.Admin
        };

        var passwordHasher = new PasswordHasher<Usuario>();
        admin.SenhaHash = passwordHasher.HashPassword(admin, "admin");

        modelBuilder.Entity<Usuario>().HasData(admin);

        // Seed Initial Room
        modelBuilder.Entity<Quarto>().HasData(new Quarto
        {
            Id = 1,
            Numero = "101",
            Tipo = "Simples",
            PrecoBase = 100.00m,
            EstaLimpo = true,
            EstaOcupado = false,
            Detalhes = "Quarto simples com cama de solteiro"
        });
    }
}
