using AuditPilot.Data;
using AuthPilot.Models.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuditPilot.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AuthController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
            _roleManager = roleManager;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            // Validate the model
            if (model == null || string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
            {
                return BadRequest("Invalid registration data.");
            }

            // Validate roles
            if (model.RoleNames == null || !model.RoleNames.Any())
            {
                return BadRequest("At least one role must be specified.");
            }

            // Create the user
            var user = new ApplicationUser
            {
                UserName = model.Username,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Assign multiple roles to the user
                var roleResult = await _userManager.AddToRolesAsync(user, model.RoleNames);

                if (roleResult.Succeeded)
                {
                    return Ok(new { message = "User registered successfully with roles!" });
                }
                else
                {
                    // If role assignment fails, you might want to delete the user or handle the error
                    await _userManager.DeleteAsync(user); // Optional: Rollback user creation
                    return BadRequest(new { message = "User created but failed to assign roles.", errors = roleResult.Errors });
                }
            }

            return BadRequest(new { message = "User registration failed.", errors = result.Errors });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var roles = await _userManager.GetRolesAsync(user);
                var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Role,roles.First()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };
                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    expires: DateTime.Now.AddHours(3),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

                return Ok(new
                {
                    user.Email,
                    user.FirstName,
                    user.LastName,
                    Role = roles.First(),
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo
                });
            }
            return Unauthorized();
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers(string search = "", int pageNumber = 1, int pageSize = 10, string role = null)
        {
            var query = _userManager.Users.AsQueryable();

            // Search filter
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(u => u.FirstName.ToLower().Contains(search) ||
                                        u.LastName.ToLower().Contains(search) ||
                                        u.UserName.ToLower().Contains(search) ||
                                        u.Email.ToLower().Contains(search));
            }

            // Fetch users first (apply search filter on DB side)
            var users = await query
                .OrderBy(u => u.UserName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Apply role filter client-side
            if (!string.IsNullOrEmpty(role))
            {
                users = users.Where(u => _userManager.GetRolesAsync(u).Result.Contains(role, StringComparer.OrdinalIgnoreCase)).ToList();
            }

            // Total count (without role filter for simplicity)
            var totalUsersQuery = _userManager.Users.AsQueryable();
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                totalUsersQuery = totalUsersQuery.Where(u => u.FirstName.ToLower().Contains(search) ||
                                                            u.LastName.ToLower().Contains(search) ||
                                                            u.UserName.ToLower().Contains(search) ||
                                                            u.Email.ToLower().Contains(search));
            }
            var totalUsers = await totalUsersQuery.CountAsync();

            // Prepare response
            var userList = new List<object>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userList.Add(new
                {
                    user.Id,
                    user.FirstName,
                    user.LastName,
                    user.UserName,
                    user.Email,
                    RoleNames = roles.ToList() // Changed from RoleName to RoleNames, returning the full list
                });
            }

            return Ok(new
            {
                TotalUsers = totalUsers,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Users = userList
            });
        }
        //[HttpGet("users")]
        //public async Task<IActionResult> GetUsers(string search = "", int pageNumber = 1, int pageSize = 10)
        //{
        //    // Base query with pagination
        //    var query = _userManager.Users.AsQueryable();

        //    // Search filter
        //    if (!string.IsNullOrEmpty(search))
        //    {
        //        search = search.ToLower();
        //        query = query.Where(u => u.FirstName.ToLower().Contains(search) ||
        //                                u.LastName.ToLower().Contains(search) ||
        //                                u.UserName.ToLower().Contains(search) ||
        //                                u.Email.ToLower().Contains(search));
        //    }

        //    // Total count
        //    var totalUsers = await query.CountAsync();

        //    // Fetch users with pagination
        //    var users = await query
        //        .OrderBy(u => u.UserName)
        //        .Skip((pageNumber - 1) * pageSize)
        //        .Take(pageSize)
        //        .ToListAsync();

        //    // Fetch roles for the selected users only
        //    var userList = new List<object>();
        //    foreach (var user in users)
        //    {
        //        var roles = await _userManager.GetRolesAsync(user);
        //        userList.Add(new
        //        {
        //            user.Id,
        //            user.FirstName,
        //            user.LastName,
        //            user.UserName,
        //            user.Email,
        //            RoleName = roles.FirstOrDefault() ?? "None"
        //        });
        //    }

        //    return Ok(new
        //    {
        //        TotalUsers = totalUsers,
        //        PageNumber = pageNumber,
        //        PageSize = pageSize,
        //        Users = userList
        //    });
        //}

        //[HttpGet("users")]
        //public async Task<IActionResult> GetUsers()
        //{
        //    var users = await _userManager.Users.ToListAsync();
        //    var userList = new List<object>();

        //    foreach (var user in users)
        //    {
        //        var roles = await _userManager.GetRolesAsync(user);
        //        userList.Add(new
        //        {
        //            user.Id,
        //            user.FirstName,
        //            user.LastName,
        //            user.UserName,
        //            user.Email,
        //            RoleName = roles.FirstOrDefault() ?? "None"
        //        });
        //    }

        //    return Ok(userList);
        //}
    }
}
//dotnet ef migrations add addDate --context AuditPilot.Data.ApplicationDbContext --startup-project .\WebApplication1\ --project .\AuditPilot.Data\
//dotnet ef database update --context AuditPilot.Data.ApplicationDbContext --startup-project .\WebApplication1\ --project .\AuditPilot.Data\
