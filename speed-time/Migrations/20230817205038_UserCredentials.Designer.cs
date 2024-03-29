﻿// <auto-generated />
using System;
using DSaladin.SpeedTime;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace DSaladin.SpeedTime.Migrations
{
    [DbContext(typeof(TimeTrackerContext))]
    [Migration("20230817205038_UserCredentials")]
    partial class UserCredentials
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.10")
                .HasAnnotation("Proxies:ChangeTracking", false)
                .HasAnnotation("Proxies:CheckEquality", false)
                .HasAnnotation("Proxies:LazyLoading", true);

            modelBuilder.Entity("DSaladin.SpeedTime.Model.TrackAttribute", b =>
                {
                    b.Property<int>("TrackTimeId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("TrackTimeId", "Name");

                    b.ToTable("TrackAttribute");
                });

            modelBuilder.Entity("DSaladin.SpeedTime.Model.TrackTime", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsBreak")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("TrackingStarted")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("TrackingStopped")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("TrackedTimes");
                });

            modelBuilder.Entity("DSaladin.SpeedTime.Model.UserCredential", b =>
                {
                    b.Property<int>("ServiceType")
                        .HasColumnType("INTEGER");

                    b.Property<byte[]>("Credential")
                        .IsRequired()
                        .HasColumnType("BLOB");

                    b.Property<string>("ServiceUri")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("ServiceType");

                    b.ToTable("UserCredential");
                });

            modelBuilder.Entity("DSaladin.SpeedTime.Model.TrackAttribute", b =>
                {
                    b.HasOne("DSaladin.SpeedTime.Model.TrackTime", "TrackTime")
                        .WithMany("Attributes")
                        .HasForeignKey("TrackTimeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("TrackTime");
                });

            modelBuilder.Entity("DSaladin.SpeedTime.Model.TrackTime", b =>
                {
                    b.Navigation("Attributes");
                });
#pragma warning restore 612, 618
        }
    }
}
