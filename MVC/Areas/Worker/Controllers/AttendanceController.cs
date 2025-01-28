using Microsoft.AspNetCore.Mvc;
using OmSaiModels.Worker;
using OmSaiServices.Admin.Implementations;
using OmSaiServices.Worker.Implementations;
using OmSaiServices.Worker.Implimentation;
using LmsServices.Common;
using Microsoft.AspNetCore.Http;
using OmSaiServices.Admin.Interfaces;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace GeneralTemplate.Areas.Worker.Controllers
{
	[Area("Worker")]
	public class AttendanceController : Controller
	{
		private readonly WorkerService _workerService;
		private readonly WorkerAttendanceService _attendanceService;
		private readonly MultiMediaService _mms;
		private readonly SiteService _siteService;
		private readonly SiteShiftService _siteShiftService;
		private readonly DashboardService _dashboardService;



		public AttendanceController()
		{
			_workerService = new WorkerService();
			_attendanceService = new WorkerAttendanceService();
			_mms = new MultiMediaService();
			_siteService = new SiteService();
			_siteShiftService = new SiteShiftService();
			_dashboardService = new DashboardService();

		}

		public IActionResult Index()
		{
			WorkerAttendanceFilter Attendancefilter = new WorkerAttendanceFilter
			{
				Year = DateTime.Now.Year
			};

			ViewBag.AttendanceHistory = _attendanceService.GetAll(Attendancefilter);
			return View();
		}

		[HttpGet]
		[Route("api/Worker/SiteShiftJson/{siteId}")]
		public IActionResult SiteShiftJson(int siteId)
		{
			var result = _siteShiftService.GetBySiteId(siteId); // Fetch data
			if (result == null)
			{
				return Json(new { success = false, message = "No data found." });
			}
			return Json(new { success = true, data = result });
		}

		[HttpPost]
		public IActionResult CreateLedger(LedgerViewModel model)
		{
			model.SiteId = model.SiteId > 0 ? model.SiteId : null;
			model.SiteShiftId = model.SiteShiftId > 0 ? model.SiteShiftId : null;
			model.Year = model.Year > 0 ? model.Year : null;
			model.Month = model.Month > 0 ? model.Month : null;


			if (ModelState.IsValid)
			{
				ViewData["success"] = "Ledger updated successfully!";
				_attendanceService.CreateLedger(model.SiteId, model.SiteShiftId, model.Year, model.Month);
				return RedirectToAction("Ledger");
			}
			else
			{
				var errorMessages = ModelState.Values
									.SelectMany(v => v.Errors)
									.Select(e => e.ErrorMessage)
									.ToList();

				TempData["errors"] = errorMessages;
			}
			return RedirectToAction("Ledger");

		}

		public IActionResult LedgerNew(LedgerViewModel model, string? hasAllLedger = "yes")
		{
			ViewBag.Sites = _siteService.GetAll();

			if (ModelState.IsValid || hasAllLedger == "yes")
			{
				ViewBag.ShowData = true;

				// Set default or provided values for the filters
				ViewBag.Month = model.Month;
				ViewBag.MonthName = model.Month.HasValue && model.Month >= 1 && model.Month <= 12
					? new DateTime(1, model.Month.Value, 1).ToString("MMM")
					: null;

				ViewBag.Year = model.Year.ToString();
				ViewBag.SiteId = model.SiteId;
				ViewBag.SiteShiftId = model.SiteShiftId;

				// Call the service with appropriate parameters
				var allData = (hasAllLedger == "yes")
					? _attendanceService.GetLedger() // Retrieve all records
					: _attendanceService.GetLedger(null, model.SiteId, model.SiteShiftId, model.Year, model.Month);

				// Handle scenarios when no data is found
				if (allData != null && allData.Count > 0)
				{
					ViewBag.AllData = allData;
					ViewBag.SiteName = model.SiteId != 0 ? allData[0].SiteName : "All Sites";
				}
				else
				{
					ViewBag.AllData = new List<dynamic>(); // Use an empty list to prevent null reference errors
					ViewBag.SiteName = "No Data Available";
				}
			}
			else
			{
				ViewBag.ShowData = false;
			}

			return View();
		}




		public IActionResult Ledger(LedgerViewModel model, string? hasAllLedger="yes")
		{
			if(hasAllLedger == "yes")
			{
				ViewBag.Sites = _siteService.GetAll();

				ViewBag.ShowData = true;
				ViewBag.Month = null;
				ViewBag.MonthName = "---";
				ViewBag.Year = null;
				ViewBag.SiteId = null;
				ViewBag.SiteShiftId = null;
				var allData = _attendanceService.GetLedger(null, null, null, null, null);
				// Handle scenarios when no data is found
				if (allData != null && allData.Count > 0)
				{
					ViewBag.AllData = allData;
					ViewBag.SiteName = "All Sites";
				}
				else
				{
					ViewBag.AllData = new List<dynamic>(); // Use an empty list to prevent null reference errors
					ViewBag.SiteName = "No Data Available";
				}

				return View();

			}
			ViewBag.Sites = _siteService.GetAll();

			if (ModelState.IsValid)
			{
				ViewBag.ShowData = true;

				ViewBag.Month = model.Month;

				ViewBag.MonthName = model.Month.HasValue && model.Month >= 1 && model.Month <= 12
					? new DateTime(1, model.Month.Value, 1).ToString("MMM")
					: null;

				ViewBag.Year = model.Year.ToString();
				ViewBag.SiteId = model.SiteId;
				ViewBag.SiteShiftId = model.SiteShiftId;

				var allData = _attendanceService.GetLedger(null, model.SiteId, model.SiteShiftId, model.Year, model.Month);
				// Handle scenarios when no data is found
				if (allData != null && allData.Count > 0)
				{
					ViewBag.AllData = allData;
					ViewBag.SiteName = model.SiteId != null ? allData[0].SiteName : "All Sites";
				}
				else
				{
					ViewBag.AllData = new List<dynamic>(); // Use an empty list to prevent null reference errors
					ViewBag.SiteName = "No Data Available";
				}

			}
			else
			{
				ViewBag.ShowData = false;
			}

			return View();
		}


		public (string statusCode, string remark) ValidateAttendance(WorkerProfileModel worker, int graceTimeIn, int halfDayIn, int graceTimeOut, int halfDayOut, string InOutType)
		{

			string statusCode = "";
			string remark = "";
			
			TimeSpan currentTime = DateTime.Now.TimeOfDay; // Get the current time as TimeSpan

			TimeSpan? startTime = worker.StartTime; // Nullable TimeSpan
			TimeSpan? endTime = worker.EndTime; // Nullable TimeSpan
			
			if (startTime.HasValue && endTime.HasValue) // Check if startTime and endTime has a value
			{

				TimeSpan difference1 = currentTime - startTime.Value; // Calculate the difference
				int timDiffIn = (int)difference1.TotalMinutes; // Get total minutes as an integer

				TimeSpan difference2 = endTime.Value - currentTime; // Calculate the difference
				int timDiffOut = (int)difference2.TotalMinutes; // Get total minutes as an integer

				if (InOutType == "in")
				{
					if (timDiffIn < 0)
					{
						return ("shift-not-started", "Welcome! Please wait a moment, your shift hasn’t started yet.");
					}
					else if (timDiffIn <= graceTimeIn)
					{
						statusCode = "on-time-in";
						remark = "Welcome! You're on time today. Have a productive day ahead.";  // On-Time In
					}
					else if (timDiffIn <= halfDayIn)
					{
						statusCode = "half-day-in";
						remark = "Half-day attendance recorded. If you need to adjust, please inform HR.";  // Half Day In
					}
					else
					{
						statusCode = "absent-in";
						remark = "You are marked absent today. Please provide any updates or request for time off.";  // Absent In
					}
				}
				else if(InOutType == "out")
				{
					// Validate TimDiffOut
					if (timDiffOut <= graceTimeOut)
					{
						statusCode = "on-time-out";
						remark = "Thank you for completing the day on time. See you tomorrow!";  // On Time Out
					}
					else if (timDiffOut <= halfDayOut)
					{
						statusCode = "half-day-out";
						remark = "Half-day attendance recorded. If you need to adjust, please inform HR.";  // Half Day Out
					}
					else
					{
						statusCode = "early-exit";
						remark = "Your early exit has been recorded. Make sure to follow up on any pending work.";  // Early Exit
					}
				}
			}
			else
			{
				statusCode = "shift-not-set";
				remark = "Sorry! your shift not set, please contact to HR.";  // shift-not-set
			}
			return (statusCode, remark);
		}
		public IActionResult Create(string workmanId)
		{
			var worker = _workerService.GetProfileById(null, workmanId);
			if (worker == null || workmanId == null)
			{
				ViewData["Errpr"] = "Invalid WorkerId";
				return RedirectToAction(nameof(Error));
			}

			ValidateAttendanceModel validateData = _attendanceService.ValidateAttendance(worker.WorkerId);
			
			ViewBag.attendanceHistory = validateData.AttendanceRecords;
			ViewBag.isAttendanceAllowed = validateData.IsAttendanceAllowed;
			ViewBag.remark = validateData.Remark;
			ViewBag.InOutType = validateData.InOutType;
			ViewBag.Worker = worker;
			ViewBag.CurrentTime = null;
			ViewBag.Status = validateData.InOutType == "in" ? validateData.StatusCode: validateData.NewStatus;
			return View();

		}


		[HttpPost]
		[Route("api/WorkerAttendance")]
		public async Task<IActionResult> WorkerAttendance([FromForm] WorkerAttendanceModel model, IFormFile SelfieImage)
		{
			model.CurrentTime = DateTime.Now;
			try
			{
				// Validate the model
				if (!ModelState.IsValid)
				{
					return Json(new
					{
						success = false,
						message = "Invalid data provided.",
						errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
					});
				}

				// Check if an image is provided
				if (SelfieImage == null || SelfieImage.Length == 0)
				{
					return Json(new
					{
						success = false,
						message = "Selfie image is required. too"
					});
				}

								try
				{
					var uploadPath = "selfies"; // Define your folder path
					var maxFileSizeKb = 30; // 30KB size limit
					var quality = 20;
					var filePath = await _mms.SaveAndCompressImageAsync(SelfieImage, uploadPath, maxFileSizeKb, quality);

					// Set the selfie path in the model
					model.SelfieImage = filePath;

					// Call the service to manage attendance
					_attendanceService.ManageAttendance(model);



					//return Ok(new { FilePath = filePath, Message = "Image uploaded successfully." });
				}
				catch (Exception ex)
				{

					return Json(new
					{
						success = false,
						message = "An error occurred while processing attendance.",
						error = ex.Message
					});

				}


				// Return a success response
				return Json(new
				{
					success = true,
					message = "Attendance processed successfully.",
				});
			}
			catch (Exception ex)
			{
				// Log the exception if necessary

				// Return a failure response
				return Json(new
				{
					success = false,
					message = "An error occurred while processing attendance.",
					error = ex.Message
				});
			}
		}


		// GET: Attendance/WorkerAttendanceDetails
		public IActionResult WorkerAttendanceDetails()
		{
			var attendanceDetails = _attendanceService.GetWorkerAttendanceDetails();
			return View(attendanceDetails);
		}

		// POST: Attendance/ManualAttendance
		[HttpPost]
		public IActionResult ManualAttendance(int workerId, DateTime inTime, DateTime outTime, string status)
		{
			try
			{
				_attendanceService.CreateOrUpdateWorkerAttendance(workerId, inTime, outTime, status);
				return Json(new { success = true, message = "Attendance updated successfully!" });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = ex.Message });
			}
		}



		public IActionResult Error(string id)
		{
			ViewBag.WorkerId = id;	// test

			return View();
		}


	}
}
