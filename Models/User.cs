using Microsoft.AspNetCore.Identity;

namespace Afstest.API.Models
{
    public class User : IdentityUser
    {
        public required string FirstName { get; set; }
        public string? OtherNames { get; set; }
        public string? LastName { get; set; }
        
        
        
        //Because you want to save bandwidth
    }
}
