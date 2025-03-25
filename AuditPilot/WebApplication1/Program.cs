using AuditPilot.Data;
using AuditPilot.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using AuthPilot.Models.AutoMapper;
using AuditPilot.API.Helpers;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using AuditPilot.Repositories.Interfaces;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddHttpContextAccessor();

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton(provider =>
{

    UserCredential credential;

    // Load credentials from the downloaded credentials.json file
    var credentialsPath = "C:\\google\\secret.json";
    //var credentialsPath = "D:\\Taqi\\secret.json";
    using (var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read))
    {
        string credPath = "token.json";
        credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
            GoogleClientSecrets.FromStream(stream).Secrets,
            new List<string>() { DriveService.Scope.Drive },
            "user",
            CancellationToken.None,
            new FileDataStore(credPath, true)).Result;

        Console.WriteLine("Credential file saved to: " + credPath);
    }

    var service = new DriveService(new BaseClientService.Initializer()
    {
        HttpClientInitializer = credential,
        ApplicationName = "DesktopClient1",
    });

    return service;
});

builder.Services.AddSingleton<GoogleDriveHelper>();
builder.Services.AddScoped<IGoogleDriveItemRepository,GoogleDriveItemRepository>();
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IClientProjectRepository, ClientProjectRepository>();
builder.Services.AddScoped<IFolderStructureRepository, FolderStructureRepository>();



builder.Services.AddAutoMapper(typeof(MappingProfile));

// Configure JWT authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            Console.WriteLine("Token validated successfully.");
            var claimsIdentity = context.Principal.Identity as ClaimsIdentity;
            if (claimsIdentity != null)
            {
                foreach (var claim in claimsIdentity.Claims)
                {
                    Console.WriteLine($"Claim Type: {claim.Type}, Claim Value: {claim.Value}");
                }
            }
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"Authentication failed: {context.Exception.Message}");
            return Task.CompletedTask;
        }
    };
});


builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Audit Pilot", Version = "v1" });

    // Define the security scheme
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your token in the text input below.\n\nExample: 'Bearer abc123xyz'"
    });

    // Apply the security scheme globally
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

var httpContextAccessor = app.Services.GetRequiredService<IHttpContextAccessor>();
SessionHelper.Configure(httpContextAccessor);

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate(); // Applies any pending migrations
}

// Apply CORS globally
app.UseCors("AllowAll");
// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Audit Pilot v1");
    });
}

app.UseHttpsRedirection();

builder.Services.AddAuthorization();
app.UseAuthentication();  // Use authentication middleware
app.UseAuthorization();   // Use authorization middleware

app.MapControllers();

app.Run();
