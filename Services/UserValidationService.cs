using System.Text.RegularExpressions;
using UserdataManagement.Models;
using UserdataManagement.Repositories;

namespace UserdataManagement.Services
{
    public class UserValidationService
    {
        private readonly IUserRepository _userRepository;

        public UserValidationService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        // Helper method to check if a string is not null, empty, or whitespace
        private bool IsRequired(string input)
        {
            return !string.IsNullOrWhiteSpace(input);
        }

        public string ValidateName(string name)
        {
            if (!IsRequired(name))
                return "Name is required.";

            string pattern = @"^.{3,30}$";
            if (!Regex.IsMatch(name, pattern))
                return "Name must be between 3 and 30 characters.";

            return null;
        }

        public string ValidateUsername(string username)
        {
            if (!IsRequired(username))
                return "Username is required.";

            string pattern = @"^[a-zA-Z0-9]{3,20}$";
            if (!Regex.IsMatch(username, pattern))
                return "Username must be alphanumeric and between 3 to 20 characters.";

            return null;
        }

        public string ValidateEmail(string email)
        {
            if (!IsRequired(email))
                return "Email is required.";

            string pattern = @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$";
            if (!Regex.IsMatch(email, pattern))
                return "Invalid email format.";

            return null;
        }

        public string ValidatePassword(string password)
        {
            if (!IsRequired(password))
                return "Password is required.";

            // Minimum 8 characters, at least 1 uppercase letter, 1 lowercase letter, 1 digit, 1 special character
            string pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,20}$";
            if (!Regex.IsMatch(password, pattern))
                return "Password must be 8-20 characters long, with at least one uppercase letter, one lowercase letter, one digit, and one special character.";

            return null;
        }

        public string ValidatePhoneNumber(string phoneNumber)
        {
            // Check if phone number is provided
            if (!IsRequired(phoneNumber))
                return "Phone number is required.";

            // Ensure the phone number has exactly 10 digits
            if (phoneNumber.Length != 10)
                return "Phone number must be exactly 10 digits.";

            // Ensure the phone number starts with 6, 7, 8, or 9
            string pattern = @"^[6-9][0-9]{9}$";
            if (!Regex.IsMatch(phoneNumber, pattern))
                return "Invalid phone number.";

            return null;
        }

        public string ValidateAge(int age)
        {
            if (age == 0)
                return "Age is required.";

            if (age < 1 || age > 100)
                return "Age must be a number between 1 and 100.";

            return null;
        }

        public string ValidateAddress(string address)
        {
            if (!IsRequired(address))
                return "Address is required.";

            if (address.Length > 200)
                return "Address cannot exceed 200 characters.";

            return null;
        }


        public string ValidateUser(UserDataModel user)
        {
            var nameError = ValidateName(user.Name);
            if (nameError != null) return nameError;

            var usernameError = ValidateUsername(user.Username);
            if (usernameError != null) return usernameError;

            if (_userRepository.IsUsernameExists(user.Username))
                return "Username already exists.";

            var emailError = ValidateEmail(user.Email);
            if (emailError != null) return emailError;

            if (_userRepository.IsEmailExists(user.Email))
                return "Email already exists.";

            var passwordError = ValidatePassword(user.Password);
            if (passwordError != null) return passwordError;

            var phoneError = ValidatePhoneNumber(user.Phone);
            if (phoneError != null) return phoneError;

            if (_userRepository.IsPhoneNumberExists(user.Phone))
                return "Phone number already exists.";

            var ageError = ValidateAge(user.Age);
            if (ageError != null) return ageError;

            var addressError = ValidateAddress(user.Address);
            if (addressError != null) return addressError;

            return null; // Validation passed
        }
    }
}
