using API.Entities;
using API.Interfaces;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Identity;

namespace API.Services
{
    public class TokenService : ITokenService
    {
        private readonly SymmetricSecurityKey _key;
        private readonly UserManager<AppUser> _userManager;

        // We need constructor because we're going to inject our configuration into this class.
        // We pass UserManager because this is how we get the UserRole.
        public TokenService(IConfiguration config, UserManager<AppUser> userManager) 
        {
            // SymmetricSecurityKey take byte array of the key, and we need to encode it because it created a key as a string, and we need to encode it into a byte array.
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"]));
            _userManager = userManager;
        }

        public async Task<string> CreateToken(AppUser user)
        {
            var claims = new List<Claim>
            { 
                // Add new claim setting the name ID to the user's username.
                // Set the UniqueName for username and NameId for users's id.
                new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
            };

            // Get the user role.
            // GetRolesAsync: because this is only asynchronous, we can't get a synchronous version of this method.
            // Inside the roles is going to be the list of roles that this user belongs to.
            var roles = await _userManager.GetRolesAsync(user);

            // Add to the claims and project it. So we select the roles from the list of role and then create a new claim, and rather than
            // using JWT registered claim names, we're going to be using ClaimTypes because the JWT registered
            // claim names do not have an option for a role inside here and we can use role claim type and add it into the token.
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            // Create new credentials
            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

            // Describe token
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(7),
                SigningCredentials = creds
            };

            // Create token handler
            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
