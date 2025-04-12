using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Net.Mail;
using System.Threading.Tasks;
using AuditPilot.Data;
using static System.Net.WebRequestMethods;
using System.Net;
using Microsoft.Extensions.Hosting.Internal;

namespace AuditPilot.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public AuthController(
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _configuration = configuration;
            _roleManager = roleManager;
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
            {
                return BadRequest("Invalid registration data.");
            }

            if (model.RoleNames == null || !model.RoleNames.Any())
            {
                return BadRequest("At least one role must be specified.");
            }

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
                var roleResult = await _userManager.AddToRolesAsync(user, model.RoleNames);

                if (roleResult.Succeeded)
                {
                    return Ok(new { message = "User registered successfully with roles!" });
                }
                else
                {
                    await _userManager.DeleteAsync(user);
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
                    new Claim(ClaimTypes.Role, roles.First()),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };
                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    expires: DateTime.Now.AddDays(5), // Changed to 5 days to match client-side token expiry
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

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(u => u.FirstName.ToLower().Contains(search) ||
                                        u.LastName.ToLower().Contains(search) ||
                                        u.UserName.ToLower().Contains(search) ||
                                        u.Email.ToLower().Contains(search));
            }

            var users = await query
                .OrderBy(u => u.UserName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (!string.IsNullOrEmpty(role))
            {
                users = users.Where(u => _userManager.GetRolesAsync(u).Result.Contains(role, StringComparer.OrdinalIgnoreCase)).ToList();
            }

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
                    RoleNames = roles.ToList()
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

        [HttpPost("validate-token")]
        public async Task<IActionResult> ValidateToken([FromBody] TokenValidationModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.Token))
            {
                return BadRequest(new { message = "Token is required." });
            }

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidAudience = _configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };

                SecurityToken validatedToken;
                var principal = tokenHandler.ValidateToken(model.Token, validationParameters, out validatedToken);

                var username = principal.FindFirst(ClaimTypes.Name)?.Value;
                var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var role = principal.FindFirst(ClaimTypes.Role)?.Value;

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Invalid token: Missing user information." });
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return Unauthorized(new { message = "User not found." });
                }

                var roles = await _userManager.GetRolesAsync(user);

                return Ok(new
                {
                    user.Id,
                    user.UserName,
                    user.Email,
                    user.FirstName,
                    user.LastName,
                    Role = roles.First(),
                    Roles = roles.ToList(),
                    TokenValid = true
                });
            }
            catch (SecurityTokenExpiredException)
            {
                return Unauthorized(new { message = "Token has expired." });
            }
            catch (SecurityTokenException ex)
            {
                return Unauthorized(new { message = "Invalid token.", error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while validating the token.", error = ex.Message });
            }
        }

        [HttpPost("check-email")]
        public async Task<IActionResult> CheckEmail([FromBody] CheckEmailRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(new { Message = "Email is required." });
            }

            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
            {
                return NotFound(new { Message = "Email not found." });
            }

            return Ok(new { Message = "Email found." });
        }

        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromBody] SendOtpRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(new { Message = "Email is required." });
            }

            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
            {
                return NotFound(new { Message = "Email not found." });
            }

            // Generate a 6-digit OTP
            var otpCode = new Random().Next(100000, 999999).ToString();

            // Save OTP to database
            var otp = new AccountConfirmations
            {
                Email = request.Email,
                Code = otpCode,
                CreatedAt = DateTime.Now,
                ExpiresAt = DateTime.Now.AddMinutes(10),
                IsUsed = false
            };
            _context.AccountConfirmations.Add(otp);
            await _context.SaveChangesAsync();

            // Send email with OTP
            var emailBody = $@"
                <h2>Password Reset Request</h2>
                <p>Dear {user.FirstName},</p>
                <p>We received a request to reset your password. Use the following OTP to proceed:</p>
                <h3>{otpCode}</h3>
                <p>This OTP is valid for 10 minutes. If you did not request this, please ignore this email.</p>
                <p>Best regards,<br>BAKERTILLY DMS Team</p>
            ";

            await EmailAsync("", request.Email, emailBody);

            return Ok(new { Message = "OTP sent to your email." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Otp) ||
                string.IsNullOrWhiteSpace(request.NewPassword) || string.IsNullOrWhiteSpace(request.ConfirmPassword))
            {
                return BadRequest(new { Message = "All fields are required." });
            }

            if (request.NewPassword != request.ConfirmPassword)
            {
                return BadRequest(new { Message = "Passwords do not match." });
            }

            // Validate OTP
            var otp = await _context.AccountConfirmations
                .FirstOrDefaultAsync(o => o.Email == request.Email && o.Code == request.Otp && !o.IsUsed);

            if (otp == null)
            {
                return BadRequest(new { Message = "Invalid or expired OTP." });
            }

            if (otp.ExpiresAt < DateTime.Now)
            {
                return BadRequest(new { Message = "OTP has expired." });
            }

            // Find user
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return NotFound(new { Message = "User not found." });
            }

            // Reset password using UserManager
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);

            if (!result.Succeeded)
            {
                return BadRequest(new { Message = "Failed to reset password.", Errors = result.Errors });
            }

            // Mark OTP as used
            otp.IsUsed = true;
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Password reset successfully." });
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                using (var client = new System.Net.Mail.SmtpClient("smtp.gmail.com", 587))
                {
                    client.EnableSsl = true;
                    client.Credentials = new System.Net.NetworkCredential(
                        "taqi.malik86@gmail.com", // SmtpUsername
                        "taqveem123"             // SmtpPassword
                    );
                    var mailMessage = new System.Net.Mail.MailMessage
                    {
                        From = new System.Net.Mail.MailAddress("taqi.malik86@gmail.com", "Taqi"), // Display name: Taqi
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    };
                    mailMessage.To.Add(toEmail);
                    await client.SendMailAsync(mailMessage);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<bool> EmailAsync(string ToName, string ToEmail, string body)
        {
            try
            {
                // Create MailMessage object
                MailMessage message = new MailMessage();

                // Set the sender (From) address
                message.From = new MailAddress("taqi.malik86@gmail.com", "Taqi Malik");

                // Add the recipient (To) address
                message.To.Add(new MailAddress(ToEmail, ToName));

                // Set email subject and body
                message.Subject = "Bakertilly: Email";
                message.IsBodyHtml = true;
                
                message.Body = body;

                // Send the email
                return await SendEmail(message);
            }
            catch (Exception ex)
            {
                throw new Exception("Error sending confirm email", ex);
            }
        }

        public async Task<bool> SendEmail(MailMessage Body)
        {
            try
            {
                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    EnableSsl = true,
                    Credentials = new NetworkCredential("taqi.malik86@gmail.com", "ylyu opaz oydx uusz")
                };
                await smtp.SendMailAsync(Body);
                Body.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }

    public class RegisterModel
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<string> RoleNames { get; set; }
    }

    public class LoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class TokenValidationModel
    {
        public string Token { get; set; }
    }

    public class CheckEmailRequest
    {
        public string Email { get; set; }
    }

    public class SendOtpRequest
    {
        public string Email { get; set; }
    }

    public class ResetPasswordRequest
    {
        public string Email { get; set; }
        public string Otp { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
}














//using AuditPilot.Data;
//using AuditPilot.Data.ViewModels;
//using AuthPilot.Models.Auth;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.IdentityModel.Tokens;
//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;
//using System.Text;
//using static System.Net.WebRequestMethods;

//namespace AuditPilot.API.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class AuthController : ControllerBase
//    {
//        private readonly UserManager<ApplicationUser> _userManager;
//        private readonly RoleManager<IdentityRole> _roleManager;
//        private readonly IConfiguration _configuration;

//        public AuthController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, IConfiguration configuration)
//        {
//            _userManager = userManager;
//            _configuration = configuration;
//            _roleManager = roleManager;
//        }
//        [HttpPost("register")]
//        public async Task<IActionResult> Register([FromBody] RegisterModel model)
//        {
//            // Validate the model
//            if (model == null || string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
//            {
//                return BadRequest("Invalid registration data.");
//            }

//            // Validate roles
//            if (model.RoleNames == null || !model.RoleNames.Any())
//            {
//                return BadRequest("At least one role must be specified.");
//            }

//            // Create the user
//            var user = new ApplicationUser
//            {
//                UserName = model.Username,
//                Email = model.Email,
//                FirstName = model.FirstName,
//                LastName = model.LastName
//            };

//            var result = await _userManager.CreateAsync(user, model.Password);

//            if (result.Succeeded)
//            {
//                // Assign multiple roles to the user
//                var roleResult = await _userManager.AddToRolesAsync(user, model.RoleNames);

//                if (roleResult.Succeeded)
//                {
//                    return Ok(new { message = "User registered successfully with roles!" });
//                }
//                else
//                {
//                    // If role assignment fails, you might want to delete the user or handle the error
//                    await _userManager.DeleteAsync(user); // Optional: Rollback user creation
//                    return BadRequest(new { message = "User created but failed to assign roles.", errors = roleResult.Errors });
//                }
//            }

//            return BadRequest(new { message = "User registration failed.", errors = result.Errors });
//        }

//        [HttpPost("login")]
//        public async Task<IActionResult> Login([FromBody] LoginModel model)
//        {
//            var user = await _userManager.FindByNameAsync(model.Username);
//            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
//            {
//                var roles = await _userManager.GetRolesAsync(user);
//                var authClaims = new List<Claim>
//            {
//                new Claim(ClaimTypes.Name, user.UserName),
//                new Claim(ClaimTypes.NameIdentifier, user.Id),
//                new Claim(ClaimTypes.Role,roles.First()),
//                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
//            };
//                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

//                var token = new JwtSecurityToken(
//                    issuer: _configuration["Jwt:Issuer"],
//                    audience: _configuration["Jwt:Audience"],
//                    expires: DateTime.Now.AddHours(3),
//                    claims: authClaims,
//                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
//                );

//                return Ok(new
//                {
//                    user.Email,
//                    user.FirstName,
//                    user.LastName,
//                    Role = roles.First(),
//                    token = new JwtSecurityTokenHandler().WriteToken(token),
//                    expiration = token.ValidTo
//                });
//            }
//            return Unauthorized();
//        }

//        [HttpGet("users")]
//        public async Task<IActionResult> GetUsers(string search = "", int pageNumber = 1, int pageSize = 10, string role = null)
//        {
//            var query = _userManager.Users.AsQueryable();

//            // Search filter
//            if (!string.IsNullOrEmpty(search))
//            {
//                search = search.ToLower();
//                query = query.Where(u => u.FirstName.ToLower().Contains(search) ||
//                                        u.LastName.ToLower().Contains(search) ||
//                                        u.UserName.ToLower().Contains(search) ||
//                                        u.Email.ToLower().Contains(search));
//            }

//            // Fetch users first (apply search filter on DB side)
//            var users = await query
//                .OrderBy(u => u.UserName)
//                .Skip((pageNumber - 1) * pageSize)
//                .Take(pageSize)
//                .ToListAsync();

//            // Apply role filter client-side
//            if (!string.IsNullOrEmpty(role))
//            {
//                users = users.Where(u => _userManager.GetRolesAsync(u).Result.Contains(role, StringComparer.OrdinalIgnoreCase)).ToList();
//            }

//            // Total count (without role filter for simplicity)
//            var totalUsersQuery = _userManager.Users.AsQueryable();
//            if (!string.IsNullOrEmpty(search))
//            {
//                search = search.ToLower();
//                totalUsersQuery = totalUsersQuery.Where(u => u.FirstName.ToLower().Contains(search) ||
//                                                            u.LastName.ToLower().Contains(search) ||
//                                                            u.UserName.ToLower().Contains(search) ||
//                                                            u.Email.ToLower().Contains(search));
//            }
//            var totalUsers = await totalUsersQuery.CountAsync();

//            // Prepare response
//            var userList = new List<object>();
//            foreach (var user in users)
//            {
//                var roles = await _userManager.GetRolesAsync(user);
//                userList.Add(new
//                {
//                    user.Id,
//                    user.FirstName,
//                    user.LastName,
//                    user.UserName,
//                    user.Email,
//                    RoleNames = roles.ToList() // Changed from RoleName to RoleNames, returning the full list
//                });
//            }

//            return Ok(new
//            {
//                TotalUsers = totalUsers,
//                PageNumber = pageNumber,
//                PageSize = pageSize,
//                Users = userList
//            });
//        }

//        [HttpPost("validate-token")]
//        public async Task<IActionResult> ValidateToken([FromBody] TokenValidationModel model)
//        {
//            if (model == null || string.IsNullOrEmpty(model.Token))
//            {
//                return BadRequest(new { message = "Token is required." });
//            }

//            try
//            {
//                var tokenHandler = new JwtSecurityTokenHandler();
//                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
//                var validationParameters = new TokenValidationParameters
//                {
//                    ValidateIssuer = true,
//                    ValidateAudience = true,
//                    ValidateLifetime = true, // Check if token is expired
//                    ValidateIssuerSigningKey = true,
//                    ValidIssuer = _configuration["Jwt:Issuer"],
//                    ValidAudience = _configuration["Jwt:Audience"],
//                    IssuerSigningKey = new SymmetricSecurityKey(key)
//                };

//                // Validate the token
//                SecurityToken validatedToken;
//                var principal = tokenHandler.ValidateToken(model.Token, validationParameters, out validatedToken);

//                // Extract claims from the token
//                var username = principal.FindFirst(ClaimTypes.Name)?.Value;
//                var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//                var role = principal.FindFirst(ClaimTypes.Role)?.Value;

//                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(userId))
//                {
//                    return Unauthorized(new { message = "Invalid token: Missing user information." });
//                }

//                // Fetch the user from the database to get additional details
//                var user = await _userManager.FindByIdAsync(userId);
//                if (user == null)
//                {
//                    return Unauthorized(new { message = "User not found." });
//                }

//                // Get the user's roles
//                var roles = await _userManager.GetRolesAsync(user);

//                // Return user details
//                return Ok(new
//                {
//                    user.Id,
//                    user.UserName,
//                    user.Email,
//                    user.FirstName,
//                    user.LastName,
//                    Role = roles.First(), // Return the first role (as per your Login API)
//                    Roles = roles.ToList(), // Return all roles for completeness
//                    TokenValid = true
//                });
//            }
//            catch (SecurityTokenExpiredException)
//            {
//                return Unauthorized(new { message = "Token has expired." });
//            }
//            catch (SecurityTokenException ex)
//            {
//                return Unauthorized(new { message = "Invalid token.", error = ex.Message });
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { message = "An error occurred while validating the token.", error = ex.Message });
//            }
//        }

