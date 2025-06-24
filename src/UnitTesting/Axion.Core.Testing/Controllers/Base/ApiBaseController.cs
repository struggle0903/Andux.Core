using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Andux.Core.Testing.Controllers.Base
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize(AuthenticationSchemes = "Bearer")]
    public class ApiBaseController : ControllerBase
    {

    }
}
