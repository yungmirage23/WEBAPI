using System.Text;
using ClassLibrary.Models;
using ClassLibrary.Models.JwtToken;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RestWebAppl.Models;

var builder = WebApplication.CreateBuilder(args);

var ConnectionString = builder.Configuration.GetConnectionString("DefaultConnectionString");
var IdentityConnectionString = builder.Configuration.GetConnectionString("IdentityConnectionString");

// Add services to the container.

// Added DbContexts | AppIdentityDbContext= UsersDB, ApplicationDbContext= Orders and Items DB
builder.Services.AddDbContext<AppIdentityDbContext>(opts => opts.UseSqlServer(IdentityConnectionString));
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(ConnectionString));
builder.Services.AddScoped<IRepository, EFRepository>();


builder.Services.AddControllers(options =>
    {
        options.EnableEndpointRouting = false;
        var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
        options.Filters.Add(new AuthorizeFilter(policy));
    }
);
var build = builder.Services.AddIdentityCore<ApplicationUser>();
var identiyBuilder = new IdentityBuilder(build.UserType, build.Services);
identiyBuilder.AddEntityFrameworkStores<AppIdentityDbContext>();
identiyBuilder.AddSignInManager<SignInManager<ApplicationUser>>();

var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("super secret key"));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(opts =>
{
    opts.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = key,
        ValidateAudience = false,
        ValidateIssuer = false
    };
});
builder.Services.AddScoped<IJwtGenerator, JwtGenerator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
    endpoints.MapControllers()
);
app.Run();
