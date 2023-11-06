using FluentAssertions;
using Library.Api.Integration.Tests.Shared;
using Library.API.Models;
using System.Net;

namespace Library.Api.Integration.Tests;

public class LibraryEndpointsTests : IClassFixture<LibraryApiFactory>, IAsyncLifetime
{
    private readonly LibraryApiFactory _factory;
    private List<string> _createdIsbns = new();

    public LibraryEndpointsTests(LibraryApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateBook_CreatesBook_WhenDataIsCorrect()
    {
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();

        var result = await httpClient.PostAsJsonAsync("/books", book);
        var createdBook = await result.Content.ReadFromJsonAsync<Book>();
        _createdIsbns.Add(book.Isbn);

        result.StatusCode.Should().Be(HttpStatusCode.Created);
        createdBook.Should().BeEquivalentTo(book);
        result.Headers.Location?.ToString().Should().Contain($"books/{book.Isbn}");
    }

    [Fact]
    public async Task CreateBook_Fails_WhenIsbnIsInvalid()
    {
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();
        book.Isbn = "INVALID_ISBN";

        var result = await httpClient.PostAsJsonAsync("/books", book);
        var errors = await result.Content.ReadFromJsonAsync<IEnumerable<ValidationError>>();
        var error = errors!.Single();

        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.PropertyName.Should().Be("Isbn");
        error.ErrorMessage.Should().Be("Value was not valid ISBN-13");
    }

    [Fact]
    public async Task CreateBook_Fails_WhenBookExists()
    {
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();

        await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book.Isbn);
        var result = await httpClient.PostAsJsonAsync("/books", book);
        var errors = await result.Content.ReadFromJsonAsync<IEnumerable<ValidationError>>();
        var error = errors!.Single();

        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.PropertyName.Should().Be("Isbn");
        error.ErrorMessage.Should().Be("A book with this ISBN-13 already exists");
    }

    [Fact]
    public async Task GetBook_ReturnsBook_WhenBooksExists()
    {
        //Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();
        await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book.Isbn);
        //Act
        var result = await httpClient.GetAsync($"/books/{book.Isbn}");
        var existingBook = await result.Content.ReadFromJsonAsync<Book>();
        //Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        existingBook.Should().BeEquivalentTo(book);
    }

    [Fact]
    public async Task GetBook_ReturnsNotFound_WhenBooksDoesNotExists()
    {
        //Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();
        //Act
        var result = await httpClient.GetAsync($"/books/{book.Isbn}");
        //Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAllBooks_ReturnsAllBooks_WhenBooksExist()
    {
        //Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();
        await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book.Isbn);
        var books = new List<Book> { book };
        //Act
        var result = await httpClient.GetAsync("/books");
        var returnedBooks = await result.Content.ReadFromJsonAsync<List<Book>>();
        //Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        returnedBooks.Should().BeEquivalentTo(books);
    }
    [Fact]
    public async Task GetAllBooks_ReturnsNoBooks_WhenNoBooksExist()
    {
        //Arrange
        var httpClient = _factory.CreateClient();
        //Act
        var result = await httpClient.GetAsync("/books");
        var returnedBooks = await result.Content.ReadFromJsonAsync<List<Book>>();
        //Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        returnedBooks.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchBooks_ReturnsBooks_WhenTitleMatches()
    {
        //Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();
        await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book.Isbn);
        var termSearch = book.Title.Substring(0, 3);
        await httpClient.GetAsync($"/books?searchTerm={termSearch}");
        var books = new List<Book> { book };
        //Act
        var result = await httpClient.GetAsync("/books");
        var returnedBooks = await result.Content.ReadFromJsonAsync<List<Book>>();
        //Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        returnedBooks.Should().BeEquivalentTo(books);
    }

    [Fact]
    public async Task UpdateBook_UpdatesBook_WhenDataIsCorrect()
    {
        //Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();
        await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book.Isbn);
        //Act
        book.PageCount = 69;
        var result = await httpClient.PutAsJsonAsync($"/books/{book.Isbn}", book);
        var updateBook = await result.Content.ReadFromJsonAsync<Book>();
        //Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        updateBook.Should().BeEquivalentTo(book);
    }

    [Fact]
    public async Task UpdateBook_DoesNotUpdatesBook_WhenDataIsIncorrect()
    {
        //Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();
        await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book.Isbn);
        //Act
        book.Title = string.Empty;
        var result = await httpClient.PutAsJsonAsync($"/books/{book.Isbn}", book);
        var errors = await result.Content.ReadFromJsonAsync<IEnumerable<ValidationError>>();
        var error = errors!.Single();
        //Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.PropertyName.Should().Be("Title");
    }

    [Fact]
    public async Task UpdateBook_ReturnsNoutFound_WhenBookDoesNotExists()
    {
        //Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();
        //Act
        var result = await httpClient.PutAsJsonAsync($"/books/{book.Isbn}", book);
        //Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteBook_ReturnsNoutFound_WhenBookDoesNotExists()
    {
        //Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();
        //Act
        var result = await httpClient.DeleteAsync($"/books/{book.Isbn}");
        //Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteBook_ReturnsNoContent_WhenBookExists()
    {
        //Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();
        await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book.Isbn);
        //Act
        var result = await httpClient.DeleteAsync($"/books/{book.Isbn}");
        //Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private Book GenerateBook()
    {
        return new Book
        {
            Isbn = GenerateIsbn(),
            Title = $"RandomBook{Random.Shared.Next(1, 500)}",
            Author = $"RandomAuthor{Random.Shared.Next(1, 500)}",
            PageCount = Random.Shared.Next(1, 500),
            ShortDescription = $"Random Description {Random.Shared.Next(1, 500)}",
            ReleaseDate = new DateTime(2023, 1, 1)
        };
    }

    private string GenerateIsbn()
    {
        return $"{Random.Shared.Next(100, 999)}" +
               $"{Random.Shared.Next(1000000000, 2100999999)}";
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        var httpClient = _factory.CreateClient();
        foreach (var createdIsbn in _createdIsbns)
        {
            await httpClient.DeleteAsync($"/books/{createdIsbn}");
        }
    }
}