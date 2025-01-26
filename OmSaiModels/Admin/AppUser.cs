using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmSaiModels.Admin
{
    class AppUser
    {
    }

	public class LoginViewModel
	{
		public string Email { get; set; }
		public string Password { get; set; }
	}

	public class CreateUserViewModel
	{
		[Required]
		[EmailAddress]
		[Display(Name = "Email")]
		public string Email { get; set; }
		public string UserName { get; set; }


		[Required]
		[DataType(DataType.Password)]
		[Display(Name = "Password")]
		public string Password { get; set; }

		[Display(Name = "Roles")]
		public List<string>? Roles { get; set; } // List of all roles

		[Display(Name = "Assigned Roles")]
		public List<string> SelectedRoles { get; set; } // List of selected roles
	}

	public class EditUserViewModel
	{
		public string Id { get; set; }
		public string Email { get; set; }
		public string UserName { get; set; }
		public List<string>? Roles { get; set; }
		public List<string> SelectedRoles { get; set; } // A list of selected roles
	}


}
