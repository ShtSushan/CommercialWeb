using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceStore.Data;
using EcommerceStore.Models;
using EcommerceStore.Models.DTOs;
using System.Security.Cryptography;
using System.Text;

namespace EcommerceStore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/users/register
        [HttpPost("register")]
        public async Task<ActionResult<UserResponseDto>> Register(UserRegistrationDto registrationDto)
        {
            try
            {
                // Check if user already exists
                if (await _context.Users.AnyAsync(u => u.Email == registrationDto.Email))
                {
                    return BadRequest(new { message = "User with this email already exists" });
                }

                // Hash the password (in production, use a proper password hashing library like BCrypt)
                var hashedPassword = HashPassword(registrationDto.Password);

                var user = new User
                {
                    Name = registrationDto.Name,
                    Email = registrationDto.Email,
                    Password = hashedPassword,
                    Address = registrationDto.Address,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var userResponse = new UserResponseDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Address = user.Address,
                    CreatedAt = user.CreatedAt
                };

                return CreatedAtAction(nameof(GetUser), new { id = user.Id }, userResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error during registration", error = ex.Message });
            }
        }

        // POST: api/users/login
        [HttpPost("login")]
        public async Task<ActionResult<UserResponseDto>> Login(UserLoginDto loginDto)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

                if (user == null || !VerifyPassword(loginDto.Password, user.Password))
                {
                    return Unauthorized(new { message = "Invalid email or password" });
                }

                var userResponse = new UserResponseDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Address = user.Address,
                    CreatedAt = user.CreatedAt
                };

                return Ok(userResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error during login", error = ex.Message });
            }
        }

        // GET: api/users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponseDto>> GetUser(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);

                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                var userResponse = new UserResponseDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Address = user.Address,
                    CreatedAt = user.CreatedAt
                };

                return Ok(userResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving user", error = ex.Message });
            }
        }

        // PUT: api/users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserRegistrationDto updateDto)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                // Check if email is being changed and if it already exists
                if (user.Email != updateDto.Email && await _context.Users.AnyAsync(u => u.Email == updateDto.Email))
                {
                    return BadRequest(new { message = "Email already exists" });
                }

                user.Name = updateDto.Name;
                user.Email = updateDto.Email;
                user.Address = updateDto.Address;

                // Only update password if provided
                if (!string.IsNullOrEmpty(updateDto.Password))
                {
                    user.Password = HashPassword(updateDto.Password);
                }

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating user", error = ex.Message });
            }
        }

        // GET: api/users/5/orders
        [HttpGet("{id}/orders")]
        public async Task<ActionResult<IEnumerable<OrderResponseDto>>> GetUserOrders(int id)
        {
            try
            {
                var orders = await _context.Orders
                    .Include(o => o.OrderItems)
                    .Where(o => o.UserId == id)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();

                var orderResponses = orders.Select(o => new OrderResponseDto
                {
                    Id = o.Id,
                    UserId = o.UserId,
                    UserName = o.UserName,
                    UserEmail = o.UserEmail,
                    ShippingAddress = o.ShippingAddress,
                    TotalAmount = o.TotalAmount,
                    OrderDate = o.OrderDate,
                    Status = o.Status,
                    Items = o.OrderItems.Select(oi => new OrderItemDto
                    {
                        ProductId = oi.ProductId,
                        ProductName = oi.ProductName,
                        Price = oi.Price,
                        Quantity = oi.Quantity,
                        Total = oi.Total
                    }).ToList()
                }).ToList();

                return Ok(orderResponses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving user orders", error = ex.Message });
            }
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            var hashedInput = HashPassword(password);
            return hashedInput == hashedPassword;
        }
    }
}