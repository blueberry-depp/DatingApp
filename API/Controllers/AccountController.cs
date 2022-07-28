using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;

        // Inject the token service too.
        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ITokenService tokenService, IMapper mapper) 
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _mapper = mapper;
        }


        [HttpPost("register")]
        // ActionResult<AppUser>: we returning AppUser.
        // "using": when we finished with this particular class, then it's going to be disposed of correctly   
        // string username, string password: The Api Controller doesn't know where these values are coming from,
        // the value can come with query string or the body of the request, we are leaving it up to the [ApiController] to figure this out for us
        // The body of request need the object form.
        // When we use ActionResult, we're able to return different HTTP status codes.
        // We returning UserDto.
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto) 
        {
            if (await UserExist(registerDto.Username)) return BadRequest("Username is taken");

            // Map from a registerDto into AppUser.
            var user = _mapper.Map<AppUser>(registerDto);

            // Make the username a lowercase username.
            user.UserName = registerDto.Username.ToLower();

            // This both create a user and saves the changes into the database.
            var result = await _userManager.CreateAsync(user, registerDto.Password);

            // Check to see if result has succeeded.    
            if (!result.Succeeded) return BadRequest(result.Errors);

            // Add the user into the member role. We're going to put any newly registered user into the member role.
            var roleResult = await _userManager.AddToRoleAsync(user, "Member");

            if (!roleResult.Succeeded) return BadRequest(result.Errors);


            return new UserDto
            {
                Username = user.UserName,
                Token = await _tokenService.CreateToken(user),
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };
        }


        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            // Get the user from database
            var user = await _userManager.Users
                // Eagerly loading the photo too.
                .Include(p => p.Photos) 
                // Get the user.
                .SingleOrDefaultAsync(x => x.UserName == loginDto.Username.ToLower());

            if (user == null) return Unauthorized("Invalid username");

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

            // Check to see if result has succeeded.    
            if (!result.Succeeded) return Unauthorized();

            return new UserDto
            {
                Username = user.UserName,
                Token = await _tokenService.CreateToken(user),
                // Even though that they have registered, it doesn't mean they've got a photo, the url maybe null, so we gave it optional property.
                // and this source is no longer going to be empty if the user doesn't have a photo, that will not cause an exception here
                // because it's simply going to return null, but if it doesn't have any photos to work with, that's when we see the exception.
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };
        }


        // Helper method
        // Check the username already in database
        private async Task<bool> UserExist(string username)
        {
            return await _userManager.Users.AnyAsync(x => x.UserName == username.ToLower());
        }
    }
}
