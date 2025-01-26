using GeneralTemplate.Filter;
using Microsoft.AspNetCore.Mvc;
using OmSaiModels.Admin;
using OmSaiServices.Admin.Implementations;
using OmSaiServices_v3.Data;



namespace GeneralTemplate.Areas.Admin.Controllers
{
	[Area("Admin")]
	[EmpAuthorizeFilter]
	public class RoleController : Controller
	{
		private readonly AppDbContext _context;

		public RoleController(AppDbContext context)
		{
			_context = context;
		}


		// List all roles
		public IActionResult Index()
		{
			var roles = _context.AspNetRoles.ToList();
			ViewBag.AllData = roles;
			return View();
		}


		// POST: Role/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(Role model)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(model.Name))
				{
					ModelState.AddModelError(string.Empty, "Role name cannot be empty.");
				}

				if (ModelState.IsValid)
				{
					TempData["success"] = "Record added successfully!";

					var role = new Role
					{
						Id = Guid.NewGuid().ToString(), // Generate a unique Id
						Name = model.Name
					};

					_context.AspNetRoles.Add(role);
					await _context.SaveChangesAsync();

					return RedirectToAction(nameof(Index));
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

				return RedirectToAction(nameof(Index)); // nameof checks method compiletime to avoid errors
			}
			catch
			{
				TempData["error"] = "Something went wrong!";
				return View("Index", model);
			}
		}

		// POST: Role/Edit/{id}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(Role model)
		{
			try
			{
				var role = await _context.AspNetRoles.FindAsync(model.Id);
				if (role == null) return NotFound();

				if (string.IsNullOrWhiteSpace(model.Name))
				{
					ModelState.AddModelError(string.Empty, "Role name cannot be empty.");
				}

				if (ModelState.IsValid)
				{
					TempData["success"] = "Record updated successfully!";

					role.Name = model.Name;
					_context.AspNetRoles.Update(role);
					await _context.SaveChangesAsync();

					return RedirectToAction(nameof(Index));
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

				return RedirectToAction(nameof(Index)); // nameof checks method compiletime to avoid errors
			}
			catch
			{
				TempData["error"] = "Something went wrong!";
				return View("Index", model);
			}
		}



		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(string id)
		{
			// Find the role by id
			var role = await _context.AspNetRoles.FindAsync(id);
			if (role == null)
			{
				return NotFound();
			}

			try
			{
				// Remove the role from the database
				_context.AspNetRoles.Remove(role);
				await _context.SaveChangesAsync();

				// Redirect to the index page if the deletion is successful
				return RedirectToAction(nameof(Index));
			}
			catch (Exception ex)
			{
				// Handle the error (e.g., log it or return a message)
				ModelState.AddModelError(string.Empty, $"Error deleting role: {ex.Message}");
				return View(role);
			}
		}

	}
}
