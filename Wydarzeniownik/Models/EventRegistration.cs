using Wydarzeniownik.Models;
using Microsoft.AspNetCore.Identity;

public class EventRegistration
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public string UserId { get; set; }

    // Dodanie daty rejestracji
    public DateTime RegisteredAt { get; set; }

    public virtual Event Event { get; set; }
    public virtual IdentityUser User { get; set; }
}
