using GeneralTemplate.Filter;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OmSaiEnvironment;
using OmSaiServices_v3.Data;
using System;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(DBConnection.DefaultConnection));



// Add services to the container.
builder.Services.AddControllersWithViews();


builder.Services.AddAuthorization(options =>
{
	options.FallbackPolicy = null; // Allow unauthenticated access by default
});


// Add services to the container.
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

builder.Services.AddControllersWithViews(options =>
{
	// Add the filter globally
	options.Filters.Add<SessionInfoFilter>();
	options.Filters.Add<CustomErrorFilter>();
});


// Add JWT Authentication
//// JWT Authentication setup
builder.Services.AddAuthentication(options =>
{
	// Set default scheme to Identity (if you use it in your app)
	options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
	options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
})
.AddJwtBearer("Jwt", options =>
{
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = true,
		ValidateAudience = true,
		ValidateLifetime = true,
		ValidateIssuerSigningKey = true,
		ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
		ValidAudience = builder.Configuration["JwtSettings:Audience"],
		IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"])) // Use the key from appsettings.json
	};
});


builder.Services.AddDistributedMemoryCache(); // For session
builder.Services.AddSession(options =>
{
	options.IdleTimeout = TimeSpan.FromMinutes(60); // Set session timeout
	options.Cookie.HttpOnly = true;
	options.Cookie.IsEssential = true;
	options.Cookie.SameSite = SameSiteMode.None; // Allows cross-origin requests
	options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Ensures cookies are sent over HTTPS

});


// Register CORS service
builder.Services.AddCors(options =>
{
	//options.AddPolicy("AllowMyApp", policy =>
	//{
	//	policy.WithOrigins("http://localhost:3000", "file://").AllowAnyMethod().AllowAnyHeader();
	//});

	options.AddPolicy("AllowAllOrigins", policy =>
	{
		policy.AllowAnyOrigin()      // Allows all origins
			  .AllowAnyMethod()     // Allows all HTTP methods (GET, POST, etc.)
			  .AllowAnyHeader();    // Allows all headers
	});
});

builder.Services.ConfigureApplicationCookie(options =>
{
	options.Cookie.SameSite = SameSiteMode.None; // Allows cross-origin requests
	options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Ensures cookies are sent over HTTPS
	options.AccessDeniedPath = "/Account/AccessDenied"; // Redirect path for access denied
});




var app = builder.Build();



// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Configure the HTTP request pipeline.
app.UseCors("AllowAllOrigins");  // Use the CORS policy



app.UseHttpsRedirection();
app.UseStaticFiles();

//app.MapRazorPages();


app.UseRouting();

// Add session middleware BEFORE endpoints
app.UseSession();



app.UseAuthentication(); // Add authentication middleware
app.UseAuthorization();  // Add authorization middleware


app.UseEndpoints(endpoints =>
{
	endpoints.MapControllerRoute(
	  name: "areas",
	  pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
	);
});

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

