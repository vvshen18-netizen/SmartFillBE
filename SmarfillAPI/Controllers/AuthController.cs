using AuthAPI.Data;
using AuthAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using SmarfillAPI.DTO;
using Microsoft.AspNetCore.Identity;
using SmarfillAPI.Models;

namespace AuthAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // 🔵 Normal User Registration
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                return BadRequest("User with this email already exists.");

            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
                return BadRequest("User with this username already exists.");

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                ContactNumber = request.ContactNumber,
                Role = "User"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("User Registered Successfully");
        }

        // 🔵 Delivery Guy Registration
        [HttpPost("register-deliveryguy")]
        public async Task<IActionResult> RegisterDeliveryGuy([FromBody] RegisterRequest request)
        {
            if (await _context.DeliveryGuys.AnyAsync(d => d.Email == request.Email))
                return BadRequest("A delivery guy with this email already exists.");

            var deliveryGuy = new DeliveryGuy
            {
                Username = request.Username,
                Email = request.Email,
                ContactNumber = request.ContactNumber,
                LicensePhoto = request.LicensePhoto,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password), // ✅ Add this line
                Status = "Pending",
            };

            _context.DeliveryGuys.Add(deliveryGuy);
            await _context.SaveChangesAsync();

            return Ok("Delivery Guy registered successfully and waiting for approval.");
        }



        // 🔵 Login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            Console.WriteLine($"Attempting login with email: {request.Email}");

            // 🔍 First try User table
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user != null)
            {
                Console.WriteLine($"Found in User table. Username: {user.Username}, Role: {user.Role}");

                var passwordMatch = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
                if (!passwordMatch)
                    return Unauthorized("Invalid Credentials");

                if (user.Role == "Pending Delivery Guy")
                    return Unauthorized("Your account is pending admin approval.");

                var token = GenerateJwtToken(user.Email, user.Role, user.Username, user.ContactNumber);
                return Ok(new
                {
                    token,
                    username = user.Username,
                    role = user.Role,
                    email = user.Email,
                    contactNumber = user.ContactNumber
                });
            }

            // 🔍 Then try DeliveryGuy table
            var deliveryGuy = await _context.DeliveryGuys.FirstOrDefaultAsync(d => d.Email == request.Email);
            if (deliveryGuy != null)
            {
                Console.WriteLine($"Found in DeliveryGuy table. Username: {deliveryGuy.Username}");

                var passwordMatch = BCrypt.Net.BCrypt.Verify(request.Password, deliveryGuy.PasswordHash);
                if (!passwordMatch)
                    return Unauthorized("Invalid Credentials");

                if (deliveryGuy.Status != "Approved")
                    return Unauthorized("Your delivery guy account is pending admin approval.");

                var token = GenerateJwtToken(deliveryGuy.Email, "Delivery Guy", deliveryGuy.Username, deliveryGuy.ContactNumber);
                return Ok(new
                {
                    token,
                    username = deliveryGuy.Username,
                    role = "Delivery Guy",
                    email = deliveryGuy.Email,
                    contactNumber = deliveryGuy.ContactNumber
                });
            }

            return Unauthorized("The Email is not Registered\nPlease Register First");
        }


        [Authorize]
        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var email = User.FindFirstValue(ClaimTypes.Name);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
                return Unauthorized("User not found.");

            // ✅ Update name if provided
            if (!string.IsNullOrWhiteSpace(request.NewName))
                user.Username = request.NewName;

            // ✅ Only attempt password update if either password field is filled
            if (!string.IsNullOrWhiteSpace(request.CurrentPassword) || !string.IsNullOrWhiteSpace(request.NewPassword))
            {
                // Require both fields to be filled
                if (string.IsNullOrWhiteSpace(request.CurrentPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
                    return BadRequest("Both current and new passwords are required to change the password.");

                if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
                    return Unauthorized("Current password is incorrect.");

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            }

            await _context.SaveChangesAsync();
            return Ok("Profile updated successfully.");
        }

        [Authorize]
        [HttpPut("update-deliveryguy-profile")]
        public async Task<IActionResult> UpdateDeliveryGuyProfile([FromBody] DeliveryGuyProfileUpdateDto dto)
        {
            var email = User.FindFirstValue(ClaimTypes.Name);
            if (email == null)
                return Unauthorized("Invalid token.");

            var deliveryGuy = await _context.DeliveryGuys.FirstOrDefaultAsync(x => x.Email == email);
            if (deliveryGuy == null)
                return NotFound("Delivery Guy not found.");

            // ✅ Update name if provided
            if (!string.IsNullOrWhiteSpace(dto.NewUsername))
                deliveryGuy.Username = dto.NewUsername;

            // ✅ Handle password only if user wants to change it
            if (!string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                if (string.IsNullOrWhiteSpace(dto.CurrentPassword))
                    return BadRequest("Current password is required to change your password.");

                var passwordMatches = BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, deliveryGuy.PasswordHash);
                if (!passwordMatches)
                    return Unauthorized("Current password is incorrect.");

                deliveryGuy.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            }

            await _context.SaveChangesAsync();
            return Ok("Profile updated successfully.");
        }

        [HttpPut("update-fuel-price")]
        public async Task<IActionResult> UpdateFuelPrice([FromBody] FuelPriceUpdate request)
        {
            var fuel = await _context.FuelPrices.FirstOrDefaultAsync();
            if (fuel == null)
            {
                fuel = new FuelPrice();
                _context.FuelPrices.Add(fuel);
            }

            fuel.Ron95Price = request.Ron95Price;
            fuel.Ron97Price = request.Ron97Price;
            fuel.DieselPrice = request.DieselPrice;

            await _context.SaveChangesAsync();
            return Ok("Fuel prices updated successfully.");
        }

        [HttpGet("get-fuel-price")]
        public async Task<IActionResult> GetFuelPrice()
        {
            var price = await _context.FuelPrices.FirstOrDefaultAsync();
            if (price == null)
            {
                return NotFound();
            }
            return Ok(price);
        }


        [Authorize]
        [HttpDelete("delete-account")]
        public async Task<IActionResult> DeleteAccount()
        {
            var email = User.FindFirstValue(ClaimTypes.Name);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
                return NotFound("User not found.");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok("Account deleted successfully.");
        }

        // 🔵 Approve Delivery Guy
        [HttpPost("approve-deliveryguy/{id}")]
        public async Task<IActionResult> ApproveDeliveryGuy(int id)
        {
            var deliveryGuy = await _context.DeliveryGuys.FirstOrDefaultAsync(d => d.Id == id);

            if (deliveryGuy == null)
                return NotFound("Delivery guy not found.");

            if (deliveryGuy.Status == "Approved")
                return BadRequest("Already approved.");

            deliveryGuy.Status = "Approved"; // or whatever flag you're using
            await _context.SaveChangesAsync();

            return Ok("Delivery Guy Approved Successfully");
        }

        // 🔵 Get Pending Delivery Guys
        [HttpGet("pending-deliveryguys")]
        public async Task<IActionResult> GetPendingDeliveryGuys()
        {
            var pendingDeliveryGuys = await _context.DeliveryGuys
                .Where(d => d.Status == "Pending") // Only if you have a status column
                .Select(d => new
                {
                    d.Id,
                    d.Username,
                    d.Email,
                    d.ContactNumber,
                    d.LicensePhoto
                })
                .ToListAsync();

            return Ok(pendingDeliveryGuys);
        }

        // 🔵 Delete Pending Delivery Guy
        [HttpDelete("delete-deliveryguy/{id}")]
        public async Task<IActionResult> DeletePendingDeliveryGuy(int id)
        {
            var deliveryGuy = await _context.DeliveryGuys.FirstOrDefaultAsync(d => d.Id == id);

            if (deliveryGuy == null)
                return NotFound("Delivery guy not found.");

            _context.DeliveryGuys.Remove(deliveryGuy);
            await _context.SaveChangesAsync();

            return Ok("Pending Delivery Guy deleted successfully.");
        }

        // Endpoint to get total number of users
        [HttpGet("total-users")]
        public async Task<IActionResult> GetTotalUsers()
        {
            var totalUsers = await _context.Users
                .Where(u => u.Role == "User")
                .CountAsync();

            return Ok(totalUsers);
        }

        // Endpoint to get total number of delivery guys
        [HttpGet("total-deliveryguys")]
        public async Task<IActionResult> GetTotalDeliveryGuys()
        {
            var totalDeliveryGuys = await _context.DeliveryGuys
                .Where(d => d.Status == "Approved")
                .CountAsync();

            return Ok(totalDeliveryGuys);
        }

        [HttpGet("all-users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users.ToListAsync();
            return Ok(users);
        }

        [HttpGet("all-deliveryguys")]
        public async Task<IActionResult> GetAllDeliveryGuys()
        {
            var deliveryGuys = await _context.DeliveryGuys
                .Where(d => d.Status == "Approved")  // Filter only approved delivery guys
                .ToListAsync();

            var result = deliveryGuys.Select(d => new
            {
                d.Id,
                d.Username,
                d.Email,
                d.ContactNumber,
                Role = "Delivery Guy"
            });

            return Ok(result);
        }




        [HttpDelete("delete-user/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Ok();
        }


        // 🔵 Upload License
        [HttpPost("upload-license")]
        public async Task<IActionResult> UploadLicense()
        {
            var file = Request.Form.Files.FirstOrDefault();
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Ok(new { FileName = fileName });
        }

        [HttpPut("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
            {
                return NotFound(new { message = "User not found with this email." });
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Password has been reset successfully." });
        }

        [HttpPut("reject-delivery-guy/{id}")]
        public async Task<IActionResult> RejectDeliveryGuy(int id, [FromBody] string reason)
        {
            var deliveryGuy = await _context.DeliveryGuys.FindAsync(id);
            if (deliveryGuy == null)
                return NotFound();

            deliveryGuy.Status = "Rejected";
            deliveryGuy.RejectionReason = string.IsNullOrWhiteSpace(reason) ? "No reason provided" : reason;

            await _context.SaveChangesAsync();
            return Ok("Delivery guy rejected.");
        }

        [Authorize]
        [HttpPost("upload-mykad")]
        public async Task<IActionResult> UploadMyKad([FromForm] UploadMykadDto request)
        {
            if (request.FrontImage == null || request.BackImage == null)
                return BadRequest("Both front and back IC images are required.");

            // Get logged-in user's email from JWT
            var email = User.FindFirstValue(ClaimTypes.Name);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
                return Unauthorized("User not found.");

            // Create upload directory
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "MyKad");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // Save Front Image
            var frontFileName = $"{Guid.NewGuid()}{Path.GetExtension(request.FrontImage.FileName)}";
            var frontPath = Path.Combine(uploadsFolder, frontFileName);
            using (var stream = new FileStream(frontPath, FileMode.Create))
                await request.FrontImage.CopyToAsync(stream);

            // Save Back Image
            var backFileName = $"{Guid.NewGuid()}{Path.GetExtension(request.BackImage.FileName)}";
            var backPath = Path.Combine(uploadsFolder, backFileName);
            using (var stream = new FileStream(backPath, FileMode.Create))
                await request.BackImage.CopyToAsync(stream);

            // Update User Database Record
            user.ICFrontUrl = $"Uploads/MyKad/{frontFileName}";
            user.ICBackUrl = $"Uploads/MyKad/{backFileName}";
            user.ICVerificationStatus = "Submitted";
            user.ICSubmittedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "MyKad uploaded successfully.",
                front = user.ICFrontUrl,
                back = user.ICBackUrl,
                status = user.ICVerificationStatus
            });
        }



        // 🔵 Token Generator
        private string GenerateJwtToken(string email, string role, string username, string contactNumber)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, email),
                new Claim(ClaimTypes.Role, role),
                new Claim("Username", username),
                new Claim("ContactNumber", contactNumber)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["JwtSettings:Issuer"],
                audience: _config["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
