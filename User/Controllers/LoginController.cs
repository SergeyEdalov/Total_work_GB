﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using User.Abstractions;
using User.Models;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;

namespace User.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly IUserAuthenticationService _userAuthenticationService; 

        public LoginController(IUserAuthenticationService userAuthenticationService) //Почему входящий интерфейс null?
        {
            if (userAuthenticationService == null) { throw new Exception("Constructor empty"); }
            _userAuthenticationService = userAuthenticationService;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("Login")]
        public IActionResult Login([FromBody] LoginModel loginModel)
        {
            var token = _userAuthenticationService.Authenticate(loginModel);
            //var token = _userAuthenticationService.AuthenticateMock(loginModel); //Заглушка

            if (token != null)
            {
                return Ok(token);
            }
            return NotFound("Error Authentication. Check that the entered data is correct.");  
        }
    }
}
