using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ArtGallery.Entities;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ArtGallery.Models.GeneralService;
using ArtGallery.DTOs;
using ArtGallery.Models.Favorite;

namespace ArtGallery.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FavoritesController : ControllerBase
    {
        private readonly ArtGalleryApiContext _context;

        public FavoritesController(ArtGalleryApiContext context)
        {
            _context = context;
        }

        [HttpGet("get-by-user")]
        [Authorize]
        public async Task<IActionResult> GetByUser()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;

            if (!identity.IsAuthenticated)
            {
                return Unauthorized(new GeneralService
                {   Success = false,
                    StatusCode = 401,
                    Message = "Not Authorized",
                    Data = "" 
                });
            }
            try
            {
                var userClaims = identity.Claims;
                var userId = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == Convert.ToInt32(userId));

                if (user == null)
                {
                    return Unauthorized(new GeneralService 
                    {   Success = false,
                        StatusCode = 401,
                        Message = "Not Authorized",
                        Data = "" 
                    });
                }

                List<Favorite> favorites = await _context.Favorite.Include(f => f.ArtWork).Where(f => f.UserId == user.Id).OrderByDescending(p => p.Id).ToListAsync();
                List<FavoriteDTO> result = new List<FavoriteDTO>();
                foreach (var item in favorites)
                {
                    result.Add(new FavoriteDTO
                    {
                        Id = item.Id,
                        ArtWorkImage=item.ArtWork.ArtWorkImage,
                        ArtWorkName=item.ArtWork.Name,
                        ArtWorkId=item.ArtWork.Id,  
                        UserId = item.UserId,
                        ArtWorkAmount =item.ArtWork.Price,
                        Series=item.ArtWork.Series,
                        createdAt = item.CreatedAt,
                        updatedAt = item.UpdatedAt,
                        deletedAt = item.DeletedAt,
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

        // POST: api/Favorites
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("addtofavorite")]
        [Authorize]
        public async Task<IActionResult> addToFavorite(CreateFavorite model)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;

            if (!identity.IsAuthenticated)
            {
                return Unauthorized(new GeneralService
                {
                    Success = false,
                    StatusCode = 401,
                    Message = "Not Authorized",
                    Data = ""
                });
            }
            try
            {
                var userClaims = identity.Claims;
                var userId = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == Convert.ToInt32(userId));

                if (user == null)
                {
                    return Unauthorized(new GeneralService
                    { Success = false,
                        StatusCode = 401,
                        Message = "Not Authorized",
                        Data = "" });
                }

                var existingFavorite = await _context.Favorite
                    .FirstOrDefaultAsync(f => f.UserId == user.Id && f.ArtWorkId == model.ArtWorkId);

                if (existingFavorite != null)
                {
                    return BadRequest(new GeneralService
                    {
                        Success = false,
                        StatusCode = 401,
                        Message = "The ArtWork already exists in the favorites list.",
                        Data = ""
                    });
                }

                Favorite favorite = new Favorite
                {
                    UserId = user.Id,
                    ArtWorkId = model.ArtWorkId,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    DeletedAt = null,
                };

                _context.Favorite.Add(favorite);
                await _context.SaveChangesAsync();

                var artWork = await _context.ArtWork.FindAsync(model.ArtWorkId);
                artWork.FavoriteCount = await _context.Favorite.Where(f => f.ArtWorkId == artWork.Id).CountAsync();

                await _context.SaveChangesAsync();

                return Created($"get-by-id?id={favorite.Id}", new FavoriteDTO
                {
                    Id = favorite.Id,
                    ArtWorkId = model.ArtWorkId,
                    UserId = user.Id,
                    createdAt = favorite.CreatedAt,
                    updatedAt = favorite.UpdatedAt,
                    deletedAt = favorite.DeletedAt,
                });

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

        [HttpDelete("removefavorite")]
        [Authorize]
        public async Task<IActionResult> removeFromFavorite(int id)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;

            if (!identity.IsAuthenticated)
            {
                return Unauthorized(new GeneralService
                { Success = false,
                    StatusCode = 401,
                    Message = "Not Authorized",
                    Data = "" 
                });
            }
            try
            {
                var userClaims = identity.Claims;
                var userId = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == Convert.ToInt32(userId));

                if (user == null)
                {
                    return Unauthorized(new GeneralService
                    { Success = false,
                        StatusCode = 401,
                        Message = "Not Authorized",
                        Data = "" 
                    });
                }

                var favorite = await _context.Favorite
                    .FirstOrDefaultAsync(f => f.UserId == user.Id && f.Id == id);

                if (favorite == null)
                {
                    return BadRequest(new GeneralService 
                    { Success = false,
                        StatusCode = 400,
                        Message = "The ArtWork does not exist in the favorites list.",
                        Data = "" 
                    });
                }

                _context.Favorite.Remove(favorite);
                await _context.SaveChangesAsync();

                var artWork = await _context.ArtWork.FindAsync(favorite.ArtWorkId);
                artWork.FavoriteCount = await _context.Favorite.Where(f => f.ArtWorkId == artWork.Id).CountAsync();

                await _context.SaveChangesAsync();

                return Ok(new GeneralService 
                {   Success = true, 
                    StatusCode = 200,
                    Message = "removed from favorites list",
                    Data = "" 
                });

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
       
        private bool FavoriteExists(int id)
        {
            return _context.Favorite.Any(e => e.Id == id);
        }
    }
}
