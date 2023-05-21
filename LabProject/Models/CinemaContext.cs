using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace LabProject.Models;

public partial class CinemaContext : DbContext
{
    public CinemaContext()
    {

    }

    public CinemaContext(DbContextOptions<CinemaContext> options)
        : base(options)
    {

    }

    public virtual DbSet<CastMember> CastMembers { get; set; }

    public virtual DbSet<Cinema> Cinemas { get; set; }

    public virtual DbSet<Genre> Genres { get; set; }

    public virtual DbSet<Hall> Halls { get; set; }

    public virtual DbSet<Movie> Movies { get; set; }

    public virtual DbSet<MovieCast> MovieCasts { get; set; }

    public virtual DbSet<MovieGenre> MovieGenres { get; set; }

    public virtual DbSet<Position> Positions { get; set; }

    public virtual DbSet<Session> Sessions { get; set; }

    public virtual DbSet<Status> Statuses { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-9O78KC4\\SQLEXPRESS;Database=Cinema;Trusted_Connection=True;Trust Server Certificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CastMember>(entity =>
        {
            entity.ToTable("CastMember");

            entity.Property(e => e.CastMemberFullName)
                .IsRequired()
                .HasMaxLength(50);
        });

        modelBuilder.Entity<Cinema>(entity =>
        {
            entity.ToTable("Cinema");

            entity.Property(e => e.CinemaAddress)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.CinemaName)
                .IsRequired()
                .HasMaxLength(50);
        });

        modelBuilder.Entity<Genre>(entity =>
        {
            entity.ToTable("Genre");

            entity.Property(e => e.GenreName)
                .IsRequired()
                .HasMaxLength(20);
        });

        modelBuilder.Entity<Hall>(entity =>
        {
            entity.ToTable("Hall");

            entity.Property(e => e.HallName)
                .IsRequired()
                .HasMaxLength(5);

            entity.HasOne(d => d.Cinema).WithMany(p => p.Halls)
                .HasForeignKey(d => d.CinemaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Hall_Cinema");
        });

        modelBuilder.Entity<Movie>(entity =>
        {
            entity.ToTable("Movie");

            entity.Property(e => e.MovieName)
                .IsRequired()
                .HasMaxLength(200);
            entity.Property(e => e.MovieReleaseDate).HasColumnType("date");
        });

        modelBuilder.Entity<MovieCast>(entity =>
        {
            entity.ToTable("MovieCast");

            entity.HasOne(d => d.CastMember).WithMany(p => p.MovieCasts)
                .HasForeignKey(d => d.CastMemberId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MovieCast_CastMember");

            entity.HasOne(d => d.Movie).WithMany(p => p.MovieCasts)
                .HasForeignKey(d => d.MovieId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MovieCast_Movie");

            entity.HasOne(d => d.Position).WithMany(p => p.MovieCasts)
                .HasForeignKey(d => d.PositionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MovieCast_Position");
        });

        modelBuilder.Entity<MovieGenre>(entity =>
        {
            entity.ToTable("MovieGenre");

            entity.HasOne(d => d.Genre).WithMany(p => p.MovieGenres)
                .HasForeignKey(d => d.GenreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MovieGenre_Genre");

            entity.HasOne(d => d.Movie).WithMany(p => p.MovieGenres)
                .HasForeignKey(d => d.MovieId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MovieGenre_Movie");
        });

        modelBuilder.Entity<Position>(entity =>
        {
            entity.ToTable("Position");

            entity.Property(e => e.PositionName)
                .IsRequired()
                .HasMaxLength(100);
        });

        modelBuilder.Entity<Session>(entity =>
        {
            entity.ToTable("Session");

            entity.Property(e => e.SessionDateTime).HasColumnType("datetime");
            entity.Property(e => e.SessionNumber)
                .IsRequired()
                .HasMaxLength(10);

            entity.HasOne(d => d.Hall).WithMany(p => p.Sessions)
                .HasForeignKey(d => d.HallId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Session_Hall");

            entity.HasOne(d => d.Movie).WithMany(p => p.Sessions)
                .HasForeignKey(d => d.MovieId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Session_Movie");

            entity.HasOne(d => d.Status).WithMany(p => p.Sessions)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Session_Status");
        });

        modelBuilder.Entity<Status>(entity =>
        {
            entity.ToTable("Status");

            entity.Property(e => e.StatusName)
                .IsRequired()
                .HasMaxLength(20);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
