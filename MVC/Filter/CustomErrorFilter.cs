using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace GeneralTemplate.Filter
{
	public class CustomErrorFilter: IExceptionFilter
	{
		public void OnException(ExceptionContext context)
		{

			//var exception = context.Exception;
			//FileLogger.LogError($"Unhandled Exception: {exception.Message}");
			//FileLogger.LogError($"Unhandled Exception: {exception.Message} | StackTrace: {exception.StackTrace}");


			var exception = context.Exception;

			// Get the request URL
			var requestUrl = context.HttpContext.Request.Path;

			// Get the client IP address
			var clientIp = context.HttpContext.Connection.RemoteIpAddress?.ToString();

			// Log the error with the exception details, request URL, and client IP address
			FileLogger.LogError($"Unhandled Exception: {exception.Message} | Request URL: {requestUrl} | Client IP: {clientIp}");

			// Optional: Log the stack trace if needed
			// FileLogger.LogError($"Unhandled Exception: {exception.Message} | StackTrace: {exception.StackTrace}");

			//ViewData["Error"] = "Oops! Something went wrong.\r\nWe're experiencing an issue while processing your request. Our team is already working on it. Please try again later, and we appreciate your patience.";


			if (context.HttpContext.Response.StatusCode == 400 || context.HttpContext.Response.StatusCode == 401 || context.HttpContext.Response.StatusCode == 403)
			{
				// Ensure session is available before clearing
				if (context.HttpContext.Session != null)
				{
					context.HttpContext.Session.Clear();
				}
				context.Result = new RedirectToActionResult("Login", "Account", null); // Adjust to your login controller/action
				context.ExceptionHandled = true;
			}
		}
	}
}
