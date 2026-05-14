using BCrypt.Net;
using Microsoft.IdentityModel.Tokens;
using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Application.Interfaces.Services;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace NexusERP.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;

        private readonly string _jwtSecret;

        public AuthService(IUserRepository userRepository, IConfiguration config)
        {
            _userRepository = userRepository;
            _jwtSecret = config["JwtSettings:Secret"] ?? throw new Exception("JWT Secret missing!");
        }

        public void Register(string fullname, string username, string plaintextPassword, UserRole role)
        {
            if (_userRepository.GetUserByUsername(username) != null)
                throw new Exception("Username already exists.");

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(plaintextPassword);

            var newUser = new User
            {
                FullName = fullname,
                Username = username,
                PasswordHashed = hashedPassword,
                Role = role
            };

            _userRepository.CreateUser(newUser);
        }

        public string Login(string username, string plaintextpassword)
        {
            var user = _userRepository.GetUserByUsername(username);
            if (user == null) 
                throw new Exception("Invalid username or Password");

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(plaintextpassword, user.PasswordHashed);
            if (!isPasswordValid)
                throw new Exception("Inavlid username or password");

            return GenerateJwtToken(user); 
        }

        public string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret);

            var tokenDescription = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(10),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescription);
            return tokenHandler.WriteToken(token);
        }
    }
}
