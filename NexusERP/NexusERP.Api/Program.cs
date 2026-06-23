using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NexusERP.Api.Middleware;
using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Application.Interfaces.Services;
using NexusERP.Infrastructure.Database;
using NexusERP.Infrastructure.Repositories;
using NexusERP.Infrastructure.Services;
using System.Text;
using NexusERP.Domain.Constants;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using DocumentFormat.OpenXml.InkML;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("InventoryDb");

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddDbContext<ApplicationDbContext>(options => 
    options.UseSqlServer(connectionString, x => x.UseNetTopologySuite()));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IAuditRepository, AuditRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IAbsenceRepository, AbsenceRepository>();
builder.Services.AddScoped<IStoreRepository, StoreRepository>();

builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IAbsenceService, AbsenceService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IExcelExportService, ExcelExportService>();
builder.Services.AddScoped<IPdfExportService, PdfExportService>();
builder.Services.AddScoped<IStoreService, StoreService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key is misssing in the appsettings.json");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
            };
        });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireProductUpsert", policy => policy.RequireClaim("Permission", Permissions.UpsertProducts));
    options.AddPolicy("RequireProductDelete", policy => policy.RequireClaim("Permission", Permissions.DeleteProducts));

    options.AddPolicy("RequireManageCategories", policy => policy.RequireClaim("Permission", Permissions.ManageCategories));
    options.AddPolicy("RequireManageSuppliers", policy => policy.RequireClaim("Permission", Permissions.ManageSuppliers));

    options.AddPolicy("RequireManageUsers", policy => policy.RequireClaim("Permission", Permissions.ManageUsers));
    options.AddPolicy("RequireViewAuditLogs", policy => policy.RequireClaim("Permission", Permissions.ViewAuditLogs));
    options.AddPolicy("RequireViewDashboard", policy => policy.RequireClaim("Permission", Permissions.ViewDashboard));

    options.AddPolicy("RequireExportExcel", policy => policy.RequireClaim("Permission", Permissions.ExportExcelTransactions));

    options.AddPolicy("RequireAbsenceManage", policy => policy.RequireClaim("Permission", Permissions.ManageAbsences));
});


builder.Services.AddMemoryCache();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.OnRejected = async (context, token) =>
    {
        var ip = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown_ip";

        var cache = context.HttpContext.RequestServices.GetRequiredService<IMemoryCache>();

        var strikes = cache.GetOrCreate($"Strikes_{ip}", entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
            return 0;
        });

        strikes++;

        if (strikes >= 5)
        {
            cache.Set($"Banned_{ip}", true, TimeSpan.FromHours(24));
        }
        else
        {
            cache.Set($"Strikes_{ip}", strikes, TimeSpan.FromHours(1));
        }

        await context.HttpContext.Response.WriteAsync("Rate limit exceeded.");
    };

    options.AddPolicy("GlobalPolicy", HttpContent =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: HttpContent.Connection.RemoteIpAddress?.ToString() ?? "unknown_ip",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 2,
            }));


    options.AddPolicy("AuthPolicy", HttpContent =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: HttpContent.Connection.RemoteIpAddress?.ToString() ?? "unknown_ip",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(15),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0,
            }));

});

var app = builder.Build();

app.UseRouting();

app.UseMiddleware<ExceptionMiddleware>();

app.UseMiddleware<IpBanningMiddleware>();

app.UseRateLimiter();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers().RequireRateLimiting("GlobalPolicy");

app.Run();
