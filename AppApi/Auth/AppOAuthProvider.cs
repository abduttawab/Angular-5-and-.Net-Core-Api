using AppApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;


using System;

using System.Threading.Tasks;
using Microsoft.Owin.Security.OAuth;

using System.Security.Claims;


namespace AppApi.Auth
{
    public class AppOAuthProvider :OAuthAuthorizationServerProvider
    {

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AppOAuthProvider(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _context = context;
            _roleManager = roleManager;
        }



        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated();
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
      
            var user = await _userManager.FindByLoginAsync(context.UserName, context.Password);

         
            if (user != null)
            {
                var identity = new ClaimsIdentity(context.Options.AuthenticationType);
                //identity.AddClaim(new Claim("Username", user.UserName));
                //identity.AddClaim(new Claim("Email", user.Email));
                //identity.AddClaim(new Claim("FirstName", user.FirstName));
                //identity.AddClaim(new Claim("LastName", user.LastName));
                //identity.AddClaim(new Claim("LoggedOn", DateTime.Now.ToString()));
                context.Validated(identity);
            }
            else
                return;
        }
    }
}
