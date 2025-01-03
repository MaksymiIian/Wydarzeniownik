using System.ComponentModel.DataAnnotations;

namespace Wydarzeniownik.Models
{
    public class UserLikes
    {
        public int Id { get; set; }

        [Required]
        public int EventId { get; set; } // Powiązanie z wydarzeniem

        [Required]
        public string UserId { get; set; } = string.Empty; // Identyfikator użytkownika, który polubił wydarzenie
    }
}
