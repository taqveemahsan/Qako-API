using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace AuditPilot.API.Helpers
{
    public class GoogleDriveHelper
    {
        private readonly DriveService _driveService;

        public GoogleDriveHelper(DriveService driveService)
        {
            _driveService = driveService;
        }

        public async Task<Google.Apis.Drive.v3.Data.File> CreateFileAsync(string filePath, string folderId = null)
        {
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = Path.GetFileName(filePath),
                Parents = folderId != null ? new List<string> { folderId } : null
            };

            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                var request = _driveService.Files.Create(fileMetadata, stream, "application/octet-stream");
                request.Fields = "id, name, mimeType";
                var file = await request.UploadAsync();

                if (file.Status != Google.Apis.Upload.UploadStatus.Completed)
                {
                    throw new Exception("File upload failed.");
                }

                return request.ResponseBody;
            }
        }

        public async Task<Google.Apis.Drive.v3.Data.File> CreateFolderAsync(string folderName, string parentFolderId = null)
        {
            var folderMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = folderName,
                MimeType = "application/vnd.google-apps.folder",
                Parents = parentFolderId != null ? new List<string> { parentFolderId } : null
            };

            var request = _driveService.Files.Create(folderMetadata);
            request.Fields = "id, name, mimeType";
            var folder = await request.ExecuteAsync();
            return folder;
        }

        public async Task<IList<Google.Apis.Drive.v3.Data.File>> GetAllItemsInFolderAsync(string folderId)
        {

            var request = _driveService.Files.List();
            request.Q = $"parents in '{folderId}'";
            request.Fields = "files(contentHints/thumbnail,fileExtension,iconLink,id,name,size,thumbnailLink,webContentLink,webViewLink,mimeType,parents)";
            request.PageToken = null;
            request.SupportsAllDrives = true;
            request.IncludeItemsFromAllDrives = true;

            var result = await request.ExecuteAsync();

            return result.Files;
        }

        public async Task DownloadFileAsync(string fileId, string saveToPath)
        {
            var request = _driveService.Files.Get(fileId);
            using (var stream = new FileStream(saveToPath, FileMode.Create, FileAccess.Write))
            {
                await request.DownloadAsync(stream);
            }
        }

        public async Task<string> ReplaceFileAsync(string fileId, string newFilePath)
        {
            var fileMetadata = new Google.Apis.Drive.v3.Data.File
            {
                Name = Path.GetFileName(newFilePath)
            };

            using (var stream = new FileStream(newFilePath, FileMode.Open))
            {
                var request = _driveService.Files.Update(fileMetadata, fileId, stream, GetMimeType(newFilePath));
                request.Fields = "id";
                var result = await request.UploadAsync();

                if (result.Status != Google.Apis.Upload.UploadStatus.Completed)
                {
                    throw new Exception($"Error updating file: {result.Exception.Message}");
                }

                return request.ResponseBody.Id;
            }
        }

        private string GetMimeType(string fileName)
        {
            var mimeType = "application/octet-stream";
            var extension = Path.GetExtension(fileName).ToLower();
            Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(extension);
            if (regKey != null && regKey.GetValue("Content Type") != null)
            {
                mimeType = regKey.GetValue("Content Type").ToString();
            }
            return mimeType;
        }
        public async Task<Google.Apis.Drive.v3.Data.File> GetOrCreateFolderAsync(string folderName, string parentFolderId = null)
        {
            // Check if the folder already exists
            var existingFolder = await GetFolderByNameAsync(folderName, parentFolderId);
            if (existingFolder != null)
                return existingFolder;

            // If not, create the folder
            return await CreateFolderAsync(folderName, parentFolderId);
        }

        private async Task<Google.Apis.Drive.v3.Data.File> GetFolderByNameAsync(string folderName, string parentFolderId)
        {
            var query = $"mimeType = 'application/vnd.google-apps.folder' and name = '{folderName}' and trashed = false";
            if (!string.IsNullOrEmpty(parentFolderId))
                query += $" and '{parentFolderId}' in parents";

            var request = _driveService.Files.List();
            request.Q = query;
            request.Fields = "files(id, name)";
            var result = await request.ExecuteAsync();

            return result.Files.FirstOrDefault();
        }

        public async Task<Google.Apis.Drive.v3.Data.File> GetItemAsync(string itemId)
        {
            try
            {
                var request = _driveService.Files.Get(itemId);
                request.Fields = "id,name,mimeType,parents";
                var item = await request.ExecuteAsync();
                return item;
            }
            catch (Google.GoogleApiException ex)
            {
                throw new Exception($"Failed to get item: {ex.Message}", ex);
            }
        }

        public async Task<Google.Apis.Drive.v3.Data.File> RenameItemAsync(string itemId, string newName)
        {
            try
            {
                var fileUpdate = new Google.Apis.Drive.v3.Data.File
                {
                    Name = newName
                };

                var request = _driveService.Files.Update(fileUpdate, itemId);
                request.Fields = "id,name,mimeType";
                var updatedFile = await request.ExecuteAsync();
                return updatedFile;
            }
            catch (Google.GoogleApiException ex)
            {
                throw; // Propagate API errors to be handled by the controller
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to rename item: {ex.Message}", ex);
            }
        }

    }
}
