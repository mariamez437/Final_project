using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lost_and_Found.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTwo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "LostPhones");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "LostPhones",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
