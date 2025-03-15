using AuditPilot.API.Helpers;
using AuditPilot.Data;
using AuditPilot.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
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

        public DocumentSyncController(
            GoogleDriveHelper googleDriveHelper,
            IGoogleDriveItemRepository driveItemRepository,
            IClientRepository clientRepository)
        {
            _googleDriveHelper = googleDriveHelper;
            _driveItemRepository = driveItemRepository;
            _clientRepository = clientRepository;
        }


        [HttpPost("upload-file")]
        public async Task<IActionResult> UploadFile([FromForm] IFormFile file, [FromForm] string ParentFolderId)
        {
            if (file == null || file.Length == 0)
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
                    IsActive = true
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
                    IsActive = true
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

            var items = await _googleDriveHelper.GetAllItemsInFolderAsync(folderId);
            var result = new List<object>();

            foreach (var item in items)
            {
                result.Add(new
                {
                    item.Id,
                    item.Name,
                    item.MimeType,
                    ThumbnailLink = item.IconLink
                });
            }

            return Ok(result);
        }

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
}
