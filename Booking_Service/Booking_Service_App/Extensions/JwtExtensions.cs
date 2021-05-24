using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Booking_Service_App.Extensions
{
    public static class JwtExtensions
    {
        public static string GetUsername(this IEnumerable<Claim> claims)
        {
            
            var username = claims.Where(claim => claim.Type == "unique_name").FirstOrDefault();
            if (username == null)
            {
                username = claims.Where(claim => claim.Type == "Username").FirstOrDefault();
            }
            return username.Value;
        }

        public static int GetHospitalId(this IEnumerable<Claim> claims)
        {
            try
            {
                var unitId = int.Parse(claims.First(claim => claim.Type == "unitId").Value);
                return unitId;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        
    }

    public class JwtSecurityKey
    {
        public static SymmetricSecurityKey Create(string secret)
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret));
        }
    }

    public class JwtIssuerOptions
    {
        public string Issuer { get; set; }
        public string Subject { get; set; }
        public string Audience { get; set; }
        public DateTime Expiration => IssuedAt.Add(ValidFor);
        public DateTime NotBefore => DateTime.UtcNow;
        public DateTime IssuedAt => DateTime.UtcNow;
        public TimeSpan ValidFor { get; set; } = TimeSpan.FromDays(365);

        public Func<Task<string>> JtiGenerator =>
          () => Task.FromResult(Guid.NewGuid().ToString());

        public SigningCredentials SigningCredentials { get; set; }

    }
}
