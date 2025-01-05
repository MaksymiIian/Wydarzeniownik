using Microsoft.AspNetCore.Identity;
using System;

namespace Wydarzeniownik.Models
{
    public class EventRegistration
    {
            public int Id { get; set; }

            public string UserId { get; set; }  // ID użytkownika
            public int EventId { get; set; }    // ID wydarzenia
            public DateTime RegisteredAt { get; set; }

        // Nawigacyjne właściwości
            public virtual IdentityUser User { get; set; }
            public virtual Event Event { get; set; }
    }
}
