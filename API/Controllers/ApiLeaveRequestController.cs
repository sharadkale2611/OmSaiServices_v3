using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OmSaiModels.Common;
using OmSaiModels.Worker;
using OmSaiServices.Admin.Implementations;
using OmSaiServices.Worker.Implementations;
using OmSaiServices.Worker.Implimentation;

namespace API.Controllers
{
	[Route("api/worker-leave")]
	//[ApiController]
	[Authorize]
	public class ApiLeaveRequestController : ControllerBase
	{
		private readonly LeaveRequestService _leaveRequestService;
		private readonly LeaveTypeService _leaveTypeservice;
		private readonly WorkerService _workerservice;

		public ApiLeaveRequestController()
		{
			_leaveRequestService = new LeaveRequestService();
			_workerservice = new WorkerService();
			_leaveTypeservice = new LeaveTypeService();
		}

		//exec usp_GetAll_LeaveTypes

		[HttpGet("get-all-leave-types")]
		public IActionResult GetAllLeaveTypes()
		{
			var request = _leaveRequestService.GetAllLeaveTypes();
			if (request == null)
			{
				var errors = new
				{
					LeaveRequestId = new[] { "Leave Types not found!" }
				};
				return BadRequest(new ApiResponseModel<object>(false, null, errors));
			}
			//var type = _leaveTypeservice.GetById(id);

			return Ok(new ApiResponseModel<object>(true, new
			{
				request,
				//type,
			}, null));
		}

		[HttpGet("get-all-leaves")]
		public IActionResult GetAllLeaveRequestId()
		{
			var request = _leaveRequestService.GetAll();
			if (request == null)
			{
				var errors = new
				{
					LeaveRequestId = new[] { "LeaveRequestId not found!" }
				};
				return BadRequest(new ApiResponseModel<object>(false, null, errors));
			}
			//var type = _leaveTypeservice.GetById(id);

			return Ok(new ApiResponseModel<object>(true, new
			{
				request,
				//type,
			}, null));

			return Ok(new { message = "This is a protected id" });
		}

		[HttpGet("get-by-worker-id/{id}")]
		//[Authorize(AuthenticationSchemes = "Jwt")]
		public IActionResult GetByWorkerId(int id)
		{
			var worker = _leaveRequestService.GetAllByWorkerId(id);
			if (worker == null)
			{
				var errors = new
				{
					WorkerId = new[] { "Worker not found!" }
				};
				return BadRequest(new ApiResponseModel<object>(false, null, errors));
			}
			//var documents = _workerDocumentService.GetAll(id);
			//var address = _workerAddressService.GetByWorkerId(id);

			return Ok(new ApiResponseModel<object>(true, new
			{
				worker
				//address,
				//documents,
			}, null));

			return Ok(new { message = "This is a protected id" });
		}

		[HttpGet("get-by-id/{id}")]
		//[Authorize(AuthenticationSchemes = "Jwt")]
		public IActionResult GetById(int id)
		{
			var worker = _leaveRequestService.GetById(id);
			if (worker == null)
			{
				var errors = new
				{
					WorkerId = new[] { "Worker not found!" }
				};
				return BadRequest(new ApiResponseModel<object>(false, null, errors));
			}
			//var documents = _workerDocumentService.GetAll(id);
			//var address = _workerAddressService.GetByWorkerId(id);

			return Ok(new ApiResponseModel<object>(true, new
			{
				worker
				//address,
				//documents,
			}, null));

			return Ok(new { message = "This is a protected id" });
		}

		[HttpPost]
		[Route("create-leave-request")]
		public async Task<IActionResult> CreateLeaveRequest([FromBody] LeaveRequestModel model)
		{
			try
			{
				if (ModelState.IsValid)
				{
					var result = await _leaveRequestService.Create(model);
					return Ok(new ApiResponseModel<object>(true, result, null));
				}
				else
				{
					var errors = ModelState.ToDictionary(
							   key => key.Key,
							   value => value.Value.Errors.Select(e => e.ErrorMessage).ToArray()
						   );

					return BadRequest(new ApiResponseModel<object>(false, null, errors));
				}
			}
			catch (Exception ex)
			{
				var errors = new
				{
					Message = new[] { ex.Message },
				};
				return BadRequest(new ApiResponseModel<object>(false, null, errors));
			}
		}


		[HttpDelete]
		[Route("soft-delete-leave-request/{id}")]
		public async Task<IActionResult> SoftDeleteLeaveRequest(int id)
		{
			try
			{
				var leaveRequest = _leaveRequestService.GetById(id);
				if (leaveRequest == null)
				{

				}
				if (ModelState.IsValid)
				{
					_leaveRequestService.Delete(id);
					return Ok(new ApiResponseModel<object>(true, id, null));
				}
				else
				{
					var errors = ModelState.ToDictionary(
							   key => key.Key,
							   value => value.Value.Errors.Select(e => e.ErrorMessage).ToArray()
						   );

					return BadRequest(new ApiResponseModel<object>(false, null, errors));
				}
			}
			catch (Exception ex)
			{
				var errors = new
				{
					Message = new[] { ex.Message },
				};
				return BadRequest(new ApiResponseModel<object>(false, null, errors));
			}
		}

		[HttpPost]
		[Route("update-leave-request")]
		//[Authorize(AuthenticationSchemes = "Jwt")]
		public async Task<IActionResult> UpdateLeaveRequest([FromBody] LeaveRequestModel model)
		{
			try
			{
				if (ModelState.IsValid)
				{

					_leaveRequestService.Update(model);
					//return Ok(new
					//{
					//	success = true,
					//	message = "Record updated successfully!"
					//});
					return Ok(new ApiResponseModel<object>(true, new
					{
						message = "Record updated successfully!"
					}, null));
				}
				else
				{
					var errors = new
					{
						LeaveRequestId = new[] { "Leave Request not found!" }
					};
					return BadRequest(new ApiResponseModel<object>(false, null, errors));
					//return BadRequest(new
					//{
					//	success = false,
					//	errors = errorMessages
					//});
				}
			}
			catch (Exception ex)
			{
				var errors = new
				{
					Message = new[] { ex.Message },
				};
				return BadRequest(new ApiResponseModel<object>(false, null, errors));
			}
		}

		[HttpPost]
		[Route("delete-leave-request/{id}")]
		public IActionResult DeleteLeaveRequest(int id)
		{
			try
			{
				var leaveRequest = _leaveRequestService.GetById(id);

				if (leaveRequest == null)
				{
					var errors = new
					{
						LeaveRequestId = new[] { "Leave Request not found!" }
					};
					return BadRequest(new ApiResponseModel<object>(false, null, errors));
				}

				if (leaveRequest.Status == "Pending")  // Check if the status is 'Pending'
				{
					// Proceed with deletion
					_leaveRequestService.Delete(id);
					return Ok(new ApiResponseModel<object>(true, new
					{
						message = "Leave request deleted successfully!"
					}, null));
				}
				else
				{
					// If the leave request status is not 'Pending', return an error
					var errors = new
					{
						Status = new[] { "Cannot delete processed or approved leave request." }
					};
					return BadRequest(new ApiResponseModel<object>(false, null, errors));
				}
			}
			catch (Exception ex)
			{
				var errors = new
				{
					Message = new[] { ex.Message },
				};
				return BadRequest(new ApiResponseModel<object>(false, null, errors));
			}
		}

	}
}
