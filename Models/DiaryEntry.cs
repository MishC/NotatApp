using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace NotatApp.Models
{
    public class DiaryEntry
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [StringLength(20000)]
        public string? Content { get; set; }

        [Required]
        public DateOnly Date { get; set; }

        [Required]
        [ForeignKey(nameof(User))]
        public string UserId { get; set; } = default!;
    }

    public class CreateDiaryEntryDto
    {
        [Required, StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [StringLength(5000)]
        public string? Content { get; set; }

        public DateOnly Date { get; set; }
    }

    public class UpdateDiaryEntryDto
    {
        [StringLength(150)]
        public string? Title { get; set; }

        [StringLength(5000)]
        public string? Content { get; set; }

        public DateOnly? Date { get; set; }
    }
}
