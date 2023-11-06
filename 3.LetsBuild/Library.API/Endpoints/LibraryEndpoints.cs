using FluentValidation;
using FluentValidation.Results;
using Library.API.Auth;
using Library.API.Endpoints.Books;
using Library.API.Endpoints.Internal;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;

namespace Library.API.Endpoints;

public  class LibraryEndpoints : IEndpoints
{
    public static void AddServices( IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IBookService, BookService>();
    }

    public static void DefineEndpoints( IEndpointRouteBuilder app)
    {
        
        app.AddBookEndpoints();

        app.MapGet("status", [EnableCors(PolicyName = "AnyOrigin")]() =>
        {
            return Results.Extensions.Html(
                @"<!doctype html> 
                <html>
                    <head><title>Status Page</title></head>
                    <body>
                        <h1>Status</h1>
                        <p>The server is working fine. Bye bye!</p>
                    </body>
                </html>"
            );
        }).ExcludeFromDescription();//RequireCors(policyName:"AnyOrigin");

    }

 
}