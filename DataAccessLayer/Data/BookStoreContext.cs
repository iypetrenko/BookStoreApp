// DataAccessLayer/Data/BookStoreContext.cs
using Microsoft.EntityFrameworkCore;
using DomainTables.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace DataAccessLayer.Data
{
    public class BookStoreContext : DbContext
    {
        public BookStoreContext(DbContextOptions<BookStoreContext> options) : base(options)
        {
        }

        public DbSet<Author> Authors { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<BookReview> BookReviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure cascade delete behavior
            modelBuilder.Entity<Book>()
                .HasOne(b => b.Author)
                .WithMany(a => a.Books)
                .HasForeignKey(b => b.AuthorId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Book>()
                .HasOne(b => b.Genre)
                .WithMany(g => g.Books)
                .HasForeignKey(b => b.GenreId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BookReview>()
                .HasOne(br => br.Book)
                .WithMany(b => b.BookReviews)
                .HasForeignKey(br => br.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed data
            modelBuilder.Entity<Author>().HasData(
                new Author { AuthorId = 1, FirstName = "Джордж", LastName = "Орвелл", Email = "orwell@example.com", DateOfBirth = new DateTime(1903, 6, 25) },
                new Author { AuthorId = 2, FirstName = "Рей", LastName = "Бредбери", Email = "bradbury@example.com", DateOfBirth = new DateTime(1920, 8, 22) }
            );

            modelBuilder.Entity<Genre>().HasData(
                new Genre { GenreId = 1, Name = "Научная фантастика", Description = "Жанр художественной литературы" },
                new Genre { GenreId = 2, Name = "Антиутопия", Description = "Жанр художественной литературы" }
            );

            modelBuilder.Entity<Book>().HasData(
                new Book { BookId = 1, Title = "1984", ISBN = "978-0-452-28423-4", PublishedDate = new DateTime(1949, 6, 8), Price = 299.99m, AuthorId = 1, GenreId = 2 },
                new Book { BookId = 2, Title = "451 градус по Фаренгейту", ISBN = "978-1-451-67331-9", PublishedDate = new DateTime(1953, 10, 19), Price = 349.99m, AuthorId = 2, GenreId = 1 }
            );
        }
    }
}