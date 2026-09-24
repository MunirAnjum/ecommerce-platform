using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeOutboxNamingConvention : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_outboxMessages",
                table: "outboxMessages");

            migrationBuilder.RenameTable(
                name: "outboxMessages",
                newName: "OutboxMessages");

            migrationBuilder.RenameIndex(
                name: "IX_outboxMessages_ProcessedOnUtc",
                table: "OutboxMessages",
                newName: "IX_OutboxMessages_ProcessedOnUtc");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OutboxMessages",
                table: "OutboxMessages",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_OutboxMessages",
                table: "OutboxMessages");

            migrationBuilder.RenameTable(
                name: "OutboxMessages",
                newName: "outboxMessages");

            migrationBuilder.RenameIndex(
                name: "IX_OutboxMessages_ProcessedOnUtc",
                table: "outboxMessages",
                newName: "IX_outboxMessages_ProcessedOnUtc");

            migrationBuilder.AddPrimaryKey(
                name: "PK_outboxMessages",
                table: "outboxMessages",
                column: "Id");
        }
    }
}
