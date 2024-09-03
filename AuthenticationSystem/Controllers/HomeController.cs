using AuthenticationSystem.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net.Http;
using Newtonsoft.Json;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

namespace AuthenticationSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HttpClient _httpClient;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            _httpClient = new HttpClient();
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Create the data to post
                var userData = new
                {
                    UserName = model.UserName,
                    Password = model.Password
                };

                // Serialize the data to JSON
                var jsonContent = new StringContent(JsonConvert.SerializeObject(userData), System.Text.Encoding.UTF8, "application/json");

                // Post the data to the external API
                var response = await _httpClient.PostAsync("https://retoolapi.dev/tggm1I/data", jsonContent);

                // Check if the post was successful
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("CheckLogin");
                }
                else
                {
                    ViewData["RegisterStatus"] = "failed";
                    return View(model);
                }
            }
            catch (HttpRequestException)
            {
                return StatusCode(500, "Internal server error: Unable to post data to the API.");
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error.");
            }
        }


        // GET: Login Page
        [HttpGet]
        public IActionResult CheckLogin()
        {
            return View();
        }

        // POST: Validate Login
        [HttpPost]
        public async Task<IActionResult> CheckLogin(LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Get the list of users from the API
                var response = await _httpClient.GetStringAsync("https://retoolapi.dev/tggm1I/data");
                var users = JsonConvert.DeserializeObject<List<LoginModel>>(response);

                // Check if the user exists
                var user = users.FirstOrDefault(u => u.UserName == model.UserName && u.Password == model.Password);

                if (user != null)
                {
                    // Create user claims
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.UserName),  // UserName as Claim
                        new Claim(ClaimTypes.Role, "User")  // Can add role for authorization if needed
                    };

                    // Create claims identity
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    // Create authentication properties
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = true,  // Whether the session should persist
                        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)  // Expiry time for the cookie
                    };

                    // Sign in the user
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                    // Redirect to the Show Users Page
                    return RedirectToAction("ShowUsers");
                }
                else
                {
                    ViewData["LoginStatus"] = "failed";
                    return View(model);
                }
            }
            catch (HttpRequestException)
            {
                return StatusCode(500, "Internal server error: Unable to retrieve data from the API.");
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error.");
            }
        }

        // POST: Logout Action
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("CheckLogin");
        }


        [HttpGet]
        [Authorize]  // This ensures only authenticated users can access this action
        public async Task<IActionResult> ShowUsers()
        {
            if (User.Identity.IsAuthenticated)
            {
                ViewBag.LoggedInUserName = User.Identity.Name; // Set the logged-in user's name
            }

            try
            {
                // Fetch data from the API
                var response = await _httpClient.GetStringAsync("https://retoolapi.dev/tggm1I/data");
                var users = JArray.Parse(response);

                // Extract UserName and Id fields
                var userList = users.Select(user => new
                {
                    UserName = (string)user["UserName"],
                    Id = (int)user["id"]
                }).ToList();

                // Return the view with the user list
                return View(userList);
            }
            catch (HttpRequestException)
            {
                return StatusCode(500, "Internal server error: Unable to retrieve data from the API.");
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            // Get the logged-in username
            var loggedInUserName = User.Identity.Name;

            // Check if the user is trying to delete their own account
            var userToDeleteResponse = await _httpClient.GetStringAsync($"https://retoolapi.dev/tggm1I/data/{id}");
            var userToDelete = JsonConvert.DeserializeObject<LoginModel>(userToDeleteResponse);

            if (userToDelete != null && userToDelete.UserName == loggedInUserName)
            {
                // Prevent the logged-in user from deleting their own account
                ViewData["DeleteStatus"] = "cannot_delete_self";
                return RedirectToAction("ShowUsers");
            }

            try
            {
                // Create the URL for the DELETE request
                var requestUrl = $"https://retoolapi.dev/tggm1I/data/{id}";

                // Send the DELETE request to the API
                var response = await _httpClient.DeleteAsync(requestUrl);

                if (response.IsSuccessStatusCode)
                {
                    // Redirect to the Show Users Page after successful deletion
                    return RedirectToAction("ShowUsers");
                }
                else
                {
                    // Handle failure response
                    ViewData["DeleteStatus"] = "failed";
                    return RedirectToAction("ShowUsers");
                }
            }
            catch (HttpRequestException)
            {
                return StatusCode(500, "Internal server error: Unable to delete user from the API.");
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error.");
            }
        }



    }
}
