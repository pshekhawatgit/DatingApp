using API.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;
[ServiceFilter(typeof(LogUserActivity))]
[ApiController]
[Route("API/[Controller]")]
public class BaseApiController : ControllerBase
{

}
