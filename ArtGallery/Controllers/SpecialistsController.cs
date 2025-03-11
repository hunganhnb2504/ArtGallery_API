using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ArtGallery.Entities;

namespace ArtGallery.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SpecialistsController : ControllerBase
    {
        private readonly ArtGalleryApiContext _context;

        public SpecialistsController(ArtGalleryApiContext context)
        {
            _context = context;
        }

        // GET: api/Specialists
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Specialists>>> GetSpecialists()
        {
            return await _context.Specialists.ToListAsync();
        }

        // GET: api/Specialists/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Specialists>> GetSpecialists(int id)
        {
            var specialists = await _context.Specialists.FindAsync(id);

            if (specialists == null)
            {
                return NotFound();
            }

            return specialists;
        }

        // PUT: api/Specialists/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSpecialists(int id, Specialists specialists)
        {
            if (id != specialists.Id)
            {
                return BadRequest();
            }

            _context.Entry(specialists).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SpecialistsExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Specialists
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Specialists>> PostSpecialists(Specialists specialists)
        {
            _context.Specialists.Add(specialists);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSpecialists", new { id = specialists.Id }, specialists);
        }

        // DELETE: api/Specialists/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSpecialists(int id)
        {
            var specialists = await _context.Specialists.FindAsync(id);
            if (specialists == null)
            {
                return NotFound();
            }

            _context.Specialists.Remove(specialists);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SpecialistsExists(int id)
        {
            return _context.Specialists.Any(e => e.Id == id);
        }
    }
}
