using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Live_Cryptocurrency_Watchlist.Migrations
{
    /// <inheritdoc />
    public partial class AddCoinAmount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                table: "SavedCoins",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Amount",
                table: "SavedCoins");
        }
    }
}
