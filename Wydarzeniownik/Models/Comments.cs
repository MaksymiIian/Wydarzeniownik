using System;
using System.ComponentModel.DataAnnotations;

namespace Wydarzeniownik.Models
{
    public class Comments
    {
        public int Id { get; set; }

        [Required]
        public int EventId { get; set; } // Powiązanie z wydarzeniem

        [Required]
        [StringLength(100)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
