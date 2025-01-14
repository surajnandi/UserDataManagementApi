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
            {
                throw new ArgumentException("Email is required.");
            }

            try
            {
                // Validate the email format
                var addr = new System.Net.Mail.MailAddress(email);
                if (addr.Address != email)
                {
                    throw new ArgumentException("Invalid email address format.");
                }

                // Generate token and expiration time
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

                var addResult = _userRepository.AddUserToken(userToken);
                if (addResult == null)
                {
                    throw new InvalidOperationException("Error saving token to the database.");
                }

                // Generate validation link
                var request = _httpContextAccessor.HttpContext.Request;
                var validationLink = $"{request.Scheme}://{request.Host}/api/UserData/VerifyToken?token={token}";

                // Prepare email body and subject
                var subject = "Verify email address!";
                var body = $"Please verify your email address.<br/><br/>" +
                           $"Use the following link to confirm your email address: <a href='{validationLink}'>Confirm Email</a><br/><br/>" +
                           $"This token is valid for 10 minutes.<br/><br/>" +
                           $"This is an automated message. Please do not reply to this email.<br/><br/><br/>" +
                           $"Thanks!";

                // Send email
                 _mailService.SendMail(email, subject, body);

            }
            catch (ArgumentException ex)
            {
                // Handle argument-related errors (e.g., invalid email format)
                throw new ArgumentException(ex.Message, ex);
            }
            catch (InvalidOperationException ex)
            {
                // Handle database or email-related issues
                throw new InvalidOperationException(ex.Message, ex);
            }
            catch (Exception ex)
            {
                // General error handling
                throw new Exception("An unexpected error occurred while processing the login token.", ex);
            }
        }

        public async Task<bool> VerifyLoginToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentException("Token cannot be empty.");
            }

            try
            {
                // Retrieve the token entry from the database
                var tokenEntry = await _userRepository.GetUserToken(token);
                if (tokenEntry == null)
                {
                    throw new InvalidOperationException("Invalid token.");
                }

                // Check if the token is expired
                if (tokenEntry.ExpirationTime < DateTime.Now)
                {
                    throw new InvalidOperationException("The token has expired.");
                }

                // Mark the token as active (successfully verified)
                tokenEntry.IsActive = true;
                var updateResult = _userRepository.UpdateUserToken(tokenEntry);
                if (updateResult == null)
                {
                    throw new InvalidOperationException("Error updating token status.");
                }

                return true;
            }
            catch (ArgumentException ex)
            {
                // Handle argument-related errors (e.g., empty token)
                throw new ArgumentException(ex.Message, ex);
            }
            catch (InvalidOperationException ex)
            {
                // Handle token verification failures
                throw new InvalidOperationException(ex.Message, ex);
            }
            catch (Exception ex)
            {
                // General error handling
                throw new Exception("An unexpected error occurred while verifying the token.", ex);
            }
        }


        public async Task<TokenModel> LoginByToken(string token)
        {
            return await _userRepository.LoginByToken(token);
        }

    }
}
