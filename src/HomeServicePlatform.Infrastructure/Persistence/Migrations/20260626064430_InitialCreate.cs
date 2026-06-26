using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HomeServicePlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:btree_gist", ",,")
                .Annotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    category_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    slug = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    icon_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categories", x => x.category_id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    role_id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    role_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.role_id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    user_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    full_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)1),
                    last_login_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    row_version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.user_id);
                    table.CheckConstraint("ck_users_status", "status IN (0, 1, 2)");
                });

            migrationBuilder.CreateTable(
                name: "services",
                columns: table => new
                {
                    service_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    category_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    duration_minutes = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_services", x => x.service_id);
                    table.CheckConstraint("ck_services_duration", "duration_minutes > 0");
                    table.ForeignKey(
                        name: "fk_services_category",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "category_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "addresses",
                columns: table => new
                {
                    address_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    province_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    district_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ward_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    address_line = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    geom = table.Column<Point>(type: "geometry", nullable: true),
                    is_default = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_addresses", x => x.address_id);
                    table.ForeignKey(
                        name: "fk_addresses_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "bookings",
                columns: table => new
                {
                    booking_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    customer_id = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    subtotal_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    discount_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: true, defaultValue: 0m),
                    final_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    row_version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bookings", x => x.booking_id);
                    table.CheckConstraint("ck_bookings_amount", "subtotal_amount >= 0 AND discount_amount >= 0 AND final_amount >= 0");
                    table.ForeignKey(
                        name: "fk_bookings_customer",
                        column: x => x.customer_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    notification_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    type = table.Column<short>(type: "smallint", nullable: false),
                    payload = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    retry_count = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.notification_id);
                    table.CheckConstraint("ck_notifications_retry", "retry_count >= 0");
                    table.ForeignKey(
                        name: "fk_notifications_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tasker_profile",
                columns: table => new
                {
                    tasker_profile_id = table.Column<long>(type: "bigint", nullable: false),
                    bio = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    is_verified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    verified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    experience_years = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    current_geom = table.Column<Point>(type: "geometry", nullable: true),
                    rating_avg = table.Column<decimal>(type: "numeric(3,2)", nullable: false, defaultValue: 0m),
                    total_reviews = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    status = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tasker_profile", x => x.tasker_profile_id);
                    table.CheckConstraint("ck_tasker_profile_experience", "experience_years >= 0");
                    table.CheckConstraint("ck_tasker_profile_rating", "rating_avg BETWEEN 0 AND 5");
                    table.ForeignKey(
                        name: "fk_tasker_profile_user",
                        column: x => x.tasker_profile_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tokens",
                columns: table => new
                {
                    token_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    token = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    expired_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    type = table.Column<short>(type: "smallint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tokens", x => x.token_id);
                    table.ForeignKey(
                        name: "fk_tokens_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_roles",
                columns: table => new
                {
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    role_id = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_roles", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "fk_user_roles_role",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "role_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_roles_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "wallets",
                columns: table => new
                {
                    wallet_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    balance = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wallets", x => x.wallet_id);
                    table.ForeignKey(
                        name: "fk_wallets_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "booking_addresses",
                columns: table => new
                {
                    booking_address_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    booking_id = table.Column<long>(type: "bigint", nullable: false),
                    full_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    province_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    district_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ward_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    address_line = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    geom = table.Column<Point>(type: "geometry", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_booking_addresses", x => x.booking_address_id);
                    table.ForeignKey(
                        name: "fk_booking_addresses_booking",
                        column: x => x.booking_id,
                        principalTable: "bookings",
                        principalColumn: "booking_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "booking_histories",
                columns: table => new
                {
                    history_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    booking_id = table.Column<long>(type: "bigint", nullable: false),
                    old_status = table.Column<short>(type: "smallint", nullable: true),
                    new_status = table.Column<short>(type: "smallint", nullable: true),
                    changed_by = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_booking_histories", x => x.history_id);
                    table.ForeignKey(
                        name: "fk_booking_histories_booking",
                        column: x => x.booking_id,
                        principalTable: "bookings",
                        principalColumn: "booking_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_booking_histories_changed_by",
                        column: x => x.changed_by,
                        principalTable: "users",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "disputes",
                columns: table => new
                {
                    dispute_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    booking_id = table.Column<long>(type: "bigint", nullable: false),
                    raised_by_id = table.Column<long>(type: "bigint", nullable: false),
                    reason = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    resolution_note = table.Column<string>(type: "text", nullable: true),
                    refund_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: true, defaultValue: 0m),
                    resolved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    row_version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_disputes", x => x.dispute_id);
                    table.CheckConstraint("ck_disputes_refund", "refund_amount >= 0");
                    table.ForeignKey(
                        name: "fk_disputes_booking",
                        column: x => x.booking_id,
                        principalTable: "bookings",
                        principalColumn: "booking_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_disputes_raised_by",
                        column: x => x.raised_by_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    payment_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    booking_id = table.Column<long>(type: "bigint", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    method = table.Column<short>(type: "smallint", nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    transaction_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    paid_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    row_version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payments", x => x.payment_id);
                    table.CheckConstraint("ck_payments_amount", "amount >= 0");
                    table.ForeignKey(
                        name: "fk_payments_booking",
                        column: x => x.booking_id,
                        principalTable: "bookings",
                        principalColumn: "booking_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "booking_items",
                columns: table => new
                {
                    booking_item_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    booking_id = table.Column<long>(type: "bigint", nullable: false),
                    service_id = table.Column<long>(type: "bigint", nullable: false),
                    tasker_id = table.Column<long>(type: "bigint", nullable: true),
                    start_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    end_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    duration_minutes = table.Column<int>(type: "integer", nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    total_price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    cancel_reject_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    row_version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_booking_items", x => x.booking_item_id);
                    table.CheckConstraint("ck_booking_items_duration", "duration_minutes > 0");
                    table.CheckConstraint("ck_booking_items_quantity", "quantity > 0");
                    table.CheckConstraint("ck_booking_items_time", "start_at < end_at");
                    table.CheckConstraint("ck_booking_items_total_price", "total_price >= 0");
                    table.CheckConstraint("ck_booking_items_unit_price", "unit_price >= 0");
                    table.ForeignKey(
                        name: "fk_booking_items_booking",
                        column: x => x.booking_id,
                        principalTable: "bookings",
                        principalColumn: "booking_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_booking_items_service",
                        column: x => x.service_id,
                        principalTable: "services",
                        principalColumn: "service_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_booking_items_tasker_profile",
                        column: x => x.tasker_id,
                        principalTable: "tasker_profile",
                        principalColumn: "tasker_profile_id");
                });

            migrationBuilder.CreateTable(
                name: "commissions",
                columns: table => new
                {
                    commission_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    service_id = table.Column<long>(type: "bigint", nullable: true),
                    tasker_id = table.Column<long>(type: "bigint", nullable: true),
                    commission_rate = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    effective_from = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    effective_to = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_commissions", x => x.commission_id);
                    table.CheckConstraint("ck_commissions_date", "effective_to IS NULL OR effective_from < effective_to");
                    table.CheckConstraint("ck_commissions_rate", "commission_rate BETWEEN 0 AND 100");
                    table.ForeignKey(
                        name: "fk_commissions_service",
                        column: x => x.service_id,
                        principalTable: "services",
                        principalColumn: "service_id");
                    table.ForeignKey(
                        name: "fk_commissions_tasker",
                        column: x => x.tasker_id,
                        principalTable: "tasker_profile",
                        principalColumn: "tasker_profile_id");
                });

            migrationBuilder.CreateTable(
                name: "tasker_schedules",
                columns: table => new
                {
                    schedule_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tasker_id = table.Column<long>(type: "bigint", nullable: false),
                    day_of_week = table.Column<short>(type: "smallint", nullable: false),
                    start_time = table.Column<TimeOnly>(type: "time", nullable: false),
                    end_time = table.Column<TimeOnly>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tasker_schedules", x => x.schedule_id);
                    table.CheckConstraint("ck_tasker_schedules_day", "day_of_week BETWEEN 0 AND 6");
                    table.CheckConstraint("ck_tasker_schedules_time", "start_time < end_time");
                    table.ForeignKey(
                        name: "fk_tasker_schedules_tasker_profile",
                        column: x => x.tasker_id,
                        principalTable: "tasker_profile",
                        principalColumn: "tasker_profile_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tasker_service_prices",
                columns: table => new
                {
                    tasker_service_price_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tasker_id = table.Column<long>(type: "bigint", nullable: false),
                    service_id = table.Column<long>(type: "bigint", nullable: false),
                    price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    effective_from = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    effective_to = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tasker_service_prices", x => x.tasker_service_price_id);
                    table.CheckConstraint("ck_service_prices_date", "effective_to IS NULL OR effective_from < effective_to");
                    table.CheckConstraint("ck_service_prices_price", "price >= 0");
                    table.ForeignKey(
                        name: "fk_service_prices_service",
                        column: x => x.service_id,
                        principalTable: "services",
                        principalColumn: "service_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_service_prices_tasker",
                        column: x => x.tasker_id,
                        principalTable: "tasker_profile",
                        principalColumn: "tasker_profile_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tasker_services",
                columns: table => new
                {
                    tasker_id = table.Column<long>(type: "bigint", nullable: false),
                    service_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tasker_services", x => new { x.tasker_id, x.service_id });
                    table.ForeignKey(
                        name: "fk_tasker_services_service",
                        column: x => x.service_id,
                        principalTable: "services",
                        principalColumn: "service_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_tasker_services_tasker_profile",
                        column: x => x.tasker_id,
                        principalTable: "tasker_profile",
                        principalColumn: "tasker_profile_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tasker_time_offs",
                columns: table => new
                {
                    time_off_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tasker_id = table.Column<long>(type: "bigint", nullable: false),
                    start_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    end_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tasker_time_offs", x => x.time_off_id);
                    table.CheckConstraint("ck_tasker_time_offs_time", "start_at < end_at");
                    table.ForeignKey(
                        name: "fk_tasker_time_offs_tasker_profile",
                        column: x => x.tasker_id,
                        principalTable: "tasker_profile",
                        principalColumn: "tasker_profile_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "wallet_transactions",
                columns: table => new
                {
                    transaction_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    wallet_id = table.Column<long>(type: "bigint", nullable: false),
                    type = table.Column<short>(type: "smallint", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    balance_before = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    balance_after = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    reference_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wallet_transactions", x => x.transaction_id);
                    table.CheckConstraint("ck_wallet_transactions_amount", "amount <> 0");
                    table.ForeignKey(
                        name: "fk_wallet_transactions_wallet",
                        column: x => x.wallet_id,
                        principalTable: "wallets",
                        principalColumn: "wallet_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "refunds",
                columns: table => new
                {
                    refund_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    payment_id = table.Column<long>(type: "bigint", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refunds", x => x.refund_id);
                    table.CheckConstraint("ck_refunds_amount", "amount >= 0");
                    table.ForeignKey(
                        name: "fk_refunds_payment",
                        column: x => x.payment_id,
                        principalTable: "payments",
                        principalColumn: "payment_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reviews",
                columns: table => new
                {
                    review_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    booking_item_id = table.Column<long>(type: "bigint", nullable: false),
                    tasker_id = table.Column<long>(type: "bigint", nullable: false),
                    customer_id = table.Column<long>(type: "bigint", nullable: false),
                    rating = table.Column<short>(type: "smallint", nullable: false),
                    comment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reviews", x => x.review_id);
                    table.CheckConstraint("ck_reviews_rating", "rating BETWEEN 1 AND 5");
                    table.ForeignKey(
                        name: "fk_reviews_booking_item",
                        column: x => x.booking_item_id,
                        principalTable: "booking_items",
                        principalColumn: "booking_item_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_reviews_customer",
                        column: x => x.customer_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_reviews_tasker_profile",
                        column: x => x.tasker_id,
                        principalTable: "tasker_profile",
                        principalColumn: "tasker_profile_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "gist_addresses_geom",
                table: "addresses",
                column: "geom")
                .Annotation("Npgsql:IndexMethod", "gist");

            migrationBuilder.CreateIndex(
                name: "ix_addresses_user_id",
                table: "addresses",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_booking_addresses_booking_id",
                table: "booking_addresses",
                column: "booking_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_booking_histories_booking_id",
                table: "booking_histories",
                column: "booking_id");

            migrationBuilder.CreateIndex(
                name: "IX_booking_histories_changed_by",
                table: "booking_histories",
                column: "changed_by");

            migrationBuilder.CreateIndex(
                name: "ix_booking_items_booking_id",
                table: "booking_items",
                column: "booking_id");

            migrationBuilder.CreateIndex(
                name: "ix_booking_items_service_id",
                table: "booking_items",
                column: "service_id");

            migrationBuilder.CreateIndex(
                name: "ix_booking_items_status",
                table: "booking_items",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_booking_items_tasker_time",
                table: "booking_items",
                columns: new[] { "tasker_id", "start_at", "end_at" });

            migrationBuilder.CreateIndex(
                name: "ix_bookings_customer_id",
                table: "bookings",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_bookings_status",
                table: "bookings",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_categories_slug",
                table: "categories",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_commissions_lookup",
                table: "commissions",
                columns: new[] { "service_id", "tasker_id" },
                filter: "effective_to IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_commissions_tasker_id",
                table: "commissions",
                column: "tasker_id");

            migrationBuilder.CreateIndex(
                name: "ix_disputes_booking_id",
                table: "disputes",
                column: "booking_id");

            migrationBuilder.CreateIndex(
                name: "IX_disputes_raised_by_id",
                table: "disputes",
                column: "raised_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_disputes_status",
                table: "disputes",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_user_status",
                table: "notifications",
                columns: new[] { "user_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_payments_booking_id",
                table: "payments",
                column: "booking_id");

            migrationBuilder.CreateIndex(
                name: "ix_refunds_payment_id",
                table: "refunds",
                column: "payment_id");

            migrationBuilder.CreateIndex(
                name: "IX_reviews_booking_item_id",
                table: "reviews",
                column: "booking_item_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_reviews_customer_id",
                table: "reviews",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_reviews_rating",
                table: "reviews",
                column: "rating");

            migrationBuilder.CreateIndex(
                name: "ix_reviews_tasker_id",
                table: "reviews",
                column: "tasker_id");

            migrationBuilder.CreateIndex(
                name: "IX_roles_role_name",
                table: "roles",
                column: "role_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_services_category_id",
                table: "services",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "gist_tasker_current_geom",
                table: "tasker_profile",
                column: "current_geom")
                .Annotation("Npgsql:IndexMethod", "gist");

            migrationBuilder.CreateIndex(
                name: "ix_tasker_profile_rating",
                table: "tasker_profile",
                column: "rating_avg");

            migrationBuilder.CreateIndex(
                name: "ix_tasker_profile_status",
                table: "tasker_profile",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_tasker_profile_verified",
                table: "tasker_profile",
                column: "is_verified");

            migrationBuilder.CreateIndex(
                name: "IX_tasker_schedules_tasker_id",
                table: "tasker_schedules",
                column: "tasker_id");

            migrationBuilder.CreateIndex(
                name: "ix_service_prices_service_id_effective",
                table: "tasker_service_prices",
                columns: new[] { "service_id", "effective_from", "effective_to" });

            migrationBuilder.CreateIndex(
                name: "IX_tasker_service_prices_tasker_id",
                table: "tasker_service_prices",
                column: "tasker_id");

            migrationBuilder.CreateIndex(
                name: "ix_tasker_services_service_id",
                table: "tasker_services",
                column: "service_id");

            migrationBuilder.CreateIndex(
                name: "ix_tasker_services_tasker_id",
                table: "tasker_services",
                column: "tasker_id");

            migrationBuilder.CreateIndex(
                name: "IX_tasker_time_offs_tasker_id",
                table: "tasker_time_offs",
                column: "tasker_id");

            migrationBuilder.CreateIndex(
                name: "ix_tokens_expired_at",
                table: "tokens",
                column: "expired_at");

            migrationBuilder.CreateIndex(
                name: "ix_tokens_user_id",
                table: "tokens",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_roles_role_id",
                table: "user_roles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ix_users_status",
                table: "users",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ux_users_email",
                table: "users",
                column: "email",
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ux_users_phone",
                table: "users",
                column: "phone",
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_wallet_transactions_ref",
                table: "wallet_transactions",
                column: "reference_id");

            migrationBuilder.CreateIndex(
                name: "ix_wallet_transactions_wallet_id_created_at",
                table: "wallet_transactions",
                columns: new[] { "wallet_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_wallets_user_id",
                table: "wallets",
                column: "user_id",
                unique: true);


            // =========================================================================
            // CODE BỔ SUNG THỦ CÔNG: NẠP FUNCTIONS, TRIGGERS VÀ EXCLUDE CONSTRAINTS
            // =========================================================================

            // 1. Tạo hàm tự động tăng row_version và updated_at cho OCC
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION update_modified_and_version() 
                RETURNS TRIGGER AS $$ 
                BEGIN 
                    IF (ROW(NEW.*) IS DISTINCT FROM ROW(OLD.*)) THEN 
                        IF NEW.updated_at IS NOT NULL THEN
                            NEW.updated_at = CURRENT_TIMESTAMP;
                        END IF;
                        BEGIN
                            NEW.row_version = OLD.row_version + 1;
                        EXCEPTION WHEN undefined_column THEN
                        END;
                    END IF; 
                    RETURN NEW; 
                END; 
                $$ LANGUAGE plpgsql;
            ");

            // 2. Tạo hàm kiểm tra đặc quyền tạo tranh chấp (Raised_by)
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION validate_dispute_raised_by()
                RETURNS TRIGGER AS $$
                DECLARE is_valid BOOLEAN;
                BEGIN
                    SELECT EXISTS (
                        SELECT 1 FROM bookings b
                        LEFT JOIN booking_items bi ON b.booking_id = bi.booking_id
                        WHERE b.booking_id = NEW.booking_id 
                          AND (b.customer_id = NEW.raised_by_id OR bi.tasker_id = NEW.raised_by_id)
                    ) INTO is_valid;
                    IF NOT is_valid THEN
                        RAISE EXCEPTION 'User % không có quyền tạo tranh chấp cho đơn hàng %', NEW.raised_by_id, NEW.booking_id;
                    END IF;
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
            ");

            // 3. Đăng ký tự động kích hoạt Trigger lên 8 bảng cốt lõi khi có hành động UPDATE
            string[] tables = { "users", "bookings", "booking_items", "payments", "disputes", "categories", "services", "reviews" };
            foreach (var table in tables)
            {
                migrationBuilder.Sql($"CREATE TRIGGER trg_update_{table} BEFORE UPDATE ON {table} FOR EACH ROW EXECUTE PROCEDURE update_modified_and_version();");
            }

            // 4. Đăng ký Trigger kiểm tra tranh chấp trước khi INSERT hoặc UPDATE vào bảng disputes
            migrationBuilder.Sql("CREATE TRIGGER trg_validate_dispute_raised_by BEFORE INSERT OR UPDATE ON disputes FOR EACH ROW EXECUTE FUNCTION validate_dispute_raised_by();");

            // 5. Cài đặt EXCLUDE CONSTRAINT dữ liệu không gian (Ngăn chặn Thợ bị trùng lịch nghỉ hoặc lịch làm việc)
            migrationBuilder.Sql(@"
                ALTER TABLE tasker_time_offs 
                ADD CONSTRAINT exclude_tasker_time_overlap 
                EXCLUDE USING gist (tasker_id WITH =, tstzrange(start_at, end_at) WITH &&);
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE booking_items 
                ADD CONSTRAINT exclude_tasker_work_overlap 
                EXCLUDE USING gist (tasker_id WITH =, tstzrange(start_at, end_at) WITH &&) WHERE (status = 1);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            // =========================================================================
            // CODE BỔ SUNG THỦ CÔNG: DỌN DẸP TRIGGERS VÀ FUNCTIONS KHI ROLLBACK
            // =========================================================================

            // 1. Xóa các Exclude Constraints trước
            migrationBuilder.Sql("ALTER TABLE booking_items DROP CONSTRAINT IF EXISTS exclude_tasker_work_overlap;");
            migrationBuilder.Sql("ALTER TABLE tasker_time_offs DROP CONSTRAINT IF EXISTS exclude_tasker_time_overlap;");

            // 2. Xóa Trigger kiểm tra tranh chấp
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS trg_validate_dispute_raised_by ON disputes;");

            // 3. Xóa Trigger OCC trên 8 bảng
            string[] tables = { "users", "bookings", "booking_items", "payments", "disputes", "categories", "services", "reviews" };
            foreach (var table in tables)
            {
                migrationBuilder.Sql($"DROP TRIGGER IF EXISTS trg_update_{table} ON {table};");
            }

            // 4. Xóa các hàm bổ trợ
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS validate_dispute_raised_by() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS update_modified_and_version() CASCADE;");


            migrationBuilder.DropTable(
                name: "addresses");

            migrationBuilder.DropTable(
                name: "booking_addresses");

            migrationBuilder.DropTable(
                name: "booking_histories");

            migrationBuilder.DropTable(
                name: "commissions");

            migrationBuilder.DropTable(
                name: "disputes");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "refunds");

            migrationBuilder.DropTable(
                name: "reviews");

            migrationBuilder.DropTable(
                name: "tasker_schedules");

            migrationBuilder.DropTable(
                name: "tasker_service_prices");

            migrationBuilder.DropTable(
                name: "tasker_services");

            migrationBuilder.DropTable(
                name: "tasker_time_offs");

            migrationBuilder.DropTable(
                name: "tokens");

            migrationBuilder.DropTable(
                name: "user_roles");

            migrationBuilder.DropTable(
                name: "wallet_transactions");

            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "booking_items");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "wallets");

            migrationBuilder.DropTable(
                name: "bookings");

            migrationBuilder.DropTable(
                name: "services");

            migrationBuilder.DropTable(
                name: "tasker_profile");

            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
