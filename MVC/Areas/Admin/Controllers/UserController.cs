using GeneralTemplate.Filter;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OmSaiModels.Admin;
using OmSaiServices_v3.Data;
using System.Text;
using System.Security.Cryptography;

namespace GeneralTemplate.Areas.Admin.Controllers
{
	[Area("Admin")]
	[EmpAuthorizeFilter]
	public class UserController : Controller
	{
		private readonly AppDbContext _context;

		public UserController(AppDbContext context)
		{
			_context = context;
		}

		// List Users
		public async Task<IActionResult> Index()
		{
			// Get all users from the AspNetUsers table
			var users = await _context.AspNetUsers.ToListAsync();
			var userRoles = new List<UserRolesViewModel>();

			foreach (var user in users)
			{
				// Get the roles assigned to each user
				var roles = await (from userRole in _context.AspNetUserRoles
								   join role in _context.AspNetRoles on userRole.RoleId equals role.Id
								   where userRole.UserId == user.Id
								   select role.Name).ToListAsync();

				// Add user with their roles to the list
				userRoles.Add(new UserRolesViewModel
				{
					Id = user.Id,
					Email = user.Email,
					Roles = roles.ToList()  // Explicitly convert to List<string>
				});
			}

			return View(userRoles); // Pass the userRoles to the view
		}

		// Create User (GET)
		public IActionResult Create()
			{
				// Retrieve all available roles from the AspNetRoles table
				var roles = _context.AspNetRoles.Select(r => r.Name).ToList();

				// Initialize the ViewModel
				var model = new CreateUserViewModel
				{
					Roles = roles // Populate with all available roles
				};

				return View(model);
		}


		// Create User (POST)
		[HttpPost]
		public async Task<IActionResult> Create(CreateUserViewModel model)
		{
			if (!ModelState.IsValid)
			{
				// Return the same view with model to display validation errors
				return View(model);
			}

			// Check if user already exists
			var existingUser = await _context.AspNetUsers
				.FirstOrDefaultAsync(u => u.Email == model.Email);

			if (existingUser != null)
			{
				ModelState.AddModelError("Email", "Email already exists.");
				return View(model);
			}

			// Create a new user and save it to the database
			var user = new ApplicationUser
			{
				UserName = model.UserName,
				Email = model.Email,
				PasswordHash = HashPassword(model.Password) // Hash the password before saving
			};

			_context.AspNetUsers.Add(user);
			await _context.SaveChangesAsync();

			// Assign roles to the user
			foreach (var roleName in model.SelectedRoles)
			{
				var role = await _context.AspNetRoles.FirstOrDefaultAsync(r => r.Name == roleName);
				if (role != null)
				{
					var userRole = new IdentityUserRole<string>
					{
						UserId = user.Id,
						RoleId = role.Id
					};
					_context.AspNetUserRoles.Add(userRole);
				}
			}

			await _context.SaveChangesAsync();

			return RedirectToAction("Index", "User");
		}

		private string HashPassword(string password)
		{
			using (var sha256 = SHA256.Create())
			{
				var hashedPassword = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
				return BitConverter.ToString(hashedPassword).Replace("-", "");
			}
		}



		// GET: User/Edit/{id}
		public async Task<IActionResult> Edit(string id)
		{
			// Retrieve the user from the AspNetUsers table
			var user = await _context.AspNetUsers.FirstOrDefaultAsync(u => u.Id == id);
			if (user == null)
			{
				return NotFound();
			}

			// Get all available roles from the AspNetRoles table
			var roles = await _context.AspNetRoles.Select(r => r.Name).ToListAsync();

			// Get the user's current roles by joining AspNetUserRoles and AspNetRoles
			var userRoles = await (from userRole in _context.AspNetUserRoles
								   join role in _context.AspNetRoles on userRole.RoleId equals role.Id
								   where userRole.UserId == user.Id
								   select role.Name).ToListAsync();

			// Prepare the ViewModel
			var model = new EditUserViewModel
			{
				Id = user.Id,
				Email = user.Email,
				Roles = roles, // List of all roles
				SelectedRoles = userRoles // List of roles assigned to the user
			};

			return View(model);
		}

