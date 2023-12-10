using Afstest.API.Data;
using Afstest.API.Dtos.Account;
using Afstest.API.Dtos.Common;
using Afstest.API.Models;
using Afstest.API.SeedWork;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Claims;

namespace Afstest.API.Services
{
    public class IdentityService
    {
        #region fields and ctors
        readonly UserManager<User> _userManager;
        readonly SignInManager<User> _signInManager;
        readonly TokenService _tokenService;
        readonly AppDbContext _dbContext;
        readonly ILogger<IdentityService> _logger;
        readonly IHttpContextAccessor _contextAccessor;

        public IdentityService(UserManager<User> userManager,
            AppDbContext dbContext,
            ILogger<IdentityService> logger,
            SignInManager<User> signInManager,
            TokenService tokenService,
            IHttpContextAccessor contextAccessor)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            _logger = logger;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _contextAccessor = contextAccessor;
        }
        #endregion fields and ctors

        public async Task<object> CreateAccountAsync(CreateAccountRequest request)
        {
            var user = new User
            {
                UserName = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,

                OtherNames = request.OtherNames,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
                throw new AfstestPlatformException(result.Errors.Select(e => e.Description));

            return new EntityCreatedResponse<string> {Id = user.Id };
        }

        public async Task<object> SignInAsync(SignInRequest request)
        {
            User? user = await _signInManager.UserManager.FindByEmailAsync(request.Email); //return error if user is not found

            if (user != null)
            {
                var hasPassword = await _signInManager.UserManager.CheckPasswordAsync(user, request.Password);

                if (hasPassword)
                {
                    //if (!await _signInManager.UserManager.IsEmailConfirmedAsync(user))
                    //{
                    //    throw new AfstestPlatformException(new string[] { "email unconfirmed" });
                    //}

                    var (accessToken, refreshToken) = await _tokenService.GetAccessAndRefreshTokensAsync(request.Email, request.RememberMe);

                    return new SignInResponse
                    {
                        AccessToken = accessToken,
                        RefreshToken = refreshToken,
                        CurrentUser = new CurrentUser
                        {
                            Email = user.Email!,
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            UserId = user.Id,
                            UserName = user.UserName!,
                        }
                    };
                }

                throw new AfstestPlatformException("Invalid password");
            }

            throw new AfstestPlatformException("Email doesnt exist") { CustomStatusCode = (int)HttpStatusCode.NotFound };
        }

        public async Task<object> ResetPasswordBeginAsync(BeginPasswordResetRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user != null)
            {
                //code has to be sent via email
                //but for this simple app just show on UI screen for user to enter
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);

                _logger.LogInformation("end BeginPasswordResetCommandHandler");

                return new BeginPasswordResetResponse { Code = code };
            }

            _logger.LogError("user {email} does not exist", request.Email);
            throw new AfstestPlatformException($"user {request.Email} does not exist") { CustomStatusCode = (int)HttpStatusCode.NotFound };
        }

        public async Task ResetPasswordAsync(ResetPasswordRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);

            if (user != null)
            {
                var result = await _userManager.ResetPasswordAsync(user, request.Code, request.Password);

                if (!result.Succeeded)
                {
                    throw new AfstestPlatformException(result.Errors.Select(e => e.Description));
                }

                return;
            }

            throw new AfstestPlatformException($"user {request.UserId} does not exist");
        }

        public async Task<string> GetRefreshTokenAsync(string sub)
        {
            if (string.IsNullOrEmpty(sub))
            {
                throw new AfstestPlatformException("invalid sub");
            }
            return await _tokenService.VerifySubAndGenerateJwtRefreshTokenAsync(sub);
        }

        public async Task<string> GetAccessTokenAsync(string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
            {
                throw new AfstestPlatformException("invalid refresh token");
            }

            return await _tokenService.GetTokenOnRefreshAsync(refreshToken);
        }

        public CurrentUser GetCurrentUser()
        {
            var claims = _contextAccessor.HttpContext!.User.Claims;

            return new CurrentUser
            {
                UserId = claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value,
                UserName = claims.First(c => c.Type == "userName").Value,
                Email = claims.First(c => c.Type == ClaimTypes.Email).Value,
            };
        }
    }
}
