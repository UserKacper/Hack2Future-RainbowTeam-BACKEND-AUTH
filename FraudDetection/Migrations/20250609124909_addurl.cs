using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FraudDetection.Migrations
{
    /// <inheritdoc />
    public partial class addurl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "InsuranceClaims",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "InsuranceClaims");
        }
    }
}
