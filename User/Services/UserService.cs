﻿using AutoMapper;
using CheckUnputDataLibrary;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using User.Abstractions;
using User.DataBase.Context;
using User.DataBase.DTO;
using User.DataBase.Entity;
using User.Models;

namespace User.Services
{
    public class UserService : IUserService
    {
        private readonly IMapper _mapper;
        private readonly UserContext _userContext;

        public UserService() { }

        public UserService(IMapper mapper, UserContext userContext)
        {
            _mapper = mapper;
            _userContext = userContext;
        }
        public Guid AddAdmin(UserModel userModel)
        {
            var count = _userContext.Users.Count();

            if (count != 0) { throw new Exception("There is already have users in system"); }

            userModel.Role = UserRole.Admin;

            var userDb = _mapper.Map<UserEntity>(CreateUser(userModel));

            _userContext.Add(userDb);
            _userContext.SaveChanges();
            return userDb.Id;
        }

        public Guid AddUser(UserModel userModel)
        {
            if (userModel.Role == 0)
            {
                var count = _userContext.Users.Count(x => x.RoleId == 0);
                if (count > 0) { throw new Exception("Second Admin!"); }
            }
            if (_userContext.Users.Select(x => x.Email.Contains(userModel.UserName)).FirstOrDefault())
            {
                throw new Exception("Email is already exsits");
            }
            var userDb = _mapper.Map<UserEntity>(CreateUser(userModel));

            _userContext.Add(userDb);
            _userContext.SaveChanges();
            return userDb.Id;
        }

        public void DeleteUser(string userName)
        {
            var deleteUser = _userContext.Users.Where(x => x.Email.Equals(userName)).FirstOrDefault();

            if (deleteUser != null)
            {
                if (deleteUser.RoleId == 0)
                {
                    throw new Exception("You can`t delete yourself");
                }
                _userContext.Users.Remove(deleteUser);
                _userContext.SaveChanges();
            }
            else { throw new Exception("User not found"); }
        }
        public IEnumerable<UserDto> GetListUsers()
        {
            var listUsers = _userContext.Users.Select(x => _mapper.Map<UserDto>(x)).ToList();
            return listUsers;
        }
        public Guid GetIdIserFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var tokenJWt = handler.ReadJwtToken(token);
            var claim = tokenJWt.Claims.FirstOrDefault(x => x.Type.Equals(ClaimTypes.NameIdentifier));

            Guid userId = Guid.Parse(claim.Value);
            return userId;
        }

        private UserDto CreateUser(UserModel userModel)
        {
            if (Class1.CheckLengthPassword(userModel.Password)
                    && Class1.CheckDifficultPassword(userModel.Password)
                    && Class1.CheckEmail(userModel.UserName))
            {
                var userDto = _mapper.Map<UserDto>(userModel);
                userDto.Id = new Guid();
                userDto.Salt = new byte[16];

                new Random().NextBytes(userDto.Salt);

                var data = userDto.Password.Concat(userDto.Salt).ToArray();

                SHA512 shaM = new SHA512Managed();

                userDto.Password = shaM.ComputeHash(data);
                return userDto;
            }
            else throw new Exception("Wrong input data");
        }
    }
}
