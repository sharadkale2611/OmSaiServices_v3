using GeneralTemplate.Filter;
using Microsoft.AspNetCore.Mvc;
using OmSaiModels.Admin;
using OmSaiServices.Admin.Implementations;
using OmSaiServices.Admin.Interfaces;

namespace GeneralTemplate.Areas.Admin.Controllers
{
	[Area("Admin")]
	[EmpAuthorizeFilter]
	public class SiteShiftController : Controller
	{
		private readonly SiteShiftService _siteShiftService;
		private readonly SiteService _siteService;
		private readonly ProjectService _projectService;

		public SiteShiftController()
		{
			_siteShiftService = new SiteShiftService();
			_siteService = new SiteService();
			_projectService = new ProjectService();
		}


		public ActionResult Index(int ProjectId = 0, int SiteId = 0)
		{
			TempData["ProjectId"] = ProjectId;
			TempData["SiteId"] = SiteId;

			var allData = _siteShiftService.GetBySiteId(SiteId);
			ViewBag.AllData = allData;
			var sites = _siteService.GetAll();
			ViewBag.Sites = sites;

			ViewBag.Sites = _siteService.GetAll();
			ViewBag.Projects = _projectService.GetAll();


			return View();
		}


		[HttpPost]
		
		public ActionResult Create(SiteShiftModel model)
		{
			try
			{
				if (ModelState.IsValid)
				{
					TempData["success"] = "Record added successfully!";
					_siteShiftService.Create(model);
				}
				else
				{
					var errorMessages = new List<string>();
					foreach (var state in ModelState)
					{
						foreach (var error in state.Value.Errors)
						{
							errorMessages.Add(error.ErrorMessage);
						}
					}
					TempData["errors"] = errorMessages;
				}

				return RedirectToAction(nameof(Index));
			}
			catch
			{
				TempData["error"] = "Something went wrong!";
				return View("Index", model);
			}
		}


		public IActionResult Edit(int id)
		{
			SiteShiftModel s = _siteShiftService.GetById(id);
			return View(s);
		}

		[HttpPost]
		
		public ActionResult Edit(SiteShiftModel model)
		{
			try
			{
				if (ModelState.IsValid)
				{
					TempData["success"] = "Record updated successfully!";
					_siteShiftService.Update(model);
				}
				else
				{
					var errorMessages = new List<string>();
					foreach (var state in ModelState)
					{
						foreach (var error in state.Value.Errors)
						{
							errorMessages.Add(error.ErrorMessage);
						}
					}
					TempData["errors"] = errorMessages;
				}

				return RedirectToAction(nameof(Index));             // nameof checks method compiletime to avoid errors
			}
			catch
			{
				TempData["error"] = "Something went wrong!";
				return View("Index", model);
			}
		}


		public IActionResult Delete(int id)
		{
			_siteShiftService.Delete(id);
			return RedirectToAction("Index");
		}



	}
}
