using iText.Html2pdf;
using iText.Kernel.Pdf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Text;
using System.Text.RegularExpressions;
using UserdataManagement.Models;
using UserdataManagement.Services;

namespace UserdataManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserDataController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly UserValidationService _validationService;
        private readonly IMailService _mailService;

        public UserDataController(IUserService userService, UserValidationService validationService, IMailService mailService)
        {
            _userService = userService;
            _validationService = validationService;
            _mailService = mailService;
        }

        [HttpGet("GetAllUsers")]
        public async Task<ActionResult<IEnumerable<UserDataModel>>> GetAllUsers()
        {
            return Ok(await _userService.GetAllUsers());
        }

        [HttpGet("GetActiveUsers")]
        public async Task<IActionResult> GetActiveUsers()
        {
            var activeUsers = await _userService.GetActiveUsers();
            return Ok(activeUsers);
        }

        [HttpGet("GetUserById")]
        public async Task<ActionResult<UserDataModel>> GetUserById(int id)
        {
            var user = await _userService.GetUserById(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [HttpPost("AddUser")]
        public async Task<IActionResult> AddUser([FromBody] UserDataModel user)
        {
            try
            {
                string validationResult = _validationService.ValidateUser(user);
                if (validationResult != null)
                {
                    return BadRequest(validationResult);
                }

                await _userService.AddUser(user);

                // Construct email body with user credentials
                string emailBody = $"<h3 style='text-align: center;'>Welcome, {user.Username}</h3>" +
                                   $"<p><strong>Name:</strong> {user.Name}</p>" +
                                   $"<p><strong>Email:</strong> {user.Email}</p>" +
                                   $"<p><strong>Password:</strong> {user.Password}</p>" +
                                   $"<p><strong>Age:</strong> {user.Age}</p>" +
                                   $"<p><strong>Phone:</strong> {user.Phone}</p>" +
                                   $"<p><strong>Address:</strong> {user.Address}</p>";

                // Send email
                _mailService.SendMail(user.Email, "User Created Successfully", emailBody);

                var responseMessage = new
                {
                    Message = "User created successfully.",
                    User = user
                };
                
                return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, responseMessage);
               
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("UpdateUser")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserDataModel user)
        {
            try
            {
                var validationError = _validationService.ValidateUser(user);
                if (validationError != null)
                {
                    return BadRequest(validationError);
                }

                await _userService.UpdateUser(id, user);
                return Ok("User updated successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("DeleteUser")]
        public async Task<ActionResult> DeleteUser(int id)
        {
            await _userService.DeleteUser(id);
            return NoContent();
        }

        [HttpPost("DeleteAllUsers")]
        public async Task<IActionResult> DeleteAllUsers([FromBody] List<int> userIds)
        {
            if (userIds == null || !userIds.Any())
            {
                return BadRequest("No user IDs provided.");
            }

            await _userService.DeactivateUsers(userIds);
            return Ok(new { message = "All users deleted successfully." });
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] UserLoginModel loginRequest)
        {
            //if (loginRequest == null || string.IsNullOrWhiteSpace(loginRequest.EmailOrUsername) || string.IsNullOrWhiteSpace(loginRequest.Password))
            //{
            //    return BadRequest("Username or email and password are required.");
            //}

            if (loginRequest == null)
            {
                return BadRequest("Invalid request.");
            }

            // Check if EmailOrUsername is missing
            if (string.IsNullOrWhiteSpace(loginRequest.EmailOrUsername))
            {
                return BadRequest("Username or email is required.");
            }

            // Check if Password is missing
            if (string.IsNullOrWhiteSpace(loginRequest.Password))
            {
                return BadRequest("Password is required.");
            }

            var user = await _userService.LoginUser(loginRequest);

            if (user == null)
            {
                return Unauthorized("Invalid credentials.");
            }

            // You can return only necessary data like email or username
            return Ok(new
            {
                Username = user.Username,
                Email = user.Email,
                Password = user.Password
            });
        }


        /// <summary>
        /// Passwordless Login
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [HttpPost("UserLogin")]
        public async Task<IActionResult> UserLogin([FromQuery] string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("Email is required.");
            }

            // Regex pattern for validating an email address
            var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (!Regex.IsMatch(email, emailPattern))
            {
                return BadRequest("Invalid email format.");
            }

            var token = await _userService.GenerateLoginTokenAsync(email);
            return Ok(new { Token = token, Message = "Login token sent to email." });
        }

        // Validate the login token
        [HttpGet("ValidateLogin")]
        public async Task<IActionResult> ValidateLogin([FromQuery] string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Token is required.");
            }

            var isValid = await _userService.ValidateLoginTokenAsync(token);
            if (isValid)
            {
                return Ok("Login successfull!");
            }
            return Unauthorized("Invalid or expired token.");
        }


        [HttpPost("SendEmail")]
        public IActionResult SendEmail(MailModel request)
        {
            try
            {
                _mailService.SendMail(request.Email, request.Subject, request.Body, request.Attachment);
                return Ok("Email sent successfully!");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error: " + ex.Message);
            }
        }


        [HttpGet("GeneratePdf")]
        public IActionResult GeneratePdf()
        {
            try
            {
                // Fetch user data
                var users =  _userService.GetAllUsers();
                if (users == null || !users.Result.Any())
                {
                    return NotFound("No users found.");
                }
                // Define the file paths
                var htmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Files", "userdata.html");

                if (!System.IO.File.Exists(htmlFilePath))
                {
                    return NotFound("HTML template file not found.");
                }

                //string htmlContent = System.IO.File.ReadAllText(htmlFilePath);

                //// Convert HTML to PDF
                //using (var pdfStream = new MemoryStream())
                //{
                //    using (var writer = new PdfWriter(pdfStream))
                //    {
                //        // Configure PdfWriter
                //        pdfStream.Position = 0;
                //        HtmlConverter.ConvertToPdf(htmlContent, writer);
                //    }

                //    // Return the PDF as a file download
                //    var fileBytes = pdfStream.ToArray();
                //    return File(fileBytes, "application/pdf", "UserList.pdf");
                //}
                string baseHtml = System.IO.File.ReadAllText(htmlFilePath);

                // Replace only the placeholders inside <td> tags dynamically
                var tableRows = string.Join(Environment.NewLine, users.Result.Select(user =>
                    $@"
            <tr>
                <td>{user.Name}</td>
                <td>{user.Email}</td>
                <td>{user.Phone}</td>
            </tr>"));

                // Replace placeholder rows in the HTML with the dynamic rows
                string finalHtml = baseHtml.Replace("{{UserRows}}", tableRows);

                // Convert the updated HTML to a PDF
                using (var pdfStream = new MemoryStream())
                {
                    using (var writer = new PdfWriter(pdfStream))
                    {
                        HtmlConverter.ConvertToPdf(finalHtml, writer);
                    }

                    // Return the PDF as a downloadable file
                    return File(pdfStream.ToArray(), "application/pdf", "UserList.pdf");
                }

            }
            catch (Exception ex)
            {
                // Handle exceptions
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

        }



    }
}
