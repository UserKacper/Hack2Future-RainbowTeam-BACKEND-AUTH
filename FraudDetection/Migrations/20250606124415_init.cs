using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FraudDetection.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ResidentCard",
                table: "AspNetUsers",
                newName: "UniqueIdNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UniqueIdNumber",
                table: "AspNetUsers",
                newName: "ResidentCard");
        }
    }
}
