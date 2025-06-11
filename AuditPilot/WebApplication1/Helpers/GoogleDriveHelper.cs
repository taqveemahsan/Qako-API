using Google;
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

        public static string[] GetRequiredScopes()
        {
            return new[]
            {
                DriveService.Scope.Drive,           // Full access to all files
                DriveService.Scope.DriveFile,       // Access to files created by the app
                DriveService.Scope.DriveMetadata    // Access to metadata of files
            };
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
                // Add shared drive support
                request.SupportsAllDrives = true;
                
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
            try
            {
                var folderMetadata = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = folderName,
                    MimeType = "application/vnd.google-apps.folder"
                };

                if (!string.IsNullOrEmpty(parentFolderId))
                {
                    folderMetadata.Parents = new List<string> { parentFolderId };
                }

                var createRequest = _driveService.Files.Create(folderMetadata);
                createRequest.Fields = "id, name, parents";
                // Add support for shared drives
                createRequest.SupportsAllDrives = true;

                var folder = await createRequest.ExecuteAsync();
                Console.WriteLine($"Folder created: {folder.Name} with ID: {folder.Id}");

                return folder;
            }
            catch (GoogleApiException ex)
            {
                Console.WriteLine($"Error creating folder: {ex.Message}");

                if (ex.HttpStatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    throw new Exception("Insufficient permissions to create folder in the specified location");
                }

                throw;
            }
        }

        public string ExtractFolderIdFromUrl(string driveUrl)
        {
            if (string.IsNullOrEmpty(driveUrl))
                return null;

            // Extract folder ID from different URL formats
            var patterns = new[]
            {
                @"folders/([a-zA-Z0-9-_]+)",
                @"id=([a-zA-Z0-9-_]+)",
                @"/d/([a-zA-Z0-9-_]+)"
            };

            foreach (var pattern in patterns)
            {
                var match = System.Text.RegularExpressions.Regex.Match(driveUrl, pattern);
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }
            }

            return null;
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
            request.SupportsAllDrives = true; // Add this
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
                request.SupportsAllDrives = true; // Add this
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
            try
            {
                Console.WriteLine($"Attempting to get or create folder: {folderName} in parent: {parentFolderId}");

                // First check if parent folder exists and is accessible
                if (!string.IsNullOrEmpty(parentFolderId))
                {
                    var parentExists = await CheckFolderExistsAsync(parentFolderId);
                    if (!parentExists)
                    {
                        Console.WriteLine($"Parent folder with ID '{parentFolderId}' not found or not accessible");
                        // Try to find if it's a shared drive root
                        var isSharedDriveRoot = await CheckIfSharedDriveRootAsync(parentFolderId);
                        if (!isSharedDriveRoot)
                        {
                            Console.WriteLine("Using root folder instead");
                            parentFolderId = null;
                        }
                    }
                }

                // Check if the folder already exists
                var existingFolder = await GetFolderByNameAsync(folderName, parentFolderId);
                if (existingFolder != null)
                {
                    Console.WriteLine($"Found existing folder: {existingFolder.Name} with ID: {existingFolder.Id}");
                    return existingFolder;
                }

                // If not, create the folder
                Console.WriteLine($"Creating new folder: {folderName}");
                return await CreateFolderAsync(folderName, parentFolderId);
            }
            catch (GoogleApiException ex)
            {
                Console.WriteLine($"Google API Error in GetOrCreateFolderAsync: {ex.Message}");
                Console.WriteLine($"Status Code: {ex.HttpStatusCode}");
                Console.WriteLine($"Error Details: {ex.Error?.Message}");

                if (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    Console.WriteLine($"Folder not found or not accessible. Attempting to create in root instead.");
                    try
                    {
                        return await CreateFolderAsync(folderName, null);
                    }
                    catch (Exception createEx)
                    {
                        Console.WriteLine($"Failed to create folder in root: {createEx.Message}");
                        throw new Exception($"Unable to create folder '{folderName}'. Original error: {ex.Message}, Root creation error: {createEx.Message}");
                    }
                }

                throw new Exception($"Failed to get or create folder '{folderName}': {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error in GetOrCreateFolderAsync: {ex.Message}");
                throw new Exception($"Unexpected error while getting or creating folder '{folderName}': {ex.Message}", ex);
            }
        }

        private async Task<bool> CheckFolderExistsAsync(string folderId)
        {
            try
            {
                Console.WriteLine($"Checking if folder exists: {folderId}");
                var request = _driveService.Files.Get(folderId);
                request.Fields = "id, name, mimeType, parents";
                request.SupportsAllDrives = true;
                
                var folder = await request.ExecuteAsync();
                Console.WriteLine($"Folder found: {folder.Name}, MimeType: {folder.MimeType}");

                return folder != null && folder.MimeType == "application/vnd.google-apps.folder";
            }
            catch (GoogleApiException ex)
            {
                Console.WriteLine($"Error checking folder existence: {ex.Message}, Status: {ex.HttpStatusCode}");
                if (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound || 
                    ex.HttpStatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    return false;
                }
                throw;
            }
        }

        private async Task<bool> CheckIfSharedDriveRootAsync(string folderId)
        {
            try
            {
                // Check if this ID corresponds to a shared drive
                var sharedDrives = await ListSharedDrivesAsync();
                return sharedDrives.Any(drive => drive.Id == folderId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking shared drives: {ex.Message}");
                return false;
            }
        }

        private async Task<Google.Apis.Drive.v3.Data.File> GetFolderByNameAsync(string folderName, string parentFolderId = null)
        {
            try
            {
                Console.WriteLine($"Searching for folder: {folderName} in parent: {parentFolderId}");
                
                var listRequest = _driveService.Files.List();

                // Build query based on parent folder
                string query = $"name='{folderName.Replace("'", "\\'")}' and mimeType='application/vnd.google-apps.folder' and trashed=false";

                if (!string.IsNullOrEmpty(parentFolderId))
                {
                    query += $" and '{parentFolderId}' in parents";
                }

                listRequest.Q = query;
                listRequest.Fields = "files(id, name, parents, mimeType)";
                listRequest.SupportsAllDrives = true;
                listRequest.IncludeItemsFromAllDrives = true;

                Console.WriteLine($"Search query: {query}");
                
                var result = await listRequest.ExecuteAsync();
                
                if (result.Files != null && result.Files.Count > 0)
                {
                    Console.WriteLine($"Found {result.Files.Count} matching folders");
                    foreach (var file in result.Files)
                    {
                        Console.WriteLine($"- {file.Name} (ID: {file.Id})");
                    }
                    return result.Files.FirstOrDefault();
                }
                else
                {
                    Console.WriteLine("No matching folders found");
                    return null;
                }
            }
            catch (GoogleApiException ex)
            {
                Console.WriteLine($"Error searching for folder: {ex.Message}, Status: {ex.HttpStatusCode}");
                return null;
            }
        }

        public async Task<Google.Apis.Drive.v3.Data.File> GetItemAsync(string itemId)
        {
            try
            {
                var request = _driveService.Files.Get(itemId);
                request.Fields = "id,name,mimeType,parents";
                request.SupportsAllDrives = true; // Add this
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
                request.SupportsAllDrives = true; // Add this
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

        public async Task<bool> VerifySharedDriveAccessAsync(string driveId)
        {
            try
            {
                Console.WriteLine($"Verifying access to shared drive: {driveId}");
                var request = _driveService.Drives.Get(driveId);
                var drive = await request.ExecuteAsync();
                Console.WriteLine($"Shared drive found: {drive.Name}");
                return drive != null;
            }
            catch (GoogleApiException ex)
            {
                Console.WriteLine($"Error accessing shared drive: {ex.Message}, Status: {ex.HttpStatusCode}");
                return false;
            }
        }

        public async Task<IList<Google.Apis.Drive.v3.Data.Drive>> ListSharedDrivesAsync()
        {
            try
            {
                var request = _driveService.Drives.List();
                request.PageSize = 100;
                var result = await request.ExecuteAsync();
                
                Console.WriteLine($"Found {result.Drives?.Count ?? 0} shared drives");
                if (result.Drives != null)
                {
                    foreach (var drive in result.Drives)
                    {
                        Console.WriteLine($"- {drive.Name} (ID: {drive.Id})");
                    }
                }
                
                return result.Drives ?? new List<Google.Apis.Drive.v3.Data.Drive>();
            }
            catch (GoogleApiException ex)
            {
                Console.WriteLine($"Error listing shared drives: {ex.Message}");
                return new List<Google.Apis.Drive.v3.Data.Drive>();
            }
        }

        // New method to debug folder access
        public async Task<string> DebugFolderAccessAsync(string folderId)
        {
            var debugInfo = new List<string>();
            
            try
            {
                debugInfo.Add($"=== Debug Info for Folder ID: {folderId} ===");
                
                // Try to get folder details
                try
                {
                    var request = _driveService.Files.Get(folderId);
                    request.Fields = "id,name,mimeType,parents,permissions,owners,shared";
                    request.SupportsAllDrives = true;
                    
                    var folder = await request.ExecuteAsync();
                    debugInfo.Add($"Folder Name: {folder.Name}");
                    debugInfo.Add($"MimeType: {folder.MimeType}");
                    debugInfo.Add($"Parents: {string.Join(", ", folder.Parents ?? new List<string>())}");
                    debugInfo.Add($"Shared: {folder.Shared}");
                }
                catch (GoogleApiException ex)
                {
                    debugInfo.Add($"Failed to get folder details: {ex.Message} (Status: {ex.HttpStatusCode})");
                }
                
                // Check if it's a shared drive
                var sharedDrives = await ListSharedDrivesAsync();
                var isSharedDrive = sharedDrives.Any(d => d.Id == folderId);
                debugInfo.Add($"Is Shared Drive: {isSharedDrive}");
                
                // Try to list contents
                try
                {
                    var listRequest = _driveService.Files.List();
                    listRequest.Q = $"parents in '{folderId}'";
                    listRequest.Fields = "files(id,name,mimeType)";
                    listRequest.SupportsAllDrives = true;
                    listRequest.IncludeItemsFromAllDrives = true;
                    listRequest.PageSize = 5;
                    
                    var fileListResult = await listRequest.ExecuteAsync();
                    debugInfo.Add($"Can list contents: Yes ({fileListResult.Files?.Count ?? 0} items found)");
                }
                catch (GoogleApiException ex)
                {
                    debugInfo.Add($"Can list contents: No - {ex.Message} (Status: {ex.HttpStatusCode})");
                }
                
            }
            catch (Exception ex)
            {
                debugInfo.Add($"Debug failed: {ex.Message}");
            }
            
            var result = string.Join("\n", debugInfo);
            Console.WriteLine(result);
            return result;
        }
    }
}












// using Google;
// using Google.Apis.Auth.OAuth2;
// using Google.Apis.Drive.v3;
// using Google.Apis.Services;
// using Google.Apis.Util.Store;

// namespace AuditPilot.API.Helpers
// {
//     public class GoogleDriveHelper
//     {
//         private readonly DriveService _driveService;

//         public GoogleDriveHelper(DriveService driveService)
//         {
//             _driveService = driveService;
//         }

//         public static string[] GetRequiredScopes()
//         {
//             return new[]
//             {
//                 DriveService.Scope.Drive,           // Full access to all files
//                 DriveService.Scope.DriveFile,       // Access to files created by the app
//                 DriveService.Scope.DriveMetadata    // Access to metadata of files
//             };
//         }

//         public async Task<Google.Apis.Drive.v3.Data.File> CreateFileAsync(string filePath, string folderId = null)
//         {
//             var fileMetadata = new Google.Apis.Drive.v3.Data.File()
//             {
//                 Name = Path.GetFileName(filePath),
//                 Parents = folderId != null ? new List<string> { folderId } : null
//             };

//             using (var stream = new FileStream(filePath, FileMode.Open))
//             {
//                 var request = _driveService.Files.Create(fileMetadata, stream, "application/octet-stream");
//                 request.Fields = "id, name, mimeType";
//                 var file = await request.UploadAsync();

//                 if (file.Status != Google.Apis.Upload.UploadStatus.Completed)
//                 {
//                     throw new Exception("File upload failed.");
//                 }

//                 return request.ResponseBody;
//             }
//         }
//         public async Task<Google.Apis.Drive.v3.Data.File> CreateFolderAsync(string folderName, string parentFolderId = null)
//         {
//             try
//             {
//                 var folderMetadata = new Google.Apis.Drive.v3.Data.File()
//                 {
//                     Name = folderName,
//                     MimeType = "application/vnd.google-apps.folder"
//                 };

//                 if (!string.IsNullOrEmpty(parentFolderId))
//                 {
//                     folderMetadata.Parents = new List<string> { parentFolderId };
//                 }

//                 var createRequest = _driveService.Files.Create(folderMetadata);
//                 createRequest.Fields = "id, name, parents";
//                 // Add support for shared drives
//                 createRequest.SupportsAllDrives = true;

//                 var folder = await createRequest.ExecuteAsync();
//                 Console.WriteLine($"Folder created: {folder.Name} with ID: {folder.Id}");

//                 return folder;
//             }
//             catch (GoogleApiException ex)
//             {
//                 Console.WriteLine($"Error creating folder: {ex.Message}");

//                 if (ex.HttpStatusCode == System.Net.HttpStatusCode.Forbidden)
//                 {
//                     throw new Exception("Insufficient permissions to create folder in the specified location");
//                 }

//                 throw;
//             }
//         }

//         public string ExtractFolderIdFromUrl(string driveUrl)
//         {
//             if (string.IsNullOrEmpty(driveUrl))
//                 return null;

//             // Extract folder ID from different URL formats
//             var patterns = new[]
//             {
//         @"folders/([a-zA-Z0-9-_]+)",
//         @"id=([a-zA-Z0-9-_]+)",
//         @"/d/([a-zA-Z0-9-_]+)"
//     };

//             foreach (var pattern in patterns)
//             {
//                 var match = System.Text.RegularExpressions.Regex.Match(driveUrl, pattern);
//                 if (match.Success)
//                 {
//                     return match.Groups[1].Value;
//                 }
//             }

//             return null;
//         }

//         //public async Task<Google.Apis.Drive.v3.Data.File> CreateFolderAsync(string folderName, string parentFolderId = null)
//         //{
//         //    var folderMetadata = new Google.Apis.Drive.v3.Data.File()
//         //    {
//         //        Name = folderName,
//         //        MimeType = "application/vnd.google-apps.folder",
//         //        Parents = parentFolderId != null ? new List<string> { parentFolderId } : null
//         //    };

//         //    var request = _driveService.Files.Create(folderMetadata);
//         //    request.Fields = "id, name, mimeType";
//         //    var folder = await request.ExecuteAsync();
//         //    return folder;
//         //}

//         public async Task<IList<Google.Apis.Drive.v3.Data.File>> GetAllItemsInFolderAsync(string folderId)
//         {
//             var request = _driveService.Files.List();
//             request.Q = $"parents in '{folderId}'";
//             request.Fields = "files(contentHints/thumbnail,fileExtension,iconLink,id,name,size,thumbnailLink,webContentLink,webViewLink,mimeType,parents)";
//             request.PageToken = null;
//             request.SupportsAllDrives = true;
//             request.IncludeItemsFromAllDrives = true;

//             var result = await request.ExecuteAsync();

//             return result.Files;
//         }

//         public async Task DownloadFileAsync(string fileId, string saveToPath)
//         {
//             var request = _driveService.Files.Get(fileId);
//             using (var stream = new FileStream(saveToPath, FileMode.Create, FileAccess.Write))
//             {
//                 await request.DownloadAsync(stream);
//             }
//         }

//         public async Task<string> ReplaceFileAsync(string fileId, string newFilePath)
//         {
//             var fileMetadata = new Google.Apis.Drive.v3.Data.File
//             {
//                 Name = Path.GetFileName(newFilePath)
//             };

//             using (var stream = new FileStream(newFilePath, FileMode.Open))
//             {
//                 var request = _driveService.Files.Update(fileMetadata, fileId, stream, GetMimeType(newFilePath));
//                 request.Fields = "id";
//                 var result = await request.UploadAsync();

//                 if (result.Status != Google.Apis.Upload.UploadStatus.Completed)
//                 {
//                     throw new Exception($"Error updating file: {result.Exception.Message}");
//                 }

//                 return request.ResponseBody.Id;
//             }
//         }

//         private string GetMimeType(string fileName)
//         {
//             var mimeType = "application/octet-stream";
//             var extension = Path.GetExtension(fileName).ToLower();
//             Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(extension);
//             if (regKey != null && regKey.GetValue("Content Type") != null)
//             {
//                 mimeType = regKey.GetValue("Content Type").ToString();
//             }
//             return mimeType;
//         }
//         //public async Task<Google.Apis.Drive.v3.Data.File> GetOrCreateFolderAsync(string folderName, string parentFolderId = null)
//         //{
//         //    // Check if the folder already exists
//         //    var existingFolder = await GetFolderByNameAsync(folderName, parentFolderId);
//         //    if (existingFolder != null)
//         //        return existingFolder;

//         //    // If not, create the folder
//         //    return await CreateFolderAsync(folderName, parentFolderId);
//         //}

//         public async Task<Google.Apis.Drive.v3.Data.File> GetOrCreateFolderAsync(string folderName, string parentFolderId = null)
//         {
//             try
//             {
//                 // First check if parent folder exists and is accessible
//                 if (!string.IsNullOrEmpty(parentFolderId))
//                 {
//                     var parentExists = await CheckFolderExistsAsync(parentFolderId);
//                     if (!parentExists)
//                     {
//                         Console.WriteLine($"Parent folder with ID '{parentFolderId}' not found or not accessible");
//                         // Instead of throwing exception, try to use root folder
//                         parentFolderId = null;
//                     }
//                 }

//                 // Check if the folder already exists
//                 var existingFolder = await GetFolderByNameAsync(folderName, parentFolderId);
//                 if (existingFolder != null)
//                     return existingFolder;

//                 // If not, create the folder
//                 return await CreateFolderAsync(folderName, parentFolderId);
//             }
//             catch (GoogleApiException ex)
//             {
//                 // Log the specific error
//                 Console.WriteLine($"Google API Error: {ex.Message}");

//                 if (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
//                 {
//                     Console.WriteLine($"Folder not found or not accessible. Attempting to create in root instead.");
//                     // Try to create in root folder instead
//                     return await CreateFolderAsync(folderName, null);
//                 }

//                 throw;
//             }
//         }

//         private async Task<bool> CheckFolderExistsAsync(string folderId)
//         {
//             try
//             {
//                 var request = _driveService.Files.Get(folderId);
//                 request.Fields = "id, name, mimeType";
//                 // Add support for shared drives
//                 request.SupportsAllDrives = true;
//                 var folder = await request.ExecuteAsync();

//                 return folder != null && folder.MimeType == "application/vnd.google-apps.folder";
//             }
//             catch (GoogleApiException ex)
//             {
//                 if (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
//                 {
//                     return false;
//                 }
//                 throw;
//             }
//         }

//         private async Task<Google.Apis.Drive.v3.Data.File> GetFolderByNameAsync(string folderName, string parentFolderId = null)
//         {
//             try
//             {
//                 var listRequest = _driveService.Files.List();

//                 // Build query based on parent folder
//                 string query = $"name='{folderName}' and mimeType='application/vnd.google-apps.folder' and trashed=false";

//                 if (!string.IsNullOrEmpty(parentFolderId))
//                 {
//                     query += $" and '{parentFolderId}' in parents";
//                 }

//                 listRequest.Q = query;
//                 listRequest.Fields = "files(id, name, parents)";
//                 // Add support for shared drives
//                 listRequest.SupportsAllDrives = true;
//                 listRequest.IncludeItemsFromAllDrives = true;

//                 var result = await listRequest.ExecuteAsync();
//                 return result.Files?.FirstOrDefault();
//             }
//             catch (GoogleApiException ex)
//             {
//                 Console.WriteLine($"Error searching for folder: {ex.Message}");
//                 return null;
//             }
//         }

//         //private async Task<Google.Apis.Drive.v3.Data.File> GetFolderByNameAsync(string folderName, string parentFolderId)
//         //{
//         //    var query = $"mimeType = 'application/vnd.google-apps.folder' and name = '{folderName}' and trashed = false";
//         //    if (!string.IsNullOrEmpty(parentFolderId))
//         //        query += $" and '{parentFolderId}' in parents";

//         //    var request = _driveService.Files.List();
//         //    request.Q = query;
//         //    request.Fields = "files(id, name)";
//         //    var result = await request.ExecuteAsync();

//         //    return result.Files.FirstOrDefault();
//         //}

//         public async Task<Google.Apis.Drive.v3.Data.File> GetItemAsync(string itemId)
//         {
//             try
//             {
//                 var request = _driveService.Files.Get(itemId);
//                 request.Fields = "id,name,mimeType,parents";
//                 var item = await request.ExecuteAsync();
//                 return item;
//             }
//             catch (Google.GoogleApiException ex)
//             {
//                 throw new Exception($"Failed to get item: {ex.Message}", ex);
//             }
//         }

//         public async Task<Google.Apis.Drive.v3.Data.File> RenameItemAsync(string itemId, string newName)
//         {
//             try
//             {
//                 var fileUpdate = new Google.Apis.Drive.v3.Data.File
//                 {
//                     Name = newName
//                 };

//                 var request = _driveService.Files.Update(fileUpdate, itemId);
//                 request.Fields = "id,name,mimeType";
//                 var updatedFile = await request.ExecuteAsync();
//                 return updatedFile;
//             }
//             catch (Google.GoogleApiException ex)
//             {
//                 throw; // Propagate API errors to be handled by the controller
//             }
//             catch (Exception ex)
//             {
//                 throw new Exception($"Failed to rename item: {ex.Message}", ex);
//             }
//         }

//         public async Task<bool> VerifySharedDriveAccessAsync(string driveId)
//         {
//             try
//             {
//                 var request = _driveService.Drives.Get(driveId);
//                 var drive = await request.ExecuteAsync();
//                 return drive != null;
//             }
//             catch (GoogleApiException ex)
//             {
//                 Console.WriteLine($"Error accessing shared drive: {ex.Message}");
//                 return false;
//             }
//         }

//         public async Task<IList<Google.Apis.Drive.v3.Data.Drive>> ListSharedDrivesAsync()
//         {
//             var request = _driveService.Drives.List();
//             request.PageSize = 100;
//             var result = await request.ExecuteAsync();
//             return result.Drives;
//         }

//     }
// }
