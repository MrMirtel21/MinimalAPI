using Microsoft.AspNetCore.Authentication;

namespace Library.API.Auth;

public class ApiKeyAuthSchemeOptions : AuthenticationSchemeOptions
{
    public string ApiKey { get; set; } = "VerySecret";
}