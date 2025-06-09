
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace FraudDetection.Services
{

    public class BlobStorageService : IBlobStorageService
    {
        private readonly BlobContainerClient _containerClient;
        private readonly ILogger<BlobStorageService> _logger;

        public BlobStorageService(IConfiguration configuration, ILogger<BlobStorageService> logger)
        {
            _logger = logger;

            var connectionString = configuration["AZURE_BLOB_CONNECTION_STRING"];
            var containerName = configuration["AZURE_BLOB_CONTAINER_NAME"] ?? "claimimages";

            if (string.IsNullOrEmpty(connectionString))
            {
                _logger.LogError("Azure Blob Storage connection string is not configured.");
                throw new ArgumentNullException("Azure Blob Storage connection string is not configured.");
            }

            _containerClient = new BlobContainerClient(connectionString, containerName);

            _containerClient.CreateIfNotExists(PublicAccessType.Blob);

            _logger.LogInformation($"BlobStorageService initialized with container: {containerName}");
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
        {
            try
            {
                var blobClient = _containerClient.GetBlobClient(fileName);

                var blobHttpHeader = new BlobHttpHeaders
                {
                    ContentType = contentType
                };

                await blobClient.UploadAsync(fileStream, new BlobUploadOptions
                {
                    HttpHeaders = blobHttpHeader,
                    // You can add metadata or other options here if needed
                });

                _logger.LogInformation("Uploaded file {FileName} to blob storage.", fileName);

                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file to blob storage");
                throw;
            }
        }
    }
}
