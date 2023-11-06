using FluentValidation;
using Library.API.Auth;
using Library.API.Data;
using Library.API.Endpoints.Internal;



var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    // WebRootPath = "./wwwroot",
    // EnvironmentName =Environment.GetEnvironmentVariable("env"),
    // ApplicationName = "Library.Api"
});

// builder.Services.Configure<JsonOptions>(options =>
// {
//     options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
//     options.JsonSerializerOptions.IncludeFields = true;
// });

builder.Services.AddCors(options =>
{
    options.AddPolicy("",x=>x.AllowAnyOrigin());
    
});

builder.Configuration.AddJsonFile("appsettings.Local.json",true,true);

builder.Services.AddAuthentication(ApiKeySchemeConstants.SchemeName)
    .AddScheme<ApiKeyAuthSchemeOptions, ApiKeyAuthHandler>(ApiKeySchemeConstants.SchemeName, _ => { });
builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IDbConnectionFactory>(_=>
    new SqliteConnectionFactory(
        builder.Configuration.GetValue<string>("Database:ConnectionString")
        ));
builder.Services.AddSingleton<DatabaseInitializer>();

builder.Services.AddEndpoints<Program>(builder.Configuration);

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();

app.UseCors();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

app.UserEndpoints<Program>();

var databaseInitializer = app.Services.GetRequiredService<DatabaseInitializer>();
await databaseInitializer.InitializeAsync();

app.Run();