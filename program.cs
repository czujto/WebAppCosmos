// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddRazorPages();
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

// Add Cosmos DB client
builder.Services.AddSingleton<CosmosClient>(sp =>
{
    return new CosmosClient(builder.Configuration["CosmosDb:ConnectionString"]);
});

// Add Blob Storage client
builder.Services.AddSingleton<BlobServiceClient>(sp =>
{
    return new BlobServiceClient(builder.Configuration["Storage:ConnectionString"]);
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

app.Run();