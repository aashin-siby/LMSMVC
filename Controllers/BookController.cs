using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text.Json;
using LMSAPI.DTO;
using LMSAPI.Models;
using Microsoft.AspNetCore.Mvc;

// Controller to handle all the functionality user can do
public class BookController : Controller
{
     private readonly HttpClient _httpClient;
     private readonly ILogger<BookController> _logger;

     public BookController(ILogger<BookController> logger)
     {
          _logger = logger;
          _httpClient = new HttpClient();
          _httpClient.BaseAddress = new Uri("http://localhost:5027/");
     }

     // GET: Display the list of books
     
     public async Task<IActionResult> Index()
     {
          try
          {

               var response = await _httpClient.GetAsync("api/UserBooks/books");

               if (response.IsSuccessStatusCode)
               {
                    var content = await response.Content.ReadAsStringAsync();
                    var books = JsonSerializer.Deserialize<IEnumerable<Book>>(content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    _logger.LogInformation("Diplayed the book list");
                    return View(books ?? Enumerable.Empty<Book>());
               }

               _logger.LogError($"API returned status code: {response.StatusCode}");
               return View();
          }
          catch (HttpRequestException httpRequestException)
          {

               _logger.LogError("Connection error" + httpRequestException);
               return View();
          }
          catch (JsonException jsonException)
          {

               _logger.LogError("Error in processing data from the API" + jsonException);
               return View();
          }
          catch (Exception exception)
          {

               _logger.LogError("Unexpected errors" + exception);
               return View();
          }
     }

     // GET: Show the form to borrow a book
     [HttpGet]
     public IActionResult BorrowBook(int? bookId)
     {
          var model = new BorrowBookDto
          {
               BookId = bookId ?? 0,
               BorrowDate = DateTime.Today
          };
          return View(model);
     }

     // POST: Handle borrowing a book
     [HttpPost]
     public async Task<IActionResult> BorrowBook(BorrowBookDto borrowBookDto)
     {
          if (!ModelState.IsValid)
          {
               return View(borrowBookDto);
          }

          var token = HttpContext.Session.GetString("AuthToken");
          if (string.IsNullOrEmpty(token))
          {
               return RedirectToAction("Login", "Auths");
          }

          var handler = new JwtSecurityTokenHandler();
          var jwtToken = handler.ReadJwtToken(token);

          var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
          if (userIdClaim == null)
          {
               return RedirectToAction("Login", "Auths");
          }

          borrowBookDto.UserId = int.Parse(userIdClaim);

          _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

          var content = new StringContent(JsonSerializer.Serialize(borrowBookDto), System.Text.Encoding.UTF8, "application/json");
          var response = await _httpClient.PostAsync("api/UserBooks/borrow", content);

          if (response.IsSuccessStatusCode)
          {
               _logger.LogInformation("Book borrowed successfully");
               TempData["SuccessMessage"] = "Book borrowed successfully.";
               return RedirectToAction("Index");
          }

          _logger.LogError("Error borrowing book");
          var errorContent = await response.Content.ReadAsStringAsync();
          ModelState.AddModelError("", $"An error occurred while borrowing the book: {errorContent}");

          var bookResponse = await _httpClient.GetAsync($"api/Books/{borrowBookDto.BookId}");
          if (bookResponse.IsSuccessStatusCode)
          {
               var bookContent = await bookResponse.Content.ReadAsStringAsync();
               var book = JsonSerializer.Deserialize<Book>(bookContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
               ViewBag.BookTitle = book.Title;
               ViewBag.BookAuthor = book.Author;
          }

          _logger.LogWarning("Refetched book details");
          return View(borrowBookDto);
     }

     // GET: Show the form to return a borrowed book
     [HttpGet]
     public IActionResult ReturnBook(int BorrowId)
     {
          var returnBookDto = new ReturnBookDto
          {
               BorrowId = BorrowId,
               ReturnDate = DateTime.Today
          };
          return View(returnBookDto);
     }

     // POST: Handle returning a borrowed book
     [HttpPost]
     public async Task<IActionResult> ReturnBook(ReturnBookDto returnBookDto)
     {
          if (!ModelState.IsValid)
          {

               _logger.LogError("Model validation fails");
               return View(returnBookDto);
          }

          var token = HttpContext.Session.GetString("AuthToken");
          if (string.IsNullOrEmpty(token))
          {

               _logger.LogError("No token found");
               return RedirectToAction("Login", "Auths");
          }

          var handler = new JwtSecurityTokenHandler();
          var jwtToken = handler.ReadJwtToken(token);

          var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
          if (userIdClaim == null)
          {

               _logger.LogError("User ID is missing");
               return RedirectToAction("Login", "Auths");
          }

          returnBookDto.UserId = int.Parse(userIdClaim);

          _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

          var content = new StringContent(JsonSerializer.Serialize(returnBookDto), System.Text.Encoding.UTF8, "application/json");
          var response = await _httpClient.PostAsync("api/UserBooks/return", content);

          if (response.IsSuccessStatusCode)
          {

               _logger.LogInformation("Retuned the book successfully");
               TempData["SuccessMessage"] = "Book returned successfully.";
               return RedirectToAction("Index");
          }

          var errorContent = await response.Content.ReadAsStringAsync();
          ModelState.AddModelError("", $"An error occurred while returning the book: {errorContent}");
          _logger.LogWarning("Refetching return book");

          return View(returnBookDto);
     }

     // GET: Show the list of all rentals (books borrowed by the user)
     public async Task<IActionResult> AllRentals()
     {
          var token = HttpContext.Session.GetString("AuthToken");
          if (string.IsNullOrEmpty(token))
          {

               _logger.LogError("No token found");
               return RedirectToAction("Login", "Auths");
          }

          _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

          var response = await _httpClient.GetAsync("api/UserBooks/rentals");
          if (response.IsSuccessStatusCode)
          {

               var content = await response.Content.ReadAsStringAsync();
               var rentals = JsonSerializer.Deserialize<IEnumerable<BorrowDetails>>(content,
                   new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
               var viewrental = rentals;

               _logger.LogInformation("List Displayed successfully");
               return View(viewrental);
          }

          _logger.LogError("Error ");
          return View();
     }
}

