using Afstest.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Afstest.API.Services
{
    public class TokenService
    {
        readonly UserManager<User> _userManager;
        readonly IConfiguration _configuration;
        readonly JwtOptions _jwtOptions;

        public TokenService(UserManager<User> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
            _jwtOptions = configuration.GetSection(JwtOptions.JwtConfiguration).Get<JwtOptions>()!;
        }

        public async Task<(string accessToken, string? refreshToken)> GetAccessAndRefreshTokensAsync(
            string email, bool rememberMe)
        {
            User user = await _userManager.Users.SingleAsync(u => u.Email == email);

            string refreshToken = string.Empty;

            if (rememberMe)
            {
                refreshToken = GenerateJwtRefreshToken(user.Id);
            }

            return (GenerateAccessToken(user), refreshToken);
        }

        public async Task<string> GetTokenOnRefreshAsync(string refreshToken)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var tokenValidationParams = new TokenValidationParameters
            {
                ClockSkew = TimeSpan.FromSeconds(0),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _jwtOptions.Issuer,
                ValidAudience = _jwtOptions.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.RefreshSecurityKey))
            };

            try
            {
                var principal = jwtTokenHandler.ValidateToken(refreshToken, tokenValidationParams, out SecurityToken validatedToken);

                //.net transforms the sub to nameIdentifier claim
                return await VerifySubAndGenerateAccessTokenAsync(principal.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<string> VerifySubAndGenerateJwtRefreshTokenAsync(string sub)
        {
            var user = await _userManager.FindByIdAsync(sub);

            if (user != null)
            {
                return GenerateJwtRefreshToken(user.Id);
            }

            throw new InvalidOperationException("invalid sub");
        }

        #region helpers
        private async Task<string> VerifySubAndGenerateAccessTokenAsync(string sub)
        {
            User user = await _userManager.Users.SingleAsync(u => u.Id == sub);

            if (user != null)
            {
                return GenerateAccessToken(user);
            }

            throw new InvalidOperationException("invalid sub");
        }

        private string GenerateJwtRefreshToken(string sub)
        {
            List<Claim> claims = new()
            {
                new Claim("sub", sub),
            };

            var tokenExpiry = DateTimeOffset.UtcNow.AddMinutes(_jwtOptions.RefreshTokenExpiryfromMins).DateTime;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.RefreshSecurityKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(_jwtOptions.Issuer, _jwtOptions.Audience, claims,
                                                 expires: tokenExpiry, signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateAccessToken(User user)
        {
            List<Claim> claims = new()
            {
                new Claim("sub", user.Id),
                new Claim("userName", user.Email!),
                new Claim("email", user.Email!),
                new Claim("firstName", user.FirstName),
                new Claim("lastName", user.LastName)
            };

            var tokenExpiry = DateTimeOffset.UtcNow.AddMinutes(_jwtOptions.ExpiryfromMins).DateTime;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecurityKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(_jwtOptions.Issuer, _jwtOptions.Audience, claims,
                                                 expires: tokenExpiry, signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        #endregion
    }

    public class JwtOptions
    {
        public const string JwtConfiguration = "JwtConfiguration";
        public string SecurityKey { get; init; } = default!;
        public string Issuer { get; init; } = default!;
        public string Audience { get; init; } = default!;
        public int ExpiryfromMins { get; init; }
        public int RefreshTokenExpiryfromMins { get; init; }
        public string RefreshSecurityKey { get; init; } = default!;
    }
}
