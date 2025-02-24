﻿// <auto-generated />
using System;
using BitcoinCash.API.DAL.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace BitcoinCash.API.Migrations
{
    [DbContext(typeof(BchApiContext))]
    [Migration("20250223203549_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("BitcoinCash.API.Models.DatabaseModels.BchTransaction", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ID"));

                    b.Property<decimal>("BchAmount")
                        .HasPrecision(18, 8)
                        .HasColumnType("decimal(18,8)");

                    b.Property<decimal>("BchPrice")
                        .HasPrecision(18, 2)
                        .HasColumnType("decimal(18,2)");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<int?>("FromID")
                        .HasColumnType("int");

                    b.Property<int>("FromType")
                        .HasColumnType("int");

                    b.Property<int>("PaymentType")
                        .HasColumnType("int");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<int?>("ToID")
                        .HasColumnType("int");

                    b.Property<int>("ToType")
                        .HasColumnType("int");

                    b.Property<string>("TxId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ID");

                    b.ToTable("BchTransaction");
                });

            modelBuilder.Entity("BitcoinCash.API.Models.DatabaseModels.Key", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ID"));

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("LastActivity")
                        .HasColumnType("datetime2");

                    b.Property<string>("PrivateKey")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("RemainingCalls")
                        .HasColumnType("int");

                    b.HasKey("ID");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("ID"), false);

                    b.HasIndex("Address");

                    SqlServerIndexBuilderExtensions.IsClustered(b.HasIndex("Address"));

                    b.ToTable("Key");
                });
#pragma warning restore 612, 618
        }
    }
}