//        [HttpPost("check-email")]
//        public async Task<IActionResult> CheckEmail([FromBody] CheckEmailRequest request)
//        {
//            if (string.IsNullOrWhiteSpace(request.Email))
//            {
//                return BadRequest(new { Message = "Email is required." });
//            }

//            var user = await _userManager.Users
//                .FirstOrDefaultAsync(u => u.Email == request.Email);

//            if (user == null)
//            {
//                return NotFound(new { Message = "Email not found." });
//            }

//            return Ok(new { Message = "Email found." });
//        }

//        [HttpPost("send-otp")]
//        public async Task<IActionResult> SendOtp([FromBody] SendOtpRequest request)
//        {
//            if (string.IsNullOrWhiteSpace(request.Email))
//            {
//                return BadRequest(new { Message = "Email is required." });
//            }

//            var user = await _userManager.Users
//                .FirstOrDefaultAsync(u => u.Email == request.Email);

//            if (user == null)
//            {
//                return NotFound(new { Message = "Email not found." });
//            }

//            // Generate a 6-digit OTP
//            var otpCode = new Random().Next(100000, 999999).ToString();

//            // Save OTP to database
//            var otp = new Otp
//            {
//                Email = request.Email,
//                Code = otpCode,
//                CreatedAt = DateTime.Now,
//                ExpiresAt = DateTime.Now.AddMinutes(10), // OTP valid for 10 minutes
//                IsUsed = false
//            };
//            _context.Otps.Add(otp);
//            await _context.SaveChangesAsync();

//            // Send email with OTP
//            var emailBody = $@"
//        <h2>Password Reset Request</h2>
//        <p>Dear {user.FirstName},</p>
//        <p>We received a request to reset your password. Use the following OTP to proceed:</p>
//        <h3>{otpCode}</h3>
//        <p>This OTP is valid for 10 minutes. If you did not request this, please ignore this email.</p>
//        <p>Best regards,<br>BAKERTILLY DMS Team</p>
//    ";

//            await SendEmailAsync(request.Email, "Password Reset OTP", emailBody);

//            return Ok(new { Message = "OTP sent to your email." });
//        }

//        public class SendOtpRequest
//        {
//            public string Email { get; set; }
//        }

//    }
//}
////dotnet ef migrations add addDate --context AuditPilot.Data.ApplicationDbContext --startup-project .\WebApplication1\ --project .\AuditPilot.Data\
////dotnet ef database update --context AuditPilot.Data.ApplicationDbContext --startup-project .\WebApplication1\ --project .\AuditPilot.Data\


