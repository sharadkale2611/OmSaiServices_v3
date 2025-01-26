using System;
using System.IO;
using System.Text;

namespace GeneralTemplate.Filter
{
	public class FileLogger
	{
		public static void LogError(string errorMessage)
		{
			try
			{
				// Define the log directory and file name
				string logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
				string logFileName = $"ErrorLog_{DateTime.Now:yyyy-MM-dd-HH}.txt";
				string logFilePath = Path.Combine(logDirectory, logFileName);

				// Ensure the directory exists
				if (!Directory.Exists(logDirectory))
				{
					Directory.CreateDirectory(logDirectory);
				}

				// Create or append to the log file
				using (var streamWriter = new StreamWriter(logFilePath, true, Encoding.UTF8))
				{
					streamWriter.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {errorMessage}");
				}
			}
			catch (Exception ex)
			{
				// Optionally handle logging errors (e.g., log to a fallback location)
				Console.WriteLine($"Failed to log error: {ex.Message}");
			}
		}
	}
}
