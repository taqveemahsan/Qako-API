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
            var fileInfo = new FileInfo(filePath);
            Console.WriteLine($"Uploading file: {filePath}, Size: {fileInfo.Length} bytes");

            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = Path.GetFileName(filePath),
                Parents = folderId != null ? new List<string> { folderId } : null
            };

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                var mimeType = GetMimeType(filePath);
                var request = _driveService.Files.Create(fileMetadata, stream, mimeType);
                request.Fields = "id, name, mimeType, size";
                request.SupportsAllDrives = true;
                
                // Enable resumable upload for larger files
                request.ChunkSize = 1024 * 1024; // 1MB chunks
                
                try
                {
                    var uploadResult = await request.UploadAsync();
                    
                    if (uploadResult.Status != Google.Apis.Upload.UploadStatus.Completed)
                    {
                        var errorMsg = uploadResult.Exception?.Message ?? "Unknown upload error";
                        throw new Exception($"Upload failed. Status: {uploadResult.Status}, Error: {errorMsg}");
                    }

                    Console.WriteLine($"Upload successful. File ID: {request.ResponseBody.Id}");
                    return request.ResponseBody;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Upload exception: {ex.Message}");
                    throw new Exception($"Google Drive upload error: {ex.Message}", ex);
                }
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
            request.SupportsAllDrives = true;
            
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
                request.SupportsAllDrives = true; // Ensure support for Shared Drives
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

        //private async Task<Google.Apis.Drive.v3.Data.File> GetFolderByNameAsync(string folderName, string parentFolderId)
        //{
        //    var query = $"mimeType = 'application/vnd.google-apps.folder' and name = '{folderName}' and trashed = false";
        //    if (!string.IsNullOrEmpty(parentFolderId))
        //        query += $" and '{parentFolderId}' in parents";

        //    var request = _driveService.Files.List();
        //    request.Q = query;
        //    request.Fields = "files(id, name)";
        //    request.SupportsAllDrives = true; // Add this line
        //    var result = await request.ExecuteAsync();

        //    return result.Files.FirstOrDefault();
        //}
        //private async Task<Google.Apis.Drive.v3.Data.File> GetFolderByNameAsync(string folderName, string parentFolderId)
        //{
        //    try
        //    {
        //        var query = $"mimeType = 'application/vnd.google-apps.folder' and name = '{folderName}' and trashed = false";
        //        if (!string.IsNullOrEmpty(parentFolderId))
        //        {
        //            query += $" and parents = '{parentFolderId}'";  // Changed from 'in parents' to 'parents ='
        //        }

        //        var request = _driveService.Files.List();
        //        request.Q = query;
        //        request.Fields = "files(id, name, parents)";
        //        request.SupportsAllDrives = true;
        //        var result = await request.ExecuteAsync();

        //        // If parentFolderId is specified, ensure we only return folders that are direct children
        //        if (!string.IsNullOrEmpty(parentFolderId))
        //        {
        //            return result.Files.FirstOrDefault(f => f.Parents != null && f.Parents.Contains(parentFolderId));
        //        }

        //        return result.Files.FirstOrDefault();
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error in GetFolderByNameAsync: {ex.Message}");
        //        return null;
        //    }
        //}

        private async Task<Google.Apis.Drive.v3.Data.File> GetFolderByNameAsync(string folderName, string parentFolderId)
        {
            try
            {
                var query = $"mimeType = 'application/vnd.google-apps.folder' and name = '{folderName}' and trashed = false";
                if (!string.IsNullOrEmpty(parentFolderId))
                {
                    // Correct syntax: use 'in parents' instead of 'parents ='
                    query += $" and '{parentFolderId}' in parents";
                }

                var request = _driveService.Files.List();
                request.Q = query;
                request.Fields = "files(id, name, parents)";
                request.SupportsAllDrives = true;
                request.IncludeItemsFromAllDrives = true; // Add this line

                var result = await request.ExecuteAsync();

                Console.WriteLine($"Query executed: {query}");
                Console.WriteLine($"Found {result.Files.Count} folders");

                return result.Files.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetFolderByNameAsync: {ex.Message}");
                return null;
            }
        }

        public async Task<Google.Apis.Drive.v3.Data.File> GetItemAsync(string itemId)
        {
            try
            {
                var request = _driveService.Files.Get(itemId);
                request.Fields = "id,name,mimeType,parents";
                request.SupportsAllDrives = true; // Add this line
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
                request.SupportsAllDrives = true; // Add this line
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

        public async Task DeleteItemAsync(string itemId)
        {
            try
            {
                var request = _driveService.Files.Delete(itemId);
                request.SupportsAllDrives = true; // Ensure support for Shared Drives
                await request.ExecuteAsync();
            }
            catch (Google.GoogleApiException ex)
            {
                throw new Exception($"Failed to delete item: {ex.Message}", ex);
            }
        }

        public async Task<bool> FileExistsAsync(string fileId)
        {
            try
            {
                var request = _driveService.Files.Get(fileId);
                request.SupportsAllDrives = true; // <-- This is important!
                var file = await request.ExecuteAsync();
                return file != null;
            }
            catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
        }
    }
}
