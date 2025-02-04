using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace GeneralTemplate.Middlewares
{
	public class CustomErrorMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ILogger<CustomErrorMiddleware> _logger;

		public CustomErrorMiddleware(RequestDelegate next, ILogger<CustomErrorMiddleware> logger)
		{
			_next = next;
			_logger = logger;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			try
			{
				await _next(context);
			}
			catch (Exception ex)
			{
				await HandleExceptionAsync(context, ex);
			}
		}

		private async Task HandleExceptionAsync(HttpContext context, Exception exception)
		{
			var requestUrl = context.Request.Path;
			var clientIp = context.Connection.RemoteIpAddress?.ToString();
			string errorMessage = $"Unhandled Exception: {exception.Message} | Request URL: {requestUrl} | Client IP: {clientIp}";

			FileLogger.LogError(errorMessage);
			_logger.LogError(exception, errorMessage);
			context.Session.Clear();
			context.Session.SetString("ErrorMessage", exception.Message);

			context.Response.Redirect("/Account/Login");
		}
	}

	public static class CustomErrorMiddlewareExtensions
	{
		public static IApplicationBuilder UseCustomErrorMiddleware(this IApplicationBuilder builder)
		{
			return builder.UseMiddleware<CustomErrorMiddleware>();
		}
	}

	public class FileLogger
	{
		public static void LogError(string errorMessage)
		{
			try
			{
				string logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
				string logFileName = $"ErrorLog_{DateTime.Now:yyyy-MM-dd-HH}.txt";
				string logFilePath = Path.Combine(logDirectory, logFileName);

				if (!Directory.Exists(logDirectory))
				{
					Directory.CreateDirectory(logDirectory);
				}

				using (var streamWriter = new StreamWriter(logFilePath, true, Encoding.UTF8))
				{
					streamWriter.WriteLine();
					streamWriter.WriteLine();
					streamWriter.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {errorMessage}");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Failed to log error: {ex.Message}");
			}
		}
	}
}
