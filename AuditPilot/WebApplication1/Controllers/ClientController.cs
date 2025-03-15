using AuditPilot.API.Helpers;
using AuditPilot.Data;
using AuditPilot.Repositories.Interfaces;
using AuthPilot.Models;
using AuthPilot.Models.Enums;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Microsoft.AspNetCore.Identity; // UserManager ke liye
using System.Security.Claims; // ClaimsPrincipal ke liye

namespace AuditPilot.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class ClientController : ControllerBase
    {
        private readonly IClientRepository _clientRepository;
        private readonly IClientProjectRepository _clientProjectRepository;
        private readonly GoogleDriveHelper _googleDriveHelper;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;

        public ClientController(
            IClientRepository clientRepository,
            IClientProjectRepository clientProjectRepository,
            GoogleDriveHelper googleDriveHelper,
            IMapper mapper,
            IConfiguration configuration,
            UserManager<ApplicationUser> userManager) 
        {
            _clientRepository = clientRepository;
            _clientProjectRepository = clientProjectRepository;
            _googleDriveHelper = googleDriveHelper;
            _mapper = mapper;
            _configuration = configuration;
            _userManager = userManager;
        }
        [HttpPost("register")]
        public async Task<IActionResult> RegisterClient([FromBody] ClientDto clientDto)
        {
            if (clientDto == null || string.IsNullOrEmpty(clientDto.Name))
                return BadRequest("Invalid client data.");

            var client = _mapper.Map<Client>(clientDto);
            client.CreatedOn = DateTime.UtcNow;
            client.CreatedBy = SessionHelper.GetCurrentUserId()!.Value;
            client.IsActive = true;

            await _clientRepository.AddAsync(client);
            return Ok(new { ClientId = client.Id });
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllClients()
        {
            var id = SessionHelper.GetCurrentUserId()!.Value;
            var clients = await _clientRepository.GetAllAsync();
            var clientDtos = _mapper.Map<IEnumerable<ClientDto>>(clients);
            return Ok(clientDtos);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteClient(Guid id)
        {
            var client = await _clientRepository.GetByIdAsync(id);
            if (client == null)
                return NotFound("Client not found.");

            await _clientRepository.DeleteAsync(client);
            return Ok(new { Message = "Client deleted successfully." });
        }

        [HttpPost("create-project")]
        public async Task<IActionResult> CreateClientProject([FromBody] ClientProjectdto projectDto)
        {
            if (projectDto == null || string.IsNullOrEmpty(projectDto.ProjectName))
                return BadRequest("Invalid project data.");

            var client = await _clientRepository.GetByIdAsync(projectDto.ClientId);
            if (client == null)
                return NotFound("Client not found.");

            string rootFolderName = client.CompanyType == (int)CompanyType.PrivateLable ? "PrivateLabel" : "PublicLabel";
            string projectTypeFolderName = projectDto.ProjectType == ProjectType.Tax ? "Tax" : "Audit";

            string clientFolderId = await EnsureFolderStructureAsync(rootFolderName, projectTypeFolderName, client.Name);
            var projectFolder = await _googleDriveHelper.CreateFolderAsync(projectDto.ProjectName, clientFolderId);

            var clientProject = _mapper.Map<ClientProject>(projectDto);
            clientProject.GoogleDriveFolderId = projectFolder.Id;
            clientProject.CreatedOn = DateTime.UtcNow;
            clientProject.CreatedBy = SessionHelper.GetCurrentUserId()!.Value;
            clientProject.IsActive = true;

            await _clientProjectRepository.AddAsync(clientProject);
            return Ok(new { ProjectId = clientProject.Id });
        }

        [HttpGet("{clientId}/projects")]
        public async Task<IActionResult> GetProjectsByClientId(Guid clientId)
        {
            if (clientId == Guid.Empty)
                return BadRequest("Invalid Client ID.");

            try
            {
                var clientProjects = await _clientProjectRepository.GetClientsProjectAsync(clientId);
                var clientProjectDtos = _mapper.Map<List<ClientProjectdto>>(clientProjects);
                return Ok(clientProjectDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{clientId}/projectsbyType")]
        public async Task<IActionResult> GetProjectsByClientIdAndType(Guid clientId)
        {
            if (clientId == Guid.Empty)
                return BadRequest("Invalid Client ID.");

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return Unauthorized("User not found.");

                var roles = await _userManager.GetRolesAsync(user);
                var userRole = roles.FirstOrDefault();

                var clientProjects = await _clientProjectRepository.GetClientsProjectAsync(clientId);

                IEnumerable<ClientProject> filteredProjects;
                switch (userRole)
                {
                    case "TaxManager":
                        filteredProjects = clientProjects.Where(p => p.ProjectType == (int)ProjectType.Tax);
                        break;
                    case "AuditManager":
                        filteredProjects = clientProjects.Where(p => p.ProjectType == (int)ProjectType.Audit);
                        break;
                    case "Partner":
                        filteredProjects = clientProjects;
                        break;
                    case "User":
                        var permissions = await _clientProjectRepository.GetPermissionsByUserIdAsync(userId);
                        var allowedProjectIds = permissions.Where(p => p.HasAccess).Select(p => p.ProjectId);
                        filteredProjects = clientProjects.Where(p => allowedProjectIds.Contains(p.Id));
                        break;
                    default:
                        return Forbid("User role not authorized to view projects.");
                }

                var clientProjectDtos = _mapper.Map<List<ClientProjectdto>>(filteredProjects);
                return Ok(clientProjectDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        
        [HttpPost("project/permission/add")]
        [Authorize(Roles = "Partner")] 
        public async Task<IActionResult> AddProjectPermission([FromBody] UserProjectPermissionDto permissionDto)
        {
            if (permissionDto == null || string.IsNullOrEmpty(permissionDto.UserId) || permissionDto.ProjectId == Guid.Empty)
                return BadRequest("Invalid permission data.");

            var permission = new UserProjectPermission
            {
                UserId = permissionDto.UserId,
                ProjectId = permissionDto.ProjectId,
                HasAccess = permissionDto.HasAccess,
                AssignedOn = DateTime.UtcNow
            };

            await _clientProjectRepository.AddPermissionAsync(permission);
            return Ok(new { Message = "Permission added successfully.", PermissionId = permission.Id });
        }

        [HttpPut("project/permission/edit/{permissionId}")]
        [Authorize(Roles = "Partner")]
        public async Task<IActionResult> EditProjectPermission(Guid permissionId, [FromBody] UserProjectPermissionDto permissionDto)
        {
            var existingPermission = await _clientProjectRepository.GetPermissionsByUserIdAsync(permissionDto.UserId)
                .ContinueWith(t => t.Result.FirstOrDefault(p => p.Id == permissionId));
            if (existingPermission == null)
                return NotFound("Permission not found.");

            existingPermission.HasAccess = permissionDto.HasAccess;
            await _clientProjectRepository.UpdatePermissionAsync(existingPermission);
            return Ok(new { Message = "Permission updated successfully." });
        }

        [HttpDelete("project/permission/delete/{permissionId}")]
        [Authorize(Roles = "Partner")]
        public async Task<IActionResult> DeleteProjectPermission(Guid permissionId)
        {
            var permission = await _clientProjectRepository.GetPermissionsByUserIdAsync(null)
                .ContinueWith(t => t.Result.FirstOrDefault(p => p.Id == permissionId));
            if (permission == null)
                return NotFound("Permission not found.");

            await _clientProjectRepository.DeletePermissionAsync(permissionId);
            return Ok(new { Message = "Permission deleted successfully." });
        }

        [HttpGet("project/{projectId}/permissions")]
        [Authorize(Roles = "Partner")]
        public async Task<IActionResult> GetPermissionsByProjectId(Guid projectId)
        {
            var permissions = await _clientProjectRepository.GetPermissionsByProjectIdAsync(projectId);
            var permissionDtos = _mapper.Map<List<UserProjectPermissionDto>>(permissions);
            return Ok(permissionDtos);
        }

        private async Task<string> EnsureFolderStructureAsync(string rootFolderName, string projectTypeFolderName, string clientName)
        {
            // Check if the root folder exists; if not, create it
            var rootFolder = await _googleDriveHelper.GetOrCreateFolderAsync(rootFolderName, _configuration["RootFolderId"]);

            // Check if the project type folder exists within the root; if not, create it
            var clientFolder = await _googleDriveHelper.GetOrCreateFolderAsync(clientName, rootFolder.Id);

            // Check if the client folder exists within the project type folder; if not, create it
            var projectTypeFolder = await _googleDriveHelper.GetOrCreateFolderAsync(projectTypeFolderName, clientFolder.Id);
          
            return projectTypeFolder.Id;
        }
    }
}












//using AuditPilot.API.Helpers;
//using AuditPilot.Data;
//using AuditPilot.Repositories.Interfaces;
//using AuthPilot.Models;
//using AuthPilot.Models.Enums;
//using AutoMapper;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using System.Data;


//namespace AuditPilot.API.Controllers
//{
//    [ApiController]
//    [Authorize]
//    [Route("api/[controller]")]
//    public class ClientController : ControllerBase
//    {
//        private readonly IClientRepository _clientRepository;
//        private readonly IClientProjectRepository _clientProjectRepository;
//        private readonly GoogleDriveHelper _googleDriveHelper;
//        private readonly IMapper _mapper;
//        private readonly IConfiguration _configuration;

//        public ClientController(
//            IClientRepository clientRepository,
//            IClientProjectRepository clientProjectRepository,
//            GoogleDriveHelper googleDriveHelper, IMapper mapper, IConfiguration configuration)
//        {
//            _clientRepository = clientRepository;
//            _clientProjectRepository = clientProjectRepository;
//            _googleDriveHelper = googleDriveHelper;
//            _mapper = mapper;
//            _configuration = configuration;
//        }

//        [HttpPost("register")]
//        public async Task<IActionResult> RegisterClient([FromBody] ClientDto clientDto)
//        {
//            if (clientDto == null || string.IsNullOrEmpty(clientDto.Name))
//                return BadRequest("Invalid client data.");

//            var client = _mapper.Map<Client>(clientDto);
//            client.CreatedOn = DateTime.UtcNow;
//            client.CreatedBy = SessionHelper.GetCurrentUserId()!.Value;
//            client.IsActive = true;

//            await _clientRepository.AddAsync(client);
//            return Ok(new { ClientId = client.Id });
//        }

//        [HttpGet("all")]
//        public async Task<IActionResult> GetAllClients()
//        {
//            var id = SessionHelper.GetCurrentUserId()!.Value;
//            var clients = await _clientRepository.GetAllAsync();
//            var clientDtos = _mapper.Map<IEnumerable<ClientDto>>(clients);
//            return Ok(clientDtos);
//        }

//        [HttpDelete("delete/{id}")]
//        public async Task<IActionResult> DeleteClient(Guid id)
//        {
//            var client = await _clientRepository.GetByIdAsync(id);
//            if (client == null)
//                return NotFound("Client not found.");

//            await _clientRepository.DeleteAsync(client);
//            return Ok(new { Message = "Client deleted successfully." });
//        }

//        [HttpPost("create-project")]
//        public async Task<IActionResult> CreateClientProject([FromBody] ClientProjectdto projectDto)
//        {
//            if (projectDto == null || string.IsNullOrEmpty(projectDto.ProjectName))
//                return BadRequest("Invalid project data.");

//            var client = await _clientRepository.GetByIdAsync(projectDto.ClientId);
//            if (client == null)
//                return NotFound("Client not found.");

//            string rootFolderName = client.CompanyType == (int)CompanyType.PrivateLable ? "PrivateLabel" : "PublicLabel";
//            string projectTypeFolderName = projectDto.ProjectType == ProjectType.Tax ? "Tax" : "Audit";

//            string clientFolderId = await EnsureFolderStructureAsync(rootFolderName, projectTypeFolderName, client.Name);
//            var projectFolder = await _googleDriveHelper.CreateFolderAsync(projectDto.ProjectName, clientFolderId);

//            var clientProject = _mapper.Map<ClientProject>(projectDto);
//            clientProject.GoogleDriveFolderId = projectFolder.Id;
//            clientProject.CreatedOn = DateTime.UtcNow;
//            clientProject.CreatedBy = SessionHelper.GetCurrentUserId()!.Value;
//            clientProject.IsActive = true;

//            await _clientProjectRepository.AddAsync(clientProject);
//            return Ok(new { ProjectId = clientProject.Id });
//        }

//        [HttpGet("{clientId}/projects")]
//        public async Task<IActionResult> GetProjectsByClientId(Guid clientId)
//        {
//            if (clientId == Guid.Empty)
//                return BadRequest("Invalid Client ID.");

//            try
//            {
//                var clientProjects = await _clientProjectRepository.GetClientsProjectAsync(clientId);
//                var clientProjectDtos = _mapper.Map<List<ClientProjectdto>>(clientProjects);
//                return Ok(clientProjectDtos);
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, $"Internal server error: {ex.Message}");
//            }
//        }



//        private async Task<string> EnsureFolderStructureAsync(string rootFolderName, string projectTypeFolderName, string clientName)
//        {
//            // Check if the root folder exists; if not, create it
//            var rootFolder = await _googleDriveHelper.GetOrCreateFolderAsync(rootFolderName, _configuration["RootFolderId"]);

//            // Check if the project type folder exists within the root; if not, create it
//            var clientFolder = await _googleDriveHelper.GetOrCreateFolderAsync(clientName, rootFolder.Id);

//            // Check if the client folder exists within the project type folder; if not, create it
//            var projectTypeFolder = await _googleDriveHelper.GetOrCreateFolderAsync(projectTypeFolderName, clientFolder.Id);

//            return projectTypeFolder.Id;
//        }
//    }
//}
