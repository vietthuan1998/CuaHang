using System.IO.Compression;
using System.Text;
using System.Threading.RateLimiting;
using CuaHang.Api.Entities;
using CuaHang.Api.Security;
using CuaHang.Api.Hubs;
using CuaHang.Api.Background;
using CuaHang.Api.Data;
using CuaHang.Api.Common;
using CuaHang.Api.Middleware;
using CuaHang.Api.Services;
using DotNetEnv;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

Env.Load();
var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ApiBehaviorOptions>(o => o.SuppressModelStateInvalidFilter = true);
// Add services to the container.
builder.Services.AddControllers(options =>
    options.Filters.Add(new AuthorizeFilter()))
    .AddNewtonsoftJson(o => CauHinhJson.ApDung(o.SerializerSettings));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập JWT access token. Ví dụ: eyJhbGciOi..."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddExceptionHandler<XuLyNgoaiLeHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddSignalR();
builder.Services.AddMemoryCache();
var jwt = builder.Configuration.GetSection(JwtCauHinh.SectionName).Get<JwtCauHinh>()
    ?? throw new InvalidOperationException("Thiếu cấu hình Jwt");
if (string.IsNullOrWhiteSpace(jwt.Issuer))
    throw new InvalidOperationException("Thiếu cấu hình Jwt:Issuer");
if (string.IsNullOrWhiteSpace(jwt.Audience))
    throw new InvalidOperationException("Thiếu cấu hình Jwt:Audience");
if (Encoding.UTF8.GetByteCount(jwt.SigningKey) < 32)
    throw new InvalidOperationException(
        "Jwt:SigningKey cần được cấu hình bằng user-secrets hoặc biến môi trường và dài tối thiểu 32 byte");
if (jwt.ThoiHanPhut is < 1 or > 60)
    throw new InvalidOperationException("Jwt:ThoiHanPhut phải nằm trong khoảng 1–60");
builder.Services.Configure<JwtCauHinh>(builder.Configuration.GetSection(JwtCauHinh.SectionName));
builder.Services.AddSingleton<IPasswordHasher<NguoiDung>, PasswordHasher<NguoiDung>>();
builder.Services.AddScoped<XacThucService>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
            RoleClaimType = System.Security.Claims.ClaimTypes.Role,
            NameClaimType = System.Security.Claims.ClaimTypes.Name
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken)
                    && context.Request.Path.StartsWithSegments("/hubs"))
                    context.Token = accessToken;
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization(options =>
    options.AddPolicy("DuocSuaSanPham", policy =>
        policy.RequireClaim("quyen", "sua_san_pham")));
var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
var normalizedCorsOrigins = corsOrigins.Select(origin =>
{
    if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri)
        || uri.Scheme is not ("http" or "https")
        || string.IsNullOrEmpty(uri.Host))
        throw new InvalidOperationException("Cors:AllowedOrigins chỉ nhận origin HTTP/HTTPS hợp lệ");
    return uri.GetLeftPart(UriPartial.Authority);
}).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
builder.Services.AddCors(options => options.AddPolicy("CuaHang", policy =>
    policy.WithOrigins(normalizedCorsOrigins).AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(["application/json"]);
});
builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
    options.Level = CompressionLevel.Fastest);
builder.Services.Configure<GzipCompressionProviderOptions>(options =>
    options.Level = CompressionLevel.Fastest);
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("dang-nhap", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            }));
});

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<SanPhamService>();
builder.Services.AddScoped<NhatKyService>();
builder.Services.AddScoped<BaoCaoService>();
builder.Services.AddScoped<SanPhamAnhService>();
builder.Services.AddScoped<HoaDonService>();
builder.Services.AddScoped<DonHangService>();
builder.Services.AddScoped<ThongBaoService>();
builder.Services.AddScoped<ThanhToanWebhookService>();
builder.Services.AddHttpClient("DichVuNgoai", client =>
    client.Timeout = TimeSpan.FromSeconds(10));
builder.Services.AddHttpClient("HopThuDi", client =>
    client.Timeout = TimeSpan.FromSeconds(10));
builder.Services.AddHostedService<DongBoService>();
builder.Services.AddHostedService<HopThuDiService>();

var app = builder.Build();

await TaoTaiKhoanMacDinhAsync(app);

// Configure the HTTP request pipeline.
app.UseExceptionHandler();
app.UseStatusCodePages(async statusContext =>
{
    var response = statusContext.HttpContext.Response;
    response.ContentType = "application/json; charset=utf-8";
    var loi = response.StatusCode switch
    {
        StatusCodes.Status401Unauthorized => "Chưa đăng nhập",
        StatusCodes.Status403Forbidden => "Không đủ quyền",
        StatusCodes.Status429TooManyRequests => "Quá nhiều yêu cầu",
        _ => "Yêu cầu không thành công"
    };
    var json = JsonConvert.SerializeObject(
        PhanHoi.Loi(loi),
        CauHinhJson.TaoSerializerSettings());
    await response.WriteAsync(json);
});
app.UseRouting();
app.UseCors("CuaHang");
app.UseResponseCompression();
app.UseStaticFiles();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ThongBaoHub>("/hubs/thong-bao");

app.Run();

static async Task TaoTaiKhoanMacDinhAsync(WebApplication app)
{
    var accounts = new[]
    {
        (Username: app.Configuration["Auth:AdminUsername"],
            Password: app.Configuration["Auth:AdminPassword"], Role: "QuanTri"),
        (Username: app.Configuration["Auth:StaffUsername"],
            Password: app.Configuration["Auth:StaffPassword"], Role: "NhanVien")
    };
    foreach (var account in accounts)
    {
        if (string.IsNullOrWhiteSpace(account.Username)
            && string.IsNullOrWhiteSpace(account.Password))
            continue;
        if (string.IsNullOrWhiteSpace(account.Username)
            || string.IsNullOrWhiteSpace(account.Password)
            || account.Password.Length < 12)
            throw new InvalidOperationException(
                "Tài khoản bootstrap phải có cả tên và mật khẩu tối thiểu 12 ký tự");

        await using var scope = app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        if (await db.NguoiDungs.AnyAsync(x => x.TenDangNhap == account.Username))
            continue;
        var user = new NguoiDung
        {
            TenDangNhap = account.Username,
            VaiTro = account.Role,
            HoatDong = true
        };
        user.MatKhauBam = scope.ServiceProvider
            .GetRequiredService<IPasswordHasher<NguoiDung>>()
            .HashPassword(user, account.Password);
        db.NguoiDungs.Add(user);
        await db.SaveChangesAsync();
    }
}

public partial class Program { }
