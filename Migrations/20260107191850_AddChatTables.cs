using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkChat2.Migrations
{
    /// <inheritdoc />
    public partial class AddChatTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "ADMIN_USER_ID",
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp", "UpdatedAt" },
                values: new object[] { "d07e3a70-14cb-44a4-9d0b-05f17d8f94aa", new DateTime(2026, 1, 7, 19, 18, 49, 919, DateTimeKind.Utc).AddTicks(1684), "AQAAAAIAAYagAAAAEDWo/psbn9SF9u7LjP2PcR7mpsSuCHUXlb11XUaz752iC7VlbF31pzmbF67mmN67vw==", "60186319-20d0-4961-acc8-8eae34f1ae06", new DateTime(2026, 1, 7, 19, 18, 49, 919, DateTimeKind.Utc).AddTicks(1687) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "ADMIN_USER_ID",
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp", "UpdatedAt" },
                values: new object[] { "7cca39f4-7c0a-437a-bab9-68eaca6246ad", new DateTime(2025, 12, 18, 18, 31, 32, 457, DateTimeKind.Utc).AddTicks(6937), "AQAAAAIAAYagAAAAEKKjl6XNRmic5yg+VaHkKyIUSQVNOiu/R+QStCkPtAoRFB1UvBfkimSgOwtcZCEyBw==", "90d396b6-3cc3-49f1-b4e7-f16ba759e658", new DateTime(2025, 12, 18, 18, 31, 32, 457, DateTimeKind.Utc).AddTicks(6939) });
        }
    }
}
