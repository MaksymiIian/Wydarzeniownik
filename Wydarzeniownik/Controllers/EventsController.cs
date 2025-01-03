using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using Wydarzeniownik.Data;
using Wydarzeniownik.Models;

namespace Wydarzeniownik.Controllers
{
    [Authorize]
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Wyświetlenie listy wydarzeń
        public IActionResult EventsViewer()
        {
            var events = _context.Events.ToList();
            return View(events);
        }

        // Wyświetlenie szczegółów wydarzenia
        public IActionResult Details(int id)
        {
            var eventItem = _context.Events
                .Include(e => e.Comments)
                .FirstOrDefault(e => e.Id == id);

            if (eventItem == null)
            {
                return NotFound();
            }

            return View(eventItem);
        }

        // Akcja do polubienia wydarzenia
        [HttpPost]
        public IActionResult Like(int id)
        {
            var eventItem = _context.Events.FirstOrDefault(e => e.Id == id);
            if (eventItem != null)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Sprawdzenie, czy użytkownik już polubił to wydarzenie
                var userLikedEvent = _context.UserLikes.FirstOrDefault(ul => ul.UserId == userId && ul.EventId == eventItem.Id);
                if (userLikedEvent == null)
                {
                    eventItem.LikeCount++;
                    _context.UserLikes.Add(new UserLikes { UserId = userId, EventId = eventItem.Id });
                    _context.SaveChanges();
                }
            }

            return RedirectToAction("Details", new { id });
        }

        // Dodawanie komentarza do wydarzenia
        [HttpPost]
        public IActionResult AddComment(int id, string content)
        {
            try
            {
                var eventItem = _context.Events.Include(e => e.Comments).FirstOrDefault(e => e.Id == id);
                if (eventItem == null)
                {
                    return NotFound("Wydarzenie nie zostało znalezione.");
                }

                var userEmail = User.FindFirstValue(ClaimTypes.Email);
                if (string.IsNullOrEmpty(userEmail))
                {
                    return Unauthorized("Użytkownik musi być zalogowany.");
                }

                if (!string.IsNullOrWhiteSpace(content))
                {
                    var newComment = new Comments
                    {
                        EventId = id,
                        UserName = userEmail,
                        Content = content,
                        CreatedAt = DateTime.Now
                    };

                    _context.Comments.Add(newComment);
                    _context.SaveChanges();
                }
                else
                {
                    return BadRequest("Treść komentarza nie może być pusta.");
                }

                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd podczas dodawania komentarza: {ex.Message}");
                return StatusCode(500, "Wewnętrzny błąd serwera.");
            }
        }

        // Wyświetlanie formularza tworzenia wydarzenia
        [HttpGet("/Events/Create")]
        public IActionResult Create()
        {
            var newEvent = new Event { Date = DateTime.Today };
            return View(newEvent);
        }

        // Tworzenie nowego wydarzenia
        [HttpPost("/Events/Create")]
public IActionResult Create(Event newEvent, string Time, IFormFile Image)
{
    if (ModelState.IsValid)
    {
        // Jeśli czas jest podany, łączymy go z datą
        if (!string.IsNullOrEmpty(Time))
        {
            try
            {
                var timeSpan = TimeSpan.Parse(Time); // Parsowanie godziny
                newEvent.Date = newEvent.Date.Date + timeSpan; // Łączenie daty z godziną
            }
            catch (FormatException)
            {
                ModelState.AddModelError("Time", "Nieprawidłowy format godziny.");
                return View(newEvent);
            }
        }

        // Walidacja zdjęcia (jeśli przesłane)
        if (Image != null && Image.Length > 0)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" }; // Dozwolone rozszerzenia obrazów
            var extension = Path.GetExtension(Image.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError("Image", "Dozwolone są tylko obrazy: jpg, jpeg, png, gif.");
                return View(newEvent);
            }

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", Image.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                Image.CopyTo(stream);
            }

            newEvent.ImagePath = $"/images/{Image.FileName}"; // Zapisywanie ścieżki zdjęcia w modelu
        }

        // Dodanie wydarzenia do bazy
        _context.Events.Add(newEvent);
        _context.SaveChanges();

        // Przekierowanie do widoku listy wydarzeń
        return RedirectToAction("EventsViewer");
    }

    // Jeśli formularz jest niepoprawny, wracamy do formularza
    return View(newEvent);
}

    }
}
