using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FullstackTemplate.Migrations
{
    /// <inheritdoc />
    public partial class InitialAnonSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActivityLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Action = table.Column<string>(type: "text", nullable: false),
                    EntityType = table.Column<string>(type: "text", nullable: false),
                    EntityId = table.Column<string>(type: "text", nullable: true),
                    EntityName = table.Column<string>(type: "text", nullable: true),
                    Details = table.Column<string>(type: "text", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Connections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Config = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false, defaultValue: "active"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Connections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Email = table.Column<string>(type: "text", nullable: true),
                    FirstName = table.Column<string>(type: "text", nullable: true),
                    LastName = table.Column<string>(type: "text", nullable: true),
                    ProfileImageUrl = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Role = table.Column<string>(type: "text", nullable: false, defaultValue: "viewer"),
                    Status = table.Column<string>(type: "text", nullable: false, defaultValue: "active"),
                    InactiveDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Department = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Configurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ConnectionId = table.Column<Guid>(type: "uuid", nullable: true),
                    ObjectName = table.Column<string>(type: "text", nullable: true),
                    SourceConnectionId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceObjectName = table.Column<string>(type: "text", nullable: true),
                    TargetConnectionId = table.Column<Guid>(type: "uuid", nullable: true),
                    TargetObjectName = table.Column<string>(type: "text", nullable: true),
                    Rules = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Configurations_Connections_ConnectionId",
                        column: x => x.ConnectionId,
                        principalTable: "Connections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Configurations_Connections_SourceConnectionId",
                        column: x => x.SourceConnectionId,
                        principalTable: "Connections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Configurations_Connections_TargetConnectionId",
                        column: x => x.TargetConnectionId,
                        principalTable: "Connections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ConnectionMetadata",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ConnectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ObjectType = table.Column<string>(type: "text", nullable: false),
                    ObjectName = table.Column<string>(type: "text", nullable: false),
                    ObjectPath = table.Column<string>(type: "text", nullable: true),
                    Columns = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    FetchedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConnectionMetadata", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConnectionMetadata_Connections_ConnectionId",
                        column: x => x.ConnectionId,
                        principalTable: "Connections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Jobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false, defaultValue: "pending"),
                    SourceConnectionId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceObjects = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    Operation = table.Column<string>(type: "text", nullable: true),
                    Mappings = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    IntegrityRules = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    Progress = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    RecordsProcessed = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    RecordsTotal = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OutputLocation = table.Column<string>(type: "text", nullable: true),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Jobs_Connections_SourceConnectionId",
                        column: x => x.SourceConnectionId,
                        principalTable: "Connections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_Timestamp",
                table: "ActivityLogs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_Configurations_ConnectionId",
                table: "Configurations",
                column: "ConnectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Configurations_SourceConnectionId",
                table: "Configurations",
                column: "SourceConnectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Configurations_TargetConnectionId",
                table: "Configurations",
                column: "TargetConnectionId");

            migrationBuilder.CreateIndex(
                name: "IX_ConnectionMetadata_ConnectionId",
                table: "ConnectionMetadata",
                column: "ConnectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_SourceConnectionId",
                table: "Jobs",
                column: "SourceConnectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityLogs");

            migrationBuilder.DropTable(
                name: "Configurations");

            migrationBuilder.DropTable(
                name: "ConnectionMetadata");

            migrationBuilder.DropTable(
                name: "Jobs");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Connections");
        }
    }
}
