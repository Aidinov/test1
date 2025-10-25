using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DesignDocService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DesignDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Product = table.Column<string>(type: "text", nullable: false),
                    Team = table.Column<string>(type: "text", nullable: false),
                    Author = table.Column<string>(type: "text", nullable: false),
                    TaskLink = table.Column<string>(type: "text", nullable: false),
                    GitRepository = table.Column<string>(type: "text", nullable: false),
                    GitFilePath = table.Column<string>(type: "text", nullable: false),
                    GitCommitHash = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DesignDocuments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Author = table.Column<string>(type: "text", nullable: false),
                    StartIndex = table.Column<int>(type: "integer", nullable: false),
                    EndIndex = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Severity = table.Column<string>(type: "text", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsResolved = table.Column<bool>(type: "boolean", nullable: false),
                    ResolvedBy = table.Column<string>(type: "text", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DocumentVersion = table.Column<string>(type: "text", nullable: true),
                    OriginalText = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comments_DesignDocuments_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "DesignDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comments_DocumentId",
                table: "Comments",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_DocumentVersion",
                table: "Comments",
                column: "DocumentVersion");

            migrationBuilder.CreateIndex(
                name: "IX_DesignDocuments_Author",
                table: "DesignDocuments",
                column: "Author");

            migrationBuilder.CreateIndex(
                name: "IX_DesignDocuments_Product",
                table: "DesignDocuments",
                column: "Product");

            migrationBuilder.CreateIndex(
                name: "IX_DesignDocuments_Status",
                table: "DesignDocuments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_DesignDocuments_Team",
                table: "DesignDocuments",
                column: "Team");

            migrationBuilder.CreateIndex(
                name: "IX_DesignDocuments_UpdatedAt",
                table: "DesignDocuments",
                column: "UpdatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "DesignDocuments");
        }
    }
}
