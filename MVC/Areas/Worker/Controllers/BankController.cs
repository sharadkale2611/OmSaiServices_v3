using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using OmSaiModels.Worker;
using OmSaiServices.Admin.Implementations;
using OmSaiServices.Worker.Implementations;
using OmSaiServices.Worker.Implimentation;

namespace OmSaiServices_v3.Areas.Worker.Controllers
{
	[Area("Worker")]
	public class BankController : Controller
	{
		private readonly BankDetailService _bankdetailService;
		private readonly ProjectService _projectService;
		private readonly SiteService _siteService;
		private WorkerService _workerService;

		public BankController()
		{
			_bankdetailService = new BankDetailService();
			_projectService = new ProjectService();
			_siteService = new SiteService();
			_workerService = new WorkerService();
		}

		public IActionResult Index(int ProjectId = 0, int SiteId = 0)
		{
			TempData["ProjectId"] = ProjectId;
			TempData["SiteId"] = SiteId;
			ViewBag.AllData = _bankdetailService.GetAll(null, null, SiteId);    // id is siteId here
			ViewBag.Projects = _projectService.GetAll();
			ViewBag.Sites = _siteService.GetAll();

			return View();
		}

		public IActionResult Create(int id=0)
		{


			var worker = _workerService.GetById(id);		// id is workerId here
			if(worker == null)
			{
				TempData["error"] = "Invalid worker!";
				return RedirectToAction(nameof(Index));
			}

			var bankDetail = _bankdetailService.GetAllByWorkerId(id);
			if (bankDetail.Count != 0)
			{
				ViewBag.BankDetail = bankDetail[0];
			}


			ViewBag.WorkerName = $"{worker.FirstName} {worker.MiddleName} {worker.LastName}";
			ViewBag.WorkerId = id;
			
			return View();
		}

		[HttpPost]
		public IActionResult Create(BankDetailModel model)
		{
			ModelState.Remove("BankProofType");
			ModelState.Remove("BankProofImage");

			if (ModelState.IsValid)
			{
				var bankDetail = _bankdetailService.GetAllByWorkerId(model.WorkerId);
				if (bankDetail.Count == 0)
				{
					_bankdetailService.Create(model);
				}
				else
				{
					model.BankDetailId = bankDetail[0].BankDetailId;
					_bankdetailService.Update(model);
				}
				TempData["success"] = "Bank details saved successfully!";
				return RedirectToAction(nameof(Index));

			}
			TempData["error"] = "its errrs";

			return View(model);
		}



	}
}
