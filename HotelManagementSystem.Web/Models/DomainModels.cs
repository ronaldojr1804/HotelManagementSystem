using System.ComponentModel.DataAnnotations;

namespace HotelManagementSystem.Web.Models;

public class Quarto
{
    public int Id { get; set; }
    [Required] public string Numero { get; set; } = string.Empty;
    public string Tipo { get; set; } = "Standard";
    public decimal PrecoBase { get; set; }
    public bool EstaLimpo { get; set; } = true;
    public bool EstaOcupado { get; set; } = false;
    public string Detalhes { get; set; } = string.Empty;
}

public class Hospede
{
    public int Id { get; set; }
    [Required] public string Nome { get; set; } = string.Empty;
    public string CPF { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "O email informado não é válido.")]
    public string Email { get; set; } = string.Empty;

    public string Contato { get; set; } = string.Empty;
    public string Endereco { get; set; } = string.Empty;
    public List<Familiar> Familiares { get; set; } = new();
}

public class Familiar
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Parentesco { get; set; } = string.Empty; // Esposa, Filho, etc.
    public int Idade { get; set; }
    public int HospedeId { get; set; }
    public Hospede? Hospede { get; set; }
}

public class Produto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal Preco { get; set; }
}

public class Reserva
{
    public int Id { get; set; }
    public int QuartoId { get; set; }
    public Quarto? Quarto { get; set; }
    public int HospedeId { get; set; }
    public Hospede? Hospede { get; set; }
    public DateTime DataEntrada { get; set; }
    public DateTime DataSaida { get; set; }
    public DateTime? DataCheckIn { get; set; }
    public DateTime? DataCheckOut { get; set; }
    public bool Cancelada { get; set; }
    public string JustificativaCancelamento { get; set; } = string.Empty;
    public string? UsuarioCriacaoId { get; set; }
    public string? UsuarioCancelamentoId { get; set; }
    public DateTime DataCadastro { get; set; } = DateTime.Now;
    public bool CheckOutRealizado => DataCheckOut.HasValue;
    public DateTime? DataRealCheckIn { get; set; }
    public string FormaPagamento { get; set; } = "Dinheiro"; // Dinheiro, Cartão, Pix
    public int QuantidadePessoas { get; set; } = 1;
    public decimal ValorDiariaNoMomento { get; set; }
    public List<Hospede> HospedesSecundarios { get; set; } = new();
    public List<ConsumoReserva> Consumos { get; set; } = new();
    public List<Limpeza> Limpezas { get; set; } = new();

    public decimal TotalConsumo => Consumos.Sum(c => c.Total);
    public decimal TotalDiarias => (decimal)(DataSaida - DataEntrada).TotalDays * ValorDiariaNoMomento;
    public decimal TotalGeral => TotalDiarias + TotalConsumo;
}

public class Limpeza
{
    public int Id { get; set; }
    public int QuartoId { get; set; }
    public Quarto? Quarto { get; set; }
    public int? ReservaId { get; set; }
    public Reserva? Reserva { get; set; }
    public DateTime DataLimpeza { get; set; } = DateTime.Now;
    public string Tipo { get; set; } = "Diária"; // Diária, Saída
    public string Observacoes { get; set; } = string.Empty;
    public string RealizadoPor { get; set; } = string.Empty;
}

public enum UserRole
{
    Admin,
    Gerente,
    Recepcionista,
    Caixa,
    Operador
}

public class HotelConfiguration
{
    public int Id { get; set; }
    public string NomeHotel { get; set; } = "Hotel Management System";
    public string Contato { get; set; } = "(00) 0000-0000";
    public string CNPJ { get; set; } = "00.000.000/0000-00";
    public string Endereco { get; set; } = "Endereço do Hotel";
    public string NomeSistema { get; set; } = "Hotel Management System";
    public string LogoUrl { get; set; } = "/images/logo.png";
    public string LoginTitle { get; set; } = "Bem-vindo ao HotelSys";
}

public class Usuario
{
    public int Id { get; set; }
    [Required] public string Nome { get; set; } = string.Empty;
    [Required] public string Email { get; set; } = string.Empty;
    [Required] public string SenhaHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Operador;
}

public class AuditLog
{
    public int Id { get; set; }
    public string? UsuarioNome { get; set; }
    public string Acao { get; set; } = string.Empty; // Create, Update, Delete
    public string Tabela { get; set; } = string.Empty; // Quartos, Hospedes, etc.
    public string Detalhes { get; set; } = string.Empty; // JSON or description
    public DateTime DataHora { get; set; } = DateTime.Now;
}

public class ConsumoReserva
{
    public int Id { get; set; }
    public int ReservaId { get; set; }
    public Reserva? Reserva { get; set; }
    public int ProdutoId { get; set; }
    public Produto? Produto { get; set; }
    public int Quantidade { get; set; }
    public decimal ValorUnitarioNoMomento { get; set; }
    public decimal Total => Quantidade * ValorUnitarioNoMomento;
    public DateTime DataConsumo { get; set; } = DateTime.Now;
}
