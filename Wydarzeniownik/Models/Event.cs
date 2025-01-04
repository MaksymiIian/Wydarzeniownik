using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using Wydarzeniownik.Models;

namespace Wydarzeniownik.Models
{
    public class Event
    {
        public int Id { get; set; }

        public string? UserId { get; set; }
        public IdentityUser? User { get; set; }

        [Required(ErrorMessage = "Tytuł jest wymagany.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Data jest wymagana.")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Opis jest wymagany.")]
        [StringLength(1000, ErrorMessage = "Opis nie może przekroczyć 1000 znaków.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Lokalizacja jest wymagana.")]
        public string Location { get; set; }

        public int LikeCount { get; set; } = 0;
        public int ShareCount { get; set; } = 0;

        // Komentarze - nie wymagamy tego pola przy tworzeniu wydarzenia
        public virtual ICollection<Comments> Comments { get; set; } = new List<Comments>();

        // Ścieżka do zdjęcia
        public string? ImagePath { get; set; }

        // Dodatkowa właściwość na e-mail użytkownika (autora wydarzenia)
        public string? UserEmail { get; set; }
    }
}
