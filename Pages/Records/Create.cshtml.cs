public class CreateModel : PageModel
{
    private readonly CosmosClient _cosmosClient;
    private readonly BlobServiceClient _blobServiceClient;
    private readonly IConfiguration _configuration;

    public CreateModel(CosmosClient cosmosClient, BlobServiceClient blobServiceClient, IConfiguration configuration)
    {
        _cosmosClient = cosmosClient;
        _blobServiceClient = blobServiceClient;
        _configuration = configuration;
    }

    [BindProperty]
    public Record Record { get; set; }

    [BindProperty]
    public IFormFile Image { get; set; }

    [BindProperty]
    public IFormFile File { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!User.Identity.IsAuthenticated)
            return Challenge();

        var container = _cosmosClient.GetContainer(
            _configuration["CosmosDb:DatabaseName"],
            _configuration["CosmosDb:ContainerName"]);

        var blobContainer = _blobServiceClient.GetBlobContainerClient(_configuration["Storage:ContainerName"]);

        // Upload image if provided
        if (Image != null)
        {
            var imageBlob = blobContainer.GetBlobClient($"images/{Guid.NewGuid()}{Path.GetExtension(Image.FileName)}");
            using var stream = Image.OpenReadStream();
            await imageBlob.UploadAsync(stream);
            Record.ImageUrl = imageBlob.Uri.ToString();
        }

        // Upload file if provided
        if (File != null)
        {
            var fileBlob = blobContainer.GetBlobClient($"files/{Guid.NewGuid()}{Path.GetExtension(File.FileName)}");
            using var stream = File.OpenReadStream();
            await fileBlob.UploadAsync(stream);
            Record.FileUrl = fileBlob.Uri.ToString();
        }

        Record.Id = Guid.NewGuid().ToString();
        Record.CreatedDate = DateTime.UtcNow;
        Record.UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        await container.CreateItemAsync(Record);

        return RedirectToPage("./Index");
    }
}