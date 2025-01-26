using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmSaiModels.Admin
{

	public class Role
	{
		public string Id { get; set; }
		public string Name { get; set; }
	}

	public class UserRolesViewModel
	{
		public string Id { get; set; }
		public string Email { get; set; }
		public List<string> Roles { get; set; }
	}
}
