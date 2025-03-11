using ArtGallery.DTOs;
using ArtGallery.Entities;
using ArtGallery.Helper;
using ArtGallery.Models.ArtWork;
using ArtGallery.Models.GeneralService;
using ArtGallery.Models.SchoolOfArt;
using ArtGallery.Models.Users;
using ArtGallery.Service.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ArtGallery.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ArtGalleryApiContext _context;
        private readonly IConfiguration _config;
        private readonly IEmailService _emailService;
        public AuthController(ArtGalleryApiContext context, IConfiguration config, IEmailService emailService) 
        {
            _context = context;
            _config = config;
            _emailService = emailService;
        }

        private string GenerateToken(User  user)
        {
            var secretKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_config["JWT:Key"]));
            var signatureKey = new SigningCredentials(secretKey,
                                    SecurityAlgorithms.HmacSha256);
            var Token = new[]
            {
                new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
                new Claim(ClaimTypes.Email,user.Email),
                new Claim(ClaimTypes.Name,user.Fullname),
                new Claim(ClaimTypes.Role,user.Role),
            };
            var token = new JwtSecurityToken(
                    _config["JWT:Issuer"],
                    _config["JWT:Audience"],
                    Token,
                    expires: DateTime.Now.AddHours(2),
                    signingCredentials: signatureKey
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        private string GenerateResetToken()
        {
            return Guid.NewGuid().ToString();
        }


        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login(LoginModel model)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.Equals(model.Email));
                if (user == null)
                {
                    return BadRequest(new GeneralService
                    {
                        Success = false,
                        StatusCode = 404,
                        Message = "Invalid email/password"
                    });
                }
                bool verified = BCrypt.Net.BCrypt.Verify(model.Password, user.Password);
                if (!verified)
                {
                    return BadRequest(new GeneralService
                    {
                        Success = false,
                        StatusCode = 404,
                        Message = "Invalid email/password"
                    });
                }

                return Ok(new GeneralService
                {
                    Success = true,
                    Message = "Authenticate success",
                    Data = GenerateToken(user)
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            try
            {

                // Kiểm tra xem email đã tồn tại trong cơ sở dữ liệu hay chưa
                bool emailExists = await _context.Users.AnyAsync(c => c.Email == model.email);

                if (emailExists)
                {
                    return BadRequest(new GeneralService
                    {
                        Success = false,
                        StatusCode = 400,
                        Message = "Email already exists",
                        Data = ""
                    });
                }

                // hash password
                var salt = BCrypt.Net.BCrypt.GenerateSalt(10);
                var hassPassword = BCrypt.Net.BCrypt.HashPassword(model.password, salt);


                User data = new User
                {
                    Fullname = model.fullname,
                    Email = model.email,
                    Password = hassPassword,
                    Phone = model.phone,
                    Role = "User",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    DeletedAt = null,
                };

                _context.Users.Add(data);
                await _context.SaveChangesAsync();

                var mailRequest = new Mailrequest
                {
                    ToEmail = model.email,
                    Subject = "Confirmation Email",
                    Body = "Thank you for signing up. Please confirm your email address."
                };

                await _emailService.SendEmailAsync(mailRequest);


                return Created($"get-by-id?id={data.Id}", new UserDTO
                {
                    Id = data.Id,
                    fullname = data.Fullname,
                    birthday = data.Birthday,
                    email = data.Email,
                    phone = data.Phone,
                    role = data.Role,
                    createdAt = data.CreatedAt,
                    updatedAt = data.UpdatedAt,
                    deletedAt = data.DeletedAt,
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new GeneralService
                {
                    Success = false,
                    StatusCode = 400,
                    Message = ex.Message,
                    Data = ""
                });
            }
        }

        [HttpPost]
        [Route("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
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

                var user = await _context.Users.FindAsync(Convert.ToInt32(userId));
                if (user != null)
                {
                    bool verified = BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.Password);

                    // Kiểm tra mật khẩu hiện tại
                    if (verified)
                    {
                        // hash password
                        var salt = BCrypt.Net.BCrypt.GenerateSalt(10);
                        var hassNewPassword = BCrypt.Net.BCrypt.HashPassword(model.NewPassword, salt);
                        // Thực hiện thay đổi mật khẩu
                        user.Password = hassNewPassword;
                        _context.SaveChanges();
                        return Ok(new GeneralService
                        {
                            Success = true,
                            StatusCode = 200,
                            Message = "Password changed successfully",
                            Data = ""
                        });
                    }
                    else
                    {
                        return BadRequest(new GeneralService
                        {
                            Success = false,
                            StatusCode = 400,
                            Message = "Incorrect current password",
                            Data = ""
                        });
                    }
                }
                else
                {
                    return NotFound(new GeneralService
                    {
                        Success = false,
                        StatusCode = 404,
                        Message = "Incorrect current password",
                        Data = ""
                    });
                }

            }
            catch (Exception ex)
            {
                return BadRequest(new GeneralService
                {
                    Success = false,
                    StatusCode = 400,
                    Message = ex.Message,
                    Data = ""
                });
            }
        }
        [HttpPost]
        [Route("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel model)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(s => s.Email.Equals(model.Email));
                if (user == null)
                {
                    return BadRequest(new GeneralService       
                    {
                        Success = false,
                        StatusCode = 404,
                        Message = "Not found"
                    });
                }

                var resetToken = GenerateResetToken();
                user.ResetToken = resetToken;
                user.ResetTokenExpiry = DateTime.Now.AddHours(1); // Thời gian hết hiệu lực của token: 1 giờ
                await _context.SaveChangesAsync();

                var resetLink = "http://localhost:3001/reset-password/" + resetToken;

                Mailrequest mailrequest = new Mailrequest();
                mailrequest.ToEmail = user.Email;
                mailrequest.Subject = "Password Reset";
                mailrequest.Body = $"Click the link to reset your password: {resetLink}";

                await _emailService.SendEmailAsync(mailrequest);


                return Ok(new GeneralService
                {
                    Success = true,
                    Message = "Password reset email sent successfully"
                });
            }
            catch (Exception e)
            {
                return null;
            }
        }
        [HttpPost("reset-password/{token}")]
        public async Task<IActionResult> ResetPassword(string token, ResetPasswordModel model)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(s => s.Email.Equals(model.Email));
                if (user == null)
                {
                    return NotFound(new GeneralService
                    {
                        Success = false,
                        StatusCode = 404,
                        Message = "Not found"
                    });
                }

                // Kiểm tra tính hợp lệ của mã reset
                if (model == null || string.IsNullOrEmpty(token) || user.ResetToken != token || user.Email != model.Email || user.ResetTokenExpiry < DateTime.UtcNow)
                {
                    return BadRequest(new GeneralService
                    {
                        Success = false,
                        StatusCode = 400,
                        Message = "Invalid or expired reset token"
                    });
                }

                // Cập nhật mật khẩu
                var salt = BCrypt.Net.BCrypt.GenerateSalt(10);
                var hassNewPassword = BCrypt.Net.BCrypt.HashPassword(model.NewPassword, salt);

                user.Password = hassNewPassword; // Hash mật khẩu trước khi lưu
                user.ResetToken = null;
                user.ResetTokenExpiry = null;
                await _context.SaveChangesAsync();

                return Ok(new GeneralService
                {
                    Success = true,
                    Message = "Password reset successful"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("profile")]
        [Authorize]
        public async Task<IActionResult> Profile()
        {// get info form token
            var identity = HttpContext.User.Identity as ClaimsIdentity;

            if (!identity.IsAuthenticated)
            {
                return Unauthorized(new GeneralService { Success = false, StatusCode = 401, Message = "Not Authorized", Data = "" });
            }

            try
            {
                var userClaims = identity.Claims;
                var userId = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == Convert.ToInt32(userId));

                if (user == null)
                {
                    return NotFound(new GeneralService
                    {
                        Success = false,
                        StatusCode = 404,
                        Message = "Incorrect current password",
                        Data = ""
                    });
                }

                return Ok(new ProfileDTO
                {
                    Id = user.Id,
                    email = user.Email,
                    fullname = user.Fullname,
                    birthday = user.Birthday,
                    address = user.Address,
                    phone = user.Phone,
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new GeneralService
                {
                    Success = false,
                    StatusCode = 404,
                    Message = ex.Message,
                    Data = ""
                });
            }
        }


        [HttpPut]
        [Route("update-profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfile model)
        {
            if (ModelState.IsValid)
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

                    user.Birthday = model.birthday;
                    user.Phone = model.phone;
                    user.Address = model.address;
                    user.UpdatedAt = DateTime.Now;

                    _context.Users.Update(user);
                    await _context.SaveChangesAsync();

                    return NoContent();

                }
                catch (Exception ex)
                {
                    return BadRequest(new GeneralService
                    {
                        Success = false,
                        StatusCode = 404,
                        Message = ex.Message,
                        Data = ""
                    });
                }
            }

            return BadRequest(new GeneralService
            {
                Success = false,
                StatusCode = 404,
                Message = "",
                Data = ""
            });

        }


        [HttpGet]
        [Route("user")]
        [Authorize(Roles = "Super Admin")]
        public async Task<IActionResult> GetUserAll(
        [FromQuery] string search = null)
        {
            try
            {
                // Bắt đầu với truy vấn gốc để lấy danh sách người dùng
                var query = _context.Users.Where(a => a.DeletedAt == null && a.Role == "User" || a.Role == "Artist");

                // Áp dụng bộ lọc tìm kiếm
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(a => a.Fullname.Contains(search));
                }

                List<User> users = await query.OrderByDescending(m => m.Id).ToListAsync();
                List<UserDTO> result = new List<UserDTO>();

                foreach (User user in users)
                {
                    var userDTO = new UserDTO
                    {
                        Id = user.Id,
                        fullname = user.Fullname,
                        role = user.Role,
                        phone = user.Phone,
                        birthday = user.Birthday,
                        email = user.Email,
                        address = user.Address,
                        createdAt = user.CreatedAt,
                        updatedAt = user.UpdatedAt,
                        deletedAt = user.DeletedAt,
                    };

                    result.Add(userDTO);
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


    }


}
