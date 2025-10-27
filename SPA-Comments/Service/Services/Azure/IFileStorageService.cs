using Dto.Azure;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services.Azure;

public interface IFileStorageService
{
    Task<string> UploadFileAsync(IFormFile file, string userName);
    Task<FileDownloadResultDto?> GetFileAsync(string blobName); 
    Task<string?> GetPublicUrlAsync(string blobName); 
}
