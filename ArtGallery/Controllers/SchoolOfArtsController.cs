using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ArtGallery.Entities;
using ArtGallery.Models.GeneralService;
using ArtGallery.DTOs;
using ArtGallery.Models.ArtWork;
using ArtGallery.Models.SchoolOfArt;
using Humanizer.Localisation;

namespace ArtGallery.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SchoolOfArtsController : ControllerBase
    {
        private readonly ArtGalleryApiContext _context;

        public SchoolOfArtsController(ArtGalleryApiContext context)
        {
            _context = context;
        }

        // GET: api/ArtWorkMovements
        [HttpGet]
        public async Task<IActionResult> GetAllSchoolOfArts()
        {
            try
            {
                List<SchoolOfArt> soa = await _context.SchoolOfArt.Where(m => m.DeletedAt == null).OrderBy(m => m.Id).ToListAsync();
                List<SchoolOfArtDTO> result = new List<SchoolOfArtDTO>();

                foreach (SchoolOfArt m in soa)
                {
                    result.Add(new SchoolOfArtDTO
                    {
                        Id = m.Id,
                        Name = m.Name,
                        createdAt = m.CreatedAt,
                        updatedAt = m.UpdatedAt,
                        deletedAt = m.DeletedAt,
                    });
                }
                return Ok(result);

            }
            catch (Exception ex)
            {
                var response = new GeneralService
                {
                    Success = false,
                    StatusCode = 400,
                    Message = ex.Message,
                    Data = ""
                };

                return BadRequest(response);
            }

        }

        // GET: api/ArtWorkMovements/5
        [HttpGet("{id}")]
        public async Task<ActionResult> GetAllSchoolOfArtsById(int id)
        {
            try
            {
                SchoolOfArt artWork = await _context.SchoolOfArt.FirstOrDefaultAsync(a => a.Id == id && a.DeletedAt == null);
                if (artWork != null)
                {
                   return Ok(new SchoolOfArtDTO
                    {
                        Id = artWork.Id,
                        Name = artWork.Name,
                        createdAt = artWork.CreatedAt,
                        updatedAt = artWork.UpdatedAt,
                        deletedAt = artWork.DeletedAt,
                    });
                    
                }
                else
                {
                    var response = new GeneralService
                    {
                        Success = false,
                        StatusCode = 404,
                        Message = "Not Found",
                        Data = ""
                    };

                    return NotFound(response);
                };
            }
            catch (Exception ex)
            {
                var response = new GeneralService
                {
                    Success = false,
                    StatusCode = 400,
                    Message = ex.Message,
                    Data = ""
                };
                return BadRequest(response);
            }
        }

        // PUT: api/ArtWorkMovements/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> EditSchoolOfArt([FromForm]EditSchoolOfArtModel model)
        {
            try
            {
                SchoolOfArt schoolOfArt = await _context.SchoolOfArt.AsNoTracking().FirstOrDefaultAsync(m => m.Id == model.Id);
                if (schoolOfArt != null)
                {
                     SchoolOfArt artWork = new SchoolOfArt
                     {

                        Id = model.Id,
                        Name = model.Name,
                        CreatedAt = schoolOfArt.CreatedAt,
                        UpdatedAt = DateTime.Now,
                        DeletedAt = null,
                    };  

                    _context.SchoolOfArt.Update(schoolOfArt);
                    await _context.SaveChangesAsync();

                    return Ok(new GeneralService
                    {
                        Success = true,
                        StatusCode = 200,
                        Message = "Edit successfully",
                        Data = ""
                    });
                }
                else
                {
                    return NotFound(new GeneralService
                    {
                        Success = false,
                        StatusCode = 404,
                        Message = "Not Found",
                        Data = ""
                    });
                }
            }
            catch (Exception ex)
            {
                var response = new GeneralService
                {
                    Success = false,
                    StatusCode = 400,
                    Message = ex.Message,
                    Data = ""
                };

                return BadRequest(response);
            }
        }

        // POST: api/ArtWorkMovements
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("create")]
        public async Task<IActionResult> CreateSchoolOfArt([FromForm] CreateSchoolOfArtModel  model)
        {
            try
            {
                bool soa = await _context.SchoolOfArt.AnyAsync(s => s.Name.Equals(model.Name));

                if (soa)
                {
                    return BadRequest(new GeneralService
                    {
                        Success = false,
                        StatusCode = 400,
                        Message = "School Of Art already exists",
                        Data = ""
                    });
                }
                 SchoolOfArt art = new SchoolOfArt
                 {
                    //ArtistId = model.ArtistId,
                    //ArtWorkId = model.ArtWorkId,
                    Name = model.Name,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    DeletedAt = null // Không xóa khi tạo mới
                };

                _context.SchoolOfArt.Add(art);
                await _context.SaveChangesAsync();

                return Created($"get-by-id?id={art.Id}", new SchoolOfArtDTO
                {
                    Name = art.Name,
                    //Slug = art.Name.ToLower().Replace(" ", "-"),
                    createdAt = DateTime.Now,
                    updatedAt = DateTime.Now,
                    deletedAt = null
                } );
            }
            catch (Exception ex)
            {
                var response = new GeneralService
                {
                    Success = false,
                    StatusCode = 400,
                    Message = ex.Message,
                    Data = null
                };

                return BadRequest(response);
            }
        }

        [HttpDelete("delete")]
        // DELETE: api/ArtWorkMovements/5
        public async Task<IActionResult> Delete(List<int> ids)
        {
            try
            {
                foreach (var id in ids)
                {
                    SchoolOfArt genre = await _context.SchoolOfArt.FindAsync(id);

                    if (genre != null)
                    {
                        genre.DeletedAt = DateTime.Now;
                    }
                }

                await _context.SaveChangesAsync();

                var response = new GeneralService
                {
                    Success = true,
                    StatusCode = 200,
                    Message = "Soft delete successful",
                    Data = ""
                };

                return Ok(response);


            }
            catch (Exception ex)
            {
                var response = new GeneralService
                {
                    Success = false,
                    StatusCode = 400,
                    Message = ex.Message,
                    Data = ""
                };

                return BadRequest(response);
            }
        }
    }
}
