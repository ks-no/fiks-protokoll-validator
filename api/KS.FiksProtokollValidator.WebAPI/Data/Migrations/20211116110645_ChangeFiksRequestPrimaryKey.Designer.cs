﻿// <auto-generated />
using System;
using KS.FiksProtokollValidator.WebAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace KS.FiksProtokollValidator.WebAPI.Data.Migrations
{
    [DbContext(typeof(FiksIOMessageDBContext))]
    [Migration("20211116110645_ChangeFiksRequestPrimaryKey")]
    partial class ChangeFiksRequestPrimaryKey
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.7")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("KS.FiksProtokollValidator.WebAPI.Models.FiksExpectedResponseMessageType", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ExpectedResponseMessageType")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TestCaseTestId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("TestCaseTestId");

                    b.ToTable("FiksExpectedResponseMessageType");
                });

            modelBuilder.Entity("KS.FiksProtokollValidator.WebAPI.Models.FiksPayload", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int?>("FiksResponseId")
                        .HasColumnType("int");

                    b.Property<string>("Filename")
                        .HasColumnType("nvarchar(max)");

                    b.Property<byte[]>("Payload")
                        .HasColumnType("varbinary(max)");

                    b.HasKey("Id");

                    b.HasIndex("FiksResponseId");

                    b.ToTable("FiksPayload");
                });

            modelBuilder.Entity("KS.FiksProtokollValidator.WebAPI.Models.FiksRequest", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int?>("CustomPayloadFileId")
                        .HasColumnType("int");

                    b.Property<Guid>("MessageGuid")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("SentAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("TestCaseTestId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<Guid?>("TestSessionId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("CustomPayloadFileId");

                    b.HasIndex("TestCaseTestId");

                    b.HasIndex("TestSessionId");

                    b.ToTable("FiksRequest");
                });

            modelBuilder.Entity("KS.FiksProtokollValidator.WebAPI.Models.FiksRequestPayload", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Filename")
                        .HasColumnType("nvarchar(max)");

                    b.Property<byte[]>("Payload")
                        .HasColumnType("varbinary(max)");

                    b.HasKey("Id");

                    b.ToTable("FiksRequestPayload");
                });

            modelBuilder.Entity("KS.FiksProtokollValidator.WebAPI.Models.FiksResponse", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<Guid?>("FiksRequestId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("ReceivedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("FiksRequestId");

                    b.ToTable("FiksResponse");
                });

            modelBuilder.Entity("KS.FiksProtokollValidator.WebAPI.Models.FiksResponseTest", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ExpectedValue")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PayloadQuery")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TestCaseTestId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("ValueType")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("TestCaseTestId");

                    b.ToTable("FiksResponseTest");
                });

            modelBuilder.Entity("KS.FiksProtokollValidator.WebAPI.Models.TestCase", b =>
                {
                    b.Property<string>("TestId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ExpectedResult")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("MessageType")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Operation")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PayloadAttachmentFileNames")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PayloadFileName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PayloadFilePath")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Protocol")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Situation")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Supported")
                        .HasColumnType("bit");

                    b.Property<string>("TestName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TestStep")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("TestId");

                    b.HasIndex("TestId")
                        .IsUnique();

                    b.ToTable("TestCases");
                });

            modelBuilder.Entity("KS.FiksProtokollValidator.WebAPI.Models.TestSession", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("RecipientId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.ToTable("TestSessions");
                });

            modelBuilder.Entity("KS.FiksProtokollValidator.WebAPI.Models.FiksExpectedResponseMessageType", b =>
                {
                    b.HasOne("KS.FiksProtokollValidator.WebAPI.Models.TestCase", null)
                        .WithMany("ExpectedResponseMessageTypes")
                        .HasForeignKey("TestCaseTestId");
                });

            modelBuilder.Entity("KS.FiksProtokollValidator.WebAPI.Models.FiksPayload", b =>
                {
                    b.HasOne("KS.FiksProtokollValidator.WebAPI.Models.FiksResponse", null)
                        .WithMany("FiksPayloads")
                        .HasForeignKey("FiksResponseId");
                });

            modelBuilder.Entity("KS.FiksProtokollValidator.WebAPI.Models.FiksRequest", b =>
                {
                    b.HasOne("KS.FiksProtokollValidator.WebAPI.Models.FiksRequestPayload", "CustomPayloadFile")
                        .WithMany()
                        .HasForeignKey("CustomPayloadFileId");

                    b.HasOne("KS.FiksProtokollValidator.WebAPI.Models.TestCase", "TestCase")
                        .WithMany()
                        .HasForeignKey("TestCaseTestId");

                    b.HasOne("KS.FiksProtokollValidator.WebAPI.Models.TestSession", null)
                        .WithMany("FiksRequests")
                        .HasForeignKey("TestSessionId");

                    b.Navigation("CustomPayloadFile");

                    b.Navigation("TestCase");
                });

            modelBuilder.Entity("KS.FiksProtokollValidator.WebAPI.Models.FiksResponse", b =>
                {
                    b.HasOne("KS.FiksProtokollValidator.WebAPI.Models.FiksRequest", null)
                        .WithMany("FiksResponses")
                        .HasForeignKey("FiksRequestId");
                });

            modelBuilder.Entity("KS.FiksProtokollValidator.WebAPI.Models.FiksResponseTest", b =>
                {
                    b.HasOne("KS.FiksProtokollValidator.WebAPI.Models.TestCase", null)
                        .WithMany("FiksResponseTests")
                        .HasForeignKey("TestCaseTestId");
                });

            modelBuilder.Entity("KS.FiksProtokollValidator.WebAPI.Models.FiksRequest", b =>
                {
                    b.Navigation("FiksResponses");
                });

            modelBuilder.Entity("KS.FiksProtokollValidator.WebAPI.Models.FiksResponse", b =>
                {
                    b.Navigation("FiksPayloads");
                });

            modelBuilder.Entity("KS.FiksProtokollValidator.WebAPI.Models.TestCase", b =>
                {
                    b.Navigation("ExpectedResponseMessageTypes");

                    b.Navigation("FiksResponseTests");
                });

            modelBuilder.Entity("KS.FiksProtokollValidator.WebAPI.Models.TestSession", b =>
                {
                    b.Navigation("FiksRequests");
                });
#pragma warning restore 612, 618
        }
    }
}
