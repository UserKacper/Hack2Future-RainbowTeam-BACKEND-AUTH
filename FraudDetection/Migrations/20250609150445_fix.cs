using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FraudDetection.Migrations
{
    /// <inheritdoc />
    public partial class fix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "InsuranceClaims");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "InsuranceClaims",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
