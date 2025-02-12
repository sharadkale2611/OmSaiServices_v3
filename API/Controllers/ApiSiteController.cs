using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OmSaiModels.Common;
using OmSaiServices.Admin.Implementations;

namespace API.Controllers
{
	[Route("api/site")]
	[ApiController]
	public class ApiSiteController : ControllerBase
	{
		private readonly SiteService _siteService;

		public ApiSiteController()
		{
			_siteService = new SiteService();
		}

		[HttpGet("/")]
		[Authorize(AuthenticationSchemes = "Jwt")]
		public async Task<IActionResult> GetSites()
		{
			try
			{
				var sites = _siteService.GetAll();
				if (sites == null)
				{
					var errors = new
					{
						WorkmanId = new[] { "Sites not found!" }
					};
					return BadRequest(new ApiResponseModel<object>(false, null, errors));
				}

				return Ok(new ApiResponseModel<object>(true, sites, null));

			}
			catch (Exception ex)
			{
				var errors = new
				{
					message = new[] { ex.Message }
				};

				return BadRequest(new ApiResponseModel<object>(false, null, errors));

			}

		}


	}
}
