using Microsoft.AspNetCore.Mvc;
using ContactsApi.Models;
using System.Collections.Generic;

namespace ContactsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Base route for this controller will be /api/Contacts
    public class ContactsController : ControllerBase
    {
        [HttpGet] // GET /api/Contacts
        public ActionResult<IEnumerable<ContactDto>> GetAllContacts()
        {
            var contacts = Utilities.GetAllContacts();
            return Ok(contacts);
        }

        [HttpGet("{id}")] // GET /api/Contacts/{id}
        public ActionResult<ContactDto> GetContact(int id)
        {
            var contact = Utilities.GetContact(id);
            if (contact == null)
            {
                return NotFound();
            }
            return Ok(contact);
        }

        [HttpPut("{uid}")] // PUT /api/Contacts/{uid}
        public IActionResult UpdateContact(int uid, [FromBody] PersonRequestDto person)
        {
            if (person == null)
            {
                return BadRequest("Person data is null.");
            }

            Utilities.UpdateContact(uid, person.FirstName, person.LastName, person.Prefix, person.Suffix, person.Address, person.City, person.State, person.Zip);
            return NoContent(); // 204 No Content for successful update
        }

        [HttpPost] // POST /api/Contacts
        public IActionResult InsertContact([FromBody] PersonRequestDto person)
        {
            if (person == null)
            {
                return BadRequest("Person data is null.");
            }

            Utilities.InsertContact(person.FirstName, person.LastName, person.Prefix, person.Suffix, person.Address, person.City, person.State, person.Zip);
            return CreatedAtAction(nameof(GetAllContacts), null); // 201 Created
        }
    }
}
