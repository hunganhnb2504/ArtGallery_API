using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ArtGallery.Entities;
using Microsoft.AspNetCore.Authorization;
using ArtGallery.Models.Artist;
using ArtGallery.Models.GeneralService;
using ArtGallery.Service.IMG;
using ArtGallery.DTOs;
using static System.Runtime.InteropServices.JavaScript.JSType;
using ArtGallery.Service.Artists;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;
using ArtGallery.Models.ArtWork;
using ArtGallery.Models.SchoolOfArt;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Drawing;
using System.Security.Claims;
using ArtGallery.Models.Offer;

namespace ArtGallery.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArtistsController : ControllerBase
    {
        private readonly ArtGalleryApiContext _context;
        private readonly IImgService _imgService;
        private readonly IArtistService _artistService;

        public ArtistsController(ArtGalleryApiContext context, IImgService imgService, IArtistService artistService)
        {
            _context = context;
            _imgService = imgService;
            _artistService = artistService;
        }



        //[HttpPost("create-artist")]
        ////[Authorize(Roles = "Super Admin")]
        //public async Task<IActionResult> CreateArtistOne([FromForm] CreateArtistModel model)
        //{
        //    var identity = HttpContext.User.Identity as ClaimsIdentity;

        //    if (!identity.IsAuthenticated)
        //    {
        //        return Unauthorized(new GeneralService
        //        {
        //            Success = false,
        //            StatusCode = 401,
        //            Message = "Not Authorized",
        //            Data = ""
        //        });
        //    }
        //    try
        //    {
        //        var userIdClaim = identity.FindFirst(ClaimTypes.NameIdentifier);
        //        if (userIdClaim == null)
        //        {
        //            return Unauthorized(new GeneralService
        //            {
        //                Success = false,
        //                StatusCode = 401,
        //                Message = "Not Authorized",
        //                Data = ""
        //            });
        //        }

        //        var userId = Convert.ToInt32(userIdClaim.Value);

        //        // Kiểm tra xem người dùng có tồn tại trong cơ sở dữ liệu không
        //        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        //        if (user == null)
        //        {
        //            return Unauthorized(new GeneralService
        //            {
        //                Success = false,
        //                StatusCode = 401,
        //                Message = "Not Authorized",
        //                Data = ""
        //            });
        //        }

        //        // Kiểm tra xem người dùng đã tạo nghệ sĩ nào chưa
        //        bool userHasArtist = await _context.UserArtist.AnyAsync(ua => ua.UserId == userId);
        //        if (userHasArtist)
        //        {
        //            return BadRequest(new GeneralService
        //            {
        //                Success = false,
        //                StatusCode = 400,
        //                Message = "User has already created an artist",
        //                Data = ""
        //            });
        //        }

        //        // Kiểm tra xem nghệ sĩ đã tồn tại chưa
        //        bool artistExists = await _context.Artist.AnyAsync(a => a.Name.Equals(model.Name));
        //        if (artistExists)
        //        {
        //            return BadRequest(new GeneralService
        //            {
        //                Success = false,
        //                StatusCode = 400,
        //                Message = "Artist already exists",
        //                Data = ""
        //            });
        //        }

        //        var image = await _imgService.UploadImageAsync(model.ImagePath, "Artist");
        //        if (image != null)
        //        {
        //            // Tạo nghệ sĩ mới từ dữ liệu trong model và ảnh đã tải lên
        //            Artist a = new Artist
        //            {
        //                Name = model.Name,
        //                Image = image,
        //                Biography = model.Biography,
        //                Description = model.Description,
        //                CreatedAt = DateTime.Now,
        //                DeletedAt = null,
        //                UpdatedAt = null,
        //            };

        //            _context.Artist.Add(a);
        //            await _context.SaveChangesAsync();

        //            // Tạo mối quan hệ giữa người dùng và nghệ sĩ
        //            var userArtist = new UserArtist
        //            {
        //                UserId = userId,
        //                ArtistId = a.Id
        //            };
        //            _context.UserArtist.Add(userArtist);
        //            await _context.SaveChangesAsync();

        //            foreach (var schoolOfArtId in model.SchoolOfArtIds)
        //            {
        //                var artistSchoolOfArts = new ArtistSchoolOfArt
        //                {
        //                    ArtistId = a.Id,
        //                    SchoolOfArtId = schoolOfArtId,
        //                };

        //                _context.ArtistSchoolOfArt.Add(artistSchoolOfArts);
        //            }

        //            await _context.SaveChangesAsync();

        //            // Trả về thông báo thành công
        //            return Created($"get-by-id?id={a.Id}", new ArtistDTO
        //            {
        //                Id = a.Id,
        //                Name = a.Name,
        //                Image = a.Image,
        //                Biography = a.Biography,
        //                createdAt = a.CreatedAt,
        //                updatedAt = a.UpdatedAt,
        //                deletedAt = a.DeletedAt,

        //            });
        //        }
        //        else
        //        {
        //            // Trả về thông báo lỗi nếu có vấn đề trong quá trình tải ảnh lên
        //            return BadRequest(new GeneralService
        //            {
        //                Success = false,
        //                StatusCode = 400,
        //                Message = "Error uploading image",
        //                Data = ""
        //            });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Trả về thông báo lỗi nếu có lỗi xảy ra trong quá trình xử lý
        //        var response = new GeneralService
        //        {
        //            Success = false,
        //            StatusCode = 500,
        //            Message = "Error creating artist: " + ex.Message,
        //            Data = ""
        //        };

        //        return StatusCode(500, response);
        //    }
        //}

        [HttpGet]
        public async Task<IActionResult> GetArtistAll([FromQuery] string search = null, [FromQuery] List<int> artWorkIds = null, [FromQuery] List<int> schoolOfArtsIds = null)
        {
            try
            {
                // Bắt đầu với truy vấn gốc để lấy danh sách nghệ sĩ
                var query = _context.Artist
                    .Include(a => a.ArtistSchoolOfArts).ThenInclude(a => a.SchoolOfArt)
                    .Include(a => a.ArtistArtWorks).ThenInclude(a => a.ArtWork)
                    .Where(a => a.DeletedAt == null);

                // Áp dụng bộ lọc tìm kiếm
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(a => a.Name.Contains(search));
                }

                //// Áp dụng bộ lọc SchoolOfArtIds
                if (schoolOfArtsIds != null && schoolOfArtsIds.Any())
                {
                    query = query.Where(a => a.ArtistSchoolOfArts.Any(a1 => schoolOfArtsIds.Contains(a1.SchoolOfArtId)));
                }

                // Áp dụng bộ lọc ArtWorkIds
                if (artWorkIds != null && artWorkIds.Any())
                {
                    query = query.Where(a => a.ArtistArtWorks.Any(aw => artWorkIds.Contains(aw.ArtWorkId)));
                }
                List<Artist> artist = await query.OrderByDescending(m => m.Id).ToListAsync();
                List<ArtistDTO> result = new List<ArtistDTO>();

                foreach (Artist a in artist)
                {
                    var artistDTO = new ArtistDTO
                    {
                        Id = a.Id,
                        Name = a.Name,
                        Biography = a.Biography,
                        Image = a.Image,
                        Description = a.Description,
                        createdAt = a.CreatedAt,
                        updatedAt = a.UpdatedAt,
                        deletedAt = a.DeletedAt,
                    };


                    var artWorks = new List<ArtWorkResponse>();
                    var schoolOfArts = new List<SchoolOfArtResponse>();
                    foreach (var item in a.ArtistSchoolOfArts)
                    {
                        var schoolOfArt = new SchoolOfArtResponse
                        {
                            Id = item.Id,
                            Name = item.SchoolOfArt.Name,
                        };
                        schoolOfArts.Add(schoolOfArt);
                    }
                    artistDTO.SchoolOfArts = schoolOfArts;
                    foreach (var item in a.ArtistArtWorks)
                    {
                        var artWork = new ArtWorkResponse
                        {
                            Id = item.Id,
                            Name = item.ArtWork.Name,
                            ArtWorkImage = item.ArtWork.ArtWorkImage,
                            Medium = item.ArtWork.Medium,
                            Materials = item.ArtWork.Materials,
                            Size = item.ArtWork.Size,
                            Condition = item.ArtWork.Condition,
                            Signature = item.ArtWork.Signature,
                            Rarity = item.ArtWork.Rarity,
                            CertificateOfAuthenticity = item.ArtWork.CertificateOfAuthenticity,
                            Frame = item.ArtWork.Frame,
                            Series = item.ArtWork.Series,
                            Price = item.ArtWork.Price,
                            FavoriteCount = item.ArtWork.FavoriteCount,

                        };
                        artWorks.Add(artWork);

                    }
                    artistDTO.ArtWork = artWorks;
                    result.Add(artistDTO);
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



        [HttpGet("{id}")]
        public async Task<IActionResult> GetArtistById(int id)
        {
            try
            {
                Artist a = await _context.Artist
                    .Include(a => a.ArtistArtWorks).ThenInclude(a => a.ArtWork)
                    .ThenInclude(aw => aw.OfferArtWork).ThenInclude(oaw => oaw.Offer)
                     .ThenInclude(o => o.User)
                    .Include(m => m.ArtistSchoolOfArts).ThenInclude(m => m.SchoolOfArt)
                    .FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null);

                if (a != null)
                {
                    var artistDto = new ArtistDTO
                    {
                        Id = a.Id,
                        Name = a.Name,
                        Biography = a.Biography,
                        Image = a.Image,
                        Description = a.Description,
                        createdAt = a.CreatedAt,
                        updatedAt = a.UpdatedAt,
                        deletedAt = a.DeletedAt,
                    };

                    var artWorks = new List<ArtWorkResponse>();
                    var schoolOfArts = new List<SchoolOfArtResponse>();

                    foreach (var item in a.ArtistSchoolOfArts)
                    {
                        var schoolOfArt = new SchoolOfArtResponse
                        {
                            Id = item.Id,
                            Name = item.SchoolOfArt.Name,
                        };
                        schoolOfArts.Add(schoolOfArt);
                    }
                    artistDto.SchoolOfArts = schoolOfArts;

                    foreach (var item in a.ArtistArtWorks)
                    {
                        var artWork = new ArtWorkResponse
                        {
                            Id = item.ArtWork.Id,
                            Name = item.ArtWork.Name,
                            ArtWorkImage = item.ArtWork.ArtWorkImage,
                            Medium = item.ArtWork.Medium,
                            Materials = item.ArtWork.Materials,
                            Size = item.ArtWork.Size,
                            Condition = item.ArtWork.Condition,
                            Signature = item.ArtWork.Signature,
                            Rarity = item.ArtWork.Rarity,
                            CertificateOfAuthenticity = item.ArtWork.CertificateOfAuthenticity,
                            Frame = item.ArtWork.Frame,
                            Series = item.ArtWork.Series,
                            Price = item.ArtWork.Price,
                            FavoriteCount = item.ArtWork.FavoriteCount,
                            Offers = item.ArtWork.OfferArtWork
                                .Where(oaw => oaw.Offer.IsPaid == 1)
                                .Select(oaw => new OfferResponse
                                {
                                    Id = oaw.Offer.Id,
                                    OfferPrice = oaw.Offer.OfferPrice,
                                    offercode = oaw.Offer.OfferCode,
                                    ToTal = oaw.Offer.Total,
                                    UserName = oaw.Offer.User.Fullname,
                                    isPaid = oaw.Offer.IsPaid,

                                })
                                .ToList()
                        };
                        artWorks.Add(artWork);
                    }
                    artistDto.ArtWork = artWorks;

                    return Ok(artistDto);
                }

            }
            catch (Exception ex)
            {
                var response = new GeneralService
                {
                    Success = false,
                    StatusCode = 500,
                    Message = "Internal server error",
                    Data = ex.Message
                };

                return StatusCode(500, response);
            }
            return NotFound();
        }



        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> CreateArtist([FromForm] CreateArtistModel model)
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
                var userIdClaim = identity.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized(new GeneralService
                    {
                        Success = false,
                        StatusCode = 401,
                        Message = "Not Authorized",
                        Data = ""
                    });
                }

                var userId = Convert.ToInt32(userIdClaim.Value);

                // Kiểm tra xem người dùng có tồn tại trong cơ sở dữ liệu không
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
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

                bool artistExists = await _context.Artist.AnyAsync(a => a.Name.Equals(model.Name));

                // Kiểm tra xem nghệ sĩ đã tồn tại hay chưa
                if (artistExists)
                {
                    return BadRequest(new GeneralService
                    {
                        Success = false,
                        StatusCode = 400,
                        Message = "Artist already exists",
                        Data = ""
                    });
                }

                var image = await _imgService.UploadImageAsync(model.ImagePath, "Artist");
                if (image != null)
                {
                    // Tạo một đối tượng Artist từ dữ liệu trong model và ảnh đã tải lên
                    Artist a = new Artist
                    {
                        Name = model.Name,
                        Image = image,
                        Biography = model.Biography,
                        Description = model.Description,
                        CreatedAt = DateTime.Now,
                        DeletedAt = null,
                        UpdatedAt = null,
                    };

                    _context.Artist.Add(a);
                    await _context.SaveChangesAsync();

                    // Tạo mối quan hệ giữa người dùng và nghệ sĩ
                    var userArtist = new UserArtist
                    {
                        UserId = userId,
                        ArtistId = a.Id
                    };
                    _context.UserArtist.Add(userArtist);
                    await _context.SaveChangesAsync();

                    foreach (var schoolOfArtId in model.SchoolOfArtIds)
                    {
                        var artistSchoolOfArts = new ArtistSchoolOfArt
                        {
                            ArtistId = a.Id,
                            SchoolOfArtId = schoolOfArtId,
                        };

                        _context.ArtistSchoolOfArt.Add(artistSchoolOfArts);
                    }
                    // Trả về thông báo thành công
                    return Created($"get-by-id?id={a.Id}", new ArtistDTO
                    {
                        Id = a.Id,
                        Name = a.Name,
                        Image = a.Image,
                        Biography = a.Biography,
                        createdAt = a.CreatedAt,
                        updatedAt = a.UpdatedAt,
                        deletedAt = a.DeletedAt,

                    });
                }
                else
                {
                    // Trả về thông báo lỗi nếu có vấn đề trong quá trình tải ảnh lên
                    return BadRequest(new GeneralService
                    {
                        Success = false,
                        StatusCode = 400,
                        Message = "Error uploading image",
                        Data = ""
                    });
                }
            }
            catch (Exception ex)
            {
                // Trả về thông báo lỗi nếu có lỗi xảy ra trong quá trình xử lý
                var response = new GeneralService
                {
                    Success = false,
                    StatusCode = 500,
                    Message = "Error creating artist: " + ex.Message,
                    Data = ""
                };

                return StatusCode(500, response);
            }
        }


        [HttpPut("edit")]
        [Authorize(Roles = "Super Admin")]
        public async Task<IActionResult> EditArtist([FromForm] EditArtistModel model)
        {
            try
            {
                // Tìm kiếm nghệ sĩ cần chỉnh sửa
                Artist existingArtist = await _context.Artist.AsNoTracking().FirstOrDefaultAsync(e => e.Id == model.Id);

                if (existingArtist != null)
                {
                    Artist artist = new Artist
                    {
                        Id = model.Id,
                        Name = model.Name,
                        Description = model.Description,
                        CreatedAt = existingArtist.CreatedAt,
                        UpdatedAt = DateTime.Now,
                        DeletedAt = null,
                    };
                    if (model.ImagePath != null)
                    {
                        string image = await _imgService.UploadImageAsync(model.ImagePath, "Artist");

                        if (image == null)
                        {
                            return BadRequest(new GeneralService
                            {
                                Success = false,
                                StatusCode = 400,
                                Message = "Failed to upload avatar.",
                                Data = ""
                            });
                        }

                        existingArtist.Image = image;
                    }
                    else
                    {
                        artist.Image = existingArtist.Image;
                    }

                    _context.Artist.Update(existingArtist);
                    await _context.SaveChangesAsync();

                    return Ok(new GeneralService
                    {
                        Success = true,
                        StatusCode = 200,
                        Message = "Artist edited successfully",
                        Data = existingArtist
                    });
                }
                else
                {
                    return NotFound(new GeneralService
                    {
                        Success = false,
                        StatusCode = 404,
                        Message = "Artist not found",
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


        [HttpDelete("delete")]
        [Authorize(Roles = "Super Admin")]
        public async Task<IActionResult> DeleteArtist(List<int> ids)
        {
            try
            {
                foreach (var id in ids)
                {
                    Artist artist = await _context.Artist.FindAsync(id);

                    if (artist != null)
                    {
                        artist.DeletedAt = DateTime.Now;
                    }
                }

                await _context.SaveChangesAsync();

                var response = new GeneralService
                {
                    Success = true,
                    StatusCode = 200,
                    Message = "Soft delete successful",
                    Data = null
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
