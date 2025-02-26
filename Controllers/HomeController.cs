using Microsoft.AspNetCore.Mvc;

namespace LMSMVC.Controllers;

// HomeController handles the main page and privacy view for the application
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    // Index action that loads the main page
    public IActionResult Index()
    {

        _logger.LogInformation("Main Page Loaded");
        return View();
    }

    // Privacy action that displays the privacy policy page
    public IActionResult Privacy()
    {

        _logger.LogInformation("Privacy Page Loaded");
        return View();
    }

  
}
