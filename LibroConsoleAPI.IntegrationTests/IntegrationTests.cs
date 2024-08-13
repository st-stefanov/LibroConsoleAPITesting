using LibroConsoleAPI.Business;
using LibroConsoleAPI.Business.Contracts;
using LibroConsoleAPI.Data.Models;
using LibroConsoleAPI.DataAccess;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;

namespace LibroConsoleAPI.IntegrationTests
{
    public class IntegrationTests : IClassFixture<BookManagerFixture>
    {
        private readonly BookManager _bookManager;
        private readonly TestLibroDbContext _dbContext;

        public IntegrationTests(BookManagerFixture fixture)
        {
            _bookManager = fixture.BookManager;
            _dbContext = fixture.DbContext;
        }

        [Fact]
        public async Task AddBookAsync_ShouldAddBook()
        {
            // Arrange
            var newBook = new Book
            {
                Title = "Test Book",
                Author = "John Doe",
                ISBN = "1234567890123",
                YearPublished = 2021,
                Genre = "Fiction",
                Pages = 100,
                Price = 19.99
            };

            // Act
            await _bookManager.AddAsync(newBook);

            // Assert
            var bookInDb = await _dbContext.Books.FirstOrDefaultAsync(b => b.ISBN == newBook.ISBN);
            Assert.NotNull(bookInDb);
            Assert.Equal("Test Book", bookInDb.Title);
            Assert.Equal("John Doe", bookInDb.Author);
        }

        [Fact]
        public async Task AddBookAsync_TryToAddBookWithInvalidTitle_ShouldThrowException()
        {
            // Arrange
            var newBook = new Book
            {
                Title = new string ('A', 500),
                Author = "John Doe",
                ISBN = "1234567890123",
                YearPublished = 2021,
                Genre = "Fiction",
                Pages = 100,
                Price = 19.99
            };

            // Act  
            var exception = Assert.ThrowsAsync<ValidationException>(() => _bookManager.AddAsync(newBook));

            // Assert
            Assert.Equal("Book is invalid.", exception.Result.Message);
        }

        [Fact]
        public async Task AddBookAsync_TryToAddBookWithInvalidISBN_ShouldThrowException()
        {
            // Arrange
            var newBook = new Book
            {
                Title = "AwesomeBook",
                Author = "John Doe",
                ISBN = "jfk",
                YearPublished = 2021,
                Genre = "Fiction",
                Pages = 100,
                Price = 19.99
            };

            // Act  
            var exception = Assert.ThrowsAsync<ValidationException>(() => _bookManager.AddAsync(newBook));

            // Assert
            Assert.Equal("Book is invalid.", exception.Result.Message);
            var bookInDb = await _dbContext.Books.FirstOrDefaultAsync();
            Assert.Null(bookInDb);
        }

        [Fact]
        public async Task DeleteBookAsync_WithValidISBN_ShouldRemoveBookFromDb()
        {
            // Arrange
            /*var newBook = new Book
            {
                Title = "AwesomeBook",
                Author = "John Doe",
                ISBN = "1234567890123",
                YearPublished = 2021,
                Genre = "Fiction",
                Pages = 100,
                Price = 19.99
            }; */
            await DatabaseSeeder.SeedDatabaseAsync(_dbContext, _bookManager);
            
            // Act
            await _bookManager.DeleteAsync("9780140449276");
            // Assert
            var bookInDb = _dbContext.Books.ToList();
            Assert.Equal(9, bookInDb.Count);
        }

        [Fact]
        public async Task DeleteBookAsync_TryToDeleteWithNullOrWhiteSpaceISBN_ShouldThrowException()
        {

            //Arrange
            await DatabaseSeeder.SeedDatabaseAsync(_dbContext, _bookManager);

            // Act
            var exception = Assert.ThrowsAsync<ArgumentException>(() => _bookManager.DeleteAsync(null));
            // Assert
            Assert.Equal("ISBN cannot be empty.", exception.Result.Message);
        }

        [Fact]
        public async Task GetAllAsync_WhenBooksExist_ShouldReturnAllBooks()
        {
            // Arrange
            await DatabaseSeeder.SeedDatabaseAsync(_dbContext, _bookManager);
            // Act
            var result = _bookManager.GetAllAsync();
            // Assert
            Assert.Equal(10, result.Result.Count());
        }

        [Fact]
        public async Task GetAllAsync_WhenNoBooksExist_ShouldThrowKeyNotFoundException()
        {
            // Arrange

            //Act
            var exception = Assert.ThrowsAsync<KeyNotFoundException>(() => _bookManager.GetAllAsync());
            // Assert
            Assert.Equal("No books found.", exception.Result.Message);
        }

        [Fact]
        public async Task SearchByTitleAsync_WithValidTitleFragment_ShouldReturnMatchingBooks()
        {
            // Arrange
            await DatabaseSeeder.SeedDatabaseAsync(_dbContext, _bookManager);
            // Act
            var result = await _bookManager.SearchByTitleAsync("War and Peace");
            // Assert
            Assert.Equal("War and Peace", result.FirstOrDefault().Title);
        }

        [Fact]
        public async Task SearchByTitleAsync_WithInvalidTitleFragment_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            await DatabaseSeeder.SeedDatabaseAsync(_dbContext, _bookManager);
            // Act
            var exception = Assert.ThrowsAsync<ArgumentException>(() => _bookManager.SearchByTitleAsync(null));
            // Assert
            Assert.Equal("Title fragment cannot be empty.", exception.Result.Message);
        }

        [Fact]
        public async Task GetSpecificAsync_WithValidIsbn_ShouldReturnBook()
        {
            // Arrange
            await DatabaseSeeder.SeedDatabaseAsync(_dbContext, _bookManager);

            // Act
            var result = await _bookManager.GetSpecificAsync("9780307743394");
            // Assert
            Assert.Equal("9780307743394", result.ISBN);
            
        }

        [Fact]
        public async Task GetSpecificAsync_WithInvalidIsbn_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            await DatabaseSeeder.SeedDatabaseAsync(_dbContext, _bookManager);

            // Act
            var exception = Assert.ThrowsAsync<ArgumentException>(() => _bookManager.GetSpecificAsync(null));
            // Assert
            Assert.Equal("ISBN cannot be empty.", exception.Result.Message);
        }

        [Fact]
        public async Task UpdateAsync_WithValidBook_ShouldUpdateBook()
        {
            // Arrange
            var newBook = new Book
            {
                Title = "TestBook",
                Author = "John Doe",
                ISBN = "9780307743394",
                YearPublished = 2021,
                Genre = "Fiction",
                Pages = 100,
                Price = 19.99
            };
            await _bookManager.AddAsync(newBook);

            newBook.Title = "UpdatedTitle";
            // Act
            await _bookManager.UpdateAsync(newBook);
            // Assert
            var bookInDb = _dbContext.Books.FirstOrDefault(b => b.Title == newBook.Title);

            Assert.Equal("UpdatedTitle", bookInDb.Title);
        }

        [Fact]
        public async Task UpdateAsync_WithInvalidBook_ShouldThrowValidationException()
        {
            // Arrange
            await DatabaseSeeder.SeedDatabaseAsync(_dbContext, _bookManager);
            // Act

            // Assert
        }

    }
}
