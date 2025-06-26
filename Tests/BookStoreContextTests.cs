using Xunit;
using Microsoft.EntityFrameworkCore;
using DataAccessLayer.Data;
using DomainTables.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Tests
{
    public class BookStoreContextTests : IDisposable
    {
        private readonly BookStoreContext _context;

        public BookStoreContextTests()
        {
            var options = new DbContextOptionsBuilder<BookStoreContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new BookStoreContext(options);
            _context.Database.EnsureCreated();
        }

        [Fact]
        public async Task CreateAuthor_ShouldAddAuthorToDatabase()
        {
            // Arrange
            var author = new Author
            {
                FirstName = "Тест",
                LastName = "Автор",
                Email = "test@example.com",
                DateOfBirth = new DateTime(1980, 1, 1)
            };

            // Act
            _context.Authors.Add(author);
            await _context.SaveChangesAsync();

            // Assert
            var savedAuthor = await _context.Authors.FirstOrDefaultAsync(a => a.Email == "test@example.com");
            Assert.NotNull(savedAuthor);
            Assert.Equal("Тест", savedAuthor.FirstName);
            Assert.Equal("Автор", savedAuthor.LastName);
        }

        [Fact]
        public async Task CreateBook_ShouldAddBookToDatabase()
        {
            // Arrange
            var genre = new Genre { Name = "Тестовый жанр", Description = "Описание" };
            var author = new Author
            {
                FirstName = "Тест",
                LastName = "Автор",
                Email = "author@test.com",
                DateOfBirth = new DateTime(1980, 1, 1)
            };

            _context.Genres.Add(genre);
            _context.Authors.Add(author);
            await _context.SaveChangesAsync();

            var book = new Book
            {
                Title = "Тестовая книга",
                ISBN = "978-0-000-00000-0",
                PublishedDate = DateTime.Now,
                Price = 199.99m,
                AuthorId = author.AuthorId,
                GenreId = genre.GenreId
            };

            // Act
            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            // Assert
            var savedBook = await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Genre)
                .FirstOrDefaultAsync(b => b.Title == "Тестовая книга");

            Assert.NotNull(savedBook);
            Assert.Equal("Тестовая книга", savedBook.Title);
            Assert.Equal(author.AuthorId, savedBook.AuthorId);
            Assert.Equal(genre.GenreId, savedBook.GenreId);
        }

        [Fact]
        public async Task UpdateAuthor_ShouldModifyAuthorInDatabase()
        {
            // Arrange
            var author = new Author
            {
                FirstName = "Старое",
                LastName = "Имя",
                Email = "old@example.com",
                DateOfBirth = new DateTime(1980, 1, 1)
            };

            _context.Authors.Add(author);
            await _context.SaveChangesAsync();

            // Act
            author.FirstName = "Новое";
            author.LastName = "Имя";
            author.Email = "new@example.com";

            _context.Authors.Update(author);
            await _context.SaveChangesAsync();

            // Assert
            var updatedAuthor = await _context.Authors.FindAsync(author.AuthorId);
            Assert.NotNull(updatedAuthor);
            Assert.Equal("Новое", updatedAuthor.FirstName);
            Assert.Equal("Имя", updatedAuthor.LastName);
            Assert.Equal("new@example.com", updatedAuthor.Email);
        }

        [Fact]
        public async Task DeleteAuthor_ShouldRemoveAuthorFromDatabase()
        {
            // Arrange
            var author = new Author
            {
                FirstName = "Удаляемый",
                LastName = "Автор",
                Email = "delete@example.com",
                DateOfBirth = new DateTime(1980, 1, 1)
            };

            _context.Authors.Add(author);
            await _context.SaveChangesAsync();
            var authorId = author.AuthorId;

            // Act
            _context.Authors.Remove(author);
            await _context.SaveChangesAsync();

            // Assert
            var deletedAuthor = await _context.Authors.FindAsync(authorId);
            Assert.Null(deletedAuthor);
        }

        [Fact]
        public async Task CascadeDelete_ShouldDeleteBooksWhenAuthorDeleted()
        {
            // Arrange
            var genre = new Genre { Name = "Тестовый жанр", Description = "Описание" };
            var author = new Author
            {
                FirstName = "Каскад",
                LastName = "Автор",
                Email = "cascade@test.com",
                DateOfBirth = new DateTime(1980, 1, 1)
            };

            _context.Genres.Add(genre);
            _context.Authors.Add(author);
            await _context.SaveChangesAsync();

            var book = new Book
            {
                Title = "Книга для каскадного удаления",
                ISBN = "978-0-000-00001-0",
                PublishedDate = DateTime.Now,
                Price = 299.99m,
                AuthorId = author.AuthorId,
                GenreId = genre.GenreId
            };

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            var bookId = book.BookId;

            // Act
            _context.Authors.Remove(author);
            await _context.SaveChangesAsync();

            // Assert
            var deletedBook = await _context.Books.FindAsync(bookId);
            Assert.Null(deletedBook);
        }

        [Fact]
        public async Task RestrictDelete_ShouldPreventGenreDeletionWithBooks()
        {
            // Arrange
            var genre = new Genre { Name = "Защищенный жанр", Description = "Описание" };
            var author = new Author
            {
                FirstName = "Тест",
                LastName = "Автор",
                Email = "protect@test.com",
                DateOfBirth = new DateTime(1980, 1, 1)
            };

            _context.Genres.Add(genre);
            _context.Authors.Add(author);
            await _context.SaveChangesAsync();

            var book = new Book
            {
                Title = "Книга с защищенным жанром",
                ISBN = "978-0-000-00002-0",
                PublishedDate = DateTime.Now,
                Price = 399.99m,
                AuthorId = author.AuthorId,
                GenreId = genre.GenreId
            };

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            // Act & Assert
            _context.Genres.Remove(genre);

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await _context.SaveChangesAsync();
            });
        }

        [Fact]
        public async Task CreateBookReview_ShouldAddReviewToDatabase()
        {
            // Arrange
            var genre = new Genre { Name = "Тестовый жанр", Description = "Описание" };
            var author = new Author
            {
                FirstName = "Тест",
                LastName = "Автор",
                Email = "review@test.com",
                DateOfBirth = new DateTime(1980, 1, 1)
            };

            _context.Genres.Add(genre);
            _context.Authors.Add(author);
            await _context.SaveChangesAsync();

            var book = new Book
            {
                Title = "Книга для отзыва",
                ISBN = "978-0-000-00003-0",
                PublishedDate = DateTime.Now,
                Price = 199.99m,
                AuthorId = author.AuthorId,
                GenreId = genre.GenreId
            };

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            var review = new BookReview
            {
                BookId = book.BookId,
                ReviewerName = "Тестовый рецензент",
                Rating = 5,
                Comment = "Отличная книга!",
                ReviewDate = DateTime.Now
            };

            // Act
            _context.BookReviews.Add(review);
            await _context.SaveChangesAsync();

            // Assert
            var savedReview = await _context.BookReviews
                .Include(br => br.Book)
                .FirstOrDefaultAsync(br => br.ReviewerName == "Тестовый рецензент");

            Assert.NotNull(savedReview);
            Assert.Equal(5, savedReview.Rating);
            Assert.Equal("Отличная книга!", savedReview.Comment);
            Assert.Equal(book.BookId, savedReview.BookId);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}