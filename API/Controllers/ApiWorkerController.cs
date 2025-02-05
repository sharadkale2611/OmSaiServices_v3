using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OmSaiEnvironment;
using OmSaiModels.Common;
using OmSaiModels.Worker;
using OmSaiServices.Worker.Implementations;
using OmSaiServices.Worker.Implimentation;
using System.IO;

namespace API.Controllers
{
	[Route("api/worker")]
	//[ApiController]
	public class ApiWorkerController : ControllerBase
	{
		private readonly WorkerService _workerService;
		private readonly WorkerAddressService _workerAddressService;
		private readonly WorkerMobileNumbersService _workerMobileNumbersService;
		private readonly WorkerDocumentService _workerDocumentService;


		public ApiWorkerController()
		{
			_workerService = new WorkerService();
			_workerAddressService = new WorkerAddressService();
			_workerMobileNumbersService = new WorkerMobileNumbersService();
			_workerDocumentService = new WorkerDocumentService();

		}

		[HttpGet("get-site-shift/{id}")]
		[Authorize]
		public async Task<IActionResult> GetSiteShift(int id)
		{
			try
			{

				var worker = await _workerService.GetSiteShifts(id);
				if (worker == null)
				{
					var errors = new
					{
						WorkmanId = new[] { "Worker Site Shift not found!" }
					};
					return BadRequest(new ApiResponseModel<object>(false, null, errors));
				}

				return Ok(new ApiResponseModel<object>(true, worker, null));

			}
			catch (Exception ex)
			{
				var errors = new
				{
					message = new[] { ex.Message }
				};

				return BadRequest(new ApiResponseModel<object>(false, null, errors));

			}

		}


		[HttpGet("profile/{id}")]
		[Authorize]
		public IActionResult GetProfile(int id)
		{
			var worker = _workerService.GetProfileById(id, null);
			if (worker == null)
			{
				var errors = new
				{
					WorkmanId = new[] { "Worker not found!" }
				};
				return Unauthorized(new ApiResponseModel<object>(false, null, errors));
			}
			var documents = _workerDocumentService.GetAll(id);
			var address = _workerAddressService.GetByWorkerId(id);

			return Ok(new ApiResponseModel<object>(true, new
			{
				worker,
				address,
				documents,
			}, null));

		}

		[HttpPost("upload-profile-image")]
		[Authorize]
		public async Task<IActionResult> UploadProfileImage([FromForm] IFormFile? file, [FromForm] int workerId, [FromForm] string? profileImage = "")
		{
			string uploadPath = "media/profile_images";

			var model = new ProfileImageModel
			{
				WorkerId = workerId,
				ProfileImage = profileImage
			};

			try
			{
				var worker = _workerService.GetById(workerId);
				if (worker == null)
				{
					var errors = new
					{
						WorkerId = new[] { "Worker not found!" },
						File = file == null ? new[] { "No file uploaded!" } : null,
						ProfileImage = string.IsNullOrEmpty(profileImage) ? new[] { "Profile image path is missing!" } : null
					};

					return BadRequest(new ApiResponseModel<object>(false, null, errors));
				}

				if (file != null && file.Length > 0)
				{
					var uploadUrl = $"{Urls.mvcURL}/api/external/upload-file"; // MVC endpoint to handle file saving

					using (var content = new MultipartFormDataContent())
					{
						var streamContent = new StreamContent(file.OpenReadStream());
						content.Add(streamContent, "file", file.FileName);

						// Add the path as StringContent
						var pathContent = new StringContent("/profile_images");
						content.Add(pathContent, "path");


						using (var client = new HttpClient())
						{
							var response = await client.PostAsync(uploadUrl, content);
							if (response.IsSuccessStatusCode)
							{

								string ProfileImage = await response.Content.ReadAsStringAsync();

								_workerService.UploadProfileImage(workerId, ProfileImage);

								return Ok(new ApiResponseModel<object>(true, new
								{
									message = "Profile image uploaded successfully!",
									filePath = ProfileImage
								}, null));
							}
							else
							{
								var errors = new
								{
									message = $"An error occurred: {await response.Content.ReadAsStringAsync()}"
								};

								return BadRequest(new ApiResponseModel<object>(false, null, errors));
							}
						}
					}

				}
				else
				{
					var errors = new
					{
						File = file == null ? new[] { "Image not found!" } : null,
					};

					return BadRequest(new ApiResponseModel<object>(false, null, errors));

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


		[HttpPost("upload-document")]
		public IActionResult UploadDocument([FromForm] IFormFile? file, [FromForm] int WorkerId, [FromForm] int DocumentId, [FromForm] string? documentNumber = "", string? documentImage = null)
		{
			string uploadPath = "media/documents";

			var model = new WorkerDocumentModel
			{
				DocumentId = DocumentId,
				DocumentNumber = documentNumber,
				WorkerId = WorkerId

			};
			try
			{
				var worker = _workerService.GetById(WorkerId);
				if (worker == null)
				{
					var errors = new
					{
						WorkerId = new[] { "Worker not found!" },
						File = file == null ? new[] { "No file uploaded!" } : null,
						DocumentId = DocumentId <= 0 ? new[] { "Invalid DocumentId!" } : null
					};
					return BadRequest(new ApiResponseModel<object>(false, null, errors));


				}
				if (file != null && file.Length > 0)
				{

					var documentPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", uploadPath);


					if (!Directory.Exists(documentPath))
						Directory.CreateDirectory(documentPath);


					var uniqueFileName = $"{Path.GetFileNameWithoutExtension(file.FileName)}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
					var filePath = Path.Combine(documentPath, uniqueFileName);


					using (var stream = new FileStream(filePath, FileMode.Create))
					{
						file.CopyTo(stream);
					}
					model.DocumentImage = $"{uploadPath}/{uniqueFileName}";
					_workerDocumentService.Update(model);

					return Ok(new ApiResponseModel<object>(true, new

					{

						message = "Document uploaded successfully!",
						filePath = $"{uploadPath}/{uniqueFileName}"
					}, null));
				}
				else
				{
					model.DocumentImage = documentImage;

					_workerDocumentService.Update(model);
					return Ok(new ApiResponseModel<object>(true, new

					{

						message = "Document updated successfully!",
						filePath = documentImage
					}, null));


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
