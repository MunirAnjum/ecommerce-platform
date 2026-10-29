using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWeightInGramsToOrderItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CartItems_CartId_ProductId",
                table: "CartItems");

            migrationBuilder.AddColumn<int>(
                name: "WeightInGrams",
                table: "OrderItems",
                type: "integer",
                nullable: false,
                defaultValue: 250);

            migrationBuilder.AddColumn<int>(
                name: "WeightInGrams",
                table: "CartItems",
                type: "integer",
                nullable: false,
                defaultValue: 250);

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_CartId_ProductId_WeightInGrams",
                table: "CartItems",
                columns: new[] { "CartId", "ProductId", "WeightInGrams" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CartItems_CartId_ProductId_WeightInGrams",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "WeightInGrams",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "WeightInGrams",
                table: "CartItems");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_CartId_ProductId",
                table: "CartItems",
                columns: new[] { "CartId", "ProductId" },
                unique: true);
        }
    }
}
