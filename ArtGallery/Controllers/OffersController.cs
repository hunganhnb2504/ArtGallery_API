using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ArtGallery.Entities;
using Microsoft.AspNetCore.Authorization;
using ArtGallery.DTOs;
using ArtGallery.Models.GeneralService;
using System.Security.Claims;
using ArtGallery.Models.Offer;
using ArtGallery.Helper;
using ArtGallery.Service.Email;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using NuGet.Packaging.Signing;

namespace ArtGallery.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OffersController : ControllerBase
    {
        private readonly ArtGalleryApiContext _context;
        private readonly IEmailService _emailService;

        public OffersController(ArtGalleryApiContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpGet("GetOfferAllArtist")]
        [Authorize]
        public async Task<IActionResult> GetOrderAllArtist()
        {
            try
            {
                // Lấy userId của người dùng đăng nhập
                var userIdentity = HttpContext.User.Identity as ClaimsIdentity;
                var userIdClaim = userIdentity.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null)
                {
                    return Unauthorized(new { message = "Not Authorized" });
                }

                int userId = Convert.ToInt32(userIdClaim.Value);

                // Lấy danh sách nghệ sĩ mà người dùng đăng nhập đã liên kết
                var userArtists = await _context.UserArtist
                    .Where(ua => ua.UserId == userId)
                    .Select(ua => ua.ArtistId)
                    .ToListAsync();

                // Lấy danh sách các tác phẩm của các nghệ sĩ mà người dùng đăng nhập đã liên kết
                var artistArtWorkIds = await _context.ArtistArtWork
                    .Where(aaw => userArtists.Contains(aaw.ArtistId))
                    .Select(aaw => aaw.ArtWorkId)
                    .ToListAsync();

                // Lấy tất cả các đề xuất từ cơ sở dữ liệu cho các tác phẩm được liên kết với nghệ sĩ
                List<Offer> offers = await _context.Offer
                    .Include(o => o.User) // Nạp thông tin người dùng
                    .Include(o => o.OfferArtWorks).ThenInclude(o => o.ArtWork)
                    .Where(o => artistArtWorkIds.Contains(o.ArtWorkId))
                    .OrderByDescending(p => p.Id)
                    .ToListAsync();
                List<OfferDTO> result = new List<OfferDTO>();
                // Chuyển đổi danh sách các đề xuất thành đối tượng DTO để hiển thị
                foreach (var offer in offers)
                {
                    result.Add(new OfferDTO
                    {
                        Id = offer.Id,
                        UserId = offer.UserId,
                        UserName = offer.User.Fullname,
                        OfferPrice = offer.OfferPrice,
                        ArtWorkId = offer.ArtWorkId,
                        OfferCode = offer.OfferCode,
                        ToTal = offer.Total,
                        ArtWorkNames = offer.OfferArtWorks.Select(oaw => oaw.ArtWork.Name).ToList(),
                        ArtWorkImages = offer.OfferArtWorks.Select(oaw => oaw.ArtWork.ArtWorkImage).ToList(),
                        IsPaid = offer.IsPaid,
                        Status = offer.Status,
                        createdAt = offer.CreatedAt,
                        updatedAt = offer.UpdatedAt,
                        deletedAt = offer.DeletedAt
                    });
                }
                // Trả về danh sách các đề xuất dưới dạng kết quả
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ nếu có lỗi xảy ra
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
        [HttpGet("get-by-id-artist/{code_order}")]
        [Authorize(Roles = "Super Admin, Artist")]
        public async Task<IActionResult> GetOrderDetailArtist(string code_order)
        {
            try
            {
                // Lấy userId của người dùng đăng nhập
                var userIdentity = HttpContext.User.Identity as ClaimsIdentity;
                var userIdClaim = userIdentity.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null)
                {
                    return Unauthorized(new { message = "Not Authorized" });
                }

                int userId = Convert.ToInt32(userIdClaim.Value);

                // Lấy danh sách nghệ sĩ mà người dùng đăng nhập đã liên kết
                var userArtists = await _context.UserArtist
                    .Where(ua => ua.UserId == userId)
                    .Select(ua => ua.ArtistId)
                    .ToListAsync();

                // Lấy danh sách các tác phẩm của các nghệ sĩ mà người dùng đăng nhập đã liên kết
                var artistArtWorkIds = await _context.ArtistArtWork
                    .Where(aaw => userArtists.Contains(aaw.ArtistId))
                    .Select(aaw => aaw.ArtWorkId)
                    .ToListAsync();

                // Lấy thông tin đề xuất từ mã đề xuất (offer code) và kiểm tra xem có liên kết với các tác phẩm của nghệ sĩ không
                Offer offer = await _context.Offer
                    .Include(o => o.User) // Nạp thông tin người dùng
                    .Include(o => o.OfferArtWorks).ThenInclude(o => o.ArtWork)
                    .FirstOrDefaultAsync(x => x.OfferCode.Equals(code_order) && artistArtWorkIds.Contains(x.ArtWorkId) && x.DeletedAt == null);

                if (offer != null)
                {
                    OfferDTO result = new OfferDTO
                    {
                        Id = offer.Id,
                        UserId = offer.UserId,
                        UserName = offer.User.Fullname,
                        OfferPrice = offer.OfferPrice,
                        ArtWorkId = offer.ArtWorkId,
                        OfferCode = offer.OfferCode,
                        ToTal = offer.Total,
                        ArtWorkNames = offer.OfferArtWorks.Select(oaw => oaw.ArtWork.Name).ToList(),
                        ArtWorkImages = offer.OfferArtWorks.Select(oaw => oaw.ArtWork.ArtWorkImage).ToList(),
                        Address = offer.Address,
                        Status = offer.Status,
                        IsPaid = offer.IsPaid,
                        createdAt = offer.CreatedAt,
                        updatedAt = offer.UpdatedAt,
                        deletedAt = offer.DeletedAt
                    };
                    return Ok(result);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ nếu có lỗi xảy ra
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

        [HttpGet("GetOrderAllAdmin")]
        [Authorize(Roles = "Super Admin, Artist")]
        public async Task<IActionResult> GetOrderAll()
        {
            try
            {
                // Lấy tất cả các đề xuất từ cơ sở dữ liệu
                List<Offer> offers = await _context.Offer
                    .Include(o => o.User) // Nạp thông tin người dùng
                    .Include(o => o.OfferArtWorks).ThenInclude(o => o.ArtWork)
                    .OrderByDescending(p => p.Id)
                    .ToListAsync();
                List<OfferDTO> result = new List<OfferDTO>();
                foreach (var offer in offers)
                {
                    result.Add(new OfferDTO
                    {
                        Id = offer.Id,
                        UserId = offer.UserId,
                        UserName = offer.User.Fullname,
                        OfferPrice = offer.OfferPrice,
                        ArtWorkId = offer.ArtWorkId,
                        OfferCode = offer.OfferCode,
                        ToTal = offer.Total,
                        ArtWorkNames = offer.OfferArtWorks.Select(oaw => oaw.ArtWork.Name).ToList(),
                        ArtWorkImages = offer.OfferArtWorks.Select(oaw => oaw.ArtWork.ArtWorkImage).ToList(),
                        IsPaid = offer.IsPaid,
                        Status = offer.Status,
                        createdAt = offer.CreatedAt,
                        updatedAt = offer.UpdatedAt,
                        deletedAt = offer.DeletedAt
                    });
                }

                // Trả về danh sách các đề xuất dưới dạng kết quả
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ nếu có lỗi xảy ra
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

        [HttpGet("get-by-id-admin/{code_order}")]
        [Authorize(Roles = "Super Admin, Artist")]
        public async Task<IActionResult> GetOrderDetail(string code_order)
        {
            try
            {
                Offer offer = await _context.Offer.Include(o => o.User).Include(o => o.OfferArtWorks)
                    .ThenInclude(o => o.ArtWork).FirstOrDefaultAsync(x => x.OfferCode.Equals(code_order) && x.DeletedAt == null);
                if (offer != null)
                {
                    OfferDTO result = new OfferDTO
                    {
                        Id = offer.Id,
                        UserId = offer.UserId,
                        UserName = offer.User.Fullname,
                        OfferPrice = offer.OfferPrice,
                        ArtWorkId = offer.ArtWorkId,
                        OfferCode = offer.OfferCode,
                        ToTal = offer.Total,
                        ArtWorkNames = offer.OfferArtWorks.Select(oaw => oaw.ArtWork.Name).ToList(),
                        ArtWorkImages = offer.OfferArtWorks.Select(oaw => oaw.ArtWork.ArtWorkImage).ToList(),
                        Address = offer.Address,
                        Status = offer.Status,
                        IsPaid = offer.IsPaid,
                        createdAt = offer.CreatedAt,
                        updatedAt = offer.UpdatedAt,
                        deletedAt = offer.DeletedAt
                    };
                    return Ok(result);
                }
                else
                {
                    return NotFound();
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



        [HttpGet("get-by-user")]
        [Authorize]
        public async Task<IActionResult> GetOfferByUser()
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

                List<Offer> offers = await _context.Offer
                    .Include(o => o.User)
                    .Include(o => o.OfferArtWorks)
                    .ThenInclude(o => o.ArtWork)
                    .Where(o => o.UserId == user.Id)
                    .OrderByDescending(o => o.Id)
                    .ToListAsync();
                List<OfferDTO> result = new List<OfferDTO>();

                foreach (var offer in offers)
                {
                    result.Add(new OfferDTO
                    {
                        Id = offer.Id,
                        UserId = offer.UserId,
                        //UserName = user.Fullname,
                        OfferPrice = offer.OfferPrice,
                        ArtWorkId = offer.ArtWorkId,
                        OfferCode = offer.OfferCode,
                        ToTal = offer.Total,
                        ArtWorkNames = offer.OfferArtWorks.Select(oaw => oaw.ArtWork.Name).ToList(),
                        ArtWorkImages = offer.OfferArtWorks.Select(oaw => oaw.ArtWork.ArtWorkImage).ToList(),
                        IsPaid = offer.IsPaid,
                        Status = offer.Status,
                        createdAt = offer.CreatedAt,
                        updatedAt = offer.UpdatedAt,
                        deletedAt = offer.DeletedAt
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


        [HttpGet("detailForUser/{OfferCode}")]
        [Authorize]
        public async Task<IActionResult> GetOfferDetailForUser(string OfferCode)
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

                Offer offer = await _context.Offer
                    .Include(o => o.User)
                    .Include(o => o.OfferArtWorks)
                    .ThenInclude(o => o.ArtWork)
                    .FirstOrDefaultAsync(x => x.OfferCode.Equals(OfferCode) && x.DeletedAt == null && x.UserId == user.Id);
                if (offer != null)
                {
                    var offerDetail = new OfferDetail
                    {
                        Id = offer.Id,
                        OfferCode = offer.OfferCode,
                        UserId = offer.UserId,
                        ArtWorkId=offer.ArtWorkId,
                        UserName = offer.User.Fullname,
                        OfferPrice = offer.OfferPrice,
                        ToTal = offer.Total,
                        Status = offer.Status,
                        Address = offer.Address,
                        ArtWorkNames = offer.OfferArtWorks.Select(oaw => oaw.ArtWork.Name).ToList(),
                        ArtWorkImages = offer.OfferArtWorks.Select(oaw => oaw.ArtWork.ArtWorkImage).ToList(),
                        IsPaid = offer.IsPaid,  
                        CreatedAt = offer.CreatedAt,
                        UpdatedAt = offer.UpdatedAt,
                        DeletedAt = offer.DeletedAt,
                    };

                    List<OfferArtWorkResponse> offerArts = new List<OfferArtWorkResponse>();

                    foreach (var item in offer.OfferArtWorks)
                    {
                        var artwork = new OfferArtWorkResponse
                        {
                            Id = item.Id,
                            OfferId = item.OfferId,
                            ArtWorkId = item.ArtWorkId,
                            ArtWorkImage = item.ArtWork.Name,
                            ArtWorkName = item.ArtWork.ArtWorkImage,
                            OfferPrice = item.Price,
                        };
                        offerArts.Add(artwork);
                    }
                    offerDetail.OfferArtWork = offerArts;

                    return Ok(offerDetail);
                }
                else
                {
                    return NotFound();
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

        [HttpPost("CreateOfferUser")]
        [Authorize]
        public async Task<IActionResult> CreateOffer(CreateOffer model)
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
                var artwork = await _context.ArtWork.FindAsync(model.ArtWorkId);
                if (artwork == null)
                {
                    return NotFound(new GeneralService
                    {
                        Success = false,
                        StatusCode = 404,
                        Message = "Artwork not found",
                        Data = ""
                    });
                }

                Offer offer = new Offer
                {

                    OfferCode = GenerateRandom.GenerateRandomString(8),
                    ArtWorkId = model.ArtWorkId,
                    UserId = user.Id,
                    Total = model.Total,
                    Status = 0,
                    Address = user.Address,
                    IsPaid = 0,
                    OfferPrice = artwork.Price,
                    //PaymentMethod = model.paymentMethod,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    DeletedAt = null,
                };
                _context.Offer.Add(offer);
                await _context.SaveChangesAsync();
                var offerArtWork = new OfferArtWork
                {
                    OfferId = offer.Id,
                    ArtWorkId = artwork.Id,
                    Price = artwork.Price
                };

                // Thêm vào danh sách OfferArtWorks của Offer
                offer.OfferArtWorks.Add(offerArtWork);
                // Thêm Offer vào context
                await _context.SaveChangesAsync();
                


                Mailrequest mailrequest = new Mailrequest();
                mailrequest.ToEmail = user.Email;
                mailrequest.Subject = "Successful ";
                //mailrequest.Body = 
                await _emailService.SendEmailAsync(mailrequest);

                return Created($"get-by-id?id={offer.Id}", new OfferDTO
                {
                    Id = offer.Id,
                    OfferCode = offer.OfferCode,
                    ArtWorkId = offer.ArtWorkId,
                    UserId = offer.UserId,
                    UserName = user.Fullname,
                    ToTal = offer.Total,
                    Status = offer.Status,
                    OfferPrice= offer.OfferPrice,
                    ArtWorkNames = new List<string> { artwork.Name }, // Thêm tên tác phẩm vào danh sách
                    ArtWorkImages = new List<string> { artwork.ArtWorkImage },
                    createdAt = offer.CreatedAt,
                    updatedAt = offer.UpdatedAt,
                    deletedAt = offer.DeletedAt,
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

        [HttpPut("update-status-Admin/{offerCode}")]
        [Authorize(Roles = "Super Admin, Artist, User")]
        public async Task<IActionResult> UpdateOfferStatus(string offerCode, [FromForm] UpdateStatusRequest request)
        {
            // Tìm đề xuất với OfferCode tương ứng
            var offer = await _context.Offer
                .Include(o => o.User)
                .Include(o => o.OfferArtWorks)
                .ThenInclude(oa => oa.ArtWork)
                .FirstOrDefaultAsync(o => o.OfferCode == offerCode);

            if (offer == null)
            {
                return NotFound("Offer not found");
            }

            if (offer.User == null)
            {
                return NotFound("User associated with the offer not found");
            }

            // Kiểm tra hành động cần thực hiện
            switch (request.Action)
            {
                case "accept":
                    // Chấp nhận offer
                    offer.Status = 1; // Trạng thái chấp nhận
                    string paymentLink = $"http://localhost:5000/payment/{offerCode}";

                    // Gửi email thông báo chấp nhận
                    await _emailService.SendEmailAsync(new Mailrequest
                    {
                        ToEmail = offer.User.Email,
                        Subject = "Offer Accepted",
                        Body = $"Your offer with Code {offerCode} has been accepted. You can complete the payment process by clicking <a href='{paymentLink}'>here</a>."
                    });

                    // Tìm tất cả các offer khác liên quan đến cùng một artwork và từ chối chúng
                    var relatedOffers = await _context.Offer
                        .Include(o => o.User)
                        .Where(o => o.ArtWorkId == offer.ArtWorkId && o.Id != offer.Id)
                        .ToListAsync();

                    foreach (var relatedOffer in relatedOffers)
                    {
                        if (relatedOffer.User != null)
                        {
                            relatedOffer.Status = -1; // Trạng thái bị từ chối
                                                      // Gửi email thông báo từ chối
                            await _emailService.SendEmailAsync(new Mailrequest
                            {
                                ToEmail = relatedOffer.User.Email,
                                Subject = "Offer Rejected",
                                Body = $"Your offer with Code {relatedOffer.OfferCode} has been rejected."
                            });
                        }
                    }
                    break;

                case "reject":
                    // Hủy offer
                    offer.Status = -1; // Trạng thái hủy
                                       // Gửi email thông báo hủy
                    await _emailService.SendEmailAsync(new Mailrequest
                    {
                        ToEmail = offer.User.Email,
                        Subject = "Offer Cancelled",
                        Body = $"Your offer with Code {offerCode} has been cancelled."
                    });
                    break;

                case "isPaid":
                    // Đánh dấu offer là đã thanh toán
                    offer.IsPaid = 1; // Trạng thái đã thanh toán
                                      // Gửi email thông báo thanh toán thành công
                    await _emailService.SendEmailAsync(new Mailrequest
                    {
                        ToEmail = offer.User.Email,
                        Subject = "Payment Success",
                        Body = $"Your offer with Code {offerCode} has been paid."
                    });
                    break;

                default:
                    return BadRequest("Invalid action");
            }

            _context.Entry(offer).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OfferExists(offer.Id))
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

        private bool OfferExists(int id)
        {
            return _context.Offer.Any(e => e.Id == id);
        }




        //[HttpGet("GetOfferAllArtist")]
        //[Authorize]
        //public async Task<IActionResult> GetOfferAllArtist()
        //{
        //    try
        //    {
        //        // Lấy userId của người dùng đăng nhập
        //        var userIdentity = HttpContext.User.Identity as ClaimsIdentity;
        //        var userIdClaim = userIdentity.FindFirst(ClaimTypes.NameIdentifier);

        //        if (userIdClaim == null)
        //        {
        //            return Unauthorized(new { message = "Not Authorized" });
        //        }

        //        int userId = Convert.ToInt32(userIdClaim.Value);

        //        // Lấy danh sách nghệ sĩ mà người dùng đăng nhập đã liên kết
        //        var userArtists = await _context.UserArtist
        //            .Where(ua => ua.UserId == userId)
        //            .Select(ua => ua.ArtistId)
        //            .ToListAsync();

        //        // Lấy danh sách các tác phẩm của các nghệ sĩ mà người dùng đăng nhập đã liên kết
        //        var artistArtWorkIds = await _context.ArtistArtWork
        //            .Where(aaw => userArtists.Contains(aaw.ArtistId))
        //            .Select(aaw => aaw.ArtWorkId)
        //            .ToListAsync();

        //        // Lấy tất cả các đề xuất từ cơ sở dữ liệu cho các tác phẩm được liên kết với nghệ sĩ
        //        List<Offer> offers = await _context.Offer
        //            .Include(o => o.User) // Nạp thông tin người dùng
        //            .Include(o => o.OfferArtWorks).ThenInclude(o => o.ArtWork)
        //            .Where(o => artistArtWorkIds.Contains(o.ArtWorkId))
        //            .OrderByDescending(p => p.Id)
        //            .ToListAsync();
        //        List<OfferDTO> result = new List<OfferDTO>();
        //        // Chuyển đổi danh sách các đề xuất thành đối tượng DTO để hiển thị
        //        foreach (var offer in offers)
        //        {
        //            result.Add(new OfferDTO
        //            {
        //                Id = offer.Id,
        //                UserId = offer.UserId,
        //                UserName = offer.User.Fullname,
        //                OfferPrice = offer.OfferPrice,
        //                ArtWorkId = offer.ArtWorkId,
        //                OfferCode = offer.OfferCode,
        //                ToTal = offer.Total,
        //                ArtWorkNames = offer.OfferArtWorks.Select(oaw => oaw.ArtWork.Name).ToList(),
        //                ArtWorkImages = offer.OfferArtWorks.Select(oaw => oaw.ArtWork.ArtWorkImage).ToList(),
        //                IsPaid = offer.IsPaid,
        //                Status = offer.Status,
        //                createdAt = offer.CreatedAt,
        //                updatedAt = offer.UpdatedAt,
        //                deletedAt = offer.DeletedAt
        //            });
        //        }
        //        // Trả về danh sách các đề xuất dưới dạng kết quả
        //        return Ok(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        // Xử lý ngoại lệ nếu có lỗi xảy ra
        //        var response = new GeneralService
        //        {
        //            Success = false,
        //            StatusCode = 400,
        //            Message = ex.Message,
        //            Data = ""
        //        };

        //        return BadRequest(response);
        //    }
        //}

        //[HttpGet("GetOfferDetailArtist/{code_order}")]

        //[Authorize]
        //public async Task<IActionResult> GetOfferDetailArtist(string code_order)
        //{
        //    try
        //    {
        //        // Lấy userId của người dùng đăng nhập
        //        var userIdentity = HttpContext.User.Identity as ClaimsIdentity;
        //        var userIdClaim = userIdentity.FindFirst(ClaimTypes.NameIdentifier);

        //        if (userIdClaim == null)
        //        {
        //            return Unauthorized(new { message = "Not Authorized" });
        //        }

        //        int userId = Convert.ToInt32(userIdClaim.Value);

        //        // Lấy danh sách nghệ sĩ mà người dùng đăng nhập đã liên kết
        //        var userArtists = await _context.UserArtist
        //            .Where(ua => ua.UserId == userId)
        //            .Select(ua => ua.ArtistId)
        //            .ToListAsync();

        //        // Lấy danh sách các tác phẩm của các nghệ sĩ mà người dùng đăng nhập đã liên kết
        //        var artistArtWorkIds = await _context.ArtistArtWork
        //            .Where(aaw => userArtists.Contains(aaw.ArtistId))
        //            .Select(aaw => aaw.ArtWorkId)
        //            .ToListAsync();

        //        // Lấy thông tin đề xuất từ mã đề xuất (offer code) và kiểm tra xem có liên kết với các tác phẩm của nghệ sĩ không
        //        Offer offer = await _context.Offer
        //            .Include(o => o.User) // Nạp thông tin người dùng
        //            .Include(o => o.OfferArtWorks).ThenInclude(o => o.ArtWork)
        //            .FirstOrDefaultAsync(x => x.OfferCode.Equals(code_order) && artistArtWorkIds.Contains(x.ArtWorkId) && x.DeletedAt == null);

        //        if (offer != null)
        //        {
        //            OfferDTO result = new OfferDTO
        //            {
        //                Id = offer.Id,
        //                UserId = offer.UserId,
        //                UserName = offer.User.Fullname,
        //                OfferPrice = offer.OfferPrice,
        //                ArtWorkId = offer.ArtWorkId,
        //                OfferCode = offer.OfferCode,
        //                ToTal = offer.Total,
        //                ArtWorkNames = offer.OfferArtWorks.Select(oaw => oaw.ArtWork.Name).ToList(),
        //                ArtWorkImages = offer.OfferArtWorks.Select(oaw => oaw.ArtWork.ArtWorkImage).ToList(),
        //                Address = offer.Address,
        //                Status = offer.Status,
        //                IsPaid = offer.IsPaid,
        //                createdAt = offer.CreatedAt,
        //                updatedAt = offer.UpdatedAt,
        //                deletedAt = offer.DeletedAt
        //            };
        //            return Ok(result);
        //        }
        //        else
        //        {
        //            return NotFound();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Xử lý ngoại lệ nếu có lỗi xảy ra
        //        var response = new GeneralService
        //        {
        //            Success = false,
        //            StatusCode = 400,
        //            Message = ex.Message,
        //            Data = ""
        //        };

        //        return BadRequest(response);
        //    }
        //}

    }
}
