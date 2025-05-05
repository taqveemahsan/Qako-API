using AuditPilot.API.Helpers;
using AuditPilot.Data;
using AuditPilot.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuditPilot.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DocumentSyncController : Controller
    {
        private readonly GoogleDriveHelper _googleDriveHelper;
        private readonly IGoogleDriveItemRepository _driveItemRepository;
        private readonly IClientRepository _clientRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public DocumentSyncController(
            GoogleDriveHelper googleDriveHelper,
            IGoogleDriveItemRepository driveItemRepository,
            IClientRepository clientRepository,
            UserManager<ApplicationUser> userManager
        )
        {
            _googleDriveHelper = googleDriveHelper;
            _driveItemRepository = driveItemRepository;
            _clientRepository = clientRepository;
            _userManager = userManager; // Assign to field
        }


        [HttpPost("upload-file")]
        public async Task<IActionResult> UploadFile([FromForm] IFormFile file, [FromForm] string ParentFolderId)
        {
            if (file == null)
                return BadRequest("No file uploaded.");

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized("User ID claim not found.");

            var userId = Guid.Parse(userIdClaim.Value);

            // Create a temp file with the original file name (preserving extension)
            var tempFilePath = Path.Combine(Path.GetTempPath(), file.FileName);

            // Save the IFormFile content to the temp file
            using (var stream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write))
            {
                await file.CopyToAsync(stream);
            }

            try
            {
                var uploadedFile = await _googleDriveHelper.CreateFileAsync(tempFilePath, ParentFolderId);

                var driveItem = new GoogleDriveItem
                {
                    Id = Guid.NewGuid(),
                    FileName = uploadedFile.Name,
                    GoogleId = uploadedFile.Id,
                    IsFolder = false,
                    CreatedBy = userId,
                    IsActive = true,
                    CreatedOn = DateTime.UtcNow,
                    ModifiedBy = userId,
                    ModifiedOn = DateTime.UtcNow
                };

                await _driveItemRepository.AddAsync(driveItem);

                return Ok(new { FileId = uploadedFile.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
            finally
            {
                System.IO.File.Delete(tempFilePath);
            }
        }

        [HttpPost("create-folder")]
        public async Task<IActionResult> CreateFolder([FromForm] string folderName, [FromForm] string parentFolderId)
        {
            if (string.IsNullOrEmpty(folderName))
                return BadRequest("Folder name is required.");

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized("User ID claim not found.");

            var userId = Guid.Parse(userIdClaim.Value);

            try
            {
                var createdFolder = await _googleDriveHelper.CreateFolderAsync(folderName, parentFolderId);

                var driveItem = new GoogleDriveItem
                {
                    Id = Guid.NewGuid(),
                    FileName = createdFolder.Name,
                    GoogleId = createdFolder.Id,
                    IsFolder = true,
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = userId,
                    IsActive = true,
                    ModifiedBy = userId,
                    ModifiedOn = DateTime.UtcNow
                };

                await _driveItemRepository.AddAsync(driveItem);

                return Ok(new { FolderId = createdFolder.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpGet("{folderId}/list-items")]
        public async Task<IActionResult> ListItems(string folderId)
        {
            if (string.IsNullOrEmpty(folderId))
                return BadRequest("Folder ID is required.");

            try
            {
                // Get items from Google Drive
                var googleItems = await _googleDriveHelper.GetAllItemsInFolderAsync(folderId);

                // Get corresponding database items
                var googleIds = googleItems.Select(item => item.Id).ToList();
                var dbItems = await _driveItemRepository.GetByGoogleIdsAsync(googleIds);
                var dbItemsDict = dbItems.ToDictionary(item => item.GoogleId, item => item);

                // Collect all unique user IDs from CreatedBy and ModifiedBy
                var userIds = dbItems
                    .SelectMany(item => new[] { item.CreatedBy, item.ModifiedBy })
                    .Distinct()
                    .ToList();

                // Fetch user details for all user IDs
                var users = new Dictionary<Guid, IdentityUser>();
                foreach (var userId in userIds)
                {
                    var user = await _userManager.FindByIdAsync(userId.ToString());
                    if (user != null)
                    {
                        users[userId] = user;
                    }
                }

                // Build the response
                var result = new List<object>();
                foreach (var googleItem in googleItems)
                {
                    var dbItem = dbItemsDict.ContainsKey(googleItem.Id) ? dbItemsDict[googleItem.Id] : null;

                    // Get usernames for CreatedBy and ModifiedBy
                    string createdByName = null;
                    string modifiedByName = null;

                    if (dbItem != null)
                    {
                        if (users.ContainsKey(dbItem.CreatedBy))
                        {
                            var user = users[dbItem.CreatedBy];
                            createdByName = $"{user.UserName}".Trim();
                            if (string.IsNullOrEmpty(createdByName))
                                createdByName = user.UserName; // Fallback to UserName
                        }

                        if (users.ContainsKey(dbItem.ModifiedBy))
                        {
                            var user = users[dbItem.ModifiedBy];
                            modifiedByName = $"{user.UserName}".Trim();
                            if (string.IsNullOrEmpty(modifiedByName))
                                modifiedByName = user.UserName; // Fallback to UserName
                        }
                    }

                    result.Add(new
                    {
                        googleItem.Id,
                        googleItem.Name,
                        googleItem.MimeType,
                        ThumbnailLink = googleItem.IconLink,
                        googleItem.Size,
                        googleItem.FileExtension,
                        // Database fields (use null if no matching db item)
                        CreatedOn = dbItem?.CreatedOn,
                        CreatedBy = createdByName ?? dbItem?.CreatedBy.ToString(), // Fallback to Guid if user not found
                        IsActive = dbItem?.IsActive,
                        ModifiedOn = dbItem?.ModifiedOn,
                        ModifiedBy = modifiedByName ?? dbItem?.ModifiedBy.ToString() // Fallback to Guid if user not found
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        //[HttpGet("{folderId}/list-items")]
        //public async Task<IActionResult> ListItems(string folderId)
        //{
        //    if (string.IsNullOrEmpty(folderId))
        //        return BadRequest("Folder ID is required.");

        //    try
        //    {
        //        var googleItems = await _googleDriveHelper.GetAllItemsInFolderAsync(folderId);

        //        var googleIds = googleItems.Select(item => item.Id).ToList();

        //        var dbItems = await _driveItemRepository.GetByGoogleIdsAsync(googleIds);
        //        var dbItemsDict = dbItems.ToDictionary(item => item.GoogleId, item => item);

        //        var result = new List<object>();
        //        foreach (var googleItem in googleItems)
        //        {
        //            var dbItem = dbItemsDict.ContainsKey(googleItem.Id) ? dbItemsDict[googleItem.Id] : null;

        //            result.Add(new
        //            {
        //                googleItem.Id,
        //                googleItem.Name,
        //                googleItem.MimeType,
        //                ThumbnailLink = googleItem.IconLink,
        //                googleItem.Size,
        //                googleItem.FileExtension,
        //                // Add database fields (use null if no matching db item)
        //                CreatedOn = dbItem?.CreatedOn,
        //                CreatedBy = dbItem?.CreatedBy.ToString(), // Convert Guid to string
        //                IsActive = dbItem?.IsActive,
        //                ModifiedOn = dbItem?.ModifiedOn,
        //                ModifiedBy = dbItem?.ModifiedBy.ToString() // Convert Guid to string
        //            });
        //        }

        //        return Ok(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"Internal server error: {ex.Message}");
        //    }
        //}


        [HttpGet("download")]
        public async Task<IActionResult> DownloadFile([FromQuery] string fileId)
        {
            if (string.IsNullOrEmpty(fileId))
                return BadRequest("File ID is required.");

            var tempFilePath = Path.GetTempFileName();
            await _googleDriveHelper.DownloadFileAsync(fileId, tempFilePath);

            var fileBytes = await System.IO.File.ReadAllBytesAsync(tempFilePath);
            var fileName = Path.GetFileName(tempFilePath);

            // Delete the temporary file
            System.IO.File.Delete(tempFilePath);

            return File(fileBytes, "application/octet-stream", fileName);
        }
        [HttpPost("replace-file")]
        public async Task<IActionResult> ReplaceFile([FromForm] string fileId, [FromForm] IFormFile newFile)
        {
            if (string.IsNullOrEmpty(fileId))
                return BadRequest("File ID is required.");

            if (newFile == null || newFile.Length == 0)
                return BadRequest("No file uploaded.");

            // Create a temp file with the original file name (preserving extension)
            var tempFilePath = Path.Combine(Path.GetTempPath(), newFile.FileName);

            try
            {
                // Ensure temp file doesn't already exist
                //if (File.Exists(tempFilePath))
                //{
                //    File.Delete(tempFilePath);
                //} 

                // Save the IFormFile content to the temp file
                using (var stream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write))
                {
                    await newFile.CopyToAsync(stream);
                }

                // Replace the file on Google Drive using the temp file path
                var updatedFileId = await _googleDriveHelper.ReplaceFileAsync(fileId, tempFilePath);

                return Ok(new { FileId = updatedFileId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
            finally
            {
                Console.WriteLine($"Failed to delete temp file {tempFilePath}:");
                //// Clean up the temp file
                //if (File.Exists(tempFilePath))
                //{
                //    try
                //    {
                //        File.Delete(tempFilePath);
                //    }
                //    catch (Exception ex)
                //    {
                //        Console.WriteLine($"Failed to delete temp file {tempFilePath}: {ex.Message}");
                //    }
                //}
            }
        }

        [HttpGet("{folderId}/size")]
        public async Task<IActionResult> GetFolderSize(string folderId)
        {
            if (string.IsNullOrEmpty(folderId))
                return BadRequest("Folder ID is required.");

            try
            {
                // Get all items in the folder
                var googleItems = await _googleDriveHelper.GetAllItemsInFolderAsync(folderId);

                // Calculate total size of files (exclude folders)
                long totalSize = 0;
                foreach (var item in googleItems)
                {
                    // Skip folders (identified by MimeType)
                    if (item.MimeType == "application/vnd.google-apps.folder")
                        continue;

                    // Add file size (handle null Size)
                    totalSize += item.Size ?? 0;
                }

                // Return the total size in bytes
                return Ok(new
                {
                    FolderId = folderId,
                    TotalSizeBytes = totalSize,
                    FormattedSize = FormatFileSize(totalSize) // Optional: human-readable format
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPatch("rename")]
        public async Task<IActionResult> RenameItem([FromBody] RenameItemRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.ItemId) || string.IsNullOrEmpty(request.NewName))
                return BadRequest("Item ID and new name are required.");

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized("User ID claim not found.");

            var userId = Guid.Parse(userIdClaim.Value);

            try
            {
                // Get the existing item from the database
                var dbItem = await _driveItemRepository.GetByGoogleIdAsync(request.ItemId);
                if (dbItem == null)
                    return NotFound("Item not found.");

                // Check for duplicate name in the same parent folder
                var item = await _googleDriveHelper.GetItemAsync(request.ItemId);
                var parentFolderId = item?.Parents?.FirstOrDefault();
                if (!string.IsNullOrEmpty(parentFolderId))
                {
                    var siblingItems = await _googleDriveHelper.GetAllItemsInFolderAsync(parentFolderId);
                    if (siblingItems.Any(item => item.Name == request.NewName && item.Id != request.ItemId))
                        return BadRequest(new { message = "An item with this name already exists in the folder." });
                }

                // Rename the item on Google Drive
                var updatedItem = await _googleDriveHelper.RenameItemAsync(request.ItemId, request.NewName);
                if (updatedItem == null)
                    return StatusCode(500, "Failed to rename item on Google Drive.");

                // Update the database
                dbItem.FileName = updatedItem.Name;
                dbItem.ModifiedBy = userId;
                dbItem.ModifiedOn = DateTime.UtcNow;
                await _driveItemRepository.UpdateAsync(dbItem);

                return Ok(new { ItemId = updatedItem.Id, NewName = updatedItem.Name });
            }
            catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Helper method to format file size (optional)
        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double size = bytes;
            int order = 0;

            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }

            return $"{size:0.##} {sizes[order]}";
        }
        //[HttpPost("replace-file")]
        //public async Task<IActionResult> ReplaceFile([FromForm] string fileId, [FromForm] IFormFile newFile)
        //{
        //    if (string.IsNullOrEmpty(fileId))
        //        return BadRequest("File ID is required.");

        //    if (newFile == null || newFile.Length == 0)
        //        return BadRequest("No file uploaded.");

        //    // Save the new file temporarily
        //    var tempFilePath = Path.GetTempFileName();
        //    using (var stream = new FileStream(tempFilePath, FileMode.Create))
        //    {
        //        await newFile.CopyToAsync(stream);
        //    }

        //    try
        //    {
        //        // Replace the file on Google Drive
        //        var updatedFileId = await _googleDriveHelper.ReplaceFileAsync(fileId, tempFilePath);

        //        // Delete the temporary file
        //        System.IO.File.Delete(tempFilePath);

        //        return Ok(new { FileId = updatedFileId });
        //    }
        //    catch (Exception ex)
        //    {
        //        // Delete the temporary file in case of an error
        //        System.IO.File.Delete(tempFilePath);
        //        return StatusCode(500, $"Internal server error: {ex.Message}");
        //    }
        //}
    }

    public class RenameItemRequest
    {
        public string ItemId { get; set; }
        public string NewName { get; set; }
    }
}
