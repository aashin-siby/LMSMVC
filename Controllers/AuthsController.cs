using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using LMSAPI.DTO;


namespace LMSMVC.Controllers;

// Controller to handle authentication (login, registration, logout)
public class AuthsController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthsController> _logger;

    public AuthsController(HttpClient httpClient, IConfiguration configuration, ILogger<AuthsController> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _httpClient.BaseAddress = new Uri("http://localhost:5027/");
        _logger = logger;
    }

    // GET: Render the login view
    public IActionResult Login()
    {
        return View();
    }

    // POST: Handle login functionality, authenticate the user
    [HttpPost]
    public async Task<IActionResult> Login(string username, string password)
    {
        var loginDto = new
        {
            Username = username,
            Password = password
        };

        var content = new StringContent(JsonSerializer.Serialize(loginDto), Encoding.UTF8, "application/json");

        // Use the named HttpClient to send the request
        var response = await _httpClient.PostAsync("api/Auth/login", content);

        var responseContent = await response.Content.ReadAsStringAsync();
        // Console.WriteLine($"Response Content: {responseContent}");
        var result = JsonSerializer.Deserialize<JsonElement>(responseContent);

        if (response.IsSuccessStatusCode)
        {
            if (result.TryGetProperty("success", out var success) && success.GetBoolean())
            {
                var token = result.GetProperty("token").GetString();
                var role = result.GetProperty("role").GetString();
                //Store the JWT token, user role, username in session 
                HttpContext.Session.SetString("AuthToken", token);
                HttpContext.Session.SetString("UserRole", role);
                HttpContext.Session.SetString("Username", username);
                _logger.LogInformation("Stored the User data into Session");
                return RedirectToAction("Index", "Book");
            }
            else
            {
                _logger.LogError("Error in storing into the Session");
                ViewBag.ErrorMessages = new List<string> { result.GetProperty("message").GetString() };
                return View();
            }
        }
        return View();
    }

    // Logout: Clear session data and redirect to the login page
    public IActionResult Logout()
    {
        _logger.LogInformation("User Logged Out");
        HttpContext.Session.Clear();
        return RedirectToAction("Login", "Auths");
    }

    // GET: Render the registration view
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    // POST: Handle user registration
    [HttpPost]
    public async Task<IActionResult> Register(UserRegister userRegister)
    {
        try
        {
            var content = new StringContent(
                JsonSerializer.Serialize(userRegister),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync("api/Auth/register", content);

            if (response.IsSuccessStatusCode)
            {

                _logger.LogInformation("Registration successful and redirected to Login");
                return RedirectToAction("Login");
            }

            _logger.LogError("Error while Registration");
            return View();
        }
        catch (Exception exception)
        {

            _logger.LogError("Exception while Registering" + exception);
            return View();
        }
    }
}