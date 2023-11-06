using FluentValidation;
using FluentValidation.Results;
using Library.API.Auth;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Authorization;

namespace Library.API.Endpoints.Books;

internal static class BookMethods
{
    internal static async Task<IResult> GetAllBooks(IBookService bookService)
    {
        var books = await bookService.GetAllAsync();
        return Results.Ok(books);
    }
    internal static async Task<IResult> GetBookByIsbn(string isbn,IBookService bookService)
    {
        var book = await bookService.GetByIsbnAsync(isbn);
        return book is not null ? Results.Ok(book): Results.NotFound();
    }

    internal static async Task<IResult> DeleteBook(string isbn,IBookService bookService)
    {
        var deleted = await bookService.DeleteAsync(isbn);
        return deleted  ? Results.NoContent(): Results.NotFound();
    }

    //[Authorize(AuthenticationSchemes = ApiKeySchemeConstants.SchemeName)]
    internal static async Task<IResult> CreateBookAsync(
        Book book,
        IBookService bookService,
        IValidator<Book> validator,
        LinkGenerator linker,
        HttpContext context)
    {
        var validationResult = await validator.ValidateAsync(book);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(validationResult.Errors);
        }

        var created = await bookService.CreateAsync(book);
        if (!created)
        {
            return Results.BadRequest(new List<ValidationFailure>
            {
                new ("Isbn","A book with this ISBN-13 already exists")
            });
        }

        //var locationPath = linker.GetPathByName("GetBook", new{isbn= book.Isbn})!;
        var locationUri = linker.GetUriByName(context, "GetBook", new{isbn= book.Isbn})!;
        return Results.Created(locationUri,book);
        // return Results.CreatedAtRoute(
        //     "GetBook",
        //     new{isbn = book.Isbn},
        //     book);
        //return Results.Created($"/books/{book.Isbn}", book);
    }


    internal static async Task<IResult> UpdateBookAsync(
        string isbn,
        Book book,
        IBookService bookService,
        IValidator<Book> validator)
    {
        book.Isbn = isbn;
        var validationResult = await validator.ValidateAsync(book);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(validationResult.Errors);
        }

        var updated = await bookService.UpdateAsync(book);

        return updated ? Results.Ok(book) : Results.NotFound();
    }

    internal static async Task<IResult> SearchBooks(IBookService bookService, string? searchTerm)
    {
        if (searchTerm is not null && !string.IsNullOrWhiteSpace(searchTerm))
        {
            var matchedBooks = await bookService.SearchByTitleAsync(searchTerm);
            return Results.Ok(matchedBooks);
        }

        var books = await bookService.GetAllAsync();
        return Results.Ok(books);
    }
}