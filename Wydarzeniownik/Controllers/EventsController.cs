using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
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

        // Wyświetlenie listy wydarzeń dla obecnie zalogowanego użytkownika
        [HttpGet("/Events/MyEvents")]
        public async Task<IActionResult> MyEventsViewerAsync()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return View(await _context.Events
                .Include(e => e.User)
                .Where(e => e.UserId == userId)
                .ToListAsync());
        }

        // Wyświetlenie listy wydarzeń
        public IActionResult EventsViewer(string sortOrder)
        {
            var events = _context.Events.AsQueryable();

            // Obsługa sortowania
            events = sortOrder switch
            {
                "date_desc" => events.OrderByDescending(e => e.Date),
                "date_asc" => events.OrderBy(e => e.Date),
                _ => events
            };

            return View(events.ToList());
        }

        // Wyświetlenie szczegółów wydarzenia
        public IActionResult Details(int id)
        {
            var eventItem = _context.Events
                .Include(e => e.Comments)
                .Include(e => e.EventRegistrations) // Dołączamy rejestracje
                .FirstOrDefault(e => e.Id == id);

            if (eventItem == null)
            {
                return NotFound();
            }

            return View(eventItem);
        }

        // Edycja wydarzenia (formularz)
        [HttpGet("/Events/Edit/{id}")]
        public IActionResult Edit(int id)
        {
            var eventItem = _context.Events.FirstOrDefault(e => e.Id == id);
            if (eventItem == null)
            {
                return NotFound();
            }

            // Sprawdzenie, czy obecny użytkownik jest autorem wydarzenia
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (eventItem.UserId != userId)
            {
                return Forbid();
            }

            return View(eventItem);
        }

        // Edycja wydarzenia (zapis z obsługą godziny i obrazu)
        [HttpPost("/Events/Edit/{id}")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Event updatedEvent, string Time, IFormFile Image)
        {
            if (id != updatedEvent.Id)
            {
                return BadRequest();
            }

            var existingEvent = _context.Events.FirstOrDefault(e => e.Id == id);
            if (existingEvent == null)
            {
                return NotFound();
            }

            // Sprawdzenie, czy obecny użytkownik jest autorem wydarzenia
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (existingEvent.UserId != userId)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                existingEvent.Title = updatedEvent.Title;
                existingEvent.Description = updatedEvent.Description;
                existingEvent.Location = updatedEvent.Location;

                // Obsługa godziny
                if (!string.IsNullOrEmpty(Time))
                {
                    try
                    {
                        var timeSpan = TimeSpan.Parse(Time);
                        existingEvent.Date = updatedEvent.Date.Date + timeSpan;
                    }
                    catch (FormatException)
                    {
                        ModelState.AddModelError("Time", "Nieprawidłowy format godziny.");
                        return View(updatedEvent);
                    }
                }

                // Obsługa nowego obrazu
                if (Image != null && Image.Length > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = Path.GetExtension(Image.FileName).ToLower();

                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("Image", "Dozwolone są tylko obrazy: jpg, jpeg, png, gif.");
                        return View(updatedEvent);
                    }

                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", Image.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        Image.CopyTo(stream);
                    }

                    existingEvent.ImagePath = $"/images/{Image.FileName}";
                }

                _context.Update(existingEvent);
                _context.SaveChanges();

                return RedirectToAction("Details", new { id = existingEvent.Id });
            }

            return View(updatedEvent);
        }

        // Usuwanie wydarzenia
        [HttpPost("/Events/Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var eventItem = await _context.Events.FindAsync(id);
            if (eventItem != null)
            {
                _context.Events.Remove(eventItem);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("EventsViewer");
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

            return RedirectToAction("EventsViewer");
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
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Ensure the user exists in the database
                var userExists = _context.Users.Any(u => u.Id == userId);
                if (!userExists)
                {
                    ModelState.AddModelError("", "The logged-in user does not exist in the database.");
                    return View(newEvent);
                }

                newEvent.UserId = userId;
                string userEmail = User.FindFirstValue(ClaimTypes.Email);
                newEvent.UserEmail = userEmail;

                if (!string.IsNullOrEmpty(Time))
                {
                    try
                    {
                        var timeSpan = TimeSpan.Parse(Time);
                        newEvent.Date = newEvent.Date.Date + timeSpan;
                    }
                    catch (FormatException)
                    {
                        ModelState.AddModelError("Time", "Nieprawidłowy format godziny.");
                        return View(newEvent);
                    }
                }

                if (Image != null && Image.Length > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
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

                    newEvent.ImagePath = $"/images/{Image.FileName}";
                }

                _context.Events.Add(newEvent);
                _context.SaveChanges();

                return RedirectToAction("EventsViewer");
            }

            return View(newEvent);
        }

        // Akcja do zapisania się na wydarzenie
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(int eventId)
        {
            var eventItem = await _context.Events
                                          .Include(e => e.EventRegistrations)
                                          .FirstOrDefaultAsync(e => e.Id == eventId);

            if (eventItem == null)
            {
                return NotFound();
            }

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Sprawdzenie, czy użytkownik jest już zapisany
            var existingRegistration = eventItem.EventRegistrations
                                                .FirstOrDefault(er => er.UserId == userId);

            if (existingRegistration == null)
            {
                // Dodanie zapisu na wydarzenie
                var registration = new EventRegistration
                {
                    EventId = eventId,
                    UserId = userId,
                    RegisteredAt = DateTime.Now // Zapis daty rejestracji
                };
                _context.EventRegistrations.Add(registration);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Zapisano na wydarzenie!";
            }
            else
            {
                // Usunięcie zapisu
                _context.EventRegistrations.Remove(existingRegistration);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Wyrejestrowano z wydarzenia!";
            }

            // Przekierowanie do szczegółów wydarzenia
            return RedirectToAction("Details", new { id = eventId });
        }
    }
}
