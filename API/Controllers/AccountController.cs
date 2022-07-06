using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext _context;
        private readonly ITokenService _tokenService;

        public AccountController(DataContext context, ITokenService tokenService) // Inject the token service too
        {
            _context = context;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        // ActionResult<AppUser>: we returning AppUser
        // "using": when we finished with this particular class, then it's going to be disposed of correctly   
        // string username, string password: The Api Controller doesn't know where these values are coming from, the value can come with query string or the body of the request, we are leaving it up to the [ApiController] to figure this out for us
        // The body of request need the object form
        // When we use ActionResult, we're able to return different HTTP status codes
        // We returning UserDto
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto) 
        {
            if (await UserExist(registerDto.Username)) return BadRequest("Username is taken");

            using var hmac = new HMACSHA512();

            // Create new user
            var user = new AppUser
            {
                UserName = registerDto.Username.ToLower(),
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
                PasswordSalt = hmac.Key
            };

            _context.Users.Add(user); // Tracking this now in Entity Framework
            await _context.SaveChangesAsync(); // Save user to the database

            return new UserDto
            {
                Username = user.UserName,
                Token = _tokenService.CreateToken(user)
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            // Get the user from database
            var user = await _context.Users
                .Include(p => p.Photos) // Eagerly loading the photo too.
                .SingleOrDefaultAsync(x => x.UserName == loginDto.Username);

            if (user == null) return Unauthorized("Invalid username");

            using var hmac = new HMACSHA512(user.PasswordSalt);

            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

            // Because this is a byte array, we need to loop over each element in this array.
            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid password");

            }

            return new UserDto
            {
                Username = user.UserName,
                Token = _tokenService.CreateToken(user),
                // Even though that they have registered, it doesn't mean they've got a photo, the url maybe null, so we gave it optional property.
                // and this source is no longer going to be empty if the user doesn't have a photo, that will not cause an exception here
                // because it's simply going to return null, but if it doesn't have any photos to work with, that's when we see the exception.
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url
            };

        }


        // Helper method
        // Check the username already in database
        private async Task<bool> UserExist(string username)
        {
            return await _context.Users.AnyAsync(x => x.UserName == username.ToLower());
        }
    }
}
