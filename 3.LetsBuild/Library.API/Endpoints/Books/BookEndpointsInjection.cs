using FluentValidation.Results;
using Library.API.Models;

namespace Library.API.Endpoints.Books;

internal static class BookEndpointsInjection
{
    private const string ContentType = "application/json";
    private const string Tag = "Books";
    private const string BaseRoute = "books";
    
    internal static void AddBookEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost(BaseRoute, BookMethods.CreateBookAsync)
            .WithName("CreateBook")
            .Accepts<Book>(ContentType)
            .Produces<Book>(201)
            .Produces<IEnumerable<ValidationFailure>>(400)
            .WithTags(Tag);

        app.MapPut( $"{BaseRoute}/{{isbn}}", BookMethods.UpdateBookAsync)
            .WithName("UpdateBook")    
            .Accepts<Book>(ContentType)
            .Produces<Book>(201)
            .Produces(404)
            .Produces<IEnumerable<ValidationFailure>>(400)
            .WithTags(Tag);

        app.MapGet(BaseRoute, BookMethods.GetAllBooks)
            .WithName("GetBooks")
            .Produces<IEnumerable<Book>>(200)
            .WithTags(Tag)
            .AllowAnonymous();

        app.MapGet($"{BaseRoute}/{{isbn}}", BookMethods.GetBookByIsbn)
            .WithName("GetBook")
            .Produces<Book>(200)
            .Produces(404)
            .WithTags(Tag);

        app.MapGet($"{BaseRoute}/search", BookMethods.SearchBooks)
            .AllowAnonymous()
            .WithName("SearchBook")
            .WithTags(Tag);

        app.MapDelete($"{BaseRoute}/{{isbn}}", BookMethods.DeleteBook)
            .WithName("DeleteBook")
            .Produces(204)
            .Produces(404)
            .WithTags(Tag);
    }
}