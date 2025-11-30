using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
namespace AgoraHora.Api.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder m)
        {
            // ESTABELECIMENTO
            m.CreateTable(
                name: "TB_ESTABELECIMENTO",
                columns: t => new
                {
                    Id = t.Column<int>(type: "int", nullable: false)
                          .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = t.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Endereco = t.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    ImagemUrl = t.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    AvaliacaoMedia = t.Column<decimal>(type: "decimal(4,2)", nullable: false),
                    AbreMin = t.Column<int>(type: "int", nullable: false),
                    FechaMin = t.Column<int>(type: "int", nullable: false),
                    Ativo = t.Column<bool>(type: "bit", nullable: false)
                },
                constraints: t => { t.PrimaryKey("PK_TB_ESTABELECIMENTO", x => x.Id); });

            // CLIENTE
            m.CreateTable(
                name: "TB_CLIENTE",
                columns: t => new
                {
                    Id = t.Column<int>(type: "int", nullable: false)
                          .Annotation("SqlServer:Identity", "1, 1"),
                    EstabelecimentoId = t.Column<int>(type: "int", nullable: false),
                    Nome = t.Column<string>(type: "nvarchar(450)", nullable: false),
                    Email = t.Column<string>(type: "nvarchar(max)", nullable: true),
                    Telefone = t.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: t => { t.PrimaryKey("PK_TB_CLIENTE", x => x.Id); });

            m.CreateIndex(
                name: "IX_TB_CLIENTE_EstabelecimentoId_Nome",
                table: "TB_CLIENTE",
                columns: new[] { "EstabelecimentoId", "Nome" });

            // PROFISSIONAL
            m.CreateTable(
                name: "TB_PROFISSIONAL",
                columns: t => new
                {
                    Id = t.Column<int>(type: "int", nullable: false)
                          .Annotation("SqlServer:Identity", "1, 1"),
                    EstabelecimentoId = t.Column<int>(type: "int", nullable: false),
                    Nome = t.Column<string>(type: "nvarchar(450)", nullable: false),
                    Ativo = t.Column<bool>(type: "bit", nullable: false)
                },
                constraints: t => { t.PrimaryKey("PK_TB_PROFISSIONAL", x => x.Id); });

            m.CreateIndex(
                name: "IX_TB_PROFISSIONAL_EstabelecimentoId_Nome",
                table: "TB_PROFISSIONAL",
                columns: new[] { "EstabelecimentoId", "Nome" });

            // SERVICO
            m.CreateTable(
                name: "TB_SERVICO",
                columns: t => new
                {
                    Id = t.Column<int>(type: "int", nullable: false)
                          .Annotation("SqlServer:Identity", "1, 1"),
                    EstabelecimentoId = t.Column<int>(type: "int", nullable: false),
                    Nome = t.Column<string>(type: "nvarchar(450)", nullable: false),
                    DuracaoMin = t.Column<int>(type: "int", nullable: false),
                    Preco = t.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Ativo = t.Column<bool>(type: "bit", nullable: false)
                },
                constraints: t => { t.PrimaryKey("PK_TB_SERVICO", x => x.Id); });

            m.CreateIndex(
                name: "IX_TB_SERVICO_EstabelecimentoId_Nome",
                table: "TB_SERVICO",
                columns: new[] { "EstabelecimentoId", "Nome" },
                unique: true);

            // AGENDAMENTO
            m.CreateTable(
                name: "TB_AGENDAMENTO",
                columns: t => new
                {
                    Id = t.Column<int>(type: "int", nullable: false)
                          .Annotation("SqlServer:Identity", "1, 1"),
                    EstabelecimentoId = t.Column<int>(type: "int", nullable: false),
                    ClienteId = t.Column<int>(type: "int", nullable: false),
                    ProfissionalId = t.Column<int>(type: "int", nullable: false),
                    ServicoId = t.Column<int>(type: "int", nullable: false),
                    DtInicio = t.Column<DateTime>(type: "datetime2", nullable: false),
                    DtFim = t.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = t.Column<int>(type: "int", nullable: false),
                    Observacao = t.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: t => { t.PrimaryKey("PK_TB_AGENDAMENTO", x => x.Id); });

            m.CreateIndex(
                name: "IX_TB_AGENDAMENTO_ProfissionalId_DtInicio_DtFim",
                table: "TB_AGENDAMENTO",
                columns: new[] { "ProfissionalId", "DtInicio", "DtFim" });

            // CONFIGURACAO
            m.CreateTable(
                name: "TB_CONFIGURACAO",
                columns: t => new
                {
                    Id = t.Column<int>(type: "int", nullable: false)
                          .Annotation("SqlServer:Identity", "1, 1"),
                    EstabelecimentoId = t.Column<int>(type: "int", nullable: false),
                    Telefone = t.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Horarios = t.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Descricao = t.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: t =>
                {
                    t.PrimaryKey("PK_TB_CONFIGURACAO", x => x.Id);
                    t.ForeignKey(
                        name: "FK_TB_CONFIGURACAO_TB_ESTABELECIMENTO_EstabelecimentoId",
                        column: x => x.EstabelecimentoId,
                        principalTable: "TB_ESTABELECIMENTO",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            m.CreateIndex(
                name: "IX_TB_CONFIGURACAO_EstabelecimentoId",
                table: "TB_CONFIGURACAO",
                column: "EstabelecimentoId",
                unique: true);
        }

        protected override void Down(MigrationBuilder m)
        {
            m.DropTable(name: "TB_CONFIGURACAO");
            m.DropTable(name: "TB_AGENDAMENTO");
            m.DropTable(name: "TB_SERVICO");
            m.DropTable(name: "TB_PROFISSIONAL");
            m.DropTable(name: "TB_CLIENTE");
            m.DropTable(name: "TB_ESTABELECIMENTO");
        }
    }
}
