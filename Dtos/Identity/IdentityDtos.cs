namespace Afstest.API.Dtos.Account
{
    public class CreateAccountRequest 
    {
        public required string FirstName { get; set; }
        public string? OtherNames { get; set; }
        public string? LastName { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string PhoneNumber { get; set; }
    }

    public class SignInRequest
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
        public bool RememberMe { get; set; }
    }

    public class BeginPasswordResetRequest
    {
        public required string Email { get; set; }
    } 
    
    public class BeginPasswordResetResponse
    {
        public required string Code { get; set; }
    }

    public class ResetPasswordRequest
    {
        public required string UserId { get; set; } = default!; 
        public required string Code { get; set; } = default!;
        public required string Password { get; set; }
        public required string ConfirmPassword { get; set; }
    }

    public class CurrentUser
    {
        public required string UserId { get; set; }
        public required string UserName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public required string Email { get; set; }
        
    }


    public class SignInResponse   
    {
        public  required string AccessToken { get; set; }
        public  string? RefreshToken { get; set; }
        
        
        public required  CurrentUser CurrentUser{ get; set; }          
    }
}
