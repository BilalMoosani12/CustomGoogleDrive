using System.Threading.Tasks;
using Google.Apis.Drive.v3;

namespace CustomGoogleDrive.Services
{
    public interface IGoogleDriveService
    {
        Task<string> CreateFolder(DriveService driveService);
        Task<string> FileUpload(DriveService driveService, string filePath);
        Task FileSharing(DriveService driveService, string fileId, string email);
    }
}
