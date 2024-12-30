using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
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

        // GET: /Events/Create
        [HttpGet("/Events/Create")]
        public IActionResult Create()
        {
            // Ustawienie domyślnej daty na dzisiejszy dzień
            var newEvent = new Event
            {
                Date = DateTime.Today
            };

            return View(newEvent); // Przekazujemy obiekt z ustawioną domyślną datą do widoku
        }

        // POST: /Events/Create
        [HttpPost("/Events/Create")]
        public IActionResult Create(Event newEvent, string Time)
        {
            if (ModelState.IsValid)
            {
                // Jeśli pole godziny nie jest puste, ustaw godzinę wydarzenia
                if (!string.IsNullOrEmpty(Time))
                {
                    // Przekształcamy godzinę w TimeSpan i ustawiamy ją na nowym wydarzeniu
                    var timeSpan = TimeSpan.Parse(Time);
                    newEvent.Date = newEvent.Date.Date + timeSpan; // Łączymy datę z godziną
                }

                _context.Events.Add(newEvent);
                _context.SaveChanges();
                return RedirectToAction("EventsViewer"); // Po zapisaniu przekierowujemy na stronę z listą wydarzeń
            }

            return View(newEvent); // Jeśli wystąpiły błędy walidacji, ponownie wyświetlamy formularz
        }
    }
}
