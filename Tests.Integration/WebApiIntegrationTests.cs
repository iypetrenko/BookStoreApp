using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Xunit;
using DataAccessLayer.Data;
using DomainTables.Models;
using System.Net;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace Tests.Integration
{
    public class WebApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _jsonOptions;

        public WebApiIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove the existing DbContext registration
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<BookStoreContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    // Add in-memory database for testing
                    services.AddDbContext<BookStoreContext>(options =>
                    {
                        options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid().ToString());
                    });
                });
            });

            _client = _factory.CreateClient();
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        [Fact]
        public async Task GetAuthors_ReturnsSuccessAndCorrectContentType()
        {
            // Act
            var response = await _client.GetAsync("/api/authors");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal("application/json; charset=utf-8",
                response.Content.Headers.ContentType?.ToString());
        }

        [Fact]
        public async Task CreateAuthor_ReturnsCreatedAuthor()
        {
            // Arrange
            var author = new Author
            {
                FirstName = "Интеграционный",
                LastName = "Тест",
                Email = "integration@test.com",
                DateOfBirth = new DateTime(1990, 1, 1)
            };

            var json = JsonSerializer.Serialize(author, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/authors", content);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var createdAuthor = JsonSerializer.Deserialize<Author>(responseContent, _jsonOptions);

            Assert.NotNull(createdAuthor);
            Assert.Equal("Интеграционный", createdAuthor.FirstName);
            Assert.Equal("Тест", createdAuthor.LastName);
            Assert.True(createdAuthor.AuthorId > 0);
        }

        [Fact]
        public async Task GetAuthorById_ReturnsCorrectAuthor()
        {
            // Arrange - создаем автора через API
            var author = new Author
            {
                FirstName = "Тестовый",
                LastName = "Автор",
                Email = "test@example.com",
                DateOfBirth = new DateTime(1985, 5, 15)
            };

            var json = JsonSerializer.Serialize(author, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var createResponse = await _client.PostAsync("/api/authors", content);

            var createResponseContent = await createResponse.Content.ReadAsStringAsync();
            var createdAuthor = JsonSerializer.Deserialize<Author>(createResponseContent, _jsonOptions);

            // Act
            var response = await _client.GetAsync($"/api/authors/{createdAuthor.AuthorId}");

            // Assert
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var retrievedAuthor = JsonSerializer.Deserialize<Author>(responseContent, _jsonOptions);

            Assert.NotNull(retrievedAuthor);
            Assert.Equal(createdAuthor.AuthorId, retrievedAuthor.AuthorId);
            Assert.Equal("Тестовый", retrievedAuthor.FirstName);
        }

        [Fact]
        public async Task UpdateAuthor_ReturnsNoContent()
        {
            // Arrange - создаем автора
            var author = new Author
            {
                FirstName = "Старое",
                LastName = "Имя",
                Email = "old@example.com",
                DateOfBirth = new DateTime(1980, 1, 1)
            };

            var json = JsonSerializer.Serialize(author, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var createResponse = await _client.PostAsync("/api/authors", content);

            var createResponseContent = await createResponse.Content.ReadAsStringAsync();
            var createdAuthor = JsonSerializer.Deserialize<Author>(createResponseContent, _jsonOptions);

            // Изменяем данные
            createdAuthor.FirstName = "Новое";
            createdAuthor.LastName = "Имя";
            createdAuthor.Email = "new@example.com";

            var updateJson = JsonSerializer.Serialize(createdAuthor, _jsonOptions);
            var updateContent = new StringContent(updateJson, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync($"/api/authors/{createdAuthor.AuthorId}", updateContent);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Проверяем, что изменения сохранились
            var getResponse = await _client.GetAsync($"/api/authors/{createdAuthor.AuthorId}");
            var getResponseContent = await getResponse.Content.ReadAsStringAsync();
            var updatedAuthor = JsonSerializer.Deserialize<Author>(getResponseContent, _jsonOptions);

            Assert.Equal("Новое", updatedAuthor.FirstName);
            Assert.Equal("new@example.com", updatedAuthor.Email);
        }

        [Fact]
        public async Task DeleteAuthor_ReturnsNoContent()
        {
            // Arrange - создаем автора
            var author = new Author
            {
                FirstName = "Удаляемый",
                LastName = "Автор",
                Email = "delete@example.com",
                DateOfBirth = new DateTime(1975, 3, 10)
            };

            var json = JsonSerializer.Serialize(author, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var createResponse = await _client.PostAsync("/api/authors", content);

            var createResponseContent = await createResponse.Content.ReadAsStringAsync();
            var createdAuthor = JsonSerializer.Deserialize<Author>(createResponseContent, _jsonOptions);

            // Act
            var response = await _client.DeleteAsync($"/api/authors/{createdAuthor.AuthorId}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Проверяем, что автор действительно удален
            var getResponse = await _client.GetAsync($"/api/authors/{createdAuthor.AuthorId}");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Fact]
        public async Task CreateBook_WithValidData_ReturnsCreatedBook()
        {
            // Arrange - создаем автора и жанр
            var author = new Author
            {
                FirstName = "Автор",
                LastName = "Книги",
                Email = "author@book.com",
                DateOfBirth = new DateTime(1970, 1, 1)
            };

            var genre = new Genre
            {
                Name = "Тестовый жанр",
                Description = "Описание жанра"
            };

            // Создаем автора
            var authorJson = JsonSerializer.Serialize(author, _jsonOptions);
            var authorContent = new StringContent(authorJson, Encoding.UTF8, "application/json");
            var authorResponse = await _client.PostAsync("/api/authors", authorContent);
            var authorResponseContent = await authorResponse.Content.ReadAsStringAsync();
            var createdAuthor = JsonSerializer.Deserialize<Author>(authorResponseContent, _jsonOptions);

            // Создаем жанр
            var genreJson = JsonSerializer.Serialize(genre, _jsonOptions);
            var genreContent = new StringContent(genreJson, Encoding.UTF8, "application/json");
            var genreResponse = await _client.PostAsync("/api/genres", genreContent);
            var genreResponseContent = await genreResponse.Content.ReadAsStringAsync();
            var createdGenre = JsonSerializer.Deserialize<Genre>(genreResponseContent, _jsonOptions);

            // Создаем книгу
            var book = new Book
            {
                Title = "Тестовая книга",
                ISBN = "978-0-000-00000-0",
                PublishedDate = DateTime.Now,
                Price = 299.99m,
                AuthorId = createdAuthor.AuthorId,
                GenreId = createdGenre.GenreId
            };

            var bookJson = JsonSerializer.Serialize(book, _jsonOptions);
            var bookContent = new StringContent(bookJson, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/books", bookContent);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var createdBook = JsonSerializer.Deserialize<Book>(responseContent, _jsonOptions);

            Assert.NotNull(createdBook);
            Assert.Equal("Тестовая книга", createdBook.Title);
            Assert.True(createdBook.BookId > 0);
        }

        [Fact]
        public async Task CreateBookReview_ReturnsCreatedReview()
        {
            // Arrange - создаем необходимые сущности
            var author = new Author
            {
                FirstName = "Автор",
                LastName = "Отзыва",
                Email = "review@author.com",
                DateOfBirth = new DateTime(1980, 1, 1)
            };

            var genre = new Genre
            {
                Name = "Жанр для отзыва",
                Description = "Описание"
            };

            // Создаем автора и жанр
            var authorJson = JsonSerializer.Serialize(author, _jsonOptions);
            var authorContent = new StringContent(authorJson, Encoding.UTF8, "application/json");
            var authorResponse = await _client.PostAsync("/api/authors", authorContent);
            var createdAuthor = JsonSerializer.Deserialize<Author>(
                await authorResponse.Content.ReadAsStringAsync(), _jsonOptions);

            var genreJson = JsonSerializer.Serialize(genre, _jsonOptions);
            var genreContent = new StringContent(genreJson, Encoding.UTF8, "application/json");
            var genreResponse = await _client.PostAsync("/api/genres", genreContent);
            var createdGenre = JsonSerializer.Deserialize<Genre>(
                await genreResponse.Content.ReadAsStringAsync(), _jsonOptions);

            // Создаем книгу
            var book = new Book
            {
                Title = "Книга для отзыва",
                ISBN = "978-0-000-00001-0",
                PublishedDate = DateTime.Now,
                Price = 199.99m,
                AuthorId = createdAuthor.AuthorId,
                GenreId = createdGenre.GenreId
            };

            var bookJson = JsonSerializer.Serialize(book, _jsonOptions);
            var bookContent = new StringContent(bookJson, Encoding.UTF8, "application/json");
            var bookResponse = await _client.PostAsync("/api/books", bookContent);
            var createdBook = JsonSerializer.Deserialize<Book>(
                await bookResponse.Content.ReadAsStringAsync(), _jsonOptions);

            // Создаем отзыв
            var review = new BookReview
            {
                BookId = createdBook.BookId,
                ReviewerName = "Тестовый рецензент",
                Rating = 5,
                Comment = "Отличная книга!"
            };

            var reviewJson = JsonSerializer.Serialize(review, _jsonOptions);
            var reviewContent = new StringContent(reviewJson, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/bookreviews", reviewContent);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var createdReview = JsonSerializer.Deserialize<BookReview>(responseContent, _jsonOptions);

            Assert.NotNull(createdReview);
            Assert.Equal("Тестовый рецензент", createdReview.ReviewerName);
            Assert.Equal(5, createdReview.Rating);
            Assert.True(createdReview.ReviewId > 0);
        }

        [Fact]
        public async Task GetNonExistentAuthor_ReturnsNotFound()
        {
            // Act
            var response = await _client.GetAsync("/api/authors/99999");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        [Fact]
        public async Task UpdateAuthor_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var fakeAuthor = new Author
            {
                AuthorId = 99999,
                FirstName = "Не",
                LastName = "Существую",
                Email = "fake@example.com",
                DateOfBirth = new DateTime(1990, 1, 1)
            };

            var json = JsonSerializer.Serialize(fakeAuthor, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync("/api/authors/99999", content);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task DeleteAuthor_WithInvalidId_ReturnsNotFound()
        {
            // Act
            var response = await _client.DeleteAsync("/api/authors/99999");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}