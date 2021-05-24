using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace Services
{
    public interface IJwtHandler
    {
        object GenerateJwtToken(int hospitalId, string username);
        int GetHospitalId(string token);
        string GetUsername(string jwt);
    }
    public class JwtHandler : IJwtHandler
    {
        private IConfiguration _configuration;

        public JwtHandler(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public object GenerateJwtToken(int hospitalId, string username)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, hospitalId.ToString()),
                new Claim("unitId", hospitalId.ToString()),
                new Claim("username", username),
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                _configuration["JwtIssuerOptions:Issuer"],
                _configuration["JwtIssuerOptions:Audience"],
                claims,

                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private JwtSecurityToken HandlingToken(string jwt)
        {
            if (jwt.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                jwt = Regex.Replace(jwt.ToString(), "Bearer ", string.Empty, RegexOptions.IgnoreCase);
            }

            var handler = new JwtSecurityTokenHandler();

            return handler.ReadJwtToken(jwt);
        }

        public int GetHospitalId(string jwt)
        {
            try
            {
                var token = HandlingToken(jwt);

                var unitId = int.Parse(token.Claims.First(claim => claim.Type == "unitId").Value);
                //var tokenString = new JwtSecurityTokenHandler().ReadToken(token) as JwtSecurityToken;
                return unitId;
            }
            catch (Exception)
            {
                return -1;
            }

        }

        public string GetUsername(string jwt)
        {
            try
            {
                var token = HandlingToken(jwt);

                var username = token.Claims.Where(claim => claim.Type == "unique_name").FirstOrDefault();
                if (username == null)
                {
                    username = token.Claims.Where(claim => claim.Type == "Username").FirstOrDefault();
                }
                
                return username.Value;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
