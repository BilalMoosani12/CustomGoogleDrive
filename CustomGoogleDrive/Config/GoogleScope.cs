using Google.Apis.Drive.v3;

namespace CustomGoogleDrive.Config
{
    public static class GoogleScope
    {
        public static readonly string[] Scopes = {
            DriveService.Scope.DriveReadonly,
            DriveService.Scope.Drive,
            DriveService.Scope.DriveFile,
            "https://www.googleapis.com/auth/plus.login"
        };
    }
}
