using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace NotatApp.Models
{
    public class Note
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Content { get; set; }

        // Represents whether the note is archived/completed
        public bool IsDone { get; set; } = false; 
        public DateOnly? ScheduledAt { get; set; }

        [ForeignKey(nameof(Folder))]
        public int? FolderId { get; set; }

        [ValidateNever]
        [JsonIgnore]
        public Folder? Folder { get; set; }

        public int OrderIndex { get; set; } = 0;

        [Required]
        [ForeignKey(nameof(User))]
        public string UserId { get; set; } = default!;
    }

    // DTOs used for incoming request bodies

    public class CreateNoteDto
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Content { get; set; }

        public int? FolderId { get; set; }
        public DateOnly? ScheduledAt { get; set; }
    }

    public class UpdateNoteDto
    {
        // Optional for partial updates; if provided must be 1..100 chars
        [StringLength(100, MinimumLength = 1)]
        public string? Title { get; set; }

        [StringLength(1000)]
        public string? Content { get; set; }

        public int? FolderId { get; set; }

        // Use the same terminology as the entity
        public bool? IsDone { get; set; }

        public DateOnly? ScheduledAt { get; set; }
    }
}
