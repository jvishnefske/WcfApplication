using Microsoft.AspNetCore.Mvc;
using ContactsApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks; // Add this for async

namespace ContactsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Base route for this controller will be /api/Contacts
    public class ContactsController : ControllerBase
    {
        private readonly Utilities _utilities; // Add field

        public ContactsController(Utilities utilities) // Add constructor for injection
        {
            _utilities = utilities;
        }

        [HttpGet] // GET /api/Contacts
        public async Task<ActionResult<IEnumerable<ContactDto>>> GetAllContacts() // Make async
        {
            var contacts = await _utilities.GetAllContactsAsync(); // Use async method
            return Ok(contacts);
        }

        [HttpGet("{id}")] // GET /api/Contacts/{id}
        public async Task<ActionResult<ContactDto>> GetContact(int id) // Make async
        {
            var contact = await _utilities.GetContactAsync(id); // Use async method
            if (contact == null)
            {
                return NotFound();
            }
            return Ok(contact);
        }

        [HttpPut("{uid}")] // PUT /api/Contacts/{uid}
        public async Task<IActionResult> UpdateContact(int uid, [FromBody] PersonRequestDto person) // Make async
        {
            if (person == null)
            {
                return BadRequest("Person data is null.");
            }

            // Use null-coalescing operator for nullable strings
            await _utilities.UpdateContactAsync(uid, 
                                                person.FirstName ?? string.Empty, 
                                                person.LastName ?? string.Empty, 
                                                person.PrefixId, // Corrected from person.Prefix
                                                person.SuffixId, // Corrected from person.Suffix
                                                person.Address, 
                                                person.City, 
                                                person.State, 
                                                person.Zip);
            return NoContent(); // 204 No Content for successful update
        }

        [HttpPost] // POST /api/Contacts
        public async Task<IActionResult> InsertContact([FromBody] PersonRequestDto person) // Make async
        {
            if (person == null)
            {
                return BadRequest("Person data is null.");
            }

            // Use null-coalescing operator for nullable strings
            await _utilities.InsertContactAsync(person.FirstName ?? string.Empty, 
                                                person.LastName ?? string.Empty, 
                                                person.PrefixId, // Corrected from person.Prefix
                                                person.SuffixId, // Corrected from person.Suffix
                                                person.Address, 
                                                person.City, 
                                                person.State, 
                                                person.Zip);
            return CreatedAtAction(nameof(GetAllContacts), null); // 201 Created
        }
    }
}
