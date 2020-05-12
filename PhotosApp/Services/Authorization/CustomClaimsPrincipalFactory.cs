using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using PhotosApp.Areas.Identity.Data;

namespace PhotosApp.Services.Authorization
{
    public class CustomClaimsPrincipalFactory : UserClaimsPrincipalFactory<PhotoAppUser, IdentityRole>
    {
        public CustomClaimsPrincipalFactory(
            UserManager<PhotoAppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, roleManager, optionsAccessor)
        { }

        public override async Task<ClaimsPrincipal> CreateAsync(PhotoAppUser user)
        {
            var principal = await base.CreateAsync(user);
            var claimsIdentity = (ClaimsIdentity)principal.Identity;

            if (user.Paid)
            {
                claimsIdentity.AddClaims(new[]
                {
                    new Claim("type", "value")
                });
            }
            
            return principal;
        }
    }
}