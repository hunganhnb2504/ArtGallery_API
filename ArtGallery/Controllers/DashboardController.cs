
﻿using ArtGallery.DTOs;
using ArtGallery.Entities;
using ArtGallery.Models.GeneralService;
using ArtGallery.Service.Artists;
using ArtGallery.Service.IMG;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ArtGallery.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly ArtGalleryApiContext _context;

        public DashboardController(ArtGalleryApiContext context)
        {
            _context = context;

        }

        // Endpoint để lấy tổng số lượng offer
        [HttpGet("total-count-offer")]
        [Authorize(Roles = "Super Admin")]
        public async Task<IActionResult> GetTotalOfferCount()
        {
            try
            {
                // Sử dụng LINQ để đếm số lượng offer trong cơ sở dữ liệu
                var totalOffer = await _context.Offer.Where(m => m.DeletedAt == null).CountAsync();

                // Trả về số lượng offer trong response
                return Ok(totalOffer);
            }
            catch (Exception ex)
            {
                // Xử lý nếu có lỗi xảy ra
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("total-count-user")]
        [Authorize(Roles = "Super Admin")]
        public async Task<IActionResult> GetUserCount()
        {
            var userCount = new
            {
                TotalUser = await _context.Users.Where(u => u.Role.Equals("User")).CountAsync(),
                TotalArtist = await _context.Users.Where(u => u.Role.Equals("Artist")).CountAsync(),
            };

            return Ok(userCount);
        }

        [HttpGet("total-count-artist")]
        [Authorize(Roles = "Super Admin")]
        public async Task<IActionResult> GetTotalArtistCount()
        {
            try
            {
                // Sử dụng LINQ để đếm số lượng artist trong cơ sở dữ liệu
                var totalArtist = await _context.Artist.Where(m => m.DeletedAt == null).CountAsync();

                // Trả về số lượng artist trong response
                return Ok(totalArtist);
            }
            catch (Exception ex)
            {
                // Xử lý nếu có lỗi xảy ra
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("total-count-artwork")]
        [Authorize(Roles = "Super Admin")]
        public async Task<IActionResult> GetTotalArtworkCount()
        {
            try
            {
                // Sử dụng LINQ để đếm số lượng artwork trong cơ sở dữ liệu
                var totalArtwork = await _context.ArtWork.Where(m => m.DeletedAt == null).CountAsync();

                // Trả về số lượng artwork trong response
                return Ok(totalArtwork);
            }
            catch (Exception ex)
            {
                // Xử lý nếu có lỗi xảy ra
                return StatusCode(500, "Internal server error");
            }
        }   

        [HttpGet("revenue/yearly")]
        [Authorize(Roles = "Super Admin")]
        public IActionResult GetYearlySales()
        {
            int currentYear = DateTime.UtcNow.Year;
            int startYear = currentYear - 4;
            List<int> allYears = Enumerable.Range(startYear, 5).ToList();
            var yearlySales = allYears
       .GroupJoin(_context.Offer.Where(o => o.IsPaid == 1), // Lọc các đơn hàng đã thanh toán
           year => year,
           order => order.CreatedAt.Value.Year,
           (year, orders) => new
           {
               Year = year,
               TotalSales = orders.Sum(o => o.Total)
           })
       .OrderBy(result => result.Year)
       .ToList();

            return Ok(yearlySales);
        }

        [HttpGet("revenue/monthly/{year}")]
        [Authorize(Roles = "Super Admin")]
        public IActionResult GetMonthlySales(int year)
        {
            // Validate the input year
            if (year <= 0)
            {
                return BadRequest("Invalid year parameter.");
            }
            DateTime startDate = new DateTime(year, 1, 1);
            List<DateTime> allMonthsOfYear = Enumerable.Range(0, 12)
                .Select(offset => startDate.AddMonths(offset))
                .ToList();
            var monthlySales = allMonthsOfYear
         .GroupJoin(_context.Offer.Where(o => o.IsPaid == 1),
             date => new { Year = date.Year, Month = date.Month },
             order => new { Year = order.CreatedAt.Value.Year, Month = order.CreatedAt.Value.Month },
             (date, orders) => new
             {
                 Year = date.Year,
                 Month = date.Month,
                 TotalSales = orders.Sum(o => o.Total)
             })
         .OrderBy(result => result.Year)
         .ThenBy(result => result.Month)
         .Select(result => new
         {
             Year = result.Year,
             Month = result.Month,
             TotalSales = result.TotalSales
         })
         .ToList();


            return Ok(monthlySales);
        }

        [HttpGet("revenue/weekly")]
        [Authorize(Roles = "Super Admin")]
        public IActionResult GetWeeklySales()
        {
            DateTime today = DateTime.UtcNow.Date;
            List<DateTime> past7Days = Enumerable.Range(0, 7)
                .Select(offset => today.AddDays(-offset))
                .ToList();
            var past7DaysSales = past7Days
        .GroupJoin(_context.Offer.Where(o => o.IsPaid == 1),
            date => date.Date,
            order => order.CreatedAt.Value.Date,
            (date, orders) => new
            {
                Date = date,
                TotalSales = orders.Sum(o => o.Total)
            })
        .Select(result => new
        {
            Date = result.Date,
            TotalSales = result.TotalSales
        })
        .ToList();


            return Ok(past7DaysSales);
        }


        [HttpGet("total-revenue")]
        [Authorize(Roles = "Super Admin")]
        public async Task<IActionResult> GetTotalRevenue()
        {
            try
            {
                var offers = await _context.Offer
                    .Where(o => o.IsPaid == 1)
                    .ToListAsync();
                var totalRevenue = offers.Sum(o => o.Total);

                return Ok(new { totalRevenue });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/Offer/TotalOfferToday
        [HttpGet("total-offer-today")]
        [Authorize(Roles = "Super Admin")]
        public async Task<IActionResult> GetTotalOfferToday()
        {
            try
            {
                DateTime today = DateTime.Today;
                var totalOfferToday = await _context.Offer
                    .Where(o => o.CreatedAt.HasValue && o.CreatedAt.Value.Date == today)
                    .CountAsync();

                return Ok(new { totalOfferToday });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("list-offer-today")]
        [Authorize(Roles = "Super Admin")]
        public async Task<IActionResult> GetListOfferToDay()
        {
            try
            {
                DateTime today = DateTime.Now.Date;
                List<Offer> offers = await _context.Offer.Include(o => o.User).Include(o => o.OfferArtWorks)
        .ThenInclude(oaw => oaw.ArtWork).Where(o => o.CreatedAt.Value.Date == today).OrderByDescending(p => p.CreatedAt).ToListAsync();
                List<OfferDTO> result = new List<OfferDTO>();
                foreach (var offer in offers)
                {
                    result.Add(new OfferDTO
                    {
                        OfferCode = offer.OfferCode,
                        ArtWorkId = offer.ArtWorkId,
                        UserId = offer.UserId,
                        //UserName = offer.User.Fullname,
                        ToTal = offer.Total,
                        Status = offer.Status,
                        ArtWorkNames = offer.OfferArtWorks.Select(oaw => oaw.ArtWork.Name).ToList(),
                        ArtWorkImages = offer.OfferArtWorks.Select(oaw => oaw.ArtWork.ArtWorkImage).ToList(),
                        OfferPrice = offer.OfferPrice,
                        createdAt = offer.CreatedAt,
                        updatedAt = offer.UpdatedAt,
                        deletedAt = offer.DeletedAt,
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

        [HttpGet("total-sold-artwork-artist")]
        [Authorize(Roles = "Artist")]
        public async Task<IActionResult> GetTotalArtworkSoldCount()
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

                // Lấy thông tin nghệ sĩ liên kết với người dùng hiện tại
                var artist = await _context.UserArtist
                    .Include(ua => ua.Artist)
                    .FirstOrDefaultAsync(ua => ua.UserId == userId);

                if (artist == null)
                {
                    return BadRequest(new GeneralService
                    {
                        Success = false,
                        StatusCode = 400,
                        Message = "Artist not found for the current user",
                        Data = ""
                    });
                }

                // Đếm số lượng tác phẩm nghệ thuật đã được bán của nghệ sĩ
                var totalSoldArtworks = await _context.OfferArtWork
            .Include(oaw => oaw.ArtWork)
            .Where(oaw => oaw.ArtWork.DeletedAt == null
                          && oaw.ArtWork.ArtistArtWorks.Any(aa => aa.ArtistId == artist.ArtistId)
                          && oaw.Offer.IsPaid == 1)
            .CountAsync();

                // Trả về số lượng tác phẩm nghệ thuật đã được bán
                return Ok(totalSoldArtworks);
            }
            catch (Exception ex)
            {
                // Xử lý nếu có lỗi xảy ra
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("list-offer-today-artist")]
        [Authorize(Roles = "Artist")]
        public async Task<IActionResult> GetListOfferToDayArtist()
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

                // Lấy thông tin nghệ sĩ liên kết với người dùng hiện tại
                var artist = await _context.UserArtist
                    .Include(ua => ua.Artist)
                    .FirstOrDefaultAsync(ua => ua.UserId == userId);

                if (artist == null)
                {
                    return BadRequest(new GeneralService
                    {
                        Success = false,
                        StatusCode = 400,
                        Message = "Artist not found for the current user",
                        Data = ""
                    });
                }

                DateTime today = DateTime.Now.Date;

                // Lọc các Offer theo ngày hiện tại và nghệ sĩ
                var offers = await _context.Offer
                    .Include(o => o.User)
                    .Include(o => o.OfferArtWorks)
                        .ThenInclude(oaw => oaw.ArtWork)
                    .Where(o => o.CreatedAt.HasValue
                                && o.CreatedAt.Value.Date == today
                                && o.OfferArtWorks.Any(oaw => oaw.ArtWork.ArtistArtWorks.Any(aa => aa.ArtistId == artist.ArtistId)))
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                var result = offers.Select(offer => new OfferDTO
                {
                    OfferCode = offer.OfferCode,
                    ArtWorkId = offer.ArtWorkId,
                    UserId = offer.UserId,
                    UserName = offer.User.Fullname,
                    ToTal = offer.Total,
                    Status = offer.Status,
                    ArtWorkNames = offer.OfferArtWorks.Select(oaw => oaw.ArtWork.Name).ToList(),
                    ArtWorkImages = offer.OfferArtWorks.Select(oaw => oaw.ArtWork.ArtWorkImage).ToList(),
                    OfferPrice = offer.OfferPrice,
                    createdAt = offer.CreatedAt,
                    updatedAt = offer.UpdatedAt,
                    deletedAt = offer.DeletedAt,
                }).ToList();

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

        [HttpGet("total-offer-today-artist")]
        [Authorize(Roles = "Artist")]
        public async Task<IActionResult> GetTotalOfferTodayArtist()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;

            if (!identity.IsAuthenticated)
            {
                return Unauthorized(new { message = "Not Authorized" });
            }

            try
            {
                var userIdClaim = identity.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized(new { message = "Not Authorized" });
                }

                var userId = Convert.ToInt32(userIdClaim.Value);

                // Lấy thông tin nghệ sĩ liên kết với người dùng hiện tại
                var artist = await _context.UserArtist
                    .Include(ua => ua.Artist)
                    .FirstOrDefaultAsync(ua => ua.UserId == userId);

                if (artist == null)
                {
                    return BadRequest(new { message = "Artist not found for the current user" });
                }

                DateTime today = DateTime.Today;

                // Lọc các Offer theo ngày hiện tại và nghệ sĩ
                var totalOfferToday = await _context.Offer
                    .Include(o => o.OfferArtWorks)
                    .Where(o => o.CreatedAt.HasValue
                                && o.CreatedAt.Value.Date == today
                                && o.OfferArtWorks.Any(oaw => oaw.ArtWork.ArtistArtWorks.Any(aa => aa.ArtistId == artist.ArtistId)))
                    .CountAsync();

                return Ok(new { totalOfferToday });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("total-revenue-artist")]
        [Authorize(Roles = "Artist")]
        public async Task<IActionResult> GetTotalRevenueArtist()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;

            if (!identity.IsAuthenticated)
            {
                return Unauthorized(new { message = "Not Authorized" });
            }

            try
            {
                var userIdClaim = identity.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized(new { message = "Not Authorized" });
                }

                var userId = Convert.ToInt32(userIdClaim.Value);

                // Lấy thông tin nghệ sĩ liên kết với người dùng hiện tại
                var artist = await _context.UserArtist
                    .Include(ua => ua.Artist)
                    .FirstOrDefaultAsync(ua => ua.UserId == userId);

                if (artist == null)
                {
                    return BadRequest(new { message = "Artist not found for the current user" });
                }

                // Lấy danh sách các Offer liên quan đến các tác phẩm nghệ thuật của nghệ sĩ
                var offers = await _context.Offer
                    .Include(o => o.OfferArtWorks)
                    .Where(o => o.IsPaid == 1
                                && o.OfferArtWorks.Any(oaw => oaw.ArtWork.ArtistArtWorks.Any(aa => aa.ArtistId == artist.ArtistId)))
                    .ToListAsync();

                // Tính tổng doanh thu từ các Offer đã lấy được
                var totalRevenue = offers.Sum(o => o.Total);

                return Ok(new { totalRevenue });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("revenue/weekly/artist")]
        [Authorize(Roles = "Artist")]
        public async Task<IActionResult> GetWeeklySalesArtist()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;

            if (!identity.IsAuthenticated)
            {
                return Unauthorized(new { message = "Not Authorized" });
            }

            try
            {
                var userIdClaim = identity.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized(new { message = "Not Authorized" });
                }

                var userId = Convert.ToInt32(userIdClaim.Value);

                // Lấy thông tin nghệ sĩ liên kết với người dùng hiện tại
                var artist = await _context.UserArtist
                    .Include(ua => ua.Artist)
                    .FirstOrDefaultAsync(ua => ua.UserId == userId);

                if (artist == null)
                {
                    return BadRequest(new { message = "Artist not found for the current user" });
                }

                DateTime today = DateTime.UtcNow.Date;
                List<DateTime> past7Days = Enumerable.Range(0, 7)
                    .Select(offset => today.AddDays(-offset))
                    .ToList();

                var past7DaysSales = past7Days
                    .GroupJoin(_context.Offer
                                    .Where(o => o.IsPaid == 1
                                                && o.OfferArtWorks.Any(oaw => oaw.ArtWork.ArtistArtWorks.Any(aa => aa.ArtistId == artist.ArtistId))),
                                date => date.Date,
                                order => order.CreatedAt.Value.Date,
                                (date, orders) => new
                                {
                                    Date = date,
                                    TotalSales = orders.Sum(o => o.Total)
                                })
                    .Select(result => new
                    {
                        Date = result.Date,
                        TotalSales = result.TotalSales
                    })
                    .ToList();

                return Ok(past7DaysSales);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("revenue/monthly/artist/{year}")]
        [Authorize(Roles = "Artist")]
        public async Task<IActionResult> GetMonthlySalesArtist(int year)
        {
            // Validate the input year
            if (year <= 0)
            {
                return BadRequest("Invalid year parameter.");
            }

            var identity = HttpContext.User.Identity as ClaimsIdentity;

            if (!identity.IsAuthenticated)
            {
                return Unauthorized(new { message = "Not Authorized" });
            }

            try
            {
                var userIdClaim = identity.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized(new { message = "Not Authorized" });
                }

                var userId = Convert.ToInt32(userIdClaim.Value);

                // Lấy thông tin nghệ sĩ liên kết với người dùng hiện tại
                var artist = await _context.UserArtist
                    .Include(ua => ua.Artist)
                    .FirstOrDefaultAsync(ua => ua.UserId == userId);

                if (artist == null)
                {
                    return BadRequest(new { message = "Artist not found for the current user" });
                }

                DateTime startDate = new DateTime(year, 1, 1);
                List<DateTime> allMonthsOfYear = Enumerable.Range(0, 12)
                    .Select(offset => startDate.AddMonths(offset))
                    .ToList();

                var monthlySales = allMonthsOfYear
                    .GroupJoin(_context.Offer
                                    .Where(o => o.IsPaid == 1
                                                && o.OfferArtWorks.Any(oaw => oaw.ArtWork.ArtistArtWorks.Any(aa => aa.ArtistId == artist.ArtistId))),
                                date => new { Year = date.Year, Month = date.Month },
                                order => new { Year = order.CreatedAt.Value.Year, Month = order.CreatedAt.Value.Month },
                                (date, orders) => new
                                {
                                    Year = date.Year,
                                    Month = date.Month,
                                    TotalSales = orders.Sum(o => o.Total)
                                })
                    .OrderBy(result => result.Year)
                    .ThenBy(result => result.Month)
                    .Select(result => new
                    {
                        Year = result.Year,
                        Month = result.Month,
                        TotalSales = result.TotalSales
                    })
                    .ToList();

                return Ok(monthlySales);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("revenue/yearly/artist")]
        [Authorize(Roles = "Artist")]
        public async Task<IActionResult> GetYearlySalesArtist()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;

            if (!identity.IsAuthenticated)
            {
                return Unauthorized(new { message = "Not Authorized" });
            }

            try
            {
                var userIdClaim = identity.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized(new { message = "Not Authorized" });
                }

                var userId = Convert.ToInt32(userIdClaim.Value);

                // Lấy thông tin nghệ sĩ liên kết với người dùng hiện tại
                var artist = await _context.UserArtist
                    .Include(ua => ua.Artist)
                    .FirstOrDefaultAsync(ua => ua.UserId == userId);

                if (artist == null)
                {
                    return BadRequest(new { message = "Artist not found for the current user" });
                }

                int currentYear = DateTime.UtcNow.Year;
                int startYear = currentYear - 4;
                List<int> allYears = Enumerable.Range(startYear, 5).ToList();

                var yearlySales = allYears
                    .GroupJoin(_context.Offer
                                    .Where(o => o.IsPaid == 1
                                                && o.OfferArtWorks.Any(oaw => oaw.ArtWork.ArtistArtWorks.Any(aa => aa.ArtistId == artist.ArtistId))),
                                year => year,
                                order => order.CreatedAt.Value.Year,
                                (year, orders) => new
                                {
                                    Year = year,
                                    TotalSales = orders.Sum(o => o.Total)
                                })
                    .OrderBy(result => result.Year)
                    .ToList();

                return Ok(yearlySales);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet("total-count-artwork-artist")]
        [Authorize]
        public async Task<IActionResult> GetTotalArtworkCountArtist()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;

            if (!identity.IsAuthenticated)
            {
                return Unauthorized(new { message = "Not Authorized" });
            }

            try
            {
                var userIdClaim = identity.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized(new { message = "Not Authorized" });
                }

                var userId = Convert.ToInt32(userIdClaim.Value);

                // Lấy thông tin nghệ sĩ liên kết với người dùng hiện tại
                var artist = await _context.UserArtist
                    .Include(ua => ua.Artist)
                    .FirstOrDefaultAsync(ua => ua.UserId == userId);

                if (artist == null)
                {
                    return BadRequest(new { message = "Artist not found for the current user" });
                }

                // Đếm số lượng tác phẩm nghệ thuật của nghệ sĩ hiện tại
                var totalArtworkByArtist = await _context.ArtistArtWork
                    .Where(aa => aa.ArtistId == artist.ArtistId && aa.ArtWork.DeletedAt == null)
                    .CountAsync();

                return Ok(new { TotalArtworkByArtist = totalArtworkByArtist });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }




        
    }
}
