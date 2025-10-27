using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Dto.Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 

namespace Service.Services.Azure;

public class AzureFileStorageService : IFileStorageService
{
    private readonly BlobContainerClient _container;

    public AzureFileStorageService(IConfiguration config)
    {
        var connectionString = config["AzureStorage:ConnectionString"];
        var containerName = config["AzureStorage:ContainerName"];

        _container = new BlobContainerClient(connectionString, containerName);
        _container.CreateIfNotExists(PublicAccessType.Blob);
    }


    public async Task<FileDownloadResultDto?> GetFileAsync(string blobName)
    {
        if (string.IsNullOrWhiteSpace(blobName))
            return null;

        var blobClient = _container.GetBlobClient(blobName);
        var exists = await blobClient.ExistsAsync();
        if (!exists.Value)
            return null;

        var properties = await blobClient.GetPropertiesAsync();
        var contentType = properties.Value.ContentType ?? "application/octet-stream";
        var fileName = Path.GetFileName(blobName);

        // Генерация SAS ссылки на blob
        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _container.Name,
            BlobName = blobName,
            Resource = "b", // blob
            ExpiresOn = DateTimeOffset.UtcNow.AddHours(1)
        };

        sasBuilder.SetPermissions(BlobSasPermissions.Read);
         
        if (!blobClient.CanGenerateSasUri)
            throw new InvalidOperationException("BlobClient не может сгенерировать SAS URI. Проверь ConnectionString и учетные данные.");

        var sasUri = blobClient.GenerateSasUri(sasBuilder);

        return new FileDownloadResultDto
        {
            FileName = fileName,
            ContentType = contentType,
            PublicUrl = GetFrontName(sasUri.ToString())
        };
    }

    private string GetFrontName(string uri)=> uri.Replace("azurite-proxy", "localhost", StringComparison.OrdinalIgnoreCase);

    public Task<string?> GetPublicUrlAsync(string blobName)
    {
        if (string.IsNullOrWhiteSpace(blobName))
            return Task.FromResult<string?>(null);

        var blobClient = _container.GetBlobClient(blobName);
        return Task.FromResult<string?>(GetFrontName(blobClient.Uri.ToString()));
    }

    public async Task<string> UploadFileAsync(IFormFile file, string userName)
    {
        var extension = Path.GetExtension(file.FileName);
        var blobName = $"{userName}/{Guid.NewGuid()}{extension}";
        var blob = _container.GetBlobClient(blobName);

        await using var stream = file.OpenReadStream();
        await blob.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });

        return blob.Uri.ToString();
    }
}