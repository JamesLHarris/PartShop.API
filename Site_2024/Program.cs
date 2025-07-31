using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;
using Site_2024.Web.Api.Data;
using Site_2024.Web.Api.Interfaces;
using Site_2024.Web.Api.Services;
using Site_2024.Web.Api.Models.User;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Add logging
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        builder => builder
            .WithOrigins("http://localhost:3000") // Adjust as per your frontend URL
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()); // Allow credentials if needed
});


// Configure Authentication with Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/api/user/login";  // Define login path
        options.Cookie.SameSite = SameSiteMode.None;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.LogoutPath = "/Account/Logout";  // Define logout path
        options.AccessDeniedPath = "/Account/AccessDenied";  // Define access denied path
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);  // Expire after 60 minutes
    });

// Configure DbContext and other services
string connString = builder.Configuration.GetConnectionString("connMSSQL");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connString));
builder.Services.AddSingleton<IDataProvider>(new DataProvider(connString));
builder.Services.AddSingleton<IPartService, PartService>();
builder.Services.AddSingleton<IAvailableService, AvailableService>();
builder.Services.AddSingleton<IModelService, ModelService>();
builder.Services.AddSingleton<IMakeService, MakeService>();
builder.Services.AddSingleton<ICatagoryService, CatagoryService>();
builder.Services.AddSingleton<ILocationService, LocationService>();
builder.Services.AddSingleton<ISiteService, SiteService>();
builder.Services.AddSingleton<IShelfService, ShelfService>();
builder.Services.AddSingleton<ISectionService, SectionService>();
builder.Services.AddSingleton<IBoxService, BoxService>();
builder.Services.AddSingleton<IAreaService, AreaService>();
builder.Services.AddSingleton<IAisleService, AisleService>();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddSingleton<IUserService, UserService>();
builder.Services.AddSingleton<IAuthenticationService<IUserAuthData>, AuthenticationService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Enable CORS in development
}

app.UseCors("CorsPolicy");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
