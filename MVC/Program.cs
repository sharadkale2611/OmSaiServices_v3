
using GeneralTemplate.Filter;
using GeneralTemplate.Middlewares;
using Microsoft.EntityFrameworkCore;
using OmSaiEnvironment;
using OmSaiServices_v3.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(DBConnection.DefaultConnection));



//builder.Services.AddDistributedMemoryCache(); // For session

builder.Services.AddDistributedSqlServerCache(options =>
{
	options.ConnectionString = DBConnection.DefaultConnection;
	options.SchemaName = "dbo";
	options.TableName = "SessionCache";
});



builder.Services.AddSession(options =>
{
	options.IdleTimeout = TimeSpan.FromMinutes(60); // Set session timeout
	options.Cookie.HttpOnly = true;
	options.Cookie.IsEssential = true;
	options.Cookie.SameSite = SameSiteMode.None; // Allows cross-origin requests
	options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Ensures cookies are sent over HTTPS
	options.Cookie.MaxAge = TimeSpan.FromMinutes(60); // Ensure MaxAge is set
});

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
	// Add global filters
	options.Filters.Add<CustomErrorFilter>();
	options.Filters.Add<SessionInfoFilter>();
}).AddRazorRuntimeCompilation();



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

app.UseRouting();
app.UseSession(); // Enable session middleware
app.UseCustomErrorMiddleware(); // Add this line before other middlewares


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

