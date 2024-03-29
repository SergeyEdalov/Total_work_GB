﻿using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using User.Abstractions;
using User.DataBase.Context;
using User.DataBase.DTO;
using User.Models;
using User.RSAKeys;

namespace User.Services
{
    public class UserAuthenticationService : IUserAuthenticationService
    {
        private readonly UserContext _userContext;
        private readonly IMapper _mapper;
        private readonly IConfiguration? _configuration;
        public UserAuthenticationService() { }

        public UserAuthenticationService(UserContext userContext, IMapper mapper, IConfiguration? configuration)
        {
            _userContext = userContext;
            _mapper = mapper;
            _configuration = configuration;
        }
        public string Authenticate(LoginModel loginModel)
        {
            using (_userContext)
            {
                var entity = _userContext.Users
                    .FirstOrDefault(x => x.Email.Equals(loginModel.Email));

                if (entity == null) return "User not found";

                var data = Encoding.UTF8.GetBytes(loginModel.Password).Concat(entity.Salt).ToArray();

                SHA512 shaM = new SHA512Managed();

                var bpassword = shaM.ComputeHash(data);

                if (entity.Password.SequenceEqual(bpassword))
                {
                    var user = _mapper.Map<UserDto>(entity);
                    return GeneratreToken(user);

                    //_mapper.Map<UserModel>(user); Надо ли?
                }
                else return "Wrong password";
            }
        }

        private string GeneratreToken(UserDto user)
        {
            //var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var securityKey = new RsaSecurityKey(RSATools.GetPrivateKey());
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256Signature);
            var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, user.RoleId.ToString())
                };
            var token = new JwtSecurityToken(_configuration["Jwt:Issuer"],_configuration["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddMinutes(60),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string AuthenticateMock(LoginModel loginModel)
        {
            var user = new UserDto();
            if (loginModel.Email == "admin" && loginModel.Password == "password")
            {
                user = new UserDto { Password = Encoding.UTF8.GetBytes(loginModel.Password), Email = loginModel.Email, RoleId = DataBase.Entity.Role.Admin };
            }
            if (loginModel.Email == "user" && loginModel.Password == "super")
            {
                user = new UserDto { Password = Encoding.UTF8.GetBytes(loginModel.Password), Email = loginModel.Email, RoleId = DataBase.Entity.Role.User };
            }
            return GeneratreToken(user);
        }
    }
}