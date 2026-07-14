using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeServicePlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ServiceNameTrigramLowerIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Thay index GIN trigram trên name bằng functional index trên lower(name)
            // để truy vấn "lower(name) LIKE '%...%'" (tìm không phân biệt hoa/thường) dùng được index.
            migrationBuilder.DropIndex(
                name: "ix_services_name_trgm",
                table: "services");

            migrationBuilder.Sql(
                "CREATE INDEX ix_services_name_trgm ON services USING gin (lower(name) gin_trgm_ops);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_services_name_trgm",
                table: "services",
                column: "name")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });
        }
    }
}
