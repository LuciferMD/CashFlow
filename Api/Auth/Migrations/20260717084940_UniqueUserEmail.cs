using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auth.Migrations
{
    /// <inheritdoc />
    public partial class UniqueUserEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Keep one row per email so the unique index can be created.
            migrationBuilder.Sql("""
                DELETE FROM users u
                WHERE u.ctid NOT IN (
                    SELECT DISTINCT ON (email) ctid
                    FROM users
                    ORDER BY email, ctid DESC
                );
                """);

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                table: "users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_users_email",
                table: "users");
        }
    }
}
