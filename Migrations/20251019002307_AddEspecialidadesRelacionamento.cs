using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgoraHora.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddEspecialidadesRelacionamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "FechaMin",
                table: "TB_ESTABELECIMENTO",
                type: "int",
                nullable: false,
                defaultValue: 1080,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "AvaliacaoMedia",
                table: "TB_ESTABELECIMENTO",
                type: "decimal(3,2)",
                precision: 3,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(4,2)",
                oldPrecision: 4,
                oldScale: 2);

            migrationBuilder.AlterColumn<bool>(
                name: "Ativo",
                table: "TB_ESTABELECIMENTO",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<int>(
                name: "AbreMin",
                table: "TB_ESTABELECIMENTO",
                type: "int",
                nullable: false,
                defaultValue: 540,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateTable(
                name: "TB_ESPECIALIDADE",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_ESPECIALIDADE", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TB_PROFISSIONAL_ESPECIALIDADE",
                columns: table => new
                {
                    ProfissionalId = table.Column<int>(type: "int", nullable: false),
                    EspecialidadeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_PROFISSIONAL_ESPECIALIDADE", x => new { x.ProfissionalId, x.EspecialidadeId });
                    table.ForeignKey(
                        name: "FK_TB_PROFISSIONAL_ESPECIALIDADE_TB_ESPECIALIDADE_EspecialidadeId",
                        column: x => x.EspecialidadeId,
                        principalTable: "TB_ESPECIALIDADE",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TB_PROFISSIONAL_ESPECIALIDADE_TB_PROFISSIONAL_ProfissionalId",
                        column: x => x.ProfissionalId,
                        principalTable: "TB_PROFISSIONAL",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TB_ESPECIALIDADE_Nome",
                table: "TB_ESPECIALIDADE",
                column: "Nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TB_PROFISSIONAL_ESPECIALIDADE_EspecialidadeId",
                table: "TB_PROFISSIONAL_ESPECIALIDADE",
                column: "EspecialidadeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TB_PROFISSIONAL_ESPECIALIDADE");

            migrationBuilder.DropTable(
                name: "TB_ESPECIALIDADE");

            migrationBuilder.AlterColumn<int>(
                name: "FechaMin",
                table: "TB_ESTABELECIMENTO",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1080);

            migrationBuilder.AlterColumn<decimal>(
                name: "AvaliacaoMedia",
                table: "TB_ESTABELECIMENTO",
                type: "decimal(4,2)",
                precision: 4,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(3,2)",
                oldPrecision: 3,
                oldScale: 2,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<bool>(
                name: "Ativo",
                table: "TB_ESTABELECIMENTO",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<int>(
                name: "AbreMin",
                table: "TB_ESTABELECIMENTO",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 540);
        }
    }
}
