using iText.Commons.Actions.Contexts;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using UserdataManagement.Models;
using UserdataManagement.Repositories;

namespace UserdataManagement.Services
{
    public class UserService : IUserService
    {
        private readonly IMailService _mailService;

        private static ConcurrentDictionary<string, UserTokenModel> _users = new ConcurrentDictionary<string, UserTokenModel>();

        private readonly IUserRepository _userRepository;

        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(IUserRepository userRepository, IMailService mailService, IHttpContextAccessor httpContextAccessor)
        {
            _userRepository = userRepository;
            _mailService = mailService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<UserDataModel>> GetAllUsers()
        {
            return await _userRepository.GetAllUsers();
        }

        public async Task<IEnumerable<UserDataModel>> GetActiveUsers()
        {
            return await _userRepository.GetActiveUsers();
        }

        public async Task<UserDataModel> GetUserById(int id)
        {
            return await _userRepository.GetUserById(id);
        }

        public async Task AddUser(UserDataModel user)
        {
            await _userRepository.AddUser(user);
        }

        public async Task UpdateUser(int id, UserDataModel user)
        {
            await _userRepository.UpdateUser(id, user);
        }

        public async Task DeleteUser(int id)
        {
            await _userRepository.DeleteUser(id);
        }

        public bool IsEmailExists(string email)
        {
            return _userRepository.IsEmailExists(email);
        }

        public bool IsUsernameExists(string username)
        {
            return _userRepository.IsUsernameExists(username);
        }

        public bool IsPhoneNumberExists(string phone)
        {
            return _userRepository.IsPhoneNumberExists(phone);
        }
        public async Task DeactivateUsers(IEnumerable<int> userIds)
        {
            await _userRepository.DeactivateUsers(userIds);
        }

        public async Task<UserDataModel> LoginUser(UserLoginModel loginRequest)
        {
            return await _userRepository.LoginUser(loginRequest.EmailOrUsername, loginRequest.Password);
        }

        //Passwordless Login
        // Generate a login token for the given email
        public async Task<string> GenerateLoginTokenAsync(string email)
        {
            var token = Guid.NewGuid().ToString(); // Generate a random GUID token
            var expiration = DateTime.UtcNow.AddMinutes(10); // Token valid for 10 minutes

            // Create or update the user token info
            var userToken = new UserTokenModel
            {
                Email = email,
                LoginToken = token,
                TokenExpiration = expiration
            };

            _users[email] = userToken;

            //await SendEmailAsync(email, token);

            var request = _httpContextAccessor.HttpContext.Request;
            var validationLink = $"{request.Scheme}://{request.Host}/api/UserData/ValidateLogin?token={token}";

            var subject = "Verify email address!";
            var body = $"Please verify your email address.<br/><br/>" +
                       $"Use the following link to confirm your email address: <a href='{validationLink}'>Confirm Email</a><br/><br/>" +
                       $"This is an automated message. Please do not reply to this email.<br/><br/><br/>" +
                       $"Thanks!";

             _mailService.SendMail(email, subject, body, bcc:"");

            return token;
        }

        // Validate the token
        public async Task<bool> ValidateLoginTokenAsync(string token)
        {
            var currentTime = DateTime.UtcNow;

            return _users.Values.Any(u =>
                u.LoginToken.Equals(token, StringComparison.OrdinalIgnoreCase)
                && u.TokenExpiration > currentTime);
        }


        public async Task SendLoginToken(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required.");

            // Generate token and expiration time
            //var token = Guid.NewGuid().ToString();
            // Generate a longer token by concatenating multiple GUIDs
            var token = $"{Guid.NewGuid()}-{Guid.NewGuid()}-{Guid.NewGuid()}";

            var expirationTime = DateTime.Now.AddMinutes(10);

            // Create and save token to the database
            var userToken = new TokenModel
            {
                Email = email,
                Token = token,
                ExpirationTime = expirationTime,
                CreatedAt = DateTime.Now,
                IsActive = false
            };

            await _userRepository.AddUserToken(userToken);

            var request = _httpContextAccessor.HttpContext.Request;
            var validationLink = $"{request.Scheme}://{request.Host}/api/UserData/VerifyToken?token={token}";

            var subject = "Verify email address!";
            var body = $"Please verify your email address.<br/><br/>" +
                       $"Use the following link to confirm your email address: <a href='{validationLink}'>Confirm Email</a><br/><br/>" +
                       $"This token is valid for 10 minutes.<br/><br/>" +
                       $"This is an automated message. Please do not reply to this email.<br/><br/><br/>" +
                       $"Thanks!";

            _mailService.SendMail(email, subject, body);

        }

        public async Task<bool> VerifyLoginToken(string token)
        {
            var tokenEntry = await _userRepository.GetUserToken(token);

            if (tokenEntry == null || tokenEntry.ExpirationTime < DateTime.Now)
            {
                return false;
            }

            tokenEntry.IsActive = true;
            await _userRepository.UpdateUserToken(tokenEntry);

            return true;
        }

        public async Task<TokenModel> LoginByToken(string token)
        {
            return await _userRepository.LoginByToken(token);
        }

    }
}
