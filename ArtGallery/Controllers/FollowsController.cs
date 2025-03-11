using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ArtGallery.Entities;
using ArtGallery.DTOs;
using ArtGallery.Models.Favorite;
using ArtGallery.Models.GeneralService;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using ArtGallery.Models.ArtWork;

namespace ArtGallery.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FollowsController : ControllerBase
    {
        private readonly ArtGalleryApiContext _context;

        public FollowsController(ArtGalleryApiContext context)
        {
            _context = context;
        }
        [HttpGet("get-by-user-follow")]
        //[Authorize]
        public async Task<IActionResult> GetByUser()
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
                    {
                        Success = false,
                        StatusCode = 401,
                        Message = "Not Authorized",
                        Data = ""
                    });
                }

                var follows = await _context.Follow
                    .Include(f => f.Artist)
                        .ThenInclude(a => a.ArtistArtWorks)
                        .ThenInclude(aw => aw.ArtWork)
                    .Where(f => f.UserId == user.Id)
                    .OrderByDescending(p => p.Id)
                    .ToListAsync();

                var result = new List<FollowDTO>();
                foreach (var follow in follows)
                {
                    var artistDto = new FollowDTO
                    {
                        Id = follow.Id,
                        ArtistImage = follow.Artist.Image,
                        ArtistName = follow.Artist.Name,
                        ArtistId = follow.Artist.Id,
                        UserId = follow.UserId,
                        createdAt = follow.CreatedAt,
                        updatedAt = follow.UpdatedAt,
                        deletedAt = follow.DeletedAt,
                        ArtWorks = follow.Artist.ArtistArtWorks
                            .Select(aw => new ArtWorkResponse
                            {
                                Id = aw.ArtWork.Id,
                                Name = aw.ArtWork.Name,
                                ArtWorkImage = aw.ArtWork.ArtWorkImage,
                                Medium = aw.ArtWork.Medium,
                                Materials = aw.ArtWork.Materials,
                                Size = aw.ArtWork.Size,
                                Condition = aw.ArtWork.Condition,
                                Signature = aw.ArtWork.Signature,
                                Rarity = aw.ArtWork.Rarity,
                                CertificateOfAuthenticity = aw.ArtWork.CertificateOfAuthenticity,
                                Frame = aw.ArtWork.Frame,
                                Series = aw.ArtWork.Series,
                                Price = aw.ArtWork.Price,
                                FavoriteCount = aw.ArtWork.FavoriteCount,
                            }).ToList()
                    };

                    result.Add(artistDto);
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



        [HttpPost("addtofollow")]
        [Authorize]
        public async Task<IActionResult> AddToFollow(CreateFavoriteArtist model)
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
                if (model.ArtistId <= 0)
                {
                    return BadRequest(new GeneralService
                    {
                        Success = false,
                        StatusCode = 400,
                        Message = "Invalid ArtistId",
                        Data = ""
                    });
                }

                var userClaims = identity.Claims;
                var userId = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == Convert.ToInt32(userId));

                if (user == null)
                {
                    return Unauthorized(new GeneralService
                    {
                        Success = false,
                        StatusCode = 401,
                        Message = "Not Authorized",
                        Data = ""
                    });
                }

                var existingFollow = await _context.Follow
                    .FirstOrDefaultAsync(f => f.UserId == user.Id && f.ArtistId == model.ArtistId);

                if (existingFollow != null)
                {
                    return BadRequest(new GeneralService
                    {
                        Success = false,
                        StatusCode = 400,
                        Message = "The Artist already exists in the follow list.",
                        Data = ""
                    });
                }

                var follow = new Follow
                {
                    UserId = user.Id,
                    ArtistId = model.ArtistId,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    DeletedAt = null,
                };

                _context.Follow.Add(follow);
                await _context.SaveChangesAsync();

                var artist = await _context.Artist.FindAsync(model.ArtistId);
                if (artist != null)
                {
                    artist.FollowCount = await _context.Follow.CountAsync(f => f.ArtistId == model.ArtistId);
                    await _context.SaveChangesAsync();
                }

                return Created($"get-by-id?id={follow.Id}", new FollowDTO
                {
                    Id = follow.Id,
                    ArtistId = model.ArtistId,
                    UserId = user.Id,
                    createdAt = follow.CreatedAt,
                    updatedAt = follow.UpdatedAt,
                    deletedAt = follow.DeletedAt,
                });
            }
            catch (Exception ex)
            {
                var response = new GeneralService
                {
                    Success = false,
                    StatusCode = 500,
                    Message = "An error occurred while processing the request.",
                    Data = ""
                };

                // Log the exception for troubleshooting
                // Logger.LogError(ex, "Error occurred in AddToFollow action");

                return StatusCode(500, response);
            }
        }

        [HttpDelete("removefollow")]
        [Authorize]
        public async Task<IActionResult> removeFromFollow(int id)
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

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == Convert.ToInt32(userId));

                if (user == null)
                {
                    return Unauthorized(new GeneralService
                    {
                        Success = false,
                        StatusCode = 401,
                        Message = "Not Authorized",
                        Data = ""
                    });
                }

                var follow = await _context.Follow
                    .FirstOrDefaultAsync(f => f.UserId == user.Id && f.Id == id);

                if (follow == null)
                {
                    return BadRequest(new GeneralService
                    {
                        Success = false,
                        StatusCode = 400,
                        Message = "The Artist does not exist in the follow list.",
                        Data = ""
                    });
                }

                _context.Follow.Remove(follow);
                await _context.SaveChangesAsync();

                var artist = await _context.Artist.FindAsync(follow.ArtistId);
                artist.FollowCount = await _context.Follow.Where(f => f.ArtistId == artist.Id).CountAsync();

                await _context.SaveChangesAsync();

                return Ok(new GeneralService
                {
                    Success = true,
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
        private bool FollowExists(int id)
        {
            return _context.Follow.Any(e => e.Id == id);
        }
    }
}
