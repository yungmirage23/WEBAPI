using ClassLibrary.Models.JwtToken;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RestWebAppl.Models;
using RestWebAppl.Models.ViewModels;
using System.Net;
using ClassLibrary.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Rest;

namespace WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController : ControllerBase
    {
        private IJwtGenerator jwtGenerator;
        private UserManager<ApplicationUser> userManager;
        private SignInManager<ApplicationUser> singInManager;

        public AccountsController(UserManager<ApplicationUser> _usrMngr, SignInManager<ApplicationUser> _singMng, IJwtGenerator _jwtGenerator)
        {
            jwtGenerator = _jwtGenerator;
            userManager = _usrMngr;
            singInManager = _singMng;
        }

        //GET api/accounts
        [HttpGet]
        public IEnumerable<ApplicationUser> Get() => userManager.Users;

        //POST api/accounts/login
        [HttpPost("login")]
        public async Task<ActionResult<ApplicationUser>> Get([FromBody] LoginModel loginModel)
        {
            ApplicationUser user = await userManager.FindByNameAsync(loginModel.Phone);
            if (user != null)
            {
                await singInManager.SignOutAsync();
                if ((await singInManager.PasswordSignInAsync(user, loginModel.Password, false, false)).Succeeded)
                {
                    return user;
                }

                return StatusCode(401);
            }
            return StatusCode(400);
        }

        [AllowAnonymous]
        [HttpPost("create/token")]
        public async Task<ActionResult<UserModel>> CreateToken([FromBody]LoginModel _loginModel)
        {
            ApplicationUser user = await userManager.FindByNameAsync(_loginModel.Phone);
            if (user == null)
            {
                return StatusCode(401);
            }
            var singInResult = await singInManager.CheckPasswordSignInAsync(user, _loginModel.Password, false);
            if (singInResult.Succeeded)
            {
                return new UserModel
                {
                    DisplayName = user.FirstName,
                    UserLastName = user.LastName,
                    Token = jwtGenerator.CreateToken(user)
                };
            }
            else
            {
                return StatusCode(401);
            }
        }
    }
}