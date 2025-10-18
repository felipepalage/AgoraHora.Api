using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgoraHora.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddEspecialidadeToProfissional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Especialidade",
                table: "TB_PROFISSIONAL",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Especialidade",
                table: "TB_PROFISSIONAL");
        }
    }
}