		// POST: User/Edit/{id}
		[HttpPost]
		
		public async Task<IActionResult> Edit(EditUserViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			// Retrieve the user from the AspNetUsers table
			var user = await _context.AspNetUsers.FirstOrDefaultAsync(u => u.Id == model.Id);
			if (user == null)
			{
				return NotFound();
			}

			// Update the user's email address
			user.Email = model.Email;

			// Update the user record in the database
			_context.AspNetUsers.Update(user);
			await _context.SaveChangesAsync();

			// Remove the user's current roles by deleting from AspNetUserRoles
			var currentRoles = await (from userRole in _context.AspNetUserRoles
									  where userRole.UserId == user.Id
									  select userRole).ToListAsync();
			_context.AspNetUserRoles.RemoveRange(currentRoles);
			await _context.SaveChangesAsync();

			// Add the selected roles to the user
			foreach (var roleName in model.SelectedRoles)
			{
				var role = await _context.AspNetRoles.FirstOrDefaultAsync(r => r.Name == roleName);
				if (role != null)
				{
					var userRole = new IdentityUserRole<string>
					{
						UserId = user.Id,
						RoleId = role.Id
					};
					_context.AspNetUserRoles.Add(userRole);
				}
			}

			await _context.SaveChangesAsync();

			return RedirectToAction("Index");
		}



		// GET: User/Delete/{id}
		public async Task<IActionResult> Delete(string id)
		{
			// Retrieve the user from the AspNetUsers table
			var user = await _context.AspNetUsers.FirstOrDefaultAsync(u => u.Id == id);
			if (user == null)
			{
				return NotFound();
			}

			// Optionally, you could delete the user's roles from AspNetUserRoles
			var userRoles = await _context.AspNetUserRoles
				.Where(ur => ur.UserId == id)
				.ToListAsync();

			_context.AspNetUserRoles.RemoveRange(userRoles);

			// Remove the user from the AspNetUsers table
			_context.AspNetUsers.Remove(user);
			await _context.SaveChangesAsync();

			return RedirectToAction("Index");
		}

		// GET: User/AssignRole/{id}
		public async Task<IActionResult> AssignRole(string id)
		{
			// Retrieve the user from the AspNetUsers table
			var user = await _context.AspNetUsers.FirstOrDefaultAsync(u => u.Id == id);
			if (user == null)
			{
				return NotFound();
			}

			// Retrieve all roles from the AspNetRoles table
			var roles = await _context.AspNetRoles.Select(r => r.Name).ToListAsync();

			// Retrieve the user's current roles by joining AspNetUserRoles and AspNetRoles
			var userRoles = await (from userRole in _context.AspNetUserRoles
								   join role in _context.AspNetRoles on userRole.RoleId equals role.Id
								   where userRole.UserId == user.Id
								   select role.Name).ToListAsync();

			// Pass the user, available roles, and current roles to the view
			ViewBag.User = user;
			ViewBag.Roles = roles;
			ViewBag.UserRoles = userRoles;

			return View();
		}

		// POST: User/AssignRole/{userId}
		[HttpPost]
		
		public async Task<IActionResult> AssignRole(string userId, string role)
		{
			// Retrieve the user from the AspNetUsers table
			var user = await _context.AspNetUsers.FirstOrDefaultAsync(u => u.Id == userId);
			if (user == null)
			{
				return NotFound();
			}

			// Check if the user is already assigned the role
			var existingRole = await _context.AspNetRoles.FirstOrDefaultAsync(r => r.Name == role);
			if (existingRole == null)
			{
				return NotFound("Role not found.");
			}

			// Check if the user already has the role
			var userRoleExists = await _context.AspNetUserRoles
				.AnyAsync(ur => ur.UserId == user.Id && ur.RoleId == existingRole.Id);

			if (!userRoleExists)
			{
				// Assign the role by adding a new entry to AspNetUserRoles
				var userRole = new IdentityUserRole<string>
				{
					UserId = user.Id,
					RoleId = existingRole.Id
				};
				_context.AspNetUserRoles.Add(userRole);
				await _context.SaveChangesAsync();
			}

			return RedirectToAction("Index");
		}



	}
}
