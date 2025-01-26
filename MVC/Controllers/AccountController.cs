using GeneralTemplate.Filter;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OmSaiModels.Admin;
using OmSaiServices_v3.Data;
using System.Text;
using Newtonsoft.Json;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;



namespace GeneralTemplate.Controllers
{
	public class AccountController : Controller
	{
		private readonly AppDbContext _context;
		private readonly PasswordHasher<ApplicationUser> _passwordHasher = new PasswordHasher<ApplicationUser>();


		public AccountController(AppDbContext context)
		{
			_context = context;
		}

		public IActionResult Index()
		{
			return RedirectToAction("Login");
		}


		// GET: Account/Login
		public IActionResult Login()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Login(string email, string password)
		{
			// Find the user by email
			var user = await _context.AspNetUsers.FirstOrDefaultAsync(u => u.Email == email);
			if (user == null)
			{
				ViewBag.Message = "Invalid Email ID or Password.";
				ViewData["error"] = "Invalid Email ID or Password.";
				return View();
			}

			var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);


			// Verify the password (assumes password is hashed)
			if (passwordVerificationResult == PasswordVerificationResult.Failed)
				//if (!VerifyPasswordHash(password, user.PasswordHash))
			{
				ViewBag.Message = "Invalid Email ID or Password.";
				ViewData["error"] = "Invalid Email ID or Password.";
				return View();
			}

			// Get the roles assigned to the user
			var roles = await (from userRole in _context.AspNetUserRoles
							   join role in _context.AspNetRoles on userRole.RoleId equals role.Id
							   where userRole.UserId == user.Id
							   select role.Name).ToListAsync();

			// Optionally, convert roles to an array if you need it
			var rolesArray = roles.ToArray();
			string employeeRoles = string.Join(",", rolesArray);
			string employeeRolesArray = JsonConvert.SerializeObject(rolesArray);

			// Save user details and roles in session
			HttpContext.Session.SetString("id", user.Id);
			HttpContext.Session.SetString("username", user.UserName);
			HttpContext.Session.SetString("employeeRoles", employeeRoles);
			HttpContext.Session.SetString("employeeRolesArray", employeeRolesArray);

			ViewBag.Message = "Login Success";
			ViewData["success"] = "Login Success";
			return RedirectToAction("Index", "Home");
		}

		//private bool VerifyPasswordHash(string password, string storedHash)
		//{
		//	using (var sha256 = SHA256.Create())
		//	{
		//		var computedHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
		//		var computedHashString = BitConverter.ToString(computedHash).Replace("-", "");
		//		return string.Equals(computedHashString, storedHash, StringComparison.OrdinalIgnoreCase);
		//	}
		//}





		[HttpGet]
		[EmpAuthorizeFilter]
		public IActionResult Logout()
		{
			// Clear all session data
			HttpContext.Session.Clear();
			return RedirectToAction("Login","Account");
		}

	}
}
