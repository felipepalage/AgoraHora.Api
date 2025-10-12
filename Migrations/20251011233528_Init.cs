using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgoraHora.Api.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TB_AGENDAMENTO",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EstabelecimentoId = table.Column<int>(type: "int", nullable: false),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    ProfissionalId = table.Column<int>(type: "int", nullable: false),
                    ServicoId = table.Column<int>(type: "int", nullable: false),
                    DtInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DtFim = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Observacao = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_AGENDAMENTO", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TB_CLIENTE",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EstabelecimentoId = table.Column<int>(type: "int", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Telefone = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_CLIENTE", x => x.Id);
                });

               migrationBuilder.CreateTable(
                name: "TB_ESTABELECIMENTO",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(maxLength: 200, nullable: false),
                    Endereco = table.Column<string>(maxLength: 300, nullable: true),
                    ImagemUrl = table.Column<string>(maxLength: 300, nullable: true),
                    AvaliacaoMedia = table.Column<decimal>(type: "decimal(4,2)", nullable: false),
                    AbreMin = table.Column<int>(nullable: false),
                    FechaMin = table.Column<int>(nullable: false),
                    Ativo = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_ESTABELECIMENTO", x => x.Id);
                });



            migrationBuilder.CreateTable(
                name: "TB_ESTABELECIMENTO",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_ESTABELECIMENTO", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TB_PROFISSIONAL",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EstabelecimentoId = table.Column<int>(type: "int", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_PROFISSIONAL", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TB_SERVICO",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EstabelecimentoId = table.Column<int>(type: "int", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DuracaoMin = table.Column<int>(type: "int", nullable: false),
                    Preco = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_SERVICO", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TB_AGENDAMENTO_ProfissionalId_DtInicio_DtFim",
                table: "TB_AGENDAMENTO",
                columns: new[] { "ProfissionalId", "DtInicio", "DtFim" });

            migrationBuilder.CreateIndex(
                name: "IX_TB_CLIENTE_EstabelecimentoId_Nome",
                table: "TB_CLIENTE",
                columns: new[] { "EstabelecimentoId", "Nome" });

            migrationBuilder.CreateIndex(
                name: "IX_TB_PROFISSIONAL_EstabelecimentoId_Nome",
                table: "TB_PROFISSIONAL",
                columns: new[] { "EstabelecimentoId", "Nome" });

            migrationBuilder.CreateIndex(
                name: "IX_TB_SERVICO_EstabelecimentoId_Nome",
                table: "TB_SERVICO",
                columns: new[] { "EstabelecimentoId", "Nome" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TB_AGENDAMENTO");

            migrationBuilder.DropTable(
                name: "TB_CLIENTE");

            migrationBuilder.DropTable(
                name: "TB_ESTABELECIMENTO");

            migrationBuilder.DropTable(
                name: "TB_PROFISSIONAL");

            migrationBuilder.DropTable(
                name: "TB_SERVICO");

            migrationBuilder.DropTable(
              name: "TB_CONFIGURACAO");

        }
    }
}
