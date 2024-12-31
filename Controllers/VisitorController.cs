using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserdataManagement.Models;
using UserdataManagement.Services;

namespace UserdataManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VisitorController : ControllerBase
    {
        private readonly IVisitorService _visitorService;

        public VisitorController(IVisitorService visitorService)
        {
            _visitorService = visitorService;
        }


        //[HttpGet("visit")]
        //public async Task<IActionResult> Visitors()
        //{
        //    // Capture IP Address
        //    var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
        //    //var ipAddress = "";

        //    // Capture User-Agent Header
        //    var userAgent = Request.Headers["User-Agent"].ToString();

        //    // Determine Device and Browser
        //    var deviceName = userAgent.Contains("Windows") ? "Windows" :
        //                     userAgent.Contains("Macintosh") || userAgent.Contains("Mac OS X") ? "Mac" :
        //                     userAgent.Contains("Android") ? "Android" :
        //                     userAgent.Contains("iPhone") ? "iPhone" :
        //                     userAgent.Contains("iPad") ? "iPad" :
        //                     userAgent.Contains("Linux") && userAgent.Contains("X11") ? "Linux" :
        //                     userAgent.Contains("Linux") ? "Linux" : "Unknown Device";

        //    var browserName = userAgent.Contains("Edg") ? "Microsoft Edge" :
        //                      userAgent.Contains("Chrome") ? "Google Chrome" :
        //                      userAgent.Contains("Firefox") ? "Mozilla Firefox" :
        //                      userAgent.Contains("Safari") && !userAgent.Contains("Chrome") ? "Apple Safari" :
        //                      "Unknown Browser";

        //    // Request IP details from ipinfo.io
        //    //string ipDetails = await new HttpClient().GetStringAsync($"https://ipinfo.io/{ipAddress}/json");
        //    string ipDetails = await new HttpClient().GetStringAsync($"https://ipinfo.io/{ipAddress}/json") ?? "{}";

        //    // Create VisitorModel Object
        //    var visitor = new VisitorModel
        //    {
        //        IpAddress = ipAddress,
        //        DeviceName = deviceName,
        //        BrowserName = browserName,
        //        VisitedTime = DateTime.Now,
        //        VisitorDetails = ipDetails
        //    };

        //    // Save Visitor Data to Database
        //    _visitorService.AddVisitor(visitor);

        //    // Return Visitor Data as Response
        //    return Ok(new { Message = "Visit logged successfully!", Visitor = visitor });
        //}




        [HttpGet("visit")]
        public async Task<IActionResult> Visitors()
        {
            // Capture IP Address
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
            //var ipAddress = Request.Headers["X-Forwarded-For"].ToString();
            //var ipAddress = "164.100.212.2";

            // Capture User-Agent Header
            var userAgent = Request.Headers["User-Agent"].ToString();

            // Determine Device and Browser
            var deviceName = userAgent.Contains("Windows") ? "Windows" :
                             userAgent.Contains("Macintosh") || userAgent.Contains("Mac OS X") ? "Mac" :
                             userAgent.Contains("Android") ? "Android" :
                             userAgent.Contains("iPhone") ? "iPhone" :
                             userAgent.Contains("iPad") ? "iPad" :
                             userAgent.Contains("Linux") && userAgent.Contains("X11") ? "Linux" :
                             userAgent.Contains("Linux") ? "Linux" : "Unknown Device";

            var browserName = userAgent.Contains("Edg") ? "Microsoft Edge" :
                              userAgent.Contains("Chrome") ? "Google Chrome" :
                              userAgent.Contains("Firefox") ? "Mozilla Firefox" :
                              userAgent.Contains("Safari") && !userAgent.Contains("Chrome") ? "Apple Safari" :
                              "Unknown Browser";

            // Request IP details from ipinfo.io
            string ipDetails = await new HttpClient().GetStringAsync($"https://ipinfo.io/{ipAddress}/json") ?? "{}";

            // Check if IP address exists in the database
            var existingVisitor = _visitorService.GetVisitorByIpAddress(ipAddress);

            //if (existingVisitor != null)
            //{
            //    existingVisitor.DeviceName = deviceName;
            //    existingVisitor.BrowserName = browserName;
            //    existingVisitor.VisitedTime = DateTime.Now;
            //    existingVisitor.VisitorDetails = ipDetails;
            //    existingVisitor.VisitorCount += 1;

            //    _visitorService.UpdateVisitor(existingVisitor);


            //    return Ok(new { Message = "Visit logged successfully!", Visitor = existingVisitor });
            //}
            //else
            //{
            //    // If the IP does not exist, create a new visitor record
            //    var visitor = new VisitorModel
            //    {
            //        IpAddress = ipAddress,
            //        DeviceName = deviceName,
            //        BrowserName = browserName,
            //        VisitedTime = DateTime.Now,
            //        VisitorDetails = ipDetails,
            //        VisitorCount = 1 // Initialize the count
            //    };
            //    _visitorService.AddVisitor(visitor);

            //    return Ok(new { Message = "Visit logged successfully!", Visitor = visitor });
            //}

            var visitor = new VisitorModel
            {
                IpAddress = ipAddress,
                DeviceName = deviceName,
                BrowserName = browserName,
                VisitedTime = DateTime.Now,
                VisitorDetails = ipDetails,
                //VisitorCount = 1
                //VisitorCount = existingVisitor.VisitorCount += 1 ?? 
                VisitorCount = existingVisitor?.VisitorCount + 1 ?? 1
            };
            _visitorService.AddVisitor(visitor);

            return Ok(new { Message = "Visit logged successfully!", Visitor = visitor });
        }


        [HttpGet("GetVisitors")]
        public ActionResult<IEnumerable<VisitorModel>> GetVisitors()
        {
            try
            {
                var visitors = _visitorService.GetVisitors();
                if (visitors == null || !visitors.Any())
                {
                    return NotFound("No visitors found.");
                }
                return Ok(visitors);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database: " + ex.Message);
            }
        }

        [HttpGet("count")]
        public ActionResult<long> GetVisitorCount()
        {
            try
            {
                var count = _visitorService.GetVisitorCount();
                return Ok(count);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving visitor count: " + ex.Message);
            }
        }

    }
}
