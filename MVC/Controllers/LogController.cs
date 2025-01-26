using Microsoft.AspNetCore.Mvc;

namespace OmSaiServices_v3.Controllers
{
	public class LogController : Controller
	{
		private readonly string logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Logs"); // Log folder path

		// Action to display the list of log files
		public IActionResult Index()
		{
			// Ensure the log directory exists
			if (!Directory.Exists(logDirectory))
			{
				Directory.CreateDirectory(logDirectory);
			}

			// Get all log files in the directory
			var logFiles = Directory.GetFiles(logDirectory, "*.txt") // Adjust file extension if needed
									.Select(file => Path.GetFileName(file)) // Get file names only
									.ToList();

			return View(logFiles); // Pass the log file names to the view
		}

		// Action to display the contents of a log file
		public IActionResult ViewLog(string fileName)
		{
			// Ensure the file exists
			var filePath = Path.Combine(logDirectory, fileName);

			if (!System.IO.File.Exists(filePath))
			{
				return NotFound(); // Return a 404 if the file doesn't exist
			}

			// Read the content of the log file
			var fileContent = System.IO.File.ReadAllText(filePath);

			return View((object)fileContent); // Pass the file content to the view
		}

		// Action to delete a log file
		[HttpPost]
		public IActionResult DeleteLog(string fileName)
		{
			var filePath = Path.Combine(logDirectory, fileName);

			if (System.IO.File.Exists(filePath))
			{
				System.IO.File.Delete(filePath); // Delete the file
				TempData["Message"] = $"Log file '{fileName}' has been deleted."; // Store a success message
			}
			else
			{
				TempData["Message"] = $"Log file '{fileName}' not found."; // Store an error message
			}

			return RedirectToAction("Index"); // Redirect back to the list of log files
		}

	}
}
