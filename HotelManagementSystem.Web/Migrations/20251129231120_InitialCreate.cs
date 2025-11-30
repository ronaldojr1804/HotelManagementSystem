using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HotelManagementSystem.Web.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var isSqlite = migrationBuilder.ActiveProvider == "Microsoft.EntityFrameworkCore.Sqlite";
            var decimalType = isSqlite ? "TEXT" : "decimal(18,2)";
            var boolType = isSqlite ? "INTEGER" : "boolean";
            var dateTimeType = isSqlite ? "TEXT" : "timestamp without time zone";

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioNome = table.Column<string>(type: "TEXT", nullable: true),
                    Acao = table.Column<string>(type: "TEXT", nullable: false),
                    Tabela = table.Column<string>(type: "TEXT", nullable: false),
                    Detalhes = table.Column<string>(type: "TEXT", nullable: false),
                    DataHora = table.Column<DateTime>(type: dateTimeType, nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_AuditLogs", x => x.Id); });

            migrationBuilder.CreateTable(
                name: "Configuracoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NomeHotel = table.Column<string>(type: "TEXT", nullable: false),
                    Contato = table.Column<string>(type: "TEXT", nullable: false),
                    CNPJ = table.Column<string>(type: "TEXT", nullable: false),
                    Endereco = table.Column<string>(type: "TEXT", nullable: false),
                    NomeSistema = table.Column<string>(type: "TEXT", nullable: false),
                    LogoUrl = table.Column<string>(type: "TEXT", nullable: false),
                    LoginTitle = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_Configuracoes", x => x.Id); });

            migrationBuilder.CreateTable(
                name: "Produtos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "TEXT", nullable: false),
                    Preco = table.Column<decimal>(type: decimalType, nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_Produtos", x => x.Id); });

            migrationBuilder.CreateTable(
                name: "Quartos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Numero = table.Column<string>(type: "TEXT", nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", nullable: false),
                    PrecoBase = table.Column<decimal>(type: decimalType, nullable: false),
                    EstaLimpo = table.Column<bool>(type: boolType, nullable: false),
                    EstaOcupado = table.Column<bool>(type: boolType, nullable: false),
                    Detalhes = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_Quartos", x => x.Id); });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    SenhaHash = table.Column<string>(type: "TEXT", nullable: false),
                    Role = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_Usuarios", x => x.Id); });

            migrationBuilder.CreateTable(
                name: "Consumos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReservaId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProdutoId = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantidade = table.Column<int>(type: "INTEGER", nullable: false),
                    ValorUnitarioNoMomento = table.Column<decimal>(type: decimalType, nullable: false),
                    DataConsumo = table.Column<DateTime>(type: dateTimeType, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Consumos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Consumos_Produtos_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produtos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Familiares",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "TEXT", nullable: false),
                    Parentesco = table.Column<string>(type: "TEXT", nullable: false),
                    Idade = table.Column<int>(type: "INTEGER", nullable: false),
                    HospedeId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_Familiares", x => x.Id); });

            migrationBuilder.CreateTable(
                name: "Hospedes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "TEXT", nullable: false),
                    CPF = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    Contato = table.Column<string>(type: "TEXT", nullable: false),
                    Endereco = table.Column<string>(type: "TEXT", nullable: false),
                    ReservaId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table => { table.PrimaryKey("PK_Hospedes", x => x.Id); });

            migrationBuilder.CreateTable(
                name: "Reservas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QuartoId = table.Column<int>(type: "INTEGER", nullable: false),
                    HospedeId = table.Column<int>(type: "INTEGER", nullable: false),
                    DataEntrada = table.Column<DateTime>(type: dateTimeType, nullable: false),
                    DataSaida = table.Column<DateTime>(type: dateTimeType, nullable: false),
                    DataCheckIn = table.Column<DateTime>(type: dateTimeType, nullable: true),
                    DataCheckOut = table.Column<DateTime>(type: dateTimeType, nullable: true),
                    Cancelada = table.Column<bool>(type: boolType, nullable: false),
                    JustificativaCancelamento = table.Column<string>(type: "TEXT", nullable: false),
                    UsuarioCriacaoId = table.Column<string>(type: "TEXT", nullable: true),
                    UsuarioCancelamentoId = table.Column<string>(type: "TEXT", nullable: true),
                    DataCadastro = table.Column<DateTime>(type: dateTimeType, nullable: false),
                    DataRealCheckIn = table.Column<DateTime>(type: dateTimeType, nullable: true),
                    FormaPagamento = table.Column<string>(type: "TEXT", nullable: false),
                    QuantidadePessoas = table.Column<int>(type: "INTEGER", nullable: false),
                    ValorDiariaNoMomento = table.Column<decimal>(type: decimalType, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reservas_Hospedes_HospedeId",
                        column: x => x.HospedeId,
                        principalTable: "Hospedes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reservas_Quartos_QuartoId",
                        column: x => x.QuartoId,
                        principalTable: "Quartos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Limpezas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QuartoId = table.Column<int>(type: "INTEGER", nullable: false),
                    ReservaId = table.Column<int>(type: "INTEGER", nullable: true),
                    DataLimpeza = table.Column<DateTime>(type: dateTimeType, nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", nullable: false),
                    Observacoes = table.Column<string>(type: "TEXT", nullable: false),
                    RealizadoPor = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Limpezas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Limpezas_Quartos_QuartoId",
                        column: x => x.QuartoId,
                        principalTable: "Quartos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Limpezas_Reservas_ReservaId",
                        column: x => x.ReservaId,
                        principalTable: "Reservas",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "Quartos",
                columns: new[] { "Id", "Detalhes", "EstaLimpo", "EstaOcupado", "Numero", "PrecoBase", "Tipo" },
                values: new object[]
                    { 1, "Quarto simples com cama de solteiro", true, false, "101", 100.00m, "Simples" });

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "Email", "Nome", "Role", "SenhaHash" },
                values: new object[]
                {
                    1, "admin@admin", "Administrador", 0,
                    "AQAAAAIAAYagAAAAEDWbhmADD0LL4a/BaHuU0EHz6EnQnuJH0vKv0J0RjkhBpbeEQ2ZDtxJO3OB3WRaY/w=="
                });

            migrationBuilder.CreateIndex(
                name: "IX_Consumos_ProdutoId",
                table: "Consumos",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_Consumos_ReservaId",
                table: "Consumos",
                column: "ReservaId");

            migrationBuilder.CreateIndex(
                name: "IX_Familiares_HospedeId",
                table: "Familiares",
                column: "HospedeId");

            migrationBuilder.CreateIndex(
                name: "IX_Hospedes_ReservaId",
                table: "Hospedes",
                column: "ReservaId");

            migrationBuilder.CreateIndex(
                name: "IX_Limpezas_QuartoId",
                table: "Limpezas",
                column: "QuartoId");

            migrationBuilder.CreateIndex(
                name: "IX_Limpezas_ReservaId",
                table: "Limpezas",
                column: "ReservaId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_HospedeId",
                table: "Reservas",
                column: "HospedeId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_QuartoId",
                table: "Reservas",
                column: "QuartoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Consumos_Reservas_ReservaId",
                table: "Consumos",
                column: "ReservaId",
                principalTable: "Reservas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Familiares_Hospedes_HospedeId",
                table: "Familiares",
                column: "HospedeId",
                principalTable: "Hospedes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Hospedes_Reservas_ReservaId",
                table: "Hospedes",
                column: "ReservaId",
                principalTable: "Reservas",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Hospedes_Reservas_ReservaId",
                table: "Hospedes");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "Configuracoes");

            migrationBuilder.DropTable(
                name: "Consumos");

            migrationBuilder.DropTable(
                name: "Familiares");

            migrationBuilder.DropTable(
                name: "Limpezas");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Produtos");

            migrationBuilder.DropTable(
                name: "Reservas");

            migrationBuilder.DropTable(
                name: "Hospedes");

            migrationBuilder.DropTable(
                name: "Quartos");
        }
    }
}
