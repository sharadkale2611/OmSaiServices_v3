using LmsServices.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OmSaiEnvironment;
using OmSaiModels.Common;
using OmSaiModels.Worker;
using OmSaiServices.Worker.Implementations;
using OmSaiServices.Worker.Implimentation;
using SixLabors.ImageSharp;

namespace API.Controllers
{
	[Route("api/attendance")]
	//[ApiController]
	[Authorize]
	public class ApiAttendaceController : ControllerBase
	{
		private readonly WorkerService _workerService;
		private readonly WorkerAttendanceService _attendanceService;
		private readonly MultiMediaService _mms;

		public ApiAttendaceController()
		{
			_workerService = new WorkerService();
			_attendanceService = new WorkerAttendanceService();
			_mms = new MultiMediaService();

		}


		[HttpPost("worker-attendance-report")]
		public async Task<IActionResult> WorkerAttendanceReport([FromBody] WorkerAttendanceFilter request)
		{
			if (request == null)
			{
				return BadRequest("Invalid request payload");
			}
			// Validate the parameters
			// Validate that at least one of the parameters is provided
			if (!request.WorkerId.HasValue && !request.SiteId.HasValue && !request.CurrentDate.HasValue && !request.Month.HasValue && !request.Year.HasValue)
			{
				return BadRequest(new
				{
					success = false,
					data = (object)null,
					errors = new
					{
						message = "At least one parameter (WorkerId, SiteId, Month, Year or CurrentDate) must be provided."
					}
				});
			}

			try
			{
				// Retrieve attendance history from the service
				var attendanceHistory = _attendanceService.GetAll(request);

				// Return a successful response
				return Ok(new
				{
					success = true,
					data = attendanceHistory,
					errors = (object)null
				});
			}
			catch (Exception ex)
			{
				// Handle exceptions
				return StatusCode(500, new
				{
					success = false,
					data = (object)null,
					errors = new
					{
						message = "An error occurred while processing the request.",
						details = ex.Message
					}
				});
			}
		}

		[HttpPost("worker-selfie-attendance")]
		
		public async Task<IActionResult> WorkerSelfieAttendance([FromForm] WorkerAttendanceModel model, IFormFile SelfieImage)
		{
			try
			{
				// Validate the model
				if (!ModelState.IsValid)
				{
					var errors = ModelState
						   .Where(x => x.Value.Errors.Count > 0)
						   .ToDictionary(
							   kvp => kvp.Key,
							   kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
						   );

					var response = new
					{
						success = false,
						data = (object)null,
						errors
					};

					return BadRequest(response);

				}

				// Check if an image is provided
				if (SelfieImage == null || SelfieImage.Length == 0)
				{

					var errors = new
					{
						SelfieImage = new[] { "Selfie image is required!" }
					};
					return BadRequest(new ApiResponseModel<object>(false, null, errors));
				}

				try
				{
					// Image compression
					// var uploadPath = "selfies"; // Define your folder path
					// var maxFileSizeKb = 30; // 30KB size limit
					// var quality = 20;
					// var filePath = await _mms.SaveAndCompressImageAsync(SelfieImage, uploadPath, maxFileSizeKb, quality);


					var uploadUrl = $"{Urls.mvcURL}/api/external/upload-file"; // MVC endpoint to handle file saving

					using (var content = new MultipartFormDataContent())
					{

						var streamContent = new StreamContent(SelfieImage.OpenReadStream());
						content.Add(streamContent, "file", SelfieImage.FileName);

						// Add the path as StringContent
						var pathContent = new StringContent("/selfies");
						content.Add(pathContent, "path");

						using (var client = new HttpClient())
						{
							var response = await client.PostAsync(uploadUrl, content);
							if (response.IsSuccessStatusCode)
							{

								string selfieImage = await response.Content.ReadAsStringAsync();

								model.SelfieImage = selfieImage;

								_attendanceService.ManageAttendance(model);

								return Ok(new ApiResponseModel<object>(true, new
								{
									message = "Attendance processed successfully!",
									filePath = selfieImage
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
				catch (Exception ex)
				{

					var errors = new
					{
						SelfieImage = new[] { ex.Message }
					};
					return Unauthorized(new ApiResponseModel<object>(false, null, errors));
				}

			}
			catch (Exception ex)
			{
				var errors = new
				{
					SelfieImage = new[] { ex.Message }
				};
				return Unauthorized(new ApiResponseModel<object>(false, null, errors));
			}
		}


		[HttpGet("validate-attendance/{workmanId}")]
		public IActionResult ValidateAttendance(string workmanId)
		{
			try
			{
				var worker = _workerService.GetProfileById(null, workmanId);
				if (worker == null || workmanId == null)
				{
					return BadRequest(new
					{
						success = false,
						data = (object)null,
						errors = new
						{
							message = "Invalid Worker!"
						}
					});
				}

				ValidateAttendanceModel validateData = _attendanceService.ValidateAttendance(worker.WorkerId);


				// Return a successful response
				return Ok(new
				{
					success = true,
					data = validateData,
					errors = (object)null
				});

			}
			catch (Exception ex)
			{
				// Handle exceptions
				return StatusCode(500, new
				{
					success = false,
					data = (object)null,
					errors = new
					{
						message = "An error occurred while processing the request.",
						details = ex.Message
					}
				});
			}
		}
	}
}
