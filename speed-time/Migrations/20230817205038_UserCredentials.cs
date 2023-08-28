using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DSaladin.SpeedTime.Migrations
{
    /// <inheritdoc />
    public partial class UserCredentials : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrackAttribute",
                columns: table => new
                {
                    TrackTimeId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackAttribute", x => new { x.TrackTimeId, x.Name });
                    table.ForeignKey(
                        name: "FK_TrackAttribute_TrackedTimes_TrackTimeId",
                        column: x => x.TrackTimeId,
                        principalTable: "TrackedTimes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserCredential",
                columns: table => new
                {
                    ServiceType = table.Column<int>(type: "INTEGER", nullable: false),
                    Credential = table.Column<byte[]>(type: "BLOB", nullable: false),
                    ServiceUri = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCredential", x => x.ServiceType);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrackAttribute");

            migrationBuilder.DropTable(
                name: "UserCredential");
        }
    }
}
