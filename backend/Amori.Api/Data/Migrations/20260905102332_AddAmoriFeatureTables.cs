using System;
using System.Collections.Generic;
using Amori.Api.Domain.Enums;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Amori.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAmoriFeatureTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "app_notifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    body = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    notification_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    reference_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_read = table.Column<bool>(type: "boolean", nullable: false),
                    read_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_app_notifications", x => x.id);
                    table.ForeignKey(
                        name: "FK_app_notifications_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "bucket_list_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    relationship_id = table.Column<Guid>(type: "uuid", nullable: false),
                    added_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    location = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    category = table.Column<int>(type: "integer", nullable: false),
                    target_date = table.Column<DateOnly>(type: "date", nullable: true),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    is_favorite = table.Column<bool>(type: "boolean", nullable: false),
                    is_completed = table.Column<bool>(type: "boolean", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bucket_list_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_bucket_list_items_relationships_relationship_id",
                        column: x => x.relationship_id,
                        principalTable: "relationships",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_bucket_list_items_users_added_by_user_id",
                        column: x => x.added_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "calendar_events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    relationship_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    event_date = table.Column<DateOnly>(type: "date", nullable: false),
                    start_time = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    end_time = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    location = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    reminder_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    reminder_minutes_before = table.Column<int>(type: "integer", nullable: true),
                    is_completed = table.Column<bool>(type: "boolean", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_shared = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_calendar_events", x => x.id);
                    table.ForeignKey(
                        name: "FK_calendar_events_relationships_relationship_id",
                        column: x => x.relationship_id,
                        principalTable: "relationships",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_calendar_events_users_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "date_ideas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    relationship_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    category = table.Column<int>(type: "integer", nullable: false),
                    location = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    estimated_cost = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    duration_minutes = table.Column<int>(type: "integer", nullable: true),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    is_favorite = table.Column<bool>(type: "boolean", nullable: false),
                    is_completed = table.Column<bool>(type: "boolean", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_date_ideas", x => x.id);
                    table.ForeignKey(
                        name: "FK_date_ideas_relationships_relationship_id",
                        column: x => x.relationship_id,
                        principalTable: "relationships",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_date_ideas_users_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "device_registrations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    device_token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    platform = table.Column<int>(type: "integer", nullable: false),
                    device_identifier = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    last_seen_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_device_registrations", x => x.id);
                    table.ForeignKey(
                        name: "FK_device_registrations_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "emergency_requests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    relationship_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sender_id = table.Column<Guid>(type: "uuid", nullable: false),
                    recipient_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    acknowledged_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    resolved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_emergency_requests", x => x.id);
                    table.ForeignKey(
                        name: "FK_emergency_requests_relationships_relationship_id",
                        column: x => x.relationship_id,
                        principalTable: "relationships",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_emergency_requests_users_recipient_id",
                        column: x => x.recipient_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_emergency_requests_users_sender_id",
                        column: x => x.sender_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "games",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    type = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_games", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "hugs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    relationship_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sender_id = table.Column<Guid>(type: "uuid", nullable: false),
                    recipient_id = table.Column<Guid>(type: "uuid", nullable: false),
                    acknowledged_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hugs", x => x.id);
                    table.ForeignKey(
                        name: "FK_hugs_relationships_relationship_id",
                        column: x => x.relationship_id,
                        principalTable: "relationships",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_hugs_users_recipient_id",
                        column: x => x.recipient_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_hugs_users_sender_id",
                        column: x => x.sender_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "important_dates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    relationship_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    is_recurring = table.Column<bool>(type: "boolean", nullable: false),
                    reminder_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    reminder_days_before = table.Column<int>(type: "integer", nullable: true),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    image_key = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_important_dates", x => x.id);
                    table.ForeignKey(
                        name: "FK_important_dates_relationships_relationship_id",
                        column: x => x.relationship_id,
                        principalTable: "relationships",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_important_dates_users_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "memories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    relationship_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    memory_date = table.Column<DateOnly>(type: "date", nullable: true),
                    location = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    latitude = table.Column<double>(type: "double precision", nullable: true),
                    longitude = table.Column<double>(type: "double precision", nullable: true),
                    tags = table.Column<List<string>>(type: "jsonb", nullable: false),
                    is_favorite = table.Column<bool>(type: "boolean", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_memories", x => x.id);
                    table.ForeignKey(
                        name: "FK_memories_relationships_relationship_id",
                        column: x => x.relationship_id,
                        principalTable: "relationships",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_memories_users_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    relationship_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sender_id = table.Column<Guid>(type: "uuid", nullable: false),
                    recipient_id = table.Column<Guid>(type: "uuid", nullable: false),
                    text = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    image_key = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    voice_note_key = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    category = table.Column<int>(type: "integer", nullable: false),
                    read_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_favorite = table.Column<bool>(type: "boolean", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_messages", x => x.id);
                    table.ForeignKey(
                        name: "FK_messages_relationships_relationship_id",
                        column: x => x.relationship_id,
                        principalTable: "relationships",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_messages_users_recipient_id",
                        column: x => x.recipient_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_messages_users_sender_id",
                        column: x => x.sender_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "mood_check_ins",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    relationship_id = table.Column<Guid>(type: "uuid", nullable: false),
                    check_in_type = table.Column<int>(type: "integer", nullable: false),
                    mood = table.Column<int>(type: "integer", nullable: false),
                    intensity = table.Column<int>(type: "integer", nullable: false),
                    what_happened = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    feelings = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    perceived_cause = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    what_i_need = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    is_shared_with_partner = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mood_check_ins", x => x.id);
                    table.ForeignKey(
                        name: "FK_mood_check_ins_relationships_relationship_id",
                        column: x => x.relationship_id,
                        principalTable: "relationships",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_mood_check_ins_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "quiz_questions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    relationship_id = table.Column<Guid>(type: "uuid", nullable: true),
                    question_text = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    correct_answer = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    options = table.Column<List<string>>(type: "jsonb", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quiz_questions", x => x.id);
                    table.ForeignKey(
                        name: "FK_quiz_questions_relationships_relationship_id",
                        column: x => x.relationship_id,
                        principalTable: "relationships",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "quiz_sessions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    relationship_id = table.Column<Guid>(type: "uuid", nullable: false),
                    score = table.Column<int>(type: "integer", nullable: false),
                    total_questions = table.Column<int>(type: "integer", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quiz_sessions", x => x.id);
                    table.ForeignKey(
                        name: "FK_quiz_sessions_relationships_relationship_id",
                        column: x => x.relationship_id,
                        principalTable: "relationships",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_quiz_sessions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_revoked = table.Column<bool>(type: "boolean", nullable: false),
                    replaced_by_token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    revoked_reason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_tokens", x => x.id);
                    table.ForeignKey(
                        name: "FK_refresh_tokens_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "relationship_incidents",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    relationship_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reported_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assigned_to_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    category = table.Column<int>(type: "integer", nullable: false),
                    sub_category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    impact = table.Column<int>(type: "integer", nullable: false),
                    urgency = table.Column<int>(type: "integer", nullable: false),
                    priority = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    resolution = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    resolution_notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    assigned_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    investigated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    resolved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    closed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    reopened_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    due_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_relationship_incidents", x => x.id);
                    table.ForeignKey(
                        name: "FK_relationship_incidents_relationships_relationship_id",
                        column: x => x.relationship_id,
                        principalTable: "relationships",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_relationship_incidents_users_assigned_to_user_id",
                        column: x => x.assigned_to_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_relationship_incidents_users_reported_by_user_id",
                        column: x => x.reported_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "relationship_invitations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    relationship_id = table.Column<Guid>(type: "uuid", nullable: false),
                    invited_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    invitee_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    invite_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    responded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_relationship_invitations", x => x.id);
                    table.ForeignKey(
                        name: "FK_relationship_invitations_relationships_relationship_id",
                        column: x => x.relationship_id,
                        principalTable: "relationships",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_relationship_invitations_users_invited_by_user_id",
                        column: x => x.invited_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "relationship_tickets",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    relationship_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assigned_to_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    subject = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    category = table.Column<int>(type: "integer", nullable: false),
                    severity = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    feelings = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    what_happened = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    what_i_need = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    what_i_prefer_in_future = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    additional_notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    resolved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_relationship_tickets", x => x.id);
                    table.ForeignKey(
                        name: "FK_relationship_tickets_relationships_relationship_id",
                        column: x => x.relationship_id,
                        principalTable: "relationships",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_relationship_tickets_users_assigned_to_user_id",
                        column: x => x.assigned_to_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_relationship_tickets_users_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "reminders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    type = table.Column<int>(type: "integer", nullable: false),
                    reminder_time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    recurrence = table.Column<int>(type: "integer", nullable: false),
                    one_time_date = table.Column<DateOnly>(type: "date", nullable: true),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    last_completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    snooze_until = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    next_occurrence_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reminders", x => x.id);
                    table.ForeignKey(
                        name: "FK_reminders_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "splitting_questions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    question = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    question_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_splitting_questions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "splitting_sessions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    relationship_id = table.Column<Guid>(type: "uuid", nullable: false),
                    feelings_selected = table.Column<List<string>>(type: "jsonb", nullable: false),
                    trigger = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    what_i_need = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    recommended_support_type = table.Column<int>(type: "integer", nullable: true),
                    actions_taken = table.Column<List<SplittingAction>>(type: "jsonb", nullable: false),
                    initial_mood = table.Column<int>(type: "integer", nullable: true),
                    final_mood = table.Column<int>(type: "integer", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_splitting_sessions", x => x.id);
                    table.ForeignKey(
                        name: "FK_splitting_sessions_relationships_relationship_id",
                        column: x => x.relationship_id,
                        principalTable: "relationships",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_splitting_sessions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "surprises",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    relationship_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    recipient_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    message_text = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    image_key = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    voice_note_key = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    scheduled_date = table.Column<DateOnly>(type: "date", nullable: true),
                    opened_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_favorite = table.Column<bool>(type: "boolean", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_surprises", x => x.id);
                    table.ForeignKey(
                        name: "FK_surprises_relationships_relationship_id",
                        column: x => x.relationship_id,
                        principalTable: "relationships",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_surprises_users_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_surprises_users_recipient_user_id",
                        column: x => x.recipient_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "timeline_events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    relationship_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    event_date = table.Column<DateOnly>(type: "date", nullable: false),
                    location = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    event_type = table.Column<int>(type: "integer", nullable: false),
                    photo_keys = table.Column<List<string>>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_timeline_events", x => x.id);
                    table.ForeignKey(
                        name: "FK_timeline_events_relationships_relationship_id",
                        column: x => x.relationship_id,
                        principalTable: "relationships",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_timeline_events_users_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_settings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    push_notifications_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    mood_reminders_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    partner_activity_notifications = table.Column<bool>(type: "boolean", nullable: false),
                    hug_notifications = table.Column<bool>(type: "boolean", nullable: false),
                    emergency_notifications = table.Column<bool>(type: "boolean", nullable: false),
                    message_notifications = table.Column<bool>(type: "boolean", nullable: false),
                    reminder_notifications = table.Column<bool>(type: "boolean", nullable: false),
                    profile_visible = table.Column<bool>(type: "boolean", nullable: false),
                    mood_share_default = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_settings", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_settings_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "voice_notes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    relationship_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    file_key = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    duration_seconds = table.Column<int>(type: "integer", nullable: false),
                    category = table.Column<int>(type: "integer", nullable: false),
                    is_favorite = table.Column<bool>(type: "boolean", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_voice_notes", x => x.id);
                    table.ForeignKey(
                        name: "FK_voice_notes_relationships_relationship_id",
                        column: x => x.relationship_id,
                        principalTable: "relationships",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_voice_notes_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "wishlist_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    relationship_id = table.Column<Guid>(type: "uuid", nullable: false),
                    added_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    image_key = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    price = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    priority = table.Column<int>(type: "integer", nullable: false),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    is_purchased = table.Column<bool>(type: "boolean", nullable: false),
                    is_favorite = table.Column<bool>(type: "boolean", nullable: false),
                    purchased_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wishlist_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_wishlist_items_relationships_relationship_id",
                        column: x => x.relationship_id,
                        principalTable: "relationships",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_wishlist_items_users_added_by_user_id",
                        column: x => x.added_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "would_you_rather_questions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    option_a = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    option_b = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_would_you_rather_questions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "game_scores",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    game_id = table.Column<Guid>(type: "uuid", nullable: false),
                    relationship_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    score = table.Column<int>(type: "integer", nullable: false),
                    played_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_scores", x => x.id);
                    table.ForeignKey(
                        name: "FK_game_scores_games_game_id",
                        column: x => x.game_id,
                        principalTable: "games",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_game_scores_relationships_relationship_id",
                        column: x => x.relationship_id,
                        principalTable: "relationships",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_game_scores_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "game_sessions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    game_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    relationship_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    score = table.Column<int>(type: "integer", nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_sessions", x => x.id);
                    table.ForeignKey(
                        name: "FK_game_sessions_games_game_id",
                        column: x => x.game_id,
                        principalTable: "games",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_game_sessions_relationships_relationship_id",
                        column: x => x.relationship_id,
                        principalTable: "relationships",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_game_sessions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "memory_media",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    memory_id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_key = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    media_type = table.Column<int>(type: "integer", nullable: false),
                    duration_seconds = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_memory_media", x => x.id);
                    table.ForeignKey(
                        name: "FK_memory_media_memories_memory_id",
                        column: x => x.memory_id,
                        principalTable: "memories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "quiz_answers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    quiz_session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    question_id = table.Column<Guid>(type: "uuid", nullable: false),
                    answer_given = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    is_correct = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quiz_answers", x => x.id);
                    table.ForeignKey(
                        name: "FK_quiz_answers_quiz_questions_question_id",
                        column: x => x.question_id,
                        principalTable: "quiz_questions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_quiz_answers_quiz_sessions_quiz_session_id",
                        column: x => x.quiz_session_id,
                        principalTable: "quiz_sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "relationship_incident_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    incident_id = table.Column<Guid>(type: "uuid", nullable: false),
                    actor_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    action = table.Column<int>(type: "integer", nullable: false),
                    old_value = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    new_value = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_relationship_incident_history", x => x.id);
                    table.ForeignKey(
                        name: "FK_relationship_incident_history_relationship_incidents_incide~",
                        column: x => x.incident_id,
                        principalTable: "relationship_incidents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_relationship_incident_history_users_actor_user_id",
                        column: x => x.actor_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "relationship_incident_lessons",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    incident_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    lesson = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_relationship_incident_lessons", x => x.id);
                    table.ForeignKey(
                        name: "FK_relationship_incident_lessons_relationship_incidents_incide~",
                        column: x => x.incident_id,
                        principalTable: "relationship_incidents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_relationship_incident_lessons_users_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "relationship_incident_notes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    incident_id = table.Column<Guid>(type: "uuid", nullable: false),
                    author_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    content = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_relationship_incident_notes", x => x.id);
                    table.ForeignKey(
                        name: "FK_relationship_incident_notes_relationship_incidents_incident~",
                        column: x => x.incident_id,
                        principalTable: "relationship_incidents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_relationship_incident_notes_users_author_user_id",
                        column: x => x.author_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "relationship_incident_responses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    incident_id = table.Column<Guid>(type: "uuid", nullable: false),
                    author_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    message = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_relationship_incident_responses", x => x.id);
                    table.ForeignKey(
                        name: "FK_relationship_incident_responses_relationship_incidents_inci~",
                        column: x => x.incident_id,
                        principalTable: "relationship_incidents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_relationship_incident_responses_users_author_user_id",
                        column: x => x.author_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "relationship_incident_reviews",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    incident_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    what_went_well = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    what_could_improve = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    future_action = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_relationship_incident_reviews", x => x.id);
                    table.ForeignKey(
                        name: "FK_relationship_incident_reviews_relationship_incidents_incide~",
                        column: x => x.incident_id,
                        principalTable: "relationship_incidents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_relationship_incident_reviews_users_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ticket_responses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ticket_id = table.Column<Guid>(type: "uuid", nullable: false),
                    responded_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    content = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ticket_responses", x => x.id);
                    table.ForeignKey(
                        name: "FK_ticket_responses_relationship_tickets_ticket_id",
                        column: x => x.ticket_id,
                        principalTable: "relationship_tickets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ticket_responses_users_responded_by_user_id",
                        column: x => x.responded_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "reminder_completions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    reminder_id = table.Column<Guid>(type: "uuid", nullable: false),
                    completed_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reminder_completions", x => x.id);
                    table.ForeignKey(
                        name: "FK_reminder_completions_reminders_reminder_id",
                        column: x => x.reminder_id,
                        principalTable: "reminders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_reminder_completions_users_completed_by_user_id",
                        column: x => x.completed_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "splitting_answers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    question_id = table.Column<Guid>(type: "uuid", nullable: false),
                    answer = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_splitting_answers", x => x.id);
                    table.ForeignKey(
                        name: "FK_splitting_answers_splitting_questions_question_id",
                        column: x => x.question_id,
                        principalTable: "splitting_questions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_splitting_answers_splitting_sessions_session_id",
                        column: x => x.session_id,
                        principalTable: "splitting_sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "would_you_rather_answers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    question_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    relationship_id = table.Column<Guid>(type: "uuid", nullable: false),
                    chose_option_a = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_would_you_rather_answers", x => x.id);
                    table.ForeignKey(
                        name: "FK_would_you_rather_answers_relationships_relationship_id",
                        column: x => x.relationship_id,
                        principalTable: "relationships",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_would_you_rather_answers_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_would_you_rather_answers_would_you_rather_questions_questio~",
                        column: x => x.question_id,
                        principalTable: "would_you_rather_questions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_app_notifications_created_at",
                table: "app_notifications",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_app_notifications_is_read",
                table: "app_notifications",
                column: "is_read");

            migrationBuilder.CreateIndex(
                name: "IX_app_notifications_user_id",
                table: "app_notifications",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_bucket_list_items_added_by_user_id",
                table: "bucket_list_items",
                column: "added_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_bucket_list_items_relationship_id",
                table: "bucket_list_items",
                column: "relationship_id");

            migrationBuilder.CreateIndex(
                name: "IX_calendar_events_created_by_user_id",
                table: "calendar_events",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_calendar_events_event_date",
                table: "calendar_events",
                column: "event_date");

            migrationBuilder.CreateIndex(
                name: "IX_calendar_events_relationship_id",
                table: "calendar_events",
                column: "relationship_id");

            migrationBuilder.CreateIndex(
                name: "IX_date_ideas_created_by_user_id",
                table: "date_ideas",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_date_ideas_relationship_id",
                table: "date_ideas",
                column: "relationship_id");

            migrationBuilder.CreateIndex(
                name: "IX_device_registrations_device_token",
                table: "device_registrations",
                column: "device_token");

            migrationBuilder.CreateIndex(
                name: "IX_device_registrations_is_active",
                table: "device_registrations",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_device_registrations_user_id",
                table: "device_registrations",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_emergency_requests_created_at",
                table: "emergency_requests",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_emergency_requests_recipient_id",
                table: "emergency_requests",
                column: "recipient_id");

            migrationBuilder.CreateIndex(
                name: "IX_emergency_requests_relationship_id",
                table: "emergency_requests",
                column: "relationship_id");

            migrationBuilder.CreateIndex(
                name: "IX_emergency_requests_sender_id",
                table: "emergency_requests",
                column: "sender_id");

            migrationBuilder.CreateIndex(
                name: "IX_emergency_requests_status",
                table: "emergency_requests",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_game_scores_game_id",
                table: "game_scores",
                column: "game_id");

            migrationBuilder.CreateIndex(
                name: "IX_game_scores_played_at",
                table: "game_scores",
                column: "played_at");

            migrationBuilder.CreateIndex(
                name: "IX_game_scores_relationship_id",
                table: "game_scores",
                column: "relationship_id");

            migrationBuilder.CreateIndex(
                name: "IX_game_scores_user_id",
                table: "game_scores",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_game_sessions_created_at",
                table: "game_sessions",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_game_sessions_game_id",
                table: "game_sessions",
                column: "game_id");

            migrationBuilder.CreateIndex(
                name: "IX_game_sessions_relationship_id",
                table: "game_sessions",
                column: "relationship_id");

            migrationBuilder.CreateIndex(
                name: "IX_game_sessions_user_id",
                table: "game_sessions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_games_is_active",
                table: "games",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_games_type",
                table: "games",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "IX_hugs_created_at",
                table: "hugs",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_hugs_recipient_id",
                table: "hugs",
                column: "recipient_id");

            migrationBuilder.CreateIndex(
                name: "IX_hugs_relationship_id",
                table: "hugs",
                column: "relationship_id");

            migrationBuilder.CreateIndex(
                name: "IX_hugs_sender_id",
                table: "hugs",
                column: "sender_id");

            migrationBuilder.CreateIndex(
                name: "IX_important_dates_created_by_user_id",
                table: "important_dates",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_important_dates_date",
                table: "important_dates",
                column: "date");

            migrationBuilder.CreateIndex(
                name: "IX_important_dates_relationship_id",
                table: "important_dates",
                column: "relationship_id");

            migrationBuilder.CreateIndex(
                name: "IX_memories_created_by_user_id",
                table: "memories",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_memories_is_deleted",
                table: "memories",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "IX_memories_relationship_id",
                table: "memories",
                column: "relationship_id");

            migrationBuilder.CreateIndex(
                name: "IX_memory_media_memory_id",
                table: "memory_media",
                column: "memory_id");

            migrationBuilder.CreateIndex(
                name: "IX_messages_created_at",
                table: "messages",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_messages_is_deleted",
                table: "messages",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "IX_messages_recipient_id",
                table: "messages",
                column: "recipient_id");

            migrationBuilder.CreateIndex(
                name: "IX_messages_relationship_id",
                table: "messages",
                column: "relationship_id");

            migrationBuilder.CreateIndex(
                name: "IX_messages_sender_id",
                table: "messages",
                column: "sender_id");

            migrationBuilder.CreateIndex(
                name: "IX_mood_check_ins_created_at",
                table: "mood_check_ins",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_mood_check_ins_mood",
                table: "mood_check_ins",
                column: "mood");

            migrationBuilder.CreateIndex(
                name: "IX_mood_check_ins_relationship_id",
                table: "mood_check_ins",
                column: "relationship_id");

            migrationBuilder.CreateIndex(
                name: "IX_mood_check_ins_user_id",
                table: "mood_check_ins",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_quiz_answers_question_id",
                table: "quiz_answers",
                column: "question_id");

            migrationBuilder.CreateIndex(
                name: "IX_quiz_answers_quiz_session_id",
                table: "quiz_answers",
                column: "quiz_session_id");

            migrationBuilder.CreateIndex(
                name: "IX_quiz_questions_is_active",
                table: "quiz_questions",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_quiz_questions_relationship_id",
                table: "quiz_questions",
                column: "relationship_id");

            migrationBuilder.CreateIndex(
                name: "IX_quiz_sessions_relationship_id",
                table: "quiz_sessions",
                column: "relationship_id");

            migrationBuilder.CreateIndex(
                name: "IX_quiz_sessions_user_id",
                table: "quiz_sessions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_expires_at",
                table: "refresh_tokens",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_token",
                table: "refresh_tokens",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_user_id",
                table: "refresh_tokens",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_incident_history_created_at",
                table: "relationship_incident_history",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_incident_history_incident_id",
                table: "relationship_incident_history",
                column: "incident_id");

            migrationBuilder.CreateIndex(
                name: "IX_relationship_incident_history_actor_user_id",
                table: "relationship_incident_history",
                column: "actor_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_incident_lessons_incident_id",
                table: "relationship_incident_lessons",
                column: "incident_id");

            migrationBuilder.CreateIndex(
                name: "IX_relationship_incident_lessons_created_by_user_id",
                table: "relationship_incident_lessons",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_incident_notes_incident_id",
                table: "relationship_incident_notes",
                column: "incident_id");

            migrationBuilder.CreateIndex(
                name: "IX_relationship_incident_notes_author_user_id",
                table: "relationship_incident_notes",
                column: "author_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_incident_responses_incident_id",
                table: "relationship_incident_responses",
                column: "incident_id");

            migrationBuilder.CreateIndex(
                name: "IX_relationship_incident_responses_author_user_id",
                table: "relationship_incident_responses",
                column: "author_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_incident_reviews_incident_id",
                table: "relationship_incident_reviews",
                column: "incident_id");

            migrationBuilder.CreateIndex(
                name: "IX_relationship_incident_reviews_created_by_user_id",
                table: "relationship_incident_reviews",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_relationship_incidents_assigned_to",
                table: "relationship_incidents",
                column: "assigned_to_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_relationship_incidents_category",
                table: "relationship_incidents",
                column: "category");

            migrationBuilder.CreateIndex(
                name: "IX_relationship_incidents_created_at",
                table: "relationship_incidents",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_relationship_incidents_due_at",
                table: "relationship_incidents",
                column: "due_at");

            migrationBuilder.CreateIndex(
                name: "IX_relationship_incidents_priority",
                table: "relationship_incidents",
                column: "priority");

            migrationBuilder.CreateIndex(
                name: "IX_relationship_incidents_relationship_id",
                table: "relationship_incidents",
                column: "relationship_id");

            migrationBuilder.CreateIndex(
                name: "IX_relationship_incidents_reported_by",
                table: "relationship_incidents",
                column: "reported_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_relationship_incidents_status",
                table: "relationship_incidents",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_relationship_invitations_invite_code",
                table: "relationship_invitations",
                column: "invite_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_relationship_invitations_invited_by_user_id",
                table: "relationship_invitations",
                column: "invited_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_relationship_invitations_relationship_id",
                table: "relationship_invitations",
                column: "relationship_id");

            migrationBuilder.CreateIndex(
                name: "IX_relationship_tickets_assigned_to_user_id",
                table: "relationship_tickets",
                column: "assigned_to_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_relationship_tickets_created_at",
                table: "relationship_tickets",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_relationship_tickets_created_by_user_id",
                table: "relationship_tickets",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_relationship_tickets_relationship_id",
                table: "relationship_tickets",
                column: "relationship_id");

            migrationBuilder.CreateIndex(
                name: "IX_relationship_tickets_status",
                table: "relationship_tickets",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_reminder_completions_completed_at",
                table: "reminder_completions",
                column: "completed_at");

            migrationBuilder.CreateIndex(
                name: "IX_reminder_completions_completed_by_user_id",
                table: "reminder_completions",
                column: "completed_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_reminder_completions_reminder_id",
                table: "reminder_completions",
                column: "reminder_id");

            migrationBuilder.CreateIndex(
                name: "IX_reminders_is_enabled",
                table: "reminders",
                column: "is_enabled");

            migrationBuilder.CreateIndex(
                name: "IX_reminders_next_occurrence_at",
                table: "reminders",
                column: "next_occurrence_at");

            migrationBuilder.CreateIndex(
                name: "IX_reminders_user_id",
                table: "reminders",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_splitting_answers_question_id",
                table: "splitting_answers",
                column: "question_id");

            migrationBuilder.CreateIndex(
                name: "IX_splitting_answers_session_id",
                table: "splitting_answers",
                column: "session_id");

            migrationBuilder.CreateIndex(
                name: "IX_splitting_questions_display_order",
                table: "splitting_questions",
                column: "display_order");

            migrationBuilder.CreateIndex(
                name: "IX_splitting_questions_is_active",
                table: "splitting_questions",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_splitting_sessions_created_at",
                table: "splitting_sessions",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_splitting_sessions_relationship_id",
                table: "splitting_sessions",
                column: "relationship_id");

            migrationBuilder.CreateIndex(
                name: "IX_splitting_sessions_status",
                table: "splitting_sessions",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_splitting_sessions_user_id",
                table: "splitting_sessions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_surprises_created_by_user_id",
                table: "surprises",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_surprises_recipient_user_id",
                table: "surprises",
                column: "recipient_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_surprises_relationship_id",
                table: "surprises",
                column: "relationship_id");

            migrationBuilder.CreateIndex(
                name: "IX_surprises_scheduled_date",
                table: "surprises",
                column: "scheduled_date");

            migrationBuilder.CreateIndex(
                name: "IX_ticket_responses_responded_by_user_id",
                table: "ticket_responses",
                column: "responded_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_ticket_responses_ticket_id",
                table: "ticket_responses",
                column: "ticket_id");

            migrationBuilder.CreateIndex(
                name: "IX_timeline_events_created_by_user_id",
                table: "timeline_events",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_timeline_events_event_date",
                table: "timeline_events",
                column: "event_date");

            migrationBuilder.CreateIndex(
                name: "IX_timeline_events_relationship_id",
                table: "timeline_events",
                column: "relationship_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_settings_user_id",
                table: "user_settings",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_voice_notes_is_deleted",
                table: "voice_notes",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "IX_voice_notes_relationship_id",
                table: "voice_notes",
                column: "relationship_id");

            migrationBuilder.CreateIndex(
                name: "IX_voice_notes_user_id",
                table: "voice_notes",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_wishlist_items_added_by_user_id",
                table: "wishlist_items",
                column: "added_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_wishlist_items_relationship_id",
                table: "wishlist_items",
                column: "relationship_id");

            migrationBuilder.CreateIndex(
                name: "IX_would_you_rather_answers_question_id",
                table: "would_you_rather_answers",
                column: "question_id");

            migrationBuilder.CreateIndex(
                name: "IX_would_you_rather_answers_relationship_id",
                table: "would_you_rather_answers",
                column: "relationship_id");

            migrationBuilder.CreateIndex(
                name: "IX_would_you_rather_answers_user_id",
                table: "would_you_rather_answers",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_would_you_rather_questions_is_active",
                table: "would_you_rather_questions",
                column: "is_active");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "app_notifications");

            migrationBuilder.DropTable(
                name: "bucket_list_items");

            migrationBuilder.DropTable(
                name: "calendar_events");

            migrationBuilder.DropTable(
                name: "date_ideas");

            migrationBuilder.DropTable(
                name: "device_registrations");

            migrationBuilder.DropTable(
                name: "emergency_requests");

            migrationBuilder.DropTable(
                name: "game_scores");

            migrationBuilder.DropTable(
                name: "game_sessions");

            migrationBuilder.DropTable(
                name: "hugs");

            migrationBuilder.DropTable(
                name: "important_dates");

            migrationBuilder.DropTable(
                name: "memory_media");

            migrationBuilder.DropTable(
                name: "messages");

            migrationBuilder.DropTable(
                name: "mood_check_ins");

            migrationBuilder.DropTable(
                name: "quiz_answers");

            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "relationship_incident_history");

            migrationBuilder.DropTable(
                name: "relationship_incident_lessons");

            migrationBuilder.DropTable(
                name: "relationship_incident_notes");

            migrationBuilder.DropTable(
                name: "relationship_incident_responses");

            migrationBuilder.DropTable(
                name: "relationship_incident_reviews");

            migrationBuilder.DropTable(
                name: "relationship_invitations");

            migrationBuilder.DropTable(
                name: "reminder_completions");

            migrationBuilder.DropTable(
                name: "splitting_answers");

            migrationBuilder.DropTable(
                name: "surprises");

            migrationBuilder.DropTable(
                name: "ticket_responses");

            migrationBuilder.DropTable(
                name: "timeline_events");

            migrationBuilder.DropTable(
                name: "user_settings");

            migrationBuilder.DropTable(
                name: "voice_notes");

            migrationBuilder.DropTable(
                name: "wishlist_items");

            migrationBuilder.DropTable(
                name: "would_you_rather_answers");

            migrationBuilder.DropTable(
                name: "games");

            migrationBuilder.DropTable(
                name: "memories");

            migrationBuilder.DropTable(
                name: "quiz_questions");

            migrationBuilder.DropTable(
                name: "quiz_sessions");

            migrationBuilder.DropTable(
                name: "relationship_incidents");

            migrationBuilder.DropTable(
                name: "reminders");

            migrationBuilder.DropTable(
                name: "splitting_questions");

            migrationBuilder.DropTable(
                name: "splitting_sessions");

            migrationBuilder.DropTable(
                name: "relationship_tickets");

            migrationBuilder.DropTable(
                name: "would_you_rather_questions");
        }
    }
}
