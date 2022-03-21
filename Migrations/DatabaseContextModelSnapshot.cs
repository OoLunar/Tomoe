﻿// <auto-generated />
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Tomoe.Models;

#nullable disable

namespace Tomoe.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    partial class DatabaseContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Tomoe.Models.AutoModel<DSharpPlus.Entities.IMention>", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<decimal>("ChannelId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("channel_id");

                    b.Property<string>("Filter")
                        .HasColumnType("text")
                        .HasColumnName("filter");

                    b.Property<int>("FilterType")
                        .HasColumnType("integer")
                        .HasColumnName("filter_type");

                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<List<string>>("Values")
                        .IsRequired()
                        .HasColumnType("text[]")
                        .HasColumnName("values");

                    b.HasKey("Id")
                        .HasName("pk_auto_mentions");

                    b.ToTable("auto_mentions", (string)null);
                });

            modelBuilder.Entity("Tomoe.Models.MenuRoleModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ButtonId")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("button_id");

                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<decimal>("RoleId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("role_id");

                    b.HasKey("Id")
                        .HasName("pk_menu_roles");

                    b.ToTable("menu_roles", (string)null);
                });

            modelBuilder.Entity("Tomoe.Models.PollModel", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<decimal>("ChannelId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("channel_id");

                    b.Property<DateTime>("ExpiresAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("expires_at");

                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<decimal>("MessageId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("message_id");

                    b.Property<string>("Question")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("question");

                    b.Property<decimal>("UserId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("user_id");

                    b.Property<Dictionary<string, ulong[]>>("Votes")
                        .IsRequired()
                        .HasColumnType("jsonb")
                        .HasColumnName("votes");

                    b.HasKey("Id")
                        .HasName("pk_polls");

                    b.ToTable("polls", (string)null);
                });

            modelBuilder.Entity("Tomoe.Models.TempRoleModel", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<decimal>("Assignee")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("assignee");

                    b.Property<decimal>("Assigner")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("assigner");

                    b.Property<DateTime>("ExpiresAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("expires_at");

                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<decimal>("RoleId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("role_id");

                    b.HasKey("Id")
                        .HasName("pk_temp_roles");

                    b.ToTable("temp_roles", (string)null);
                });
#pragma warning restore 612, 618
        }
    }
}
