using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using CustomGoogleDrive.Extension;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Requests;
using Google.Apis.Services;
using Microsoft.Extensions.Logging;

namespace CustomGoogleDrive.Services
{
    public class GoogleDriveService : IGoogleDriveService
    {
        private readonly ILogger<GoogleDriveService> _logger;

        public GoogleDriveService(ILogger<GoogleDriveService> logger)
        {
            _logger = logger;
        }

        public async Task<string> CreateFolder(DriveService driveService)
        {
            try
            {
                var fileMetadata = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = "Invoices",

                    MimeType = "application/vnd.google-apps.folder"
                };
                var request = driveService.Files.Create(fileMetadata);
                request.Fields = "id";
                var file = await request.ExecuteAsync();
                _logger.LogInformation("Folder ID: " + file.Id);

                return file.Id;
            }
            catch (Exception e)
            {
                _logger.LogError(default(EventId), e, e.Message);
                throw;
            }
        }

        public async Task<string> FileUpload(DriveService driveService, string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            var extension = Path.GetExtension(fileName);
            var mimeType = MimeTypeMap.GetMimeType(extension);
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = fileName
            };
            FilesResource.CreateMediaUpload request;
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                request = driveService.Files.Create(fileMetadata, stream, mimeType);
                request.Fields = "id";
                await request.UploadAsync();
            }
            var file = request.ResponseBody;
            _logger.LogInformation("File ID: " + file.Id);
            return file.Id;
        }

        public async Task FileSharing(DriveService driveService, string fileId, string email)
        {
            try
            {
                var batch = new BatchRequest(driveService);
                BatchRequest.OnResponse<Permission> callback = delegate (
                    Permission permission,
                    RequestError error,
                    int index,
                    HttpResponseMessage message)
                {
                    if (error != null)
                    {
                        // Handle error
                        foreach (var singleError in error.Errors)
                        {
                            _logger.LogError($"Message: {singleError.Message}");
                            _logger.LogError($"Domain: {singleError.Domain}");
                            _logger.LogError($"Location: {singleError.Location}");
                            _logger.LogError($"LocationType: {singleError.LocationType}");
                            _logger.LogError($"Reason: {singleError.Reason}");
                        }
                    }
                    else
                    {
                        _logger.LogInformation("Permission ID: " + permission.Id);
                    }
                };
                var userPermission = new Permission
                {
                    Type = "user",
                    Role = "writer",
                    EmailAddress = email,
                };
                var request = driveService.Permissions.Create(userPermission, fileId);
                request.Fields = "id";

                batch.Queue(request, callback);

                await batch.ExecuteAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(default(EventId), e, e.Message);
                throw;
            }
        }
    }
}
