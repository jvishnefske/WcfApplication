using Microsoft.AspNetCore.Mvc;
using ContactsApi.Models;
using System.Collections.Generic;

namespace ContactsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Base route for this controller will be /api/Lookups
    public class LookupsController : ControllerBase
    {
        [HttpGet("prefixes")] // GET /api/Lookups/prefixes
        public ActionResult<IEnumerable<LookupDto>> GetPrefixes()
        {
            var prefixes = Utilities.GetPrefixes();
            return Ok(prefixes);
        }

        [HttpGet("suffixes")] // GET /api/Lookups/suffixes
        public ActionResult<IEnumerable<LookupDto>> GetSuffixes()
        {
            var suffixes = Utilities.GetSuffixes();
            return Ok(suffixes);
        }
    }
}
