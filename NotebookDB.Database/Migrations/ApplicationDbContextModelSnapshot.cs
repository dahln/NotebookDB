﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NotebookDB.Database;

#nullable disable

namespace NotebookDB.Database.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "9.0.2");

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex");

                    b.ToTable("AspNetRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ClaimType")
                        .HasColumnType("TEXT");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("TEXT");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUser", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("TEXT");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("INTEGER");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("TEXT");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("TEXT");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("TEXT");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("INTEGER");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("TEXT");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("INTEGER");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex");

                    b.ToTable("AspNetUsers", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ClaimType")
                        .HasColumnType("TEXT");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("TEXT");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("TEXT");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("TEXT");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("TEXT");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("TEXT");

                    b.Property<string>("RoleId")
                        .HasColumnType("TEXT");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("TEXT");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<string>("Value")
                        .HasColumnType("TEXT");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("NotebookDB.Database.Field", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<int>("Column")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("NotebookId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Options")
                        .HasColumnType("TEXT");

                    b.Property<int>("Order")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Required")
                        .HasColumnType("INTEGER");

                    b.Property<int>("SearchOrder")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("SearchShow")
                        .HasColumnType("INTEGER");

                    b.Property<string>("SectionId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("Type")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("NotebookId");

                    b.HasIndex("SectionId");

                    b.ToTable("Fields");
                });

            modelBuilder.Entity("NotebookDB.Database.File", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<long>("FileSize")
                        .HasColumnType("INTEGER");

                    b.Property<string>("InstanceId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("MimeType")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("Uploaded")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("InstanceId");

                    b.ToTable("Files");
                });

            modelBuilder.Entity("NotebookDB.Database.FileShard", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<byte[]>("Data")
                        .IsRequired()
                        .HasColumnType("BLOB");

                    b.Property<string>("FileId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("Index")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("FileId");

                    b.ToTable("FileShards");
                });

            modelBuilder.Entity("NotebookDB.Database.Instance", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("CreatedById")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("TEXT");

                    b.Property<string>("NotebookId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("UpdatedById")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("UpdatedOn")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("NotebookId");

                    b.ToTable("Instances");
                });

            modelBuilder.Entity("NotebookDB.Database.InstanceValue", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("FieldId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("InstanceId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("FieldId");

                    b.HasIndex("InstanceId");

                    b.ToTable("Values");
                });

            modelBuilder.Entity("NotebookDB.Database.Notebook", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<bool>("AllowAttachments")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Notebooks");
                });

            modelBuilder.Entity("NotebookDB.Database.NotebookAuthorizedUser", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<bool>("AllowInstanceEdits")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsNotebookAdmin")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsNotebookOwner")
                        .HasColumnType("INTEGER");

                    b.Property<string>("NotebookId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("NotebookId");

                    b.ToTable("NotebookAuthorizedUsers");
                });

            modelBuilder.Entity("NotebookDB.Database.Section", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<int>("Columns")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("NotebookId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("Order")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("NotebookId");

                    b.ToTable("Sections");
                });

            modelBuilder.Entity("NotebookDB.Database.SystemSetting", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("EmailDomainRestriction")
                        .HasColumnType("TEXT");

                    b.Property<bool>("RegistrationEnabled")
                        .HasColumnType("INTEGER");

                    b.Property<string>("SendGridKey")
                        .HasColumnType("TEXT");

                    b.Property<string>("SendGridSystemEmailAddress")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("SystemSettings");
                });

            modelBuilder.Entity("NotebookDB.Database.TemplateField", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<int>("Column")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("NotebookId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Options")
                        .HasColumnType("TEXT");

                    b.Property<int>("Order")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Required")
                        .HasColumnType("INTEGER");

                    b.Property<int>("SearchOrder")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("SearchShow")
                        .HasColumnType("INTEGER");

                    b.Property<string>("SectionId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("Type")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("NotebookId");

                    b.HasIndex("SectionId");

                    b.ToTable("TemplateFields");
                });

            modelBuilder.Entity("NotebookDB.Database.TemplateNotebook", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<bool>("AllowAttachments")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Icon")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("TemplateNotebooks");
                });

            modelBuilder.Entity("NotebookDB.Database.TemplateSection", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<int>("Columns")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("NotebookId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("Order")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("NotebookId");

                    b.ToTable("TemplateSections");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("NotebookDB.Database.Field", b =>
                {
                    b.HasOne("NotebookDB.Database.Notebook", "Notebook")
                        .WithMany("Fields")
                        .HasForeignKey("NotebookId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("NotebookDB.Database.Section", "Section")
                        .WithMany("Fields")
                        .HasForeignKey("SectionId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("Notebook");

                    b.Navigation("Section");
                });

            modelBuilder.Entity("NotebookDB.Database.File", b =>
                {
                    b.HasOne("NotebookDB.Database.Instance", "Instance")
                        .WithMany("Files")
                        .HasForeignKey("InstanceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Instance");
                });

            modelBuilder.Entity("NotebookDB.Database.FileShard", b =>
                {
                    b.HasOne("NotebookDB.Database.File", "File")
                        .WithMany()
                        .HasForeignKey("FileId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("File");
                });

            modelBuilder.Entity("NotebookDB.Database.Instance", b =>
                {
                    b.HasOne("NotebookDB.Database.Notebook", "Notebook")
                        .WithMany("Instances")
                        .HasForeignKey("NotebookId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Notebook");
                });

            modelBuilder.Entity("NotebookDB.Database.InstanceValue", b =>
                {
                    b.HasOne("NotebookDB.Database.Field", "Field")
                        .WithMany()
                        .HasForeignKey("FieldId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("NotebookDB.Database.Instance", "Instance")
                        .WithMany("Values")
                        .HasForeignKey("InstanceId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("Field");

                    b.Navigation("Instance");
                });

            modelBuilder.Entity("NotebookDB.Database.NotebookAuthorizedUser", b =>
                {
                    b.HasOne("NotebookDB.Database.Notebook", "Notebook")
                        .WithMany("AuthorizedUsers")
                        .HasForeignKey("NotebookId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Notebook");
                });

            modelBuilder.Entity("NotebookDB.Database.Section", b =>
                {
                    b.HasOne("NotebookDB.Database.Notebook", "Notebook")
                        .WithMany("Sections")
                        .HasForeignKey("NotebookId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Notebook");
                });

            modelBuilder.Entity("NotebookDB.Database.TemplateField", b =>
                {
                    b.HasOne("NotebookDB.Database.TemplateNotebook", "Notebook")
                        .WithMany("Fields")
                        .HasForeignKey("NotebookId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("NotebookDB.Database.TemplateSection", "Section")
                        .WithMany("Fields")
                        .HasForeignKey("SectionId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("Notebook");

                    b.Navigation("Section");
                });

            modelBuilder.Entity("NotebookDB.Database.TemplateSection", b =>
                {
                    b.HasOne("NotebookDB.Database.TemplateNotebook", "Notebook")
                        .WithMany("Sections")
                        .HasForeignKey("NotebookId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Notebook");
                });

            modelBuilder.Entity("NotebookDB.Database.Instance", b =>
                {
                    b.Navigation("Files");

                    b.Navigation("Values");
                });

            modelBuilder.Entity("NotebookDB.Database.Notebook", b =>
                {
                    b.Navigation("AuthorizedUsers");

                    b.Navigation("Fields");

                    b.Navigation("Instances");

                    b.Navigation("Sections");
                });

            modelBuilder.Entity("NotebookDB.Database.Section", b =>
                {
                    b.Navigation("Fields");
                });

            modelBuilder.Entity("NotebookDB.Database.TemplateNotebook", b =>
                {
                    b.Navigation("Fields");

                    b.Navigation("Sections");
                });

            modelBuilder.Entity("NotebookDB.Database.TemplateSection", b =>
                {
                    b.Navigation("Fields");
                });
#pragma warning restore 612, 618
        }
    }
}
