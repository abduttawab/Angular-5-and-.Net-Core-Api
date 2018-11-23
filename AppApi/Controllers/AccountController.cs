using AppApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {



        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _context = context;
            _roleManager = roleManager;
        }



        // POST api/Account
        [HttpPost]
       
        public async Task<IdentityResult> Post(AccountModel accountModel)
        {
        

            var user = new ApplicationUser()
            {

                UserName = accountModel.UserName,
                Email = accountModel.Email,
                FirstName = accountModel.FirstName,
                LastName = accountModel.LastName
            };

            var result = await _userManager.CreateAsync(user, accountModel.Password);

            return result;
        }
    }


}
