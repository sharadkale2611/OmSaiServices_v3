using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OmSaiModels.Common;
using OmSaiModels.Worker;
using OmSaiServices.Worker.Implementations;
using OmSaiServices.Worker.Implimentation;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OmSaiServices_v3.Areas.Worker.Controllers
{
	[Route("api/external")]
	//[ApiController]
	public class ApiExternalController : ControllerBase
	{

		private readonly WorkerService _workerService;


		public ApiExternalController()
		{
			_workerService = new WorkerService();
		}

		[HttpPost("upload-file")]
		public IActionResult UploadFile([FromForm] IFormFile? file, [FromForm] string path= "")
		{
			try
			{
				string uploadPath = "media"+ path;

				if (file != null && file.Length > 0)
				{
					var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", uploadPath);

					if (!Directory.Exists(filePath))
						Directory.CreateDirectory(filePath);

					var uniqueFileName = $"{Path.GetFileNameWithoutExtension(file.FileName)}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
					var completeFilePath = Path.Combine(filePath, uniqueFileName);

					using (var stream = new FileStream(completeFilePath, FileMode.Create))
					{
						file.CopyTo(stream);
					}

					string finalImagePath = $"{uploadPath}/{uniqueFileName}";

					return Ok(finalImagePath);
				}
				else
				{
					return BadRequest("File not found!");
				}

			}
			catch (Exception ex)
			{
				var errors = new
				{

					message = $"An error occurred: {ex.Message}"

				};

				return BadRequest(new ApiResponseModel<object>(false, null, errors));

			}


		}



	}
}
