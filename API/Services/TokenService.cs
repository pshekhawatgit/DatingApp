using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace API.Services;

public class TokenService : ITokenService
{
    private readonly SymmetricSecurityKey _key;
    public TokenService(IConfiguration config)
    {
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"]));
    }
    public string CreateToken(AppUser user)
    {
        // Create Claims list that a User may have in a request
        var claims = new List<Claim>{
            new Claim(JwtRegisteredClaimNames.NameId, user.UserName)
        };

        // Create Credentials by encripting the key
        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

        // Create Token Descriptor to contain information that is used to create tokens
        var tokenDescriptor = new SecurityTokenDescriptor{
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddDays(7),
            SigningCredentials = creds
        };
        // Create Token
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}
