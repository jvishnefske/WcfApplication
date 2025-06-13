using Microsoft.AspNetCore.Mvc;
using ContactsApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks; // Add this for async

namespace ContactsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Base route for this controller will be /api/Lookups
    public class LookupsController : ControllerBase
    {
        private readonly Utilities _utilities; // Add field

        public LookupsController(Utilities utilities) // Add constructor for injection
        {
            _utilities = utilities;
        }

        [HttpGet("prefixes")] // GET /api/Lookups/prefixes
        public async Task<ActionResult<IEnumerable<LookupDto>>> GetPrefixes() // Make async
        {
            var prefixes = await _utilities.GetPrefixesAsync(); // Use async method
            return Ok(prefixes);
        }

        [HttpGet("suffixes")] // GET /api/Lookups/suffixes
        public async Task<ActionResult<IEnumerable<LookupDto>>> GetSuffixes() // Make async
        {
            var suffixes = await _utilities.GetSuffixesAsync(); // Use async method
            return Ok(suffixes);
        }
    }
}
