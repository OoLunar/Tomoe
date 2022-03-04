﻿// <auto-generated />
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Tomoe.Models;

#nullable disable

namespace Tomoe.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20220126192707_3.0.0-alpha-autoreactions-rewrite")]
    partial class _300alphaautoreactionsrewrite
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Tomoe.Models.AutoMentionModel", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<decimal>("ChannelId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("channel_id");

                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<bool>("IsRole")
                        .HasColumnType("boolean")
                        .HasColumnName("is_role");

                    b.Property<string>("Regex")
                        .HasColumnType("text")
                        .HasColumnName("regex");

                    b.Property<decimal>("Snowflake")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("snowflake");

                    b.HasKey("Id")
                        .HasName("pk_auto_mentions");

                    b.ToTable("auto_mentions", (string)null);
                });

            modelBuilder.Entity("Tomoe.Models.AutoReactionModel", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<decimal>("ChannelId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("channel_id");

                    b.Property<int>("FilterType")
                        .HasColumnType("integer")
                        .HasColumnName("filter_type");

                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<string>("Regex")
                        .HasColumnType("text")
                        .HasColumnName("regex");

                    b.HasKey("Id")
                        .HasName("pk_auto_reactions");

                    b.ToTable("auto_reactions", (string)null);
                });

            modelBuilder.Entity("Tomoe.Models.EmojiData", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<Guid?>("AutoReactionModelId")
                        .HasColumnType("uuid")
                        .HasColumnName("auto_reaction_model_id");

                    b.Property<decimal>("EmojiId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("emoji_id");

                    b.Property<string>("EmojiName")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("emoji_name");

                    b.HasKey("Id")
                        .HasName("pk_emoji_data");

                    b.HasIndex("AutoReactionModelId")
                        .HasDatabaseName("ix_emoji_data_auto_reaction_model_id");

                    b.ToTable("emoji_data", (string)null);
                });

            modelBuilder.Entity("Tomoe.Models.GuildConfigModel", b =>
                {
                    b.Property<decimal>("GuildId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.HasKey("GuildId")
                        .HasName("pk_guild_configs");

                    b.ToTable("guild_configs", (string)null);
                });

            modelBuilder.Entity("Tomoe.Models.SnowflakePermissionsModel", b =>
                {
                    b.Property<decimal>("SnowflakeId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("snowflake_id");

                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<int>("Permissions")
                        .HasColumnType("integer")
                        .HasColumnName("permissions");

                    b.HasKey("SnowflakeId")
                        .HasName("pk_snowflake_perms");

                    b.ToTable("snowflake_perms", (string)null);
                });

            modelBuilder.Entity("Tomoe.Models.TagModel", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<List<string>>("Aliases")
                        .IsRequired()
                        .HasColumnType("text[]")
                        .HasColumnName("aliases");

                    b.Property<decimal>("AuthorId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("author_id");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("content");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<int>("UsageCount")
                        .HasColumnType("integer")
                        .HasColumnName("usage_count");

                    b.HasKey("Id")
                        .HasName("pk_tags");

                    b.ToTable("tags", (string)null);
                });

            modelBuilder.Entity("Tomoe.Models.EmojiData", b =>
                {
                    b.HasOne("Tomoe.Models.AutoReactionModel", null)
                        .WithMany("EmojiData")
                        .HasForeignKey("AutoReactionModelId")
                        .HasConstraintName("fk_emoji_data_auto_reactions_auto_reaction_model_id");
                });

            modelBuilder.Entity("Tomoe.Models.AutoReactionModel", b =>
                {
                    b.Navigation("EmojiData");
                });
#pragma warning restore 612, 618
        }
    }
}