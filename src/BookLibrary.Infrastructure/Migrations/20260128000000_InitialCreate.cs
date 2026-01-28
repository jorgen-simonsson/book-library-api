using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BookLibrary.Infrastructure.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "places",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                descr = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_places", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "books",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                isbn = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                author = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                publisher = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                published_year = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                pagecount = table.Column<int>(type: "integer", nullable: false),
                place_id = table.Column<int>(type: "integer", nullable: true),
                api_info = table.Column<string>(type: "jsonb", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_books", x => x.id);
                table.ForeignKey(
                    name: "FK_books_places_place_id",
                    column: x => x.place_id,
                    principalTable: "places",
                    principalColumn: "id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateIndex(
            name: "IX_books_place_id",
            table: "books",
            column: "place_id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "books");

        migrationBuilder.DropTable(
            name: "places");
    }
}
