using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(jwtOptions =>
{
	jwtOptions.TokenValidationParameters = new TokenValidationParameters
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
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configure the HTTP request pipeline.
app.UseCors("AllowAllOrigins");  // Use the CORS policy


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
