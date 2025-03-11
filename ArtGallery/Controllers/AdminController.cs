using ArtGallery.DTOs;
using ArtGallery.Entities;
using ArtGallery.Helper;
using ArtGallery.Models.Artist;
using ArtGallery.Models.GeneralService;
using ArtGallery.Models.Offer;
using ArtGallery.Models.SchoolOfArt;
using ArtGallery.Service.Artists;
using ArtGallery.Service.Email;
using ArtGallery.Service.IMG;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ArtGallery.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly ArtGalleryApiContext _context;
        private readonly IImgService _imgService;
        private readonly IEmailService _emailService;


        public AdminController(ArtGalleryApiContext context, IImgService imgService, IEmailService emailService)
        {
            _context = context;
            _imgService = imgService;
            _emailService = emailService;

        }

        [HttpPost("request-artist")]
        [Authorize/*(Roles = "User")]*/]
        public async Task<IActionResult> RequestArtist([FromForm] ArtistRequestModel model)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (!identity.IsAuthenticated)
            {
                return Unauthorized();
            }
            try
            {
                var userClaims = identity.Claims;
                var userId = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == Convert.ToInt32(userId));
                if (user == null)
                {
                    return NotFound("User not found");
                }
                // Tạo một thực thể mới đại diện cho yêu cầu           
                var image = await _imgService.UploadImageAsync(model.Image, "ArtistRequest");
                if (image != null)
                {
                    var a = new ArtistRequest
                    {
                        UserId = user.Id,
                        UserName = user.Fullname,
                        NameArtist = model.NameArtist,
                        Image = image,
                        Biography = model.Biography,
                        StatusRequest = 0,
                        SchoolOfArt = model.SchoolOfArt,
                        CreatedAt = DateTime.Now,
                    };
                    _context.ArtistRequests.Add(a);
                    await _context.SaveChangesAsync();
                    var mailrequest = new Mailrequest
                    {
                        ToEmail = "projectsem3123@gmail.com",
                        Subject = "New Artist Request",
                        Body = $"A new artist request has been submitted.\n\nArtist Name: {a.NameArtist}\nUser Name: {a.UserName}"
                    };
                    await _emailService.SendEmailAsync(mailrequest);

                    return Created($"get-by-id?id={a.Id}", new ArtistRequestDTO
                    {
                        Id = a.Id,
                        UserId = user.Id,
                        UserName = a.UserName,
                        NameArtist = model.NameArtist,
                        Image = a.Image,
                        SchoolOfArt = a.SchoolOfArt,
                        Biography = a.Biography,
                        createdAt = a.CreatedAt,

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
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("accept-artist-request/{id}")]
        //[Authorize/*(Roles = "Admin")*/]
        public async Task<IActionResult> AcceptArtistRequest(int id, [FromForm] UpdateStatusRequest request)
        {
            try
            {
                var artistRequest = await _context.ArtistRequests.FirstOrDefaultAsync(ar => ar.Id == id);
                if (artistRequest == null)
                {
                    return NotFound("Artist request not found");
                }
                var user = await _context.Users.FindAsync(artistRequest.UserId);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                switch (request.Action)
                {
                    case "accept":
                        artistRequest.StatusRequest = 1;
                        user.Role = "Artist";
                        var newArtist = new Artist
                        {
                            Name = artistRequest.NameArtist,
                            Image = artistRequest.Image,
                            Biography = artistRequest.Biography,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        };
                        _context.Artist.Add(newArtist);
                        await _context.SaveChangesAsync();
                        var userArtist = new UserArtist
                        {
                            UserId = user.Id,
                            ArtistId = newArtist.Id
                        };
                        _context.UserArtist.Add(userArtist);
                        await _context.SaveChangesAsync();
                        var ArtistSchoolofart = new ArtistSchoolOfArt
                        {
                            ArtistId = newArtist.Id,
                            SchoolOfArtId = newArtist.Id
                        };
                        _context.ArtistSchoolOfArt.Add(ArtistSchoolofart);
                        await _context.SaveChangesAsync();

                        //Gửi email thông báo chấp nhận yêu cầu
                        var mailRequest = new Mailrequest
                        {
                            ToEmail = user.Email,
                            Subject = "Artist Request Accepted",
                            Body = "Your artist request has been accepted. You are now an artist!"
                        };
                        await _emailService.SendEmailAsync(mailRequest);

                        return Ok("Artist request accepted successfully");

                    case "reject":
                        artistRequest.StatusRequest = 0;
                        // Gửi email thông báo từ chối
                        var rejectMailRequest = new Mailrequest
                        {
                            ToEmail = user.Email,
                            Subject = "Artist Request Rejected",
                            Body = "Your artist request has been rejected."
                        };
                        await _emailService.SendEmailAsync(rejectMailRequest);

                        // Xóa bản ghi ArtistRequest đã bị từ chối
                        _context.ArtistRequests.Remove(artistRequest);
                        await _context.SaveChangesAsync();

                        return Ok("Artist request rejected successfully");

                    default:
                        return BadRequest("Invalid action");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }




        [HttpGet("getall-request-artist")]
        public async Task<IActionResult> GetAllArtistRequests()
        {
            try
            {
                var artistRequests = await _context.ArtistRequests.ToListAsync();

                if (artistRequests == null || !artistRequests.Any())
                {
                    return NotFound("No artist requests found");
                }
                // Chuyển đổi danh sách các yêu cầu thành đối tượng DTO để hiển thị
                var result = artistRequests.Select(request => new ArtistRequestDTO
                {
                    Id = request.Id,
                    UserId = request.UserId,
                    UserName = request.UserName,
                    NameArtist = request.NameArtist,
                    Status = request.StatusRequest,
                    SchoolOfArt = request.SchoolOfArt,
                    Image = request.Image,
                    Biography = request.Biography,
                    createdAt = request.CreatedAt,

                }).ToList();
                // Trả về danh sách các yêu cầu dưới dạng kết quả
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ nếu có lỗi xảy ra
                var response = new GeneralService
                {
                    Success = false,
                    StatusCode = 500,
                    Message = $"Internal server error: {ex.Message}",
                    Data = ""
                };

                return StatusCode(500, response);
            }
        }

        [HttpGet("get-request-artist/{id}")]
        public async Task<IActionResult> GetArtistRequestById(int id)
        {
            try
            {
                var artistRequest = await _context.ArtistRequests.FindAsync(id);

                if (artistRequest == null)
                {
                    return NotFound("Artist request not found");
                }

                // Chuyển đổi yêu cầu thành đối tượng DTO để hiển thị
                var result = new ArtistRequestDTO
                {
                    Id = artistRequest.Id,
                    UserId = artistRequest.UserId,
                    UserName = artistRequest.UserName,
                    NameArtist = artistRequest.NameArtist,
                    Status = artistRequest.StatusRequest,
                    Image = artistRequest.Image,
                    SchoolOfArt = artistRequest.SchoolOfArt,
                    Biography = artistRequest.Biography,
                    createdAt = artistRequest.CreatedAt,
                };

                // Trả về yêu cầu dưới dạng kết quả
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ nếu có lỗi xảy ra
                var response = new GeneralService
                {
                    Success = false,
                    StatusCode = 500,
                    Message = $"Internal server error: {ex.Message}",
                    Data = ""
                };

                return StatusCode(500, response);
            }
        }

    }
}
