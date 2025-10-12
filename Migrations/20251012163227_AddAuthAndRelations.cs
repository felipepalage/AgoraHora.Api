using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
namespace AgoraHora.Api.Migrations
{
    public partial class AddAuthAndRelations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // USUÁRIO
            migrationBuilder.CreateTable(
                name: "TB_USUARIO",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SenhaHash = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table => { table.PrimaryKey("PK_TB_USUARIO", x => x.Id); });

            // USUÁRIO x ESTABELECIMENTO
            migrationBuilder.CreateTable(
                name: "TB_USUARIO_ESTABELECIMENTO",
                columns: table => new
                {
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    EstabelecimentoId = table.Column<int>(type: "int", nullable: false),
                    Papel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "owner")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_USUARIO_ESTABELECIMENTO", x => new { x.UsuarioId, x.EstabelecimentoId });
                    table.ForeignKey(
                        name: "FK_TB_USUARIO_ESTABELECIMENTO_TB_ESTABELECIMENTO_EstabelecimentoId",
                        column: x => x.EstabelecimentoId,
                        principalTable: "TB_ESTABELECIMENTO",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TB_USUARIO_ESTABELECIMENTO_TB_USUARIO_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "TB_USUARIO",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // ÍNDICES
            migrationBuilder.CreateIndex(
                name: "IX_TB_AGENDAMENTO_ClienteId",
                table: "TB_AGENDAMENTO",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_TB_AGENDAMENTO_EstabelecimentoId",
                table: "TB_AGENDAMENTO",
                column: "EstabelecimentoId");

            migrationBuilder.CreateIndex(
                name: "IX_TB_AGENDAMENTO_ServicoId",
                table: "TB_AGENDAMENTO",
                column: "ServicoId");

            migrationBuilder.CreateIndex(
                name: "IX_TB_USUARIO_Email",
                table: "TB_USUARIO",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TB_USUARIO_ESTABELECIMENTO_EstabelecimentoId",
                table: "TB_USUARIO_ESTABELECIMENTO",
                column: "EstabelecimentoId");

            // FKs
            migrationBuilder.AddForeignKey(
                name: "FK_TB_AGENDAMENTO_TB_CLIENTE_ClienteId",
                table: "TB_AGENDAMENTO",
                column: "ClienteId",
                principalTable: "TB_CLIENTE",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TB_AGENDAMENTO_TB_ESTABELECIMENTO_EstabelecimentoId",
                table: "TB_AGENDAMENTO",
                column: "EstabelecimentoId",
                principalTable: "TB_ESTABELECIMENTO",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TB_AGENDAMENTO_TB_PROFISSIONAL_ProfissionalId",
                table: "TB_AGENDAMENTO",
                column: "ProfissionalId",
                principalTable: "TB_PROFISSIONAL",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TB_AGENDAMENTO_TB_SERVICO_ServicoId",
                table: "TB_AGENDAMENTO",
                column: "ServicoId",                // <-- sem acento
                principalTable: "TB_SERVICO",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TB_CLIENTE_TB_ESTABELECIMENTO_EstabelecimentoId",
                table: "TB_CLIENTE",
                column: "EstabelecimentoId",
                principalTable: "TB_ESTABELECIMENTO",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TB_PROFISSIONAL_TB_ESTABELECIMENTO_EstabelecimentoId",
                table: "TB_PROFISSIONAL",
                column: "EstabelecimentoId",
                principalTable: "TB_ESTABELECIMENTO",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TB_SERVICO_TB_ESTABELECIMENTO_EstabelecimentoId",
                table: "TB_SERVICO",
                column: "EstabelecimentoId",
                principalTable: "TB_ESTABELECIMENTO",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey("FK_TB_AGENDAMENTO_TB_CLIENTE_ClienteId", "TB_AGENDAMENTO");
            migrationBuilder.DropForeignKey("FK_TB_AGENDAMENTO_TB_ESTABELECIMENTO_EstabelecimentoId", "TB_AGENDAMENTO");
            migrationBuilder.DropForeignKey("FK_TB_AGENDAMENTO_TB_PROFISSIONAL_ProfissionalId", "TB_AGENDAMENTO");
            migrationBuilder.DropForeignKey("FK_TB_AGENDAMENTO_TB_SERVICO_ServicoId", "TB_AGENDAMENTO");
            migrationBuilder.DropForeignKey("FK_TB_CLIENTE_TB_ESTABELECIMENTO_EstabelecimentoId", "TB_CLIENTE");
            migrationBuilder.DropForeignKey("FK_TB_PROFISSIONAL_TB_ESTABELECIMENTO_EstabelecimentoId", "TB_PROFISSIONAL");
            migrationBuilder.DropForeignKey("FK_TB_SERVICO_TB_ESTABELECIMENTO_EstabelecimentoId", "TB_SERVICO");

            migrationBuilder.DropIndex("IX_TB_AGENDAMENTO_ClienteId", "TB_AGENDAMENTO");
            migrationBuilder.DropIndex("IX_TB_AGENDAMENTO_EstabelecimentoId", "TB_AGENDAMENTO");
            migrationBuilder.DropIndex("IX_TB_AGENDAMENTO_ServicoId", "TB_AGENDAMENTO");

            migrationBuilder.DropTable(name: "TB_USUARIO_ESTABELECIMENTO");
            migrationBuilder.DropTable(name: "TB_USUARIO");
        }
    }
}

