using Afstest.API.Dtos.Account;
using Afstest.API.Dtos.Common;
using Afstest.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Afstest.API.Controllers
{
    [Route("api/identity")]
    [ApiController]
    public class IdentityController : ControllerBase
    {
        readonly IdentityService _identityService;
        public IdentityController(IdentityService identityService)
        {
            _identityService = identityService;
        }

        [HttpPost("signin")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(SignInResponse))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(ErrorDto))]
        public async Task<IActionResult> Signin(SignInRequest request)
        {
            return Ok((await _identityService.SignInAsync(request)));
        }

        [HttpPost("signup")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(EntityCreatedResponse<string>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(ErrorDto))]
        public async Task<IActionResult> SignUp(CreateAccountRequest request)
        {
            return Ok((await _identityService.CreateAccountAsync(request)));
        }

        /// <summary>
        /// Return otp used for email confirmation
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [HttpPost("begin-reset/{email}")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BeginPasswordResetResponse))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(ErrorDto))]
        public async Task<IActionResult> BeginPasswordReset(string email)
        {
            return Ok(await _identityService.ResetPasswordBeginAsync(new BeginPasswordResetRequest { Email = email }));
        }

        /// <summary>
        /// reset with reset/otp code
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("reset")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(ErrorDto))]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
        {
            await _identityService.ResetPasswordAsync(request);
            return Ok();
        }
        
        [HttpPost("refresh-token")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(ErrorDto))]
        public async Task<IActionResult> GetRefreshToken(string sub)
        {
            await _identityService.GetRefreshTokenAsync(sub);
            return Ok();
        }
        
        [HttpPost("access-token")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(ErrorDto))]
        public async Task<IActionResult> GetAccessToken(string refreshToken)
        {
            await _identityService.GetAccessTokenAsync(refreshToken);
            return Ok();
        }
    }
}
